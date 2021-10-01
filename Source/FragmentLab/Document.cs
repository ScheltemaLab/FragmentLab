/* Copyright (C) 2015, Biomolecular Mass Spectrometry and Proteomics (http://hecklab.com)
 * This file is part of HeckLib.
 * 
 * HeckLib is free software; you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as published by
 * the Free Software Foundation; either version 2.1 of the License, or
 * (at your option) any later version.
 * 
 * HeckLib is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser General Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser General Public License
 * along with HeckLib; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
 */


// C#
using System;
using System.IO;
using System.Linq;
using System.Threading;

using System.Text;
using System.Text.RegularExpressions;

using System.Windows.Forms;
using System.Collections.Generic;

// HeckLib
using HeckLib;
using HeckLib.utils;
using HeckLib.masspec;
using HeckLib.chemistry;
using HeckLib.backgroundworker;

using HeckLib.io;
using HeckLib.io.xml;
using HeckLib.io.fasta;
using HeckLib.io.database;
using HeckLib.io.fileformats;

using hecklib.graphics;
using hecklib.graphics.csvimport;





namespace FragmentLab
{
	public class Document : IDisposable
	{
		public enum ImportType
		{
			GENERIC,
			PEPXML,
			MZIDENTML,
			RAW,
			ERROR
		}

		public class BackgroundWorkerData
		{
			public string Filename;
			public FragmentLabSettings Settings;
			public List<PeptideSpectrumMatch> PsmList;

			public Dictionary<string, string> ColumnMapping;

			// results
			public FrequentFlyers FreqFyerResult;
			public FrequentFlyers.InterpretedData FreqFlyerData;
		}


		#region rawfile access
		private static HeckLibRawFiles.HeckLibRawFile OpenFile(string filename, out string path)
		{
			try
			{
				path = FilesA.GetDirectoryName(filename);

				if (File.Exists(filename + ".raw"))
					return new HeckLibRawFileThermo.HeckLibRawFileThermo(filename + ".raw");
				else if (File.Exists(filename + ".mgf"))
					return new HeckLibRawFileMgf.HeckLibMgfRawFile(filename + ".mgf");
				else if (File.Exists(filename + ".tdf"))
					return new HeckLibRawFileBruker.HeckLibBrukerRawFile(path, new Tolerance(50, Tolerance.ErrorUnit.PPM), 400, new Tolerance(20, Tolerance.ErrorUnit.PPM));
				else if (File.Exists(filename + ".mzxml"))
					return new HeckLibRawFileMzXml.HeckLibMzXmlRawFile(filename + ".mzxml");
				else if (File.Exists(filename + ".mzml"))
					return new HeckLibRawFileMzXml.HeckLibMzMlRawFile(filename + ".mzml");
				else
					return null;
			}
			catch (Exception e)
			{
				MessageBox.Show(e.Message + "\n" + e.StackTrace);
			}
			path = "";
			return null;
		}

		private bool OpenRawfileStream(PeptideSpectrumMatch psm)
		{
			string rawfilename = psm.RawFile;
			if (rawfilename != m_sCurrentRawFile)
			{
				if (m_pCurrentRawFile != null)
					m_pCurrentRawFile.Close();

				m_sCurrentRawFile = rawfilename;
				string path;
				m_pCurrentRawFile = OpenFile(rawfilename, out path);

				return m_pCurrentRawFile != null;
			}
			return true;
		}

		private static string CreateRawFileName(PeptideSpectrumMatch psm, string path, Dictionary<string, string> filename_cache, out bool cancelall)
		{
			cancelall = false;
			string filename = Path.GetFileNameWithoutExtension(psm.RawFile);
			if (filename_cache.ContainsKey(filename))
				return filename_cache[filename];

			string path_mq = Path.GetFullPath(Path.Combine(path, "..\\..\\"));
			string[] files_mq = Directory.GetFiles(path_mq, filename + ".*", SearchOption.TopDirectoryOnly);
			string[] files_normal = Directory.GetFiles(path, filename + ".*", SearchOption.TopDirectoryOnly);

			string result = null;
			if (CorrectExtensionPresent(files_normal))
			{
				result = Path.Combine(path, filename);
			}
			else if (CorrectExtensionPresent(files_mq))
			{
				result = Path.Combine(path_mq, filename);
			}
			else
			{
				OpenFileDialog dlg = new OpenFileDialog();
				dlg.Title = "Can't locate raw-file, please select";
				dlg.Filter = HeckLibRawFiles.HeckLibRawFile.FileDialogExtensions;
				dlg.FileName = Path.GetFileName(filename);

				DialogResult r = dlg.ShowDialog();
				if (r == DialogResult.OK)
					result = Path.Combine(Path.GetDirectoryName(dlg.FileName), Path.GetFileNameWithoutExtension(psm.RawFile));
				else if (r == DialogResult.Cancel)
				{
					if (MessageBox.Show("Do you want to cancel locating other raw-files as well?", "Cancel all?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
						cancelall = true;
				}
			}

			filename_cache[filename] = result;
			return result;
		}

		private static bool CorrectExtensionPresent(string[] files)
		{
			foreach (string file in files)
			{
				string ext = Path.GetExtension(file);
				foreach (string rext in HeckLibRawFiles.HeckLibRawFile.SupportedExtensions)
					if (ext.ToLower() == rext.ToLower()) return true;
			}
			return false;
		}
		#endregion


		#region constructor(s)
		public Document(HeckLibSettingsDatabase settingsdb)
		{
			m_pSettingsDatabase = settingsdb;
		}

		public Document(HeckLibSettingsDatabase settingsdb, List<PeptideSpectrumMatch> psms, string path)
		{
			m_pSettingsDatabase = settingsdb;
			m_sPath = path;

			m_lPsms = new Dictionary<string, List<PeptideSpectrumMatch>>();
			m_nNumberPsms = psms.Count;
			foreach (PeptideSpectrumMatch psm in psms)
			{
				if (!m_lPsms.ContainsKey(psm.RawFile))
					m_lPsms.Add(psm.RawFile, new List<PeptideSpectrumMatch>());
				m_lPsms[psm.RawFile].Add(psm);
			}
		}

		public Document(HeckLibSettingsDatabase settingsdb, string[] filenames, ImportType type, HlBackgroundWorker<Document.BackgroundWorkerData> backgroundworker, List<PeptideSpectrumMatch> old_psms)
		{
			m_pSettingsDatabase = settingsdb;
			m_sPath = FilesA.GetDirectoryName(filenames[0]);

			// load the psms and sort them on raw-file; if available retain the old PSM list as well
			m_nNumberPsms = 0;
			m_lPsms = new Dictionary<string, List<PeptideSpectrumMatch>>();

			if (old_psms != null)
			{
				foreach (PeptideSpectrumMatch psm in old_psms)
				{
					if (!m_lPsms.ContainsKey(psm.RawFile))
						m_lPsms.Add(psm.RawFile, new List<PeptideSpectrumMatch>());
					m_lPsms[psm.RawFile].Add(psm);
				}
			}

			Document.BackgroundWorkerData data = new Document.BackgroundWorkerData();
			foreach (string filename in filenames)
			{
				switch (type)
				{
					case ImportType.GENERIC:
						CsvImport dlg = new CsvImport();
						dlg.StartPosition	= FormStartPosition.CenterParent;
						dlg.FileName		= filename;
						dlg.FileFormat		= new GenericFileFormat();
						dlg.AutoMap			= true;
						if (dlg.ShowDialog() != DialogResult.OK)
							return;

						data = new Document.BackgroundWorkerData {
								Filename		= filename,
								Settings		= new FragmentLabSettings(),
								PsmList			= null,
								ColumnMapping	= dlg.Mapping
							};
						backgroundworker.TriggerBackgroundWorker(LoadGeneric, data);
						break;
					case ImportType.PEPXML:
						data = new Document.BackgroundWorkerData {
								Filename		= filename,
								Settings		= new FragmentLabSettings(),
								PsmList			= null,
								ColumnMapping	= null
							};
						backgroundworker.TriggerBackgroundWorker(LoadPepXML, data);
						break;
					case ImportType.MZIDENTML:
						data = new Document.BackgroundWorkerData {
								Filename		= filename,
								Settings		= new FragmentLabSettings(),
								PsmList			= null,
								ColumnMapping	= null
							};
						backgroundworker.TriggerBackgroundWorker(LoadMzIdentML, data);
						break;
					case ImportType.RAW:
						data = new Document.BackgroundWorkerData {
								Filename		= filename,
								Settings		= new FragmentLabSettings(),
								PsmList			= null,
								ColumnMapping	= null
							};
						backgroundworker.TriggerBackgroundWorker(LoadRawFile, data);
						break;
					default:
						MessageBox.Show("This filter is not yet supported :)", "Error");
						return;
				}
			}

			// deferred localisation of raw-files to have the ability to start a file-dialog
			bool cancelall = false;
			Dictionary<string, string> rawfilenames = new Dictionary<string, string>();
			foreach (string key in m_lPsms.Keys)
			{
				foreach (PeptideSpectrumMatch psm in m_lPsms[key])
				{
					psm.RawFile = CreateRawFileName(psm, m_sPath, rawfilenames, out cancelall);
					if (cancelall)
						break;
				}
				if (cancelall)
					break;
			}

			// order the psms on scannumber
			foreach (string rawfile in m_lPsms.Keys)
				m_lPsms[rawfile].Sort((a, b) => a.MinScan.CompareTo(b.MinScan));
		}

		private void LoadGeneric(BackgroundWorkerData data, IProgress<int> progress, CancellationToken iscancelled)
		{
			try
			{
				GenericFileFormat.Format format = GenericFileFormat.Format.UNKNOWN;

				Dictionary<string, Modification> modifications = m_pSettingsDatabase.GetModifications();
				CsvFileReader<GenericFileFormat>.Parse(data.Filename, delegate (GenericFileFormat record, double preogress, out bool cancel) {
						progress.Report((int) Math.Floor(100 * preogress));
						cancel = false;
						if (iscancelled.IsCancellationRequested)
						{
							cancel = true;
							m_lPsms.Clear();
							m_nNumberPsms = 0;
							return;
						}
					
						Peptide peptide = GenericFileFormat.CreatePeptide(record, modifications, format, out format);
						if (peptide.Length != peptide.Modifications.Length)
							GenericFileFormat.CreatePeptide(record, modifications, format, out format);

						float energy = 0;
						float.TryParse(record.FragmentationEnergy, out energy);
						PeptideSpectrumMatch psm = new PeptideSpectrumMatch {
								MinScan				= record.ScanNumber,
								MaxScan				= record.ScanNumber,
								Fragmentation		= Spectrum.TranslateFragmentation(record.FragmentationType),
								FragmentationEnergy	= energy,
								RetentionTime		= record.RetentionTime,
								RawFile				= record.SpectrumFile,
								Mz					= record.Mz,
								Charge				= record.Charge,
								Peptide				= peptide,
								Score				= record.Score,
								Proteins			= record.Proteins,
								GeneNames			= record.GeneNames
							};
								
						if (!m_lPsms.ContainsKey(psm.RawFile))
							m_lPsms.Add(psm.RawFile, new List<PeptideSpectrumMatch>());
						m_lPsms[psm.RawFile].Add(psm);
						m_nNumberPsms++;
					}, false, data.ColumnMapping);
			}
			catch (IOException e)
			{
				System.Diagnostics.Debug.WriteLine(e.Message);
				System.Diagnostics.Debug.WriteLine(e.StackTrace);
				m_lPsms.Clear();
				m_nNumberPsms = 0;
				MessageBox.Show("Unable to open file\n\n'" + data.Filename + "'\n\nand exiting.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			catch (Exception e)
			{
				System.Diagnostics.Debug.WriteLine(e.Message);
				System.Diagnostics.Debug.WriteLine(e.StackTrace);
				m_lPsms.Clear();
				m_nNumberPsms = 0;
				MessageBox.Show("Unable to interpret file\n\n'" + data.Filename + "'\n\nand exiting.\n\n'" + e.Message + "'", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void LoadPepXML(BackgroundWorkerData data, IProgress<int> progress, CancellationToken iscancelled)
		{
			string rawfile = null;
			PepXmlReader.Parse(data.Filename,
					delegate (PepXml.RunSummary summary)
					{
						rawfile = Path.Combine(Path.GetDirectoryName(data.Filename), summary.BaseName + "." + summary.RawDataType);
					},
					delegate (PepXml.SearchSummary summary) {},
					delegate (PepXml.SpectrumQuery query)
					{
						if (query.SearchHits.Count == 0)
							return;

						// ensure best rank first
						query.SearchHits.Sort((a, b) => a.Rank.CompareTo(b.Rank));

						string accession = "";
						if (!string.IsNullOrEmpty(query.SearchHits[0].Protein))
							accession = FastaUtils<SearchableSequenceDatabase>.RetrieveAccession(">" + query.SearchHits[0].Protein);

						PeptideSpectrumMatch psm = new PeptideSpectrumMatch();
						psm.RawFile			= rawfile;
						psm.Charge			= query.Charge;
						psm.Mz				= query.Mz;
						psm.RetentionTime	= query.Charge;
						psm.MinScan			= query.MinScan;
						psm.MaxScan			= query.MaxScan;
						psm.Fragmentation	= Spectrum.FragmentationType.HCD;
						psm.Peptide			= query.SearchHits[0].Peptide;
						psm.Proteins		= accession;
						psm.Score			= query.SearchHits[0].SearchScores.Count > 0 ? query.SearchHits[0].SearchScores[0].Score : double.NaN;

						if (!m_lPsms.ContainsKey(psm.RawFile))
							m_lPsms.Add(psm.RawFile, new List<PeptideSpectrumMatch>());
						m_lPsms[psm.RawFile].Add(psm);
						m_nNumberPsms++;
					}
				);
		}

		private void LoadMzIdentML(BackgroundWorkerData data, IProgress<int> progress, CancellationToken iscancelled)
		{
			Regex regex_number = new Regex(@"(?<scannumber>\d+)");

			Dictionary<string, string> rawfiles = new Dictionary<string, string>();
			Dictionary<string, Peptide> peptides = new Dictionary<string, Peptide>();
			Dictionary<string, Modification> modifications = new Dictionary<string, Modification>();

			MzIdentReader.Parse(data.Filename,
					// meta-data
					delegate (MzIdent.SpectrumIdentification mzid_si) { },
					delegate (MzIdent.SpectrumIdentificationProtocol mzid_protocol) { },
					delegate (MzIdent.Inputs mzid_inputs)
					{
						// PD reports mzml files that are not there?
						rawfiles.Add(mzid_inputs.SpectraData.Identifier, Path.GetFileNameWithoutExtension(mzid_inputs.SpectraData.Location));
					},
					// data
					delegate (MzIdent.DbSequence mzid_dbsequence) { },
					delegate (MzIdent.Peptide mzid_peptide)
					{
						// extract the peptide
						Peptide peptide = new Peptide(mzid_peptide.Sequence);
						
						// extract the relevant modifications
						foreach (MzIdent.Modification mzid_modification in mzid_peptide.Modifications)
						{
							CvParam cv = mzid_modification.CvParams.Values.ToArray()[0];
							if (!modifications.ContainsKey(cv.Accession))
							{
								Modification modification = new Modification();
								modification.Title			= cv.Name;
								modification.Description	= cv.Name;
								modification.Delta			= mzid_modification.Delta;
								modification.Sites			= new Modification.Site[0];
								modifications.Add(cv.Accession, modification);
							}

							if (mzid_modification.Location == 0)
								peptide.Nterm = modifications[cv.Accession];
							else if (mzid_modification.Location == peptide.Length + 1)
								peptide.Cterm = modifications[cv.Accession];
							else
								peptide.Modifications[mzid_modification.Location - 1] = modifications[cv.Accession];
						}

						peptides.Add(mzid_peptide.Identifier, peptide);
					},
					delegate (MzIdent.PeptideEvidence mzid_evidence) { },
					delegate (MzIdent.SpectrumIdentificationList mzid_list) { },
					delegate (MzIdent.SpectrumIdentificationResult mzid_evidence)
					{
						if (mzid_evidence.Items.Count == 0)
							return;
						if (!rawfiles.ContainsKey(mzid_evidence.SpectraDataRef))
							return;
						MzIdent.SpectrumIdentificationItem identification = mzid_evidence.Items[0];

						string s = regex_number.Match(mzid_evidence.SpectrumIdentifier).Groups["scannumber"].Value;
						int scannumber = int.Parse(s);

						PeptideSpectrumMatch psm = new PeptideSpectrumMatch();
						psm.RawFile			= rawfiles[mzid_evidence.SpectraDataRef];
						psm.Charge			= identification.Charge;
						psm.Mz				= identification.ExperimentaldMz;
						psm.RetentionTime	= mzid_evidence.CvParams.ContainsKey("MS:1000894") ? float.Parse(mzid_evidence.CvParams["MS:1000894"].Value) : 0;
						psm.MinScan			= scannumber;
						psm.MaxScan			= scannumber;
						psm.Fragmentation	= Spectrum.FragmentationType.HCD;
						psm.Peptide			= peptides[identification.PeptideRef];

						if (!m_lPsms.ContainsKey(psm.RawFile))
							m_lPsms.Add(psm.RawFile, new List<PeptideSpectrumMatch>());
						m_lPsms[psm.RawFile].Add(psm);
						m_nNumberPsms++;
					}
				);
		}

		private void LoadRawFile(BackgroundWorkerData data, IProgress<int> progress, CancellationToken iscancelled)
		{
			// open the file, this will overwrite the path which is needed for path-based raw-data (e.g. Bruker)
			string file_no_ext = Path.GetFileNameWithoutExtension(data.Filename);
			string path = Path.GetDirectoryName(data.Filename);
			string correct_path = Path.Combine(path, file_no_ext);

			using (HeckLibRawFiles.HeckLibRawFile rawfile = OpenFile(correct_path, out m_sPath))
			{
				int sn_first = rawfile.FirstSpectrumNumber();
				int sn_last = rawfile.LastSpectrumNumber();
				for (int scannumber = sn_first; scannumber <= sn_last; ++scannumber)
				{
					double current_progress = sn_last == sn_first ? 100 : Math.Floor(100 * scannumber / (double)(sn_last - sn_first));
					progress.Report((int)current_progress);
					if (iscancelled.IsCancellationRequested)
						break;

					ScanHeader header = rawfile.GetScanHeader(scannumber);
					PrecursorInfo precursor = rawfile.GetPrecursorInfo(scannumber);
					if (header.ScanType != Spectrum.ScanType.MSn)
						continue;

					PeptideSpectrumMatch psm = new PeptideSpectrumMatch {
							MinScan				= header.ScanNumber,
							MaxScan				= header.ScanNumber,
							ImsIndex			= header.IonMobilityIndex,
							Fragmentation		= precursor.Fragmentation,
							RetentionTime		= (float)precursor.RetentionTime,
							RawFile				= correct_path,
							Mz					= precursor.Mz,
							Charge				= (short)precursor.Charge,
							Mobility			= precursor.Mobility,
							Intensity			= precursor.Intensity,
							Peptide				= new Peptide("")
						};
					if (!m_lPsms.ContainsKey(psm.RawFile))
						m_lPsms.Add(psm.RawFile, new List<PeptideSpectrumMatch>());
					m_lPsms[psm.RawFile].Add(psm);
					m_nNumberPsms++;
				}
			}

			if (iscancelled.IsCancellationRequested)
			{
				m_lPsms.Clear();
				m_nNumberPsms = 0;
			}
		}

		public void Dispose()
		{
			if (m_pCurrentRawFile != null)
				m_pCurrentRawFile.Close();
			if (m_pSpectralPredictions != null)
				m_pSpectralPredictions.Dispose();
		}
		#endregion


		#region rawfile access
		public string GetPsmPath()
		{
			return m_sPath;
		}

		public List<PeptideSpectrumMatch> GetPsms()
		{
			return m_lPsms != null
				? m_lPsms.Values.SelectMany(x => x).ToList()
				: null;
		}

		public string[] GetUniqueRawfiles()
		{
			return m_lPsms.Keys.ToArray();
		}

		public PeptideSpectrumMatch[] GetPsmsForRawfile(string rawfile)
		{
			return m_lPsms[rawfile].ToArray();
		}

		public HeckLibRawFiles.HeckLibRawFile GetRawFile(string rawfile)
		{
			if (!OpenRawfileStream(new PeptideSpectrumMatch { RawFile = rawfile }))
				return null;
			return m_pCurrentRawFile;
		}

		public Centroid[] LoadSpectrum(PeptideSpectrumMatch psm, FragmentLabSettings settings, out int[] topxranks, out PeptideFragment.FragmentModel model, out INoiseDistribution noise, out PrecursorInfo precursor, out ScanHeader scanheader)
		{
			Centroid[] spectrum;
			Cursor.Current = Cursors.WaitCursor;
			{
				topxranks = null;
				model = null;
				noise = null;
				precursor = null;
				scanheader = null;

				// make sure the stream is to the proper file
				if (!OpenRawfileStream(psm))
					return null;
				if (m_pCurrentRawFile == null)
					return null;

				// verify whether the spectrum is there
				int sn = m_pCurrentRawFile.GetScanNumber(psm.MinScan, 0);
				if (sn < m_pCurrentRawFile.FirstSpectrumNumber() || sn > m_pCurrentRawFile.LastSpectrumNumber())
				{
					MessageBox.Show("Unknown scannumber: " + psm.MinScan, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return null;
				}

				// load the data
				double minTIC, maxTIC, meanTIC;
				m_pCurrentRawFile.GetMs2TicStatistics(out minTIC, out maxTIC, out meanTIC);

				// get the spectrum
				precursor = m_pCurrentRawFile.GetPrecursorInfo(m_pCurrentRawFile.GetScanNumber(psm.MinScan, psm.ImsIndex));

				scanheader = m_pCurrentRawFile.GetScanHeader(psm.MinScan);
				Centroid[] centroids = m_pCurrentRawFile.GetSpectrum(m_pCurrentRawFile.GetScanNumber(psm.MinScan, psm.ImsIndex), out noise);

				// detect isotopes
				IsotopePattern[] isotopes = IsotopePatternDetection.Process(centroids, new IsotopePatternDetection.Settings { MaxCharge = (short)(psm.Charge + 1) });
				centroids = SpectrumUtils.Deisotope(centroids, isotopes, psm.Charge, true);

				// filter S/N
				centroids = SpectrumUtils.FilterForSignalToNoise(centroids, settings.SignalToNoise);
				centroids = SpectrumUtils.FilterForBasepeak(centroids, settings.PercentOfBasepeak / 100);

				// load the fragmentation model
				Spectrum.FragmentationType fragtype = Spectrum.FragmentationType.CID;
				if (settings.Fragmentation != Spectrum.FragmentationType.None)
					fragtype = settings.Fragmentation;
				else if (precursor.Fragmentation != Spectrum.FragmentationType.None)
					fragtype = precursor.Fragmentation;
				model = new PeptideFragment.FragmentModel(PeptideFragment.GetFragmentModel(fragtype));
				model.tolerance = settings.MatchTolerance;
				if (settings.ExcludeNeutralLosses == true)
					model.TurnOff(PeptideFragment.MASSSHIFT_WATERLOSS | PeptideFragment.MASSSHIFT_AMMONIALOSS | PeptideFragment.MASSSHIFT_NEUTRALLOSS);

				spectrum = SpectrumUtils.TopX(centroids, model.topx, model.topx_massrange, out topxranks);
			}
			Cursor.Current = Cursors.Default;

			return spectrum;
		}

		public PrecursorInfo LoadPrecursorInfo(PeptideSpectrumMatch psm)
		{
			PrecursorInfo precursor;

			Cursor.Current = Cursors.WaitCursor;
			{
				// make sure the stream is to the proper file
				OpenRawfileStream(psm);

				// load the data
				precursor = m_pCurrentRawFile.GetPrecursorInfo(m_pCurrentRawFile.GetScanNumber(psm.MinScan, psm.ImsIndex));
			}
			Cursor.Current = Cursors.Default;

			return precursor;
		}

		public void SavePsms(BackgroundWorkerData data, IProgress<int> progress, CancellationToken iscancelled)
		{
			using (CsvFileWriter<GenericFileFormat> writer = new CsvFileWriter<GenericFileFormat>(data.Filename))
			{
				GenericFileFormat f = new GenericFileFormat();

				int psm_count = 0;
				List<PeptideSpectrumMatch> psms = data.PsmList;
				foreach (PeptideSpectrumMatch psm in psms) 
				{
					progress.Report((int)(100.0 * psm_count++ / psms.Count));

					f.SpectrumFile			= psm.RawFile;
					f.ScanNumber			= psm.MinScan;
					f.ScanNumberString		= psm.MinScan.ToString();
					f.RetentionTime			= psm.RetentionTime;
					f.Mz					= psm.Mz;
					f.Charge				= psm.Charge;
					f.Modifications			= psm.Peptide.GetAnnotatedModifications();
					f.Sequence				= psm.Peptide.Sequence;
					f.FragmentationType		= psm.Fragmentation.ToString();
					f.Score					= psm.Score;

					writer.Write(f);
				}
			}
		}
		#endregion


		#region export routines
		public void ExportMassLists(BackgroundWorkerData data, IProgress<int> progress, CancellationToken iscancelled)
		{
			string path = data.Filename;
			FragmentLabSettings settings = data.Settings;

			int numberpsms = 0;
			foreach (PeptideSpectrumMatch psm in data.PsmList)
			{
				if (iscancelled.IsCancellationRequested)
					break;

				string myfilename = Path.Combine(path, Path.GetFileName(FilesA.GetFileNameWithoutExtension(psm.RawFile) + "_" + psm.MinScan + ".txt"));

				int[] topxranks;
				PeptideFragment.FragmentModel model;
				INoiseDistribution noise;
				PrecursorInfo precursor;
				ScanHeader scanheader;
				Centroid[] spectrum = LoadSpectrum(psm, settings, out topxranks, out model, out noise, out precursor, out scanheader);
				if (spectrum == null)
					continue;

				Peptide peptide = settings.Peptide != null ? settings.Peptide : psm.Peptide;
				PeptideFragment[] fragments = SpectrumUtils.MatchFragments(peptide, settings.Charge == 0 ? psm.Charge : settings.Charge, spectrum, model);

				// dump the results
				CsvWriter writer = new CsvWriter(myfilename);
				writer.AddColumn("m/z", typeof(double));
				writer.AddColumn("min m/z", typeof(float));
				writer.AddColumn("max m/z", typeof(float));
				writer.AddColumn("intensity", typeof(float));
				writer.AddColumn("signal to noise", typeof(float));
				writer.AddColumn("charge", typeof(short));
				writer.AddColumn("ion type", typeof(string));
				writer.AddColumn("ion loss", typeof(string));
				writer.AddColumn("position", typeof(int));
				writer.AddColumn("series-nr", typeof(int));
				writer.AddColumn("theoretical charge", typeof(int));
				writer.AddColumn("mz accuracy " + settings.MatchTolerance.Unit, typeof(double));
				writer.AddColumn("description", typeof(string));
				for (int i = 0; i < spectrum.Length; ++i)
				{
					if (iscancelled.IsCancellationRequested)
						break;

					CsvRow row = writer.CreateRow();

					row["m/z"] = spectrum[i].Mz;
					row["charge"] = spectrum[i].Charge;
					row["min m/z"] = spectrum[i].MinMz;
					row["max m/z"] = spectrum[i].MaxMz;
					row["intensity"] = spectrum[i].Intensity;
					row["signal to noise"] = spectrum[i].SignalToNoise;

					if (fragments[i] != null)
					{
						row["ion type"] = PeptideFragment.IonToString(fragments[i].FragmentType);
						row["ion loss"] = PeptideFragment.MassShiftToString(fragments[i].MassShift);
						row["position"] = fragments[i].Position;
						row["series-nr"] = fragments[i].SeriesNr;
						row["theoretical charge"] = (int)fragments[i].Charge;
						row["mz accuracy " + settings.MatchTolerance.Unit] = settings.MatchTolerance.Accuracy(fragments[i].Mz, fragments[i].Mz - spectrum[i].Mz);
						row["description"] = fragments[i].Description;
					}

					writer.WriteRow(row);
				}
				writer.Flush();
				writer.Close();

				// update progress
				numberpsms++;
				progress.Report((int)(100.0 * numberpsms / m_nNumberPsms));
			}
		}

		public void ExportPeptideProperties(BackgroundWorkerData data, IProgress<int> progress, CancellationToken iscancelled)
		{
			const string COLUMN_RAWFILE						= "Raw file";
			const string COLUMN_SEQUENCE					= "Sequence";
			const string COLUMN_SCANNUMBER					= "Scan number";
			const string COLUMN_RETENTIONTIME				= "Retention time";
			const string COLUMN_GRAVYSCORE					= "GRAVY score";
			
			using (CsvWriter writer = new CsvWriter(data.Filename))
			{
				writer.AddColumn(COLUMN_RAWFILE,		typeof(string));
				writer.AddColumn(COLUMN_SEQUENCE,		typeof(string));
				writer.AddColumn(COLUMN_SCANNUMBER,		typeof(int));
				writer.AddColumn(COLUMN_RETENTIONTIME,	typeof(float));
				writer.AddColumn(COLUMN_GRAVYSCORE,		typeof(double));

				int numberpsms = 0;
				FrequentFlyers freqflyers = new FrequentFlyers(50, 800, new Tolerance(40, Tolerance.ErrorUnit.PPM));
				foreach (PeptideSpectrumMatch psm in data.PsmList)
				{
					if (iscancelled.IsCancellationRequested)
						break;
					CsvRow row = writer.CreateRow();

					Peptide peptide = data.Settings.Peptide != null ? data.Settings.Peptide : psm.Peptide;

					row[COLUMN_RAWFILE] = psm.RawFile;
					row[COLUMN_SEQUENCE] = peptide.GetModifiedSequence();
					row[COLUMN_SCANNUMBER] = psm.MinScan;
					row[COLUMN_RETENTIONTIME] = psm.RetentionTime;
					row[COLUMN_GRAVYSCORE] = psm.Peptide.CalculateGravyScore();

					writer.WriteRow(row);

					// update progress
					numberpsms++;
					progress.Report((int)(100.0 * numberpsms / m_nNumberPsms));
				}
			}
		}

		public void ExportFragmentReport(BackgroundWorkerData data, IProgress<int> progress, CancellationToken iscancelled)
		{
			const string COLUMN_RAWFILE						= "Raw file";
			const string COLUMN_SEQUENCE					= "Sequence";
			const string COLUMN_SEQUENCE_LENGTH				= "Sequence length";
			const string COLUMN_SCANNUMBER					= "Scan number";
			const string COLUMN_DETECTEDIONCOUNT			= "Detected ion count";
			const string COLUMN_ION_OVERALL					= "{0} overall count";
			const string COLUMN_ION_OVERALL_COVERAGE		= "{0} overall coverage";
			const string COLUMN_ION_BASEION					= "{0} base-ion count";
			const string COLUMN_ION_BASEION_COVERAGE		= "{0} base-ion coverage";
			const string COLUMN_ION_NEUTRALLLOSS			= "{0} neutralloss count";
			const string COLUMN_ION_NEUTRALLLOSS_COVERAGE	= "{0} neutralloss coverage";
			const string COLUMN_OBSERVED_FRAGMENTS			= "Observed fragments";
			const string COLUMN_SEQUENCE_COVERAGE			= "Sequence coverage";
			const string COLUMN_SEQUENCE_COVERAGE_CTERM		= "Sequence coverage C-term";
			const string COLUMN_SEQUENCE_COVERAGE_NTERM		= "Sequence coverage N-term";
			const string COLUMN_FRAGMENTATION_EFFICIENCY	= "Fragmentation efficiency";
			const string COLUMN_ASSIGNED_IONLOAD			= "Assigned ion load";
			const string COLUMN_PRECURSOR_ASSIGNED			= "Precursor assigned";
			const string COLUMN_PERCENT_SPECTRUM_EXPLAINED	= "Percentage spectrum explained";
			const string COLUMN_ENCYCLOPEDIA_SCORE			= "EncyclopeDIA score";

			// setup the CsvWriter
			string filename = data.Filename;
			FragmentLabSettings settings = data.Settings;
			using (CsvWriter writer = new CsvWriter(filename))
			{
				// grab the fragment model we're using
				PeptideFragment.FragmentModel fragmodel = PeptideFragment.GetFragmentModel(settings.Fragmentation);
				if (fragmodel == null)
					fragmodel = PeptideFragment.modelAll;

				Dictionary<int, PeptideFragment.FragmentRange> activeseries = new Dictionary<int, PeptideFragment.FragmentRange>();
				if (fragmodel.A != null) activeseries.Add(PeptideFragment.ION_A, fragmodel.A);
				if (fragmodel.B != null) activeseries.Add(PeptideFragment.ION_B, fragmodel.B);
				if (fragmodel.C != null) activeseries.Add(PeptideFragment.ION_C, fragmodel.C);
				if (fragmodel.X != null) activeseries.Add(PeptideFragment.ION_X, fragmodel.X);
				if (fragmodel.Y != null) activeseries.Add(PeptideFragment.ION_Y, fragmodel.Y);
				if (fragmodel.Z != null) activeseries.Add(PeptideFragment.ION_Z, fragmodel.Z);

				// add the columns
				writer.AddColumn(COLUMN_RAWFILE,						typeof(string));
				writer.AddColumn(COLUMN_SEQUENCE,						typeof(string));
				writer.AddColumn(COLUMN_SEQUENCE_LENGTH,				typeof(int));
				writer.AddColumn(COLUMN_SCANNUMBER,						typeof(int));
				writer.AddColumn(COLUMN_DETECTEDIONCOUNT,				typeof(int));
				foreach (int iontype in activeseries.Keys)
				{
					PeptideFragment.FragmentRange series = activeseries[iontype];

					writer.AddColumn(string.Format(COLUMN_ION_BASEION, PeptideFragment.IonToString(iontype)), typeof(int));
					writer.AddColumn(string.Format(COLUMN_ION_BASEION_COVERAGE, PeptideFragment.IonToString(iontype)), typeof(double));
					if ((series.MassShifts & PeptideFragment.MASSSHIFT_WATERLOSS) != 0 || (series.MassShifts & PeptideFragment.MASSSHIFT_AMMONIALOSS) != 0)
					{
						writer.AddColumn(string.Format(COLUMN_ION_NEUTRALLLOSS, PeptideFragment.IonToString(iontype)), typeof(int));
						writer.AddColumn(string.Format(COLUMN_ION_NEUTRALLLOSS_COVERAGE, PeptideFragment.IonToString(iontype)), typeof(double));
					}
					writer.AddColumn(string.Format(COLUMN_ION_OVERALL, PeptideFragment.IonToString(iontype)), typeof(int));
					writer.AddColumn(string.Format(COLUMN_ION_OVERALL_COVERAGE, PeptideFragment.IonToString(iontype)), typeof(double));
				}
				writer.AddColumn(COLUMN_OBSERVED_FRAGMENTS,				typeof(int));
				writer.AddColumn(COLUMN_SEQUENCE_COVERAGE,				typeof(double));
				writer.AddColumn(COLUMN_SEQUENCE_COVERAGE_CTERM,		typeof(double));
				writer.AddColumn(COLUMN_SEQUENCE_COVERAGE_NTERM,		typeof(double));
				writer.AddColumn(COLUMN_ASSIGNED_IONLOAD,				typeof(double));
				writer.AddColumn(COLUMN_FRAGMENTATION_EFFICIENCY,		typeof(double));
				writer.AddColumn(COLUMN_PRECURSOR_ASSIGNED,				typeof(bool));
				writer.AddColumn(COLUMN_PERCENT_SPECTRUM_EXPLAINED,		typeof(double));
				writer.AddColumn(COLUMN_ENCYCLOPEDIA_SCORE,				typeof(double));

				int numberpsms = 0;
				foreach (PeptideSpectrumMatch psm in data.PsmList)
				{
					if (iscancelled.IsCancellationRequested)
						break;

					// load the spectrum
					int[] topxranks;
					PeptideFragment.FragmentModel model;
					INoiseDistribution noise;
					PrecursorInfo precursor;
					ScanHeader scanheader;
					Centroid[] spectrum = LoadSpectrum(psm, settings, out topxranks, out model, out noise, out precursor, out scanheader);
					if (spectrum == null)
						continue;
					model.tolerance = settings.MatchTolerance;

					// create the fragments
					short charge = settings.Charge == 0 ? psm.Charge : settings.Charge;
					Peptide peptide = settings.Peptide != null ? settings.Peptide : psm.Peptide;
					PeptideFragment[] fragments = SpectrumUtils.MatchFragments(peptide, charge, spectrum, model);
					PeptideFragmentAnnotator annotator = new PeptideFragmentAnnotator(psm.Peptide, fragments, spectrum);

					// match against encyclopeDIA
					Centroid[] predicted_spectrum;
					PeptideFragment[] predicted_annotations;
					this.GetSpectralPrediction(peptide, charge, out predicted_spectrum, out predicted_annotations);
					double spectral_angle = double.NaN;
					if (predicted_spectrum != null)
						spectral_angle = SpectrumUtils.SpectrumAngle(predicted_spectrum, spectrum);

					// calculate the coverage for each ion-type
					CsvRow row = writer.CreateRow();

					row[COLUMN_RAWFILE]						= psm.RawFile;
					row[COLUMN_SEQUENCE]					= psm.Peptide.GetModifiedSequence();
					row[COLUMN_SEQUENCE_LENGTH]				= psm.Peptide.Length;
					row[COLUMN_SCANNUMBER]					= psm.MinScan;
					row[COLUMN_DETECTEDIONCOUNT]			= spectrum.Length;
					foreach (int iontype in activeseries.Keys)
					{
						PeptideFragment.FragmentRange series = activeseries[iontype];

						int hits_baseion = annotator.GetNumberOfHits(iontype, PeptideFragment.MASSSHIFT_NONE);
						row[string.Format(COLUMN_ION_BASEION, PeptideFragment.IonToString(iontype))] = hits_baseion;
						row[string.Format(COLUMN_ION_BASEION_COVERAGE, PeptideFragment.IonToString(iontype))] = hits_baseion / (double)series.GetTheoreticalBreaks(psm.Peptide.Sequence);
						if ((series.MassShifts & PeptideFragment.MASSSHIFT_WATERLOSS) != 0 || (series.MassShifts & PeptideFragment.MASSSHIFT_AMMONIALOSS) != 0)
						{
							int hitsneutral = annotator.GetNumberOfHits(iontype, PeptideFragment.MASSSHIFT_WATERLOSS | PeptideFragment.MASSSHIFT_AMMONIALOSS);
							row[string.Format(COLUMN_ION_NEUTRALLLOSS, PeptideFragment.IonToString(iontype))] = hitsneutral;
							row[string.Format(COLUMN_ION_NEUTRALLLOSS_COVERAGE, PeptideFragment.IonToString(iontype))] = hitsneutral / (double)series.GetTheoreticalBreaks(psm.Peptide.Sequence);
						}
						int hits_overall = annotator.GetNumberOfHits(iontype, -1);
						row[string.Format(COLUMN_ION_OVERALL, PeptideFragment.IonToString(iontype))] = hits_overall;
						row[string.Format(COLUMN_ION_OVERALL_COVERAGE, PeptideFragment.IonToString(iontype))] = hits_overall / (double)series.GetTheoreticalBreaks(psm.Peptide.Sequence);
					}
					row[COLUMN_OBSERVED_FRAGMENTS]				= annotator.ObservedFragments;
					row[COLUMN_SEQUENCE_COVERAGE]				= annotator.SequenceCoverage;
					row[COLUMN_SEQUENCE_COVERAGE_CTERM]			= annotator.SequenceCoverageCterm;
					row[COLUMN_SEQUENCE_COVERAGE_NTERM]			= annotator.SequenceCoverageNterm;
					row[COLUMN_ASSIGNED_IONLOAD]				= annotator.AssignedIonLoad;
					row[COLUMN_FRAGMENTATION_EFFICIENCY]		= annotator.FragmentationEfficiency;
					row[COLUMN_PRECURSOR_ASSIGNED]				= annotator.PrecursorAssigned;
					row[COLUMN_PERCENT_SPECTRUM_EXPLAINED]		= annotator.AssignedIonLoad;
					row[COLUMN_ENCYCLOPEDIA_SCORE]				= spectral_angle;

					writer.WriteRow(row);

					// update progress
					numberpsms++;
					progress.Report((int)(100.0 * numberpsms / m_nNumberPsms));
				}
			}
		}

		public void ExportProsit(BackgroundWorkerData data, IProgress<int> progress, CancellationToken iscancelled)
		{
			const string COLUMN_SEQUENCE				= "modified_sequence";	// length [7-30]
			const string COLUMN_COLISION_ENERGY			= "collision_energy";	// energy [10-50] (according to the paper, NCE)
			const string COLUMN_PRECURSOR_CHARGE		= "precursor_charge";	// charge-state [1-6]

			
			// open the writer
			CsvWriter writer;
			try
			{
				writer = new CsvWriter(data.Filename, ',');
				writer.AddColumn(COLUMN_SEQUENCE,					typeof(string));
				writer.AddColumn(COLUMN_COLISION_ENERGY,			typeof(int));
				writer.AddColumn(COLUMN_PRECURSOR_CHARGE,			typeof(short));
			}
			catch (Exception)
			{
				MessageBox.Show("Couldn't open file '" + data.Filename + "'");
				return;
			}
			FragmentLabSettings settings = data.Settings;

			// process the data
			int numberpsms = 0;
			foreach (PeptideSpectrumMatch psm in data.PsmList)
			{
				if (iscancelled.IsCancellationRequested)
					break;

				// sanity checks, prosit doesn't deal with everyting
				if (psm.Charge < 1 || psm.Charge > 6)
					continue;
				if (psm.Peptide.Length < 7 || psm.Peptide.Length > 30)
					continue;

				// convert the sequence - we check here whether the modifications are supported
				StringBuilder str_sequence = new StringBuilder();
				if (psm.Peptide.Nterm != null || psm.Peptide.Cterm != null)
					continue;

				bool sequence_ok = true;
				for (int pos = 0; pos < psm.Peptide.Length; ++pos)
				{
					if (psm.Peptide.Modifications[pos] == null)
					{
						str_sequence.Append(psm.Peptide.Sequence[pos]);
					}
					else if (psm.Peptide.Modifications[pos].Title == "Carbamidomethyl (C)")
					{
						str_sequence.Append(psm.Peptide.Sequence[pos]);
					}
					else if (psm.Peptide.Modifications[pos].Title == "Oxidation (M)")
					{
						str_sequence.Append(psm.Peptide.Sequence[pos]);
						str_sequence.Append("(ox)");
					}
					else
					{
						sequence_ok = false;
						break;
					}
				}

				if (!sequence_ok)
					continue;

				// get the precursor info
				PrecursorInfo precursor = LoadPrecursorInfo(psm);

				// make the entry
				CsvRow row = writer.CreateRow();
				row[COLUMN_SEQUENCE]						= str_sequence.ToString();
				row[COLUMN_PRECURSOR_CHARGE]				= psm.Charge;
				row[COLUMN_COLISION_ENERGY]					= (int)precursor.FragmentationEnergy;
				writer.WriteRow(row);

				// update progress
				numberpsms++;
				progress.Report((int)(100.0 * numberpsms / m_nNumberPsms));
			}
			writer.Close();
		}

		public void ExportEncyclopeDIA(BackgroundWorkerData data, IProgress<int> progress, CancellationToken iscancelled)
		{
			FragmentLabSettings settings = data.Settings;

			// create the database
			using (EncyclopediaDatabase db = new EncyclopediaDatabase(data.Filename, new DatabaseInformation { FileName = data.Filename, Created = DateTime.Now, LastAccessed = DateTime.Now, HashCode = "" })) { ; }

			using (EncyclopediaDatabase database = new EncyclopediaDatabase(data.Filename))
			{
				// collect all the unique peptide sequences, so we can select the best scoring one for the database
				Dictionary<string, List<PeptideSpectrumMatch>> grouped_psms = new Dictionary<string, List<PeptideSpectrumMatch>>();
				foreach (PeptideSpectrumMatch psm in data.PsmList)
				{
					if (iscancelled.IsCancellationRequested)
						break;

					string modified_sequence = psm.Peptide.GetModifiedSequence();
					if (!grouped_psms.ContainsKey(modified_sequence))
						grouped_psms.Add(modified_sequence, new List<PeptideSpectrumMatch>());
					grouped_psms[modified_sequence].Add(psm);
				}

				// process the spectra
				int numberpsms = 0;
				foreach (string modified_sequence in grouped_psms.Keys)
				{
					if (iscancelled.IsCancellationRequested)
						break;

					// get the information for the current modified sequence
					int psm_i = 0; int best_psm_i = 0; double best_psm_score = -1;
					PeptideSpectrumMatch best_psm = null; Centroid[] best_spectrum = null; PeptideFragment[] best_fragments = null;
					do
					{
						PeptideSpectrumMatch current_psm = grouped_psms[modified_sequence][psm_i];

						short charge = current_psm.Charge;
						double mass = MassSpectrometry.ToMass(current_psm.Mz, current_psm.Charge);

						int[] topxranks; PeptideFragment.FragmentModel model; INoiseDistribution noise; PrecursorInfo precursor; ScanHeader scanheader;
						Centroid[] spectrum = LoadSpectrum(current_psm, settings, out topxranks, out model, out noise, out precursor, out scanheader);
						if (spectrum == null)
							continue;

						Peptide peptide = current_psm.Peptide;
						PeptideFragment[] fragments = SpectrumUtils.MatchFragments(peptide, current_psm.Charge, spectrum, model);

						SpectrumUtils.PsmScore psmscore = SpectrumUtils.CalcPsmScore(peptide, charge, spectrum, topxranks, model);
						if (psmscore.Score > best_psm_score)
						{
							best_psm_i			= psm_i;
							best_psm_score		= psmscore.Score;
							best_spectrum		= spectrum;
							best_fragments		= fragments;
							best_psm			= current_psm;
						}
					} while (++psm_i < grouped_psms[modified_sequence].Count);

					// store the peaks that are actually annotated in the database and move on
					int nr_peaks = 0;
					foreach (PeptideFragment fragment in best_fragments)
						if (fragment != null) nr_peaks++;

					Centroid[] final_spectrum = new Centroid[nr_peaks];
					PeptideFragment[] final_annotations = new PeptideFragment[nr_peaks];

					int current_i = 0;
					for (int i = 0; i < best_spectrum.Length; ++i)
					{
						if (best_fragments[i] == null)
							continue;
						Centroid.Copy(ref best_spectrum[i], ref final_spectrum[current_i]);
						final_annotations[current_i] = best_fragments[i];
						current_i++;
					}

					database.AddSpectrumForPeptide(best_psm, final_spectrum, final_annotations);

					// update progress
					numberpsms++;
					progress.Report((int)(100.0 * numberpsms / grouped_psms.Count));
				}
			}
		}

		public void ExportGlycoScores(BackgroundWorkerData data, IProgress<int> progress, CancellationToken iscancelled)
		{
			const string COLUMN_RAWFILE					= "Raw file";
			const string COLUMN_SCANNUMBER				= "Scan number";
			const string COLUMN_PRECURSOR_MZ			= "Precursor m/z";
			const string COLUMN_PRECURSOR_CHARGE		= "Precursor charge";
			const string COLUMN_PRECURSOR_GLYCAN_SCORE	= "Glycan score";
			const string COLUMN_PRECURSOR_GLCNAC_GALNAC	= "GlcNac/GalNac ratio";
			const string COLUMN_PRECURSOR_MSCORE		= "M-score";
			const string COLUMN_PRECURSOR_PHOSPHOPEAK	= "Hex-Phospho peak [243.03Th]";

			
			// open the writer
			CsvWriter writer;
			try
			{
				writer = new CsvWriter(data.Filename);
				writer.AddColumn(COLUMN_RAWFILE,					typeof(string));
				writer.AddColumn(COLUMN_SCANNUMBER,					typeof(int));
				writer.AddColumn(COLUMN_PRECURSOR_MZ,				typeof(double));
				writer.AddColumn(COLUMN_PRECURSOR_CHARGE,			typeof(short));
				writer.AddColumn(COLUMN_PRECURSOR_GLYCAN_SCORE,		typeof(double));
				writer.AddColumn(COLUMN_PRECURSOR_GLCNAC_GALNAC,	typeof(double));
				writer.AddColumn(COLUMN_PRECURSOR_MSCORE,			typeof(double));
				writer.AddColumn(COLUMN_PRECURSOR_PHOSPHOPEAK,		typeof(string));
			}
			catch (Exception)
			{
				MessageBox.Show("Couldn't open file '" + data.Filename + "'");
				return;
			}
			FragmentLabSettings settings = data.Settings;

			// process the data
			int numberpsms = 0;
			foreach (PeptideSpectrumMatch psm in data.PsmList)
			{
				if (iscancelled.IsCancellationRequested)
					break;

				// load the spectrum
				int[] topxranks;
				PeptideFragment.FragmentModel model;
				INoiseDistribution noise;
				PrecursorInfo precursor;
				ScanHeader scanheader;
				Centroid[] spectrum = LoadSpectrum(psm, settings, out topxranks, out model, out noise, out precursor, out scanheader);
				if (spectrum == null)
					continue;
				model.tolerance = settings.MatchTolerance;

				// perform calculations
				Glycan.SimpleScore simplescore = Glycan.CalculateSimpleScore(spectrum, topxranks, settings.MatchTolerance, model.topx, model.topx_massrange);

				int[] saccharides;
				double mscore = Glycan.CalculateDoublePlayMScore(spectrum, topxranks, settings.MatchTolerance, model.topx, model.topx_massrange, out saccharides);

				bool hexphos = Glycan.HexPhosOxoniumIonPresent(spectrum, topxranks, settings.MatchTolerance, model.topx, model.topx_massrange);

				// dump the results
				CsvRow row = writer.CreateRow();
				row[COLUMN_RAWFILE]						= psm.RawFile;
				row[COLUMN_SCANNUMBER]					= psm.MinScan;
				row[COLUMN_PRECURSOR_MZ]				= psm.Mz;
				row[COLUMN_PRECURSOR_CHARGE]			= psm.Charge;
				row[COLUMN_PRECURSOR_GLYCAN_SCORE]		= simplescore == null ? double.NaN : simplescore.GlycanScore;
				row[COLUMN_PRECURSOR_GLCNAC_GALNAC]		= simplescore == null ? double.NaN : simplescore.GlcNAcGalNAcRatio;
				row[COLUMN_PRECURSOR_MSCORE]			= mscore;
				row[COLUMN_PRECURSOR_PHOSPHOPEAK]		= hexphos ? "+" : "";
				writer.WriteRow(row);

				// update progress
				numberpsms++;
				progress.Report((int)(100.0 * numberpsms / m_nNumberPsms));
			}
			writer.Close();
		}
		
		public void ConvertMgfToMeta(BackgroundWorkerData data, IProgress<int> progress, CancellationToken iscancelled)
		{
			// get the filenames
			string[] files = data.Filename.Split('\t');

			// parse the mgf file
			CsvWriter writer = null;
			HeckLibRawFileMgf.MgfRawFile.Parse(files[0], delegate (Centroid[] spectrum, PrecursorInfo precursor, string title, Spectrum.MassAnalyzer massanalyzer) {
					// if it's the first entry create the writer with the appropriate columns
					if (writer == null)
					{
						writer = new CsvWriter(files[1]);
						writer.AddColumn("Scan number",			typeof(string));
						writer.AddColumn("Retention time",		typeof(string));
						writer.AddColumn("m/z",					typeof(string));
						writer.AddColumn("Charge",				typeof(string));
						writer.AddColumn("Monoisotopic mass",	typeof(string));
						
						Dictionary<string, string> _title_dict = ConvertMgfToMeta_title_to_dict(title);
						foreach (string t in _title_dict.Keys)
							writer.AddColumn(t, typeof(string));
					}

					// dump the data
					CsvRow row = writer.CreateRow();
					row["Scan number"]			= precursor.ScanNumber.ToString();
					row["Retention time"]		= precursor.RetentionTime.ToString();
					row["m/z"]					= precursor.Mz.ToString();
					row["Charge"]				= precursor.Charge.ToString();
					row["Monoisotopic mass"]	= MassSpectrometry.ToMass(precursor.Mz, precursor.Charge).ToString();

					Dictionary<string, string> title_dict = ConvertMgfToMeta_title_to_dict(title);
					foreach (string t in title_dict.Keys)
						row[t] = title_dict[t];

					writer.WriteRow(row);
				});

			if (writer != null)
				writer.Close();
		}

		private Dictionary<string, string> ConvertMgfToMeta_title_to_dict(string title)
		{
			string[] tokens1 = title.Split(' ');
			Dictionary<string, string> result = new Dictionary<string, string>();
			foreach (string token1 in tokens1)
			{
				string[] tokens2 = token1.Split(':');
				if (tokens2.Length < 2)
					continue;
				result.Add(tokens2[0], tokens2[1]);
			}
			return result;
		}
		#endregion


		#region calculation routines
		public void CalculateFrequentFlyers(BackgroundWorkerData data, IProgress<int> progress, CancellationToken iscancelled)
		{
			int numberpsms = 0;
			FrequentFlyers freqflyers = new FrequentFlyers(50, 800, new Tolerance(40, Tolerance.ErrorUnit.PPM));
			foreach (PeptideSpectrumMatch psm in data.PsmList)
			{
				if (iscancelled.IsCancellationRequested)
					break;

				int[] topxranks;
				PeptideFragment.FragmentModel model;
				INoiseDistribution noise;
				PrecursorInfo precursor;
				ScanHeader scanheader;
				Centroid[] spectrum = LoadSpectrum(psm, data.Settings, out topxranks, out model, out noise, out precursor, out scanheader);
				if (spectrum == null)
					continue;

				Peptide peptide = data.Settings.Peptide != null ? data.Settings.Peptide : psm.Peptide;
				freqflyers.ProcessSpectrum(peptide, spectrum);

				// update progress
				numberpsms++;
				progress.Report((int)(100.0 * numberpsms / m_nNumberPsms));
			}

			data.FreqFyerResult = iscancelled.IsCancellationRequested ? null : freqflyers;
		}
		#endregion


		#region spectrum prediction
		public void LoadSpectralPredictions(BackgroundWorkerData data, IProgress<int> progress, CancellationToken iscancelled)
		{
			progress.Report(20);
			m_pSpectralPredictions = new EncyclopediaDatabase(data.Filename);
			progress.Report(100);
		}

		public void ClearSpectralPredictions()
		{
			m_pSpectralPredictions = null;
		}

		public void GetSpectralPrediction(Peptide peptide, short charge, out Centroid[] spectrum, out PeptideFragment[] annotations)
		{
			spectrum = null;
			annotations = null;
			if (m_pSpectralPredictions == null)
				return;
			spectrum = m_pSpectralPredictions.GetSpectrumForPeptide(peptide, charge, out annotations);
		}
		#endregion


		#region the mess that is export to mgf...
		public class ExportAsMgfThread : WorkerThread, IDisposable
		{
			public ExportAsMgfThread(string outfile, string metafile, string path, List<int>[] grouped_psms, PeptideSpectrumMatch[] allpsms, FragmentLabSettings settings) : base("Export as MGF")
			{
				m_sPath = path;
				m_aAllPsms = allpsms;
				m_aGroupedPsms = grouped_psms;
				m_pSettings = settings;

				m_fMgfWriter = new StreamWriter(new FileStream(outfile, FileMode.Create, FileAccess.Write, FileShare.Read, 65536, true));

				m_fMgfMetaWriter = new CsvWriter(metafile);
				m_fMgfMetaWriter.AddColumn("Raw file",				typeof(string));
				m_fMgfMetaWriter.AddColumn("Scan number",			typeof(string));
				m_fMgfMetaWriter.AddColumn("Retention time",		typeof(string));
				m_fMgfMetaWriter.AddColumn("m/z",					typeof(string));
				m_fMgfMetaWriter.AddColumn("Charge",				typeof(string));
				m_fMgfMetaWriter.AddColumn("Monoisotopic mass",		typeof(string));
				m_fMgfMetaWriter.AddColumn("SPECTRA",				typeof(string));
				m_fMgfMetaWriter.AddColumn("MOBILITY",				typeof(string));
				m_fMgfMetaWriter.AddColumn("MOBILITYLENGTH",		typeof(string));
				m_fMgfMetaWriter.AddColumn("CCS",					typeof(string));
				m_fMgfMetaWriter.AddColumn("INTENSITY",				typeof(string));
				m_fMgfMetaWriter.AddColumn("D",						typeof(string));
				m_fMgfMetaWriter.AddColumn("GDFR",					typeof(string));
				m_fMgfMetaWriter.AddColumn("Xrea",					typeof(string));
			}

			public override void Dispose()
			{
				m_fMgfWriter.Close();
				m_fMgfMetaWriter.Close();
			}

			public override void Execute()
			{
				m_nCurrentGroupedPsm = 0;
				double minTIC = 0, maxTIC = 0, meanTIC = 0;
				foreach (List<int> psmIds in m_aGroupedPsms)
				{
					m_nCurrentGroupedPsm++;

					// process
					List<double> ccss = new List<double>();
					List<double> mobilities = new List<double>();
					List<double> mobility_lengths = new List<double>();
					List<double[]> all_mzs = new List<double[]>();
					List<float[]> all_intensities = new List<float[]>();
					INoiseDistribution best_noisedistribution = null; PrecursorInfo best_precursor = null; ScanHeader best_scanheader = null;

					short max_charge = 0;
					float max_totalintensity = 0;
					foreach (int psmId in psmIds)
					{
						PeptideSpectrumMatch psm = m_aAllPsms[psmId];

						// open the file (?)
						string rawfilename = FilesA.GetFileNameWithoutExtension(psm.RawFile);
						if (rawfilename != m_sCurrentRawFile)
						{
							if (m_pCurrentRawFile != null)
								m_pCurrentRawFile.Close();

							m_sCurrentRawFile = rawfilename;
							string path;
							m_pCurrentRawFile = OpenFile(rawfilename, out path);

							m_pCurrentRawFile.GetMs2TicStatistics(out minTIC, out maxTIC, out meanTIC);
						}

						// extract the meta info
						PrecursorInfo precursor = m_pCurrentRawFile.GetPrecursorInfo(psm.MinScan);
						ScanHeader scanheader = m_pCurrentRawFile.GetScanHeader(psm.MinScan);
						if (precursor == null || precursor.Charge == 1 || precursor.Mz < 100)
							continue;

						// store the mobility data
						ccss.Add(precursor.CollisionCrossSection);
						mobilities.Add(precursor.Mobility);
						mobility_lengths.Add(precursor.IonMobilityLength);

						// load the data
						INoiseDistribution noise;
						double[] _mzs; float[] _intensities;
						m_pCurrentRawFile.GetSpectrum(psm.MinScan, out _mzs, out _intensities, out noise);
						all_mzs.Add(_mzs);
						all_intensities.Add(_intensities);

						if (_mzs.Length == 0)
							continue;

						max_charge = Math.Max(psm.Charge, max_charge);

						float totalintensity = Statistics.Max(_intensities);
						if (totalintensity > max_totalintensity)
						{
							max_totalintensity = totalintensity;

							best_scanheader = scanheader;
							best_precursor = new PrecursorInfo(precursor);
							best_precursor.Charge = max_charge;
						}

						// retain the highest noise level
						if (best_noisedistribution == null || best_noisedistribution.GetNoise(500) < noise.GetNoise(500))
							best_noisedistribution = noise;
					}

					// create averaged mobility values
					double ccs = Statistics.Median(ccss.ToArray());
					double mobility = Statistics.Median(mobilities.ToArray());
					double mobility_length = 0;
					if (mobility_lengths.Count > 0)
						mobility_length = Statistics.Max(mobility_lengths.ToArray());

					// check whether there we usable spectra
					if (best_precursor == null)
						continue;

					// combine the spectra
					double[] mzs;
					float[] intensities;
					SpectrumUtils.CombineProfileSpectra(all_mzs, all_intensities, m_pSettings.MatchTolerance, out mzs, out intensities);

					// process the merged spectrum
					Centroid[] centroids = CentroidDetection.Process(mzs, intensities, best_noisedistribution, new CentroidDetection.Settings());

					// filter S/N
					centroids = SpectrumUtils.FilterForSignalToNoise(centroids, m_pSettings.SignalToNoise);
					centroids = SpectrumUtils.FilterForBasepeak(centroids, m_pSettings.PercentOfBasepeak / 100);

					// detect isotopes
					IsotopePattern[] isotopes = IsotopePatternDetection.Process(centroids, new IsotopePatternDetection.Settings { MaxCharge = best_precursor.Charge });
					centroids = SpectrumUtils.Deisotope(centroids, isotopes, best_precursor.Charge, true);

					// load the fragmentation model
					PeptideFragment.FragmentModel model = new PeptideFragment.FragmentModel(PeptideFragment.GetFragmentModel(m_pSettings.Fragmentation == Spectrum.FragmentationType.None ? best_precursor.Fragmentation : m_pSettings.Fragmentation));
					model.tolerance = m_pSettings.MatchTolerance;
					if (m_pSettings.ExcludeNeutralLosses == true)
						model.TurnOff(PeptideFragment.MASSSHIFT_WATERLOSS | PeptideFragment.MASSSHIFT_AMMONIALOSS | PeptideFragment.MASSSHIFT_NEUTRALLOSS);

					int[] topxranks;
					Centroid[] centroids_filtered = SpectrumUtils.TopX(centroids, model.topx, model.topx_massrange, out topxranks);

					// quality control
					if (centroids_filtered.Length < 10)
						continue;

					// calculate discriminant scores
					double msmsEval = SpectrumUtils.MsmsEval(best_precursor, centroids, meanTIC);
					double goodDiffFraction = SpectrumUtils.GoodDiffFraction(centroids);
					double xrea = SpectrumUtils.Xrea(SpectrumUtils.CumulativeIntensityNormalization(centroids));

					// write the spectrum
					string title = string.Format("SPECTRA:{0} MOBILITY:{1} MOBILITYLENGTH:{2} CCS:{3} INTENSITY:{4} D:{5} GDFR:{6} Xrea:{7}", all_mzs.Count, mobility, mobility_length, ccs, best_precursor.Intensity, msmsEval, goodDiffFraction, xrea);

					PrecursorInfo pinfo = new PrecursorInfo(best_precursor);
					HeckLibRawFileMgf.MgfRawFile.WriteSpectrum(m_fMgfWriter, title, pinfo, centroids_filtered, best_scanheader.Polarity, Spectrum.MassAnalyzer.TimeOfFlight);

					// write the meta info
					CsvRow row = m_fMgfMetaWriter.CreateRow();
					row["Raw file"]					= (string)pinfo.RawFile;
					row["Scan number"]				= (string)pinfo.ScanNumber.ToString();
					row["Retention time"]			= (string)pinfo.RetentionTime.ToString();
					row["m/z"]						= (string)pinfo.Mz.ToString();
					row["Charge"]					= (string)pinfo.Charge.ToString();
					row["Monoisotopic mass"]		= (string)MassSpectrometry.ToMass(pinfo.Mz, pinfo.Charge).ToString();
					row["SPECTRA"]					= (string)all_mzs.Count.ToString();
					row["MOBILITY"]					= (string)mobility.ToString();
					row["MOBILITYLENGTH"]			= (string)mobility_length.ToString();
					row["CCS"]						= (string)ccs.ToString();
					row["INTENSITY"]				= (string)best_precursor.Intensity.ToString();
					row["D"]						= (string)msmsEval.ToString();
					row["GDFR"]						= (string)goodDiffFraction.ToString();
					row["Xrea"]						= (string)xrea.ToString();
					m_fMgfMetaWriter.WriteRow(row);
				}
			}

			public override double Progress()
			{
				return m_nCurrentGroupedPsm / (double)m_aGroupedPsms.Length;
			}

			private StreamWriter m_fMgfWriter;
			private CsvWriter m_fMgfMetaWriter;

			private string m_sPath;
			private string m_sCurrentRawFile;
			private HeckLibRawFiles.HeckLibRawFile m_pCurrentRawFile;

			private FragmentLabSettings m_pSettings;

			private int m_nCurrentGroupedPsm;
			private List<int>[] m_aGroupedPsms;
			private PeptideSpectrumMatch[] m_aAllPsms;
		}

		public void ExportAsMgfFile(BackgroundWorkerData data, IProgress<int> progress, CancellationToken iscancelled)
		{
			FragmentLabSettings settings = data.Settings;
			PeptideSpectrumMatch[] psms = data.PsmList.ToArray();

			// check which scans are to be combined
			// TODO should we do this de-charged?
			List<List<int>> groupedpsms = new List<List<int>>();
			if (settings.CombineSpectra)
			{
				// construct the sorted mz array
				int[] psms_indices = new int[psms.Length];
				double[] psms_mzs = new double[psms.Length];
				for (int i = 0; i < psms.Length; ++i)
				{
					psms_indices[i] = i;
					psms_mzs[i] = psms[i].Mz;
				}
				int[] order = NumericalRecipes.OrderedIndices(psms_mzs, false);
				psms_mzs = ArraysA.SubSelect(psms_mzs, order);
				psms_indices = ArraysA.SubSelect(psms_indices, order);

				// start combining the same precursors
				Tolerance tolerance_mz = new Tolerance(25, Tolerance.ErrorUnit.PPM);
				float tolerance_rt = 1.5F;
				double tolerance_mobility_percent = 5;

				HashSet<int> processed = new HashSet<int>();
				for (int i = 0; i < psms.Length; ++i)
				{
					if (iscancelled.IsCancellationRequested)
						break;
					if (processed.Contains(i))
						continue;
					processed.Add(i);

					PeptideSpectrumMatch psm_i = psms[psms_indices[i]];
					if (psm_i.Mz < 100 || psm_i.Charge == 1)
						continue;

					List<int> connected_psms = new List<int>();
					connected_psms.Add(psms_indices[i]);
					List<double> connected_psms_mzs = new List<double>();
					connected_psms_mzs.Add(psm_i.Mz);

					for (int j = i + 1; j < psms.Length; ++j)
					{
						if (processed.Contains(j))
							continue;
						PeptideSpectrumMatch psm_j = psms[psms_indices[j]];

						// check whether adding this will still make the m/z's fit
						double[] mzs = new double[connected_psms_mzs.Count + 1];
						for (int k = 0; k < connected_psms_mzs.Count; ++k)
							mzs[k] = connected_psms_mzs[k];
						mzs[mzs.Length - 1] = psm_j.Mz;

						double median_mz = Statistics.Median(mzs.ToArray());
						double delta = tolerance_mz.Delta(median_mz);
						if (psm_i.Mz < median_mz - delta || psm_j.Mz > median_mz + delta)
							break;

						// check whether this fits in terms of rawfile, charge and retention time
						if (psm_i.Charge != psm_j.Charge)
							continue;
						if (psm_i.RawFile != psm_j.RawFile)
							continue;
						if (Math.Abs(psm_i.RetentionTime - psm_j.RetentionTime) > tolerance_rt)
							continue;
						if (Math.Abs(psm_i.Mobility - psm_j.Mobility) > Math.Max(psm_i.Mobility, psm_j.Mobility) * (tolerance_mobility_percent/100))
							continue;

						// we make the cut
						processed.Add(j);
						connected_psms.Add(psms_indices[j]);
						connected_psms_mzs.Add(psm_j.Mz);
					}

					// add the paired spectra to the collection
					groupedpsms.Add(connected_psms);
				}
			}
			else
			{
				for (int i = 0; i < psms.Length; ++i)
					groupedpsms.Add(new List<int>(new int[] { i }));
			}

			// break up the indices in tranches
			double nrprocessors = settings.NumberCores;//Math.Max(1, Environment.ProcessorCount - 1.0);
			List<List<int>[]> sliced_groupedpsms = ArraysA.Slice(groupedpsms.ToArray(), (int)Math.Ceiling(groupedpsms.Count / nrprocessors));

			// make the separate threads
			List<string> filenames = new List<string>();
			List<string> metanames = new List<string>();
			WorkerThreadGroup exportgrp = new WorkerThreadGroup();
			for (int i = 0; i < sliced_groupedpsms.Count; ++i)
			{
				filenames.Add(Path.GetTempFileName());
				metanames.Add(Path.GetTempFileName());
				exportgrp.AddWorker(new ExportAsMgfThread(filenames.Last(), metanames.Last(), m_sPath, sliced_groupedpsms[i], psms, settings));
			}
			exportgrp.Start(!HeckLibSettings.MultiThreading);
			while (exportgrp.IsRunning())
			{
				progress.Report((int)(100 * exportgrp.Progress()));
				if (iscancelled.IsCancellationRequested)
					exportgrp.Abort();
				Thread.Sleep(1000);
			}
			exportgrp.Dispose();

			// combine the various output files into one
			FilesA.MergeFiles(filenames.ToArray(), data.Filename);
			FilesA.MergeFiles(metanames.ToArray(), data.Filename + "meta", true);
		}
		#endregion


		#region data
		private string m_sPath;

		private int m_nNumberPsms;
		private Dictionary<string, List<PeptideSpectrumMatch>> m_lPsms;

		private string m_sCurrentRawFile = null;
		private HeckLibRawFiles.HeckLibRawFile m_pCurrentRawFile = null;

		private HeckLibSettingsDatabase m_pSettingsDatabase;

		private EncyclopediaDatabase m_pSpectralPredictions = null;
		#endregion
	}
}
