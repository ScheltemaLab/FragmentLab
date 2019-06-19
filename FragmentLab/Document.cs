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

using System.Windows.Forms;
using System.Collections.Generic;

// HeckLib
using HeckLib;
using HeckLib.utils;
using HeckLib.masspec;
using HeckLib.chemistry;

using HeckLib.io;
using HeckLib.io.mascot;





namespace FragmentLab
{
	public class Document
	{
		public enum ImportType
		{
			PD,
			MQ_EVIDENCE,
			MQ_MSMS,
			RAW,
			ERROR
		}

		public class BackgroundWorkerData
		{
			public string Filename;
			public FragmentLabSettings Settings;
			public List<PeptideSpectrumMatch> PsmList;
		}


		#region rawfile access
		public abstract class RawFile : IDisposable
		{
			#region IDisposable override
			public void Dispose()
			{
				Close();
			}
			#endregion

			#region interface
			public abstract void Close();

			public abstract int FirstSpectrumNumber();

			public abstract int LastSpectrumNumber();

			public abstract ScanHeader GetScanHeader(int scannumber);

			public abstract PrecursorInfo GetPrecursorInfo(int scannumber);

			public abstract Centroid[] GetSpectrum(int scannumber, out INoiseDistribution noise);

			public abstract void GetSpectrum(int scannumber, out double[] mzs, out float[] intensities, out INoiseDistribution noise);

			public abstract int GetScanNumber(int frameid, int ionmobilityindex);

			public abstract Dictionary<string, string> GetMetadata(int scannumber);
			#endregion
		}

		private class RawFileThermo : RawFile
		{
			public RawFileThermo(string filename)
			{
				m_pThermoRawFile = new HeckLibRawFileThermo.ThermoRawFile();
				m_pThermoRawFile.Open(filename);
				m_pThermoRawFile.SetCurrentController(HeckLibRawFileThermo.ThermoRawFile.CONTROLLER_MS, 1);

				m_sFilename = Path.GetFileNameWithoutExtension(filename);
			}

			public override void Close()
			{
				m_pThermoRawFile.Close();
			}

			public override int FirstSpectrumNumber()
			{
				return m_pThermoRawFile.GetFirstSpectrumNumber();
			}

			public override int LastSpectrumNumber()
			{
				return m_pThermoRawFile.GetLastSpectrumNumber();
			}

			public override ScanHeader GetScanHeader(int scannumber)
			{
				HeckLibRawFileThermo.ThermoRawFile.FilterLine filter = m_pThermoRawFile.GetFilterForScanNum(scannumber);
				float rt = (float)m_pThermoRawFile.GetRetentionTime(scannumber);
				return new ScanHeader {
					RawFile = m_sFilename,
					MassAnalyzer = filter.Analyzer,
					Polarity = filter.Polarity,
					ScanMode = filter.Format,
					ScanType = filter.ScanType,
					ScanNumber = scannumber,
					IonMobilityIndex = -1,
					IonMobilityLength = -1,
					RetentionTime = rt
				};
			}

			public override PrecursorInfo GetPrecursorInfo(int scannumber)
			{
				HeckLibRawFileThermo.ThermoRawFile.FilterLine filter = m_pThermoRawFile.GetFilterForScanNum(scannumber);
				HeckLibRawFileThermo.ThermoRawFile.PrecursorInfo precursor = m_pThermoRawFile.GetPrecursorScanInfoForScan(scannumber);

				float rt = (float)m_pThermoRawFile.GetRetentionTime(scannumber);

				if (filter.ScanType != Spectrum.ScanType.MSn)
					return null;

				return new PrecursorInfo {
					ScanNumber					= scannumber,
					RetentionTime				= rt,
					RawFile						= m_sFilename,
					Mz							= precursor.MonoIsotopicMass,
					Charge						= (short)precursor.ChargeState,
					MsOrder						= filter.MsOrder,
					Fragmentation				= filter.Fragmentation,
					FragmentationEnergy			= filter.FragmentationEnergy,
					SourceInducedDissociation	= (float)filter.Sid,
					// for later
					IsolationMin				= -1,
					IsolationMax				= -1,
					Intensity					= -1,
					IonMobilityIndex			= -1,
					IonMobilityLength			= -1,
					IsolationInterference		= -1,
					SignalToNoise				= -1,
					PrevPrecursors				= null
				};
			}

			public override Centroid[] GetSpectrum(int scannumber, out INoiseDistribution noise)
			{
				// load the spectral data
				double[] mzs;
				float[] intensities;
				m_pThermoRawFile.GetMassListFromScanNum(scannumber, false, out mzs, out intensities);

				noise = m_pThermoRawFile.GetNoiseDistribution(scannumber);

				HeckLibRawFileThermo.ThermoRawFile.FilterLine filter = m_pThermoRawFile.GetFilterForScanNum(scannumber);

				Centroid[] centroids;
				if (filter.Format == Spectrum.ScanModeType.Profile)
				{
					centroids = CentroidDetection.Process(mzs, intensities, noise, new CentroidDetection.Settings());
				}
				else
				{
					centroids = CentroidDetection.ConvertCentroids(mzs, intensities, Orbitrap.ToleranceForResolution(15000));
					for (int i = 0; i < centroids.Length; ++i)
						centroids[i].SignalToNoise = noise.GetSignalToNoise(centroids[i].Mz, centroids[i].Intensity);
				}

				return centroids;
			}

			public override void GetSpectrum(int scannumber, out double[] mzs, out float[] intensities, out INoiseDistribution noise)
			{
				m_pThermoRawFile.GetMassListFromScanNum(scannumber, false, out mzs, out intensities);
				noise = m_pThermoRawFile.GetNoiseDistribution(scannumber);
			}

			public override int GetScanNumber(int frameid, int ionmobilityindex)
			{
				return frameid;
			}

			public override Dictionary<string, string> GetMetadata(int scannumber)
			{
				Dictionary<string, string> result = new Dictionary<string, string>();

				Dictionary<string, string> statusLog = m_pThermoRawFile.GetStatusLogForScanNum(scannumber);
				foreach (string key in statusLog.Keys)
					result.Add("status | " + key, statusLog[key]);

				Dictionary<string, string> trailerExtra = m_pThermoRawFile.GetTrailerExtraForScanNum(scannumber);
				foreach (string key in trailerExtra.Keys)
					result.Add("trailer | " + key, trailerExtra[key]);

				return result;
			}

			private string m_sFilename;
			private HeckLibRawFileThermo.ThermoRawFile m_pThermoRawFile;
		}

		private class RawFileBruker : RawFile
		{
			public RawFileBruker(string filename)
			{
				m_sFilename = Path.GetFileNameWithoutExtension(filename);
				m_pBrukerRawFile = new HeckLibRawFileBruker.BrukerRawFile(filename);

				// go through all the scans and construct the headers already
				m_lFrameIds = new List<int>();
				m_lPrecursorIds = new List<int>();
				m_lScanHeaders = new List<ScanHeader>();
				m_lPrecursorInfos = new List<PrecursorInfo>();
				for (int frameid = m_pBrukerRawFile.GetFirstFrameId(); frameid < m_pBrukerRawFile.GetLastFrameId(); frameid++)
				{
					HeckLibRawFileBruker.BrukerRawFile.Frame frame = m_pBrukerRawFile.GetFrame(frameid);

					// check what to add
					if (frame.MsType == Spectrum.ScanType.Full)
					{
						int scannumber = m_lScanHeaders.Count;
						
						// construct the header(s)
						ScanHeader header = new ScanHeader();
						header.IonMobilityIndex							= 0;
						header.IonMobilityLength						= 0;
						header.ScanNumber								= scannumber/*frameid*/;
						header.MassAnalyzer								= Spectrum.MassAnalyzer.TimeOfFlight;
						header.Polarity									= frame.Polarity;
						header.RawFile									= m_sFilename;
						header.RetentionTime							= frame.Time;
						header.ScanMode									= frame.ScanMode;
						header.ScanType									= frame.MsType;
						m_lScanHeaders.Add(header);

						// add the empty precursor info to keep the scannumber count correct
						m_lPrecursorInfos.Add(null);

						// add the frame ids
						m_lFrameIds.Add(frameid);
						m_lPrecursorIds.Add(-1);

						// add the translation
						m_dFrameidimsidxToScanNumber.Add(new Tuple<int, int>(frameid, 0), scannumber);
					}
					else
					{
						HeckLibRawFileBruker.BrukerRawFile.PasefFrameMsMsInfo[] infos = m_pBrukerRawFile.GetPasefFrameMsMsInfosForFrame(frame.Id);
						foreach (HeckLibRawFileBruker.BrukerRawFile.PasefFrameMsMsInfo info in infos)
						{
							int scannumber = m_lScanHeaders.Count;
							HeckLibRawFileBruker.BrukerRawFile.PrecursorInfo precursor = m_pBrukerRawFile.GetPrecursor(info.PrecursorId);

							// construct the header
							ScanHeader header = new ScanHeader();
							header.IonMobilityIndex						= info.ScanNumBegin;
							header.IonMobilityLength					= info.ScanNumEnd - info.ScanNumBegin;
							header.ScanNumber							= scannumber/*frameid*/;
							header.MassAnalyzer							= Spectrum.MassAnalyzer.TimeOfFlight;
							header.Polarity								= frame.Polarity;
							header.RawFile								= m_sFilename;
							header.RetentionTime						= frame.Time;
							header.ScanMode								= frame.ScanMode;
							header.ScanType								= frame.MsType;
							m_lScanHeaders.Add(header);

							// construct the precursor info
							PrecursorInfo precursorinfo = new PrecursorInfo();
							precursorinfo.IonMobilityIndex				= info.ScanNumBegin;
							precursorinfo.IonMobilityLength				= info.ScanNumEnd - info.ScanNumBegin;
							precursorinfo.ScanNumber					= scannumber/*frameid*/;
							precursorinfo.Fragmentation					= Spectrum.FragmentationType.CID;
							precursorinfo.FragmentationEnergy			= info.CollisionEnergy;
							precursorinfo.MsOrder						= 2;
							precursorinfo.Mz							= precursor.MonoIsotopicMz;
							precursorinfo.Charge						= precursor.Charge;
							precursorinfo.RawFile						= m_sFilename;
							precursorinfo.RetentionTime					= frame.Time;
							precursorinfo.Intensity						= precursor.Intensity;
							precursorinfo.IsolationMin					= -1; // for later
							precursorinfo.IsolationMax					= -1;
							precursorinfo.IsolationInterference			= -1;
							precursorinfo.SignalToNoise					= -1;
							precursorinfo.PrevPrecursors				= null;
							precursorinfo.SourceInducedDissociation		= -1;
							precursorinfo.Mobility						= 1 / info.Mobility; // bruker tims reports 1/K0
							precursorinfo.CollisionCrossSection			= double.NaN;
							m_lPrecursorInfos.Add(precursorinfo);

							// plug in collision cross section
							if (precursorinfo.Charge > 0)
							{
								precursorinfo.CollisionCrossSection = CollisionCrossSectionDetection.CalculateFromTime(
										precursorinfo.Mz, 
										precursorinfo.Charge, 
										1 / info.Mobility, 
										CollisionCrossSectionDetection.brukerTims
									);
							}

							// add the frame ids
							m_lFrameIds.Add(frameid);
							m_lPrecursorIds.Add(precursor.Id);

							// add the translation
							m_dFrameidimsidxToScanNumber.Add(new Tuple<int, int>(frameid, info.ScanNumBegin), scannumber);
						}
					}
				}
			}

			public override void Close()
			{
				m_pBrukerRawFile.Dispose();
			}

			public override int FirstSpectrumNumber()
			{
				return 0;
			}

			public override int LastSpectrumNumber()
			{
				return m_lScanHeaders.Count - 1;
			}

			public override ScanHeader GetScanHeader(int scannumber)
			{
				return m_lScanHeaders[scannumber];
			}

			public override PrecursorInfo GetPrecursorInfo(int scannumber)
			{
				return m_lPrecursorInfos[scannumber];
			}

			public override Centroid[] GetSpectrum(int scannumber, out INoiseDistribution noise)
			{
				int frameid = m_lFrameIds[scannumber];

				// get the spectrum
				int precursorid = m_lPrecursorIds[scannumber];

				HeckLibRawFileBruker.BrukerRawFile.PasefFrameMsMsInfo msmsinfo = m_pBrukerRawFile.GetPasefFrameMsMsInfosForFrameAndPrecursor(frameid, precursorid);

				double[] mzs;
				float[] intensities;
				m_pBrukerRawFile.GetSpectrum(frameid/*scannumber*/, msmsinfo.ScanNumBegin, msmsinfo.ScanNumEnd, 30000, 50, 3000, out mzs, out intensities, out noise);

				Centroid[] centroids = CentroidDetection.Process(mzs, intensities, noise, new CentroidDetection.Settings());

				return centroids;
			}

			public override void GetSpectrum(int scannumber, out double[] mzs, out float[] intensities, out INoiseDistribution noise)
			{
				int frameid = m_lFrameIds[scannumber];
				int precursorid = m_lPrecursorIds[scannumber];

				HeckLibRawFileBruker.BrukerRawFile.PasefFrameMsMsInfo msmsinfo = m_pBrukerRawFile.GetPasefFrameMsMsInfosForFrameAndPrecursor(frameid, precursorid);

				m_pBrukerRawFile.GetSpectrum(frameid/*scannumber*/, msmsinfo.ScanNumBegin, msmsinfo.ScanNumEnd, 30000, 50, 3000, out mzs, out intensities, out noise);
			}

			public override int GetScanNumber(int frameid, int ionmobilityindex)
			{
				//return m_dFrameidimsidxToScanNumber[new Tuple<int, int>(frameid, ionmobilityindex)];
				return frameid;
			}

			public override Dictionary<string, string> GetMetadata(int scannumber)
			{
				Dictionary<string, string> metadata = new Dictionary<string, string>();

				int frameid = m_lFrameIds[scannumber];
				int precursorid = m_lPrecursorIds[scannumber];

				HeckLibRawFileBruker.BrukerRawFile.Frame frame = m_pBrukerRawFile.GetFrame(frameid);
				HeckLibRawFileBruker.BrukerRawFile.PasefFrameMsMsInfo msmsinfo = m_pBrukerRawFile.GetPasefFrameMsMsInfosForFrameAndPrecursor(frameid, precursorid);
				if (msmsinfo == null)
					msmsinfo = new HeckLibRawFileBruker.BrukerRawFile.PasefFrameMsMsInfo();

				double ccs = 0;
				if (precursorid != -1)
				{
					HeckLibRawFileBruker.BrukerRawFile.PrecursorInfo precursorinfo = m_pBrukerRawFile.GetPrecursor(precursorid);
					ccs = CollisionCrossSectionDetection.CalculateFromTime(precursorinfo.MonoIsotopicMz, precursorinfo.Charge, msmsinfo.Mobility, CollisionCrossSectionDetection.brukerTims);
				}

				metadata["frame | AccumulationTime"]		= frame.AccumulationTime.ToString();
				metadata["frame | MaxIntensity"]			= frame.MaxIntensity.ToString();
				metadata["frame | MinMz"]					= frame.MinMz.ToString();
				metadata["frame | MaxMz"]					= frame.MaxMz.ToString();
				metadata["frame | MsType"]					= frame.MsType.ToString();
				metadata["frame | MzCalibration"]			= frame.MzCalibration.ToString();
				metadata["frame | NumPeaks"]				= frame.NumPeaks.ToString();
				metadata["frame | NumScans"]				= frame.NumScans.ToString();
				metadata["frame | Polarity"]				= frame.Polarity.ToString();
				metadata["frame | RampTime"]				= frame.RampTime.ToString();
				metadata["frame | Resolution"]				= frame.Resolution.ToString();
				metadata["frame | ScanMode"]				= frame.ScanMode.ToString();
				metadata["frame | SummedIntensities"]		= frame.SummedIntensities.ToString();
				metadata["frame | T1"]						= frame.T1.ToString();
				metadata["frame | T2"]						= frame.T2.ToString();
				metadata["frame | Time"]					= frame.Time.ToString();
				metadata["frame | TimsCalibration"]			= frame.TimsCalibration.ToString();
				metadata["frame | TimsId"]					= frame.TimsId.ToString();

				metadata["msmsinfo | CollisionEnergy"]		= msmsinfo.CollisionEnergy.ToString();
				metadata["msmsinfo | IsolationMz"]			= msmsinfo.IsolationMz.ToString();
				metadata["msmsinfo | IsolationWidth"]		= msmsinfo.IsolationWidth.ToString();
				metadata["msmsinfo | PrecursorId"]			= msmsinfo.PrecursorId.ToString();
				metadata["msmsinfo | ScanNumBegin"]			= msmsinfo.ScanNumBegin.ToString();
				metadata["msmsinfo | ScanNumEnd"]			= msmsinfo.ScanNumEnd.ToString();

				metadata["homegrown | CCS"]					= ccs.ToString();

				return metadata;
			}

			Dictionary<Tuple<int, int>, int> m_dFrameidimsidxToScanNumber = new Dictionary<Tuple<int, int>, int>();
			private string m_sFilename;
			private List<int> m_lFrameIds;
			private List<int> m_lPrecursorIds;
			private List<ScanHeader> m_lScanHeaders;
			private List<PrecursorInfo> m_lPrecursorInfos;
			private HeckLibRawFileBruker.BrukerRawFile m_pBrukerRawFile;
		}

		private RawFile OpenFile(string filename, out string path)
		{
			string extension = Path.GetExtension(filename).ToLower();

			if (extension == ".raw")
			{
				path = Path.GetDirectoryName(filename);
				return new RawFileThermo(filename);
			}
			else if (extension == ".d")
			{
				path = Directory.GetParent(filename).FullName;
				return new RawFileBruker(filename);
			}
			else if (extension == ".tdf")
			{
				path = Directory.GetParent(Path.GetDirectoryName(filename)).FullName;
				return new RawFileBruker(Path.GetDirectoryName(filename));
			}
			else
				throw new Exception("unknown extension '" + extension + "'.");
		}
		#endregion


		#region constructor(s)
		public Document()
		{

		}

		public Document(string[] filenames, ImportType type)
		{
			m_sPath = Path.GetDirectoryName(filenames[0]);

			// load the psms and sort them on raw-file
			m_nNumberPsms = 0;
			m_lPsms = new Dictionary<string, List<PeptideSpectrumMatch>>();

			switch (type)
			{
				case ImportType.PD:
					foreach (string filename in filenames)
					{
						MascotReader.Parse(filename, m_lModifications, delegate (PeptideSpectrumMatch psm) {
								if (!m_lPsms.ContainsKey(psm.RawFile))
									m_lPsms.Add(psm.RawFile, new List<PeptideSpectrumMatch>());
								m_lPsms[psm.RawFile].Add(psm);
								m_nNumberPsms++;
							});
					}
					break;
				case ImportType.MQ_MSMS:
					foreach (string filename in filenames)
					{
						HeckLib.io.maxquant.MsmsScans.Parse(filename, delegate (PeptideSpectrumMatch psm) {
								psm.RawFile = @"..\..\" + psm.RawFile;
								if (!m_lPsms.ContainsKey(psm.RawFile))
									m_lPsms.Add(psm.RawFile, new List<PeptideSpectrumMatch>());
								m_lPsms[psm.RawFile].Add(psm);
								m_nNumberPsms++;
							});
					}
					break;
				case ImportType.MQ_EVIDENCE:
					foreach (string filename in filenames)
					{
						Dictionary<string, Modification> modifications = Modification.Parse("modifications.xml");
						HeckLib.io.maxquant.EvidenceReader.Parse(filename, modifications, delegate (PeptideSpectrumMatch psm) {
								// simply choosing HCD
								psm.Fragmentation = Spectrum.FragmentationType.HCD;

								if (!m_lPsms.ContainsKey(psm.RawFile))
									m_lPsms.Add(psm.RawFile, new List<PeptideSpectrumMatch>());
								m_lPsms[psm.RawFile].Add(psm);
								m_nNumberPsms++;
							});
					}
					break;
				case ImportType.RAW:
					foreach (string filename in filenames)
					{
						// open the file, this will overwrite the path which is needed for path-based raw-data (e.g. Bruker)
						using (RawFile rawfile = OpenFile(filename, out m_sPath))
						{
							for (int scannumber = rawfile.FirstSpectrumNumber(); scannumber <= rawfile.LastSpectrumNumber(); ++scannumber)
							{
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
										RawFile				= header.RawFile,
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
					}
					break;
				default:
					MessageBox.Show("This filter is not yet supported :)", "Error");
					return;
			}

			// order the psms on scannumber
			foreach (string rawfile in m_lPsms.Keys)
				m_lPsms[rawfile].Sort((a, b) => a.MinScan.CompareTo(b.MinScan));
		}
		#endregion


		#region rawfile access
		public string GetPsmPath()
		{
			return m_sPath;
		}

		public List<PeptideSpectrumMatch> GetPsms()
		{
			return m_lPsms.Values.SelectMany(x => x).ToList();
		}

		public string[] GetUniqueRawfiles()
		{
			return m_lPsms.Keys.ToArray();
		}

		public PeptideSpectrumMatch[] GetPsmsForRawfile(string rawfile)
		{
			return m_lPsms[rawfile].ToArray();
		}

		public Centroid[] LoadSpectrum(PeptideSpectrumMatch psm, FragmentLabSettings settings, out int[] topxranks, out PeptideFragment.FragmentModel model, out INoiseDistribution noise, out PrecursorInfo precursor, out ScanHeader scanheader)
		{
			string rawfilename = Path.Combine(m_sPath, Path.GetFileNameWithoutExtension(psm.RawFile));
			if (rawfilename != m_sCurrentRawFile)
			{
				if (m_pCurrentRawFile != null)
					m_pCurrentRawFile.Close();

				m_sCurrentRawFile = rawfilename;
				if (File.Exists(rawfilename + ".raw"))
					m_pCurrentRawFile = new RawFileThermo(rawfilename + ".raw");
				else if (Directory.Exists(rawfilename + ".d"))
					m_pCurrentRawFile = new RawFileBruker(rawfilename + ".d");
				else
					throw new Exception("Unknown file format");
			}

			return LoadSpectrum(m_pCurrentRawFile, psm, settings, out topxranks, out model, out noise, out precursor, out scanheader);
		}

		private static Centroid[] LoadSpectrum(RawFile rawfile, PeptideSpectrumMatch psm, FragmentLabSettings settings, out int[] topxranks, out PeptideFragment.FragmentModel model, out INoiseDistribution noise, out PrecursorInfo precursor, out ScanHeader scanheader)
		{
			// get the spectrum
			precursor = rawfile.GetPrecursorInfo(psm.MinScan);

			scanheader = rawfile.GetScanHeader(psm.MinScan);
			Centroid[] centroids = rawfile.GetSpectrum(rawfile.GetScanNumber(psm.MinScan, psm.ImsIndex), out noise);

			// filter S/N
			centroids = SpectrumUtils.FilterForSignalToNoise(centroids, settings.SignalToNoise);
			centroids = SpectrumUtils.FilterForBasepeak(centroids, settings.PercentOfBasepeak);

			// detect isotopes
			IsotopePattern[] isotopes = IsotopePatternDetection.Process(centroids, new IsotopePatternDetection.Settings { MaxCharge = (short)(psm.Charge + 1) });
			centroids = SpectrumUtils.Deisotope(centroids, isotopes, psm.Charge, true);

			// load the fragmentation model
			model = new PeptideFragment.FragmentModel(PeptideFragment.GetFragmentModel(settings.Fragmentation == Spectrum.FragmentationType.None ? precursor.Fragmentation : settings.Fragmentation));
			model.tolerance = settings.MatchTolerance;
			if (settings.ExcludeNeutralLosses == true)
				model.TurnOff(PeptideFragment.MASSSHIFT_WATERLOSS | PeptideFragment.MASSSHIFT_AMMONIALOSS | PeptideFragment.MASSSHIFT_NEUTRALLOSS);

			return SpectrumUtils.TopX(centroids, model.topx, model.topx_massrange, out topxranks);
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

				string myfilename = Path.Combine(path, Path.GetFileNameWithoutExtension(psm.RawFile) + "_" + psm.MinScan + ".txt");

				int[] topxranks;
				PeptideFragment.FragmentModel model;
				INoiseDistribution noise;
				PrecursorInfo precursor;
				ScanHeader scanheader;
				Centroid[] spectrum = LoadSpectrum(psm, settings, out topxranks, out model, out noise, out precursor, out scanheader);

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

		public void ExportFrequentFlyers(BackgroundWorkerData data, IProgress<int> progress, CancellationToken iscancelled)
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

				Peptide peptide = data.Settings.Peptide != null ? data.Settings.Peptide : psm.Peptide;
				freqflyers.ProcessSpectrum(peptide, spectrum);

				// update progress
				numberpsms++;
				progress.Report((int)(100.0 * numberpsms / m_nNumberPsms));
			}

			// output the freqflyer result
			freqflyers.InterpretMzDiffs(data.Filename);
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
					model.tolerance = settings.MatchTolerance;

					// create the fragments
					Peptide peptide = settings.Peptide != null ? settings.Peptide : psm.Peptide;
					PeptideFragment[] fragments = SpectrumUtils.MatchFragments(peptide, settings.Charge == 0 ? psm.Charge : settings.Charge, spectrum, model);
					PeptideFragmentAnnotator annotator = new PeptideFragmentAnnotator(psm.Peptide, fragments, spectrum);

					// calculate the coverage for each ion-type
					CsvRow row = writer.CreateRow();

					row[COLUMN_RAWFILE] = psm.RawFile;
					row[COLUMN_SEQUENCE] = psm.Peptide.GetModifiedSequence();
					row[COLUMN_SEQUENCE_LENGTH] = psm.Peptide.Length;
					row[COLUMN_SCANNUMBER] = psm.MinScan;
					row[COLUMN_DETECTEDIONCOUNT] = spectrum.Length;
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
					row[COLUMN_OBSERVED_FRAGMENTS] = annotator.ObservedFragments;
					row[COLUMN_SEQUENCE_COVERAGE] = annotator.SequenceCoverage;
					row[COLUMN_SEQUENCE_COVERAGE_CTERM] = annotator.SequenceCoverageCterm;
					row[COLUMN_SEQUENCE_COVERAGE_NTERM] = annotator.SequenceCoverageNterm;
					row[COLUMN_ASSIGNED_IONLOAD] = annotator.AssignedIonLoad;
					row[COLUMN_FRAGMENTATION_EFFICIENCY] = annotator.FragmentationEfficiency;
					row[COLUMN_PRECURSOR_ASSIGNED] = annotator.PrecursorAssigned;
					row[COLUMN_PERCENT_SPECTRUM_EXPLAINED] = annotator.AssignedIonLoad;

					writer.WriteRow(row);

					// update progress
					numberpsms++;
					progress.Report((int)(100.0 * numberpsms / m_nNumberPsms));
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
				model.tolerance = settings.MatchTolerance;

				// perform calculations
				Glycan.SimpleScore simplescore = Glycan.CalculateSimpleScore(spectrum, topxranks, settings.MatchTolerance, model.topx, model.topx_massrange);

				int[] saccharides;
				double mscore = Glycan.CalculateDoublePlayMScore(spectrum, topxranks, settings.MatchTolerance, model.topx, model.topx_massrange, out saccharides);

				bool hexphos = Glycan.HexPhosOxoniumIonPresent(spectrum, topxranks, settings.MatchTolerance, model.topx, model.topx_massrange);

				// dump the results
				CsvRow row = writer.CreateRow();
				row[COLUMN_RAWFILE] = psm.RawFile;
				row[COLUMN_SCANNUMBER] = psm.MinScan;
				row[COLUMN_PRECURSOR_MZ] = psm.Mz;
				row[COLUMN_PRECURSOR_CHARGE] = psm.Charge;
				row[COLUMN_PRECURSOR_GLYCAN_SCORE] = simplescore == null ? double.NaN : simplescore.GlycanScore;
				row[COLUMN_PRECURSOR_GLCNAC_GALNAC] = simplescore == null ? double.NaN : simplescore.GlcNAcGalNAcRatio;
				row[COLUMN_PRECURSOR_MSCORE] = mscore;
				row[COLUMN_PRECURSOR_PHOSPHOPEAK] = hexphos ? "+" : "";
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
			MgfReader.Parse(files[0], delegate (Centroid[] spectrum, PrecursorInfo precursor, string title, Spectrum.MassAnalyzer massanalyzer) {
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


		#region the mess that is export to mgf...
		public class ExportAsMgfThread : WorkerThread, IDisposable
		{
			public ExportAsMgfThread(string outfile, string path, List<int>[] grouped_psms, PeptideSpectrumMatch[] allpsms, FragmentLabSettings settings) : base("Export as MGF")
			{
				m_sPath = path;
				m_aAllPsms = allpsms;
				m_aGroupedPsms = grouped_psms;
				m_pSettings = settings;

				m_fMgfWriter = new StreamWriter(new FileStream(outfile, FileMode.Create, FileAccess.Write, FileShare.Read, 65536, true));
			}

			public override void Dispose()
			{
				m_fMgfWriter.Close();
			}

			public override void Execute()
			{
				m_nCurrentGroupedPsm = 0;
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
						string rawfilename = Path.Combine(m_sPath, Path.GetFileNameWithoutExtension(psm.RawFile));
						if (rawfilename != m_sCurrentRawFile)
						{
							if (m_pCurrentRawFile != null)
								m_pCurrentRawFile.Close();

							m_sCurrentRawFile = rawfilename;
							if (File.Exists(rawfilename + ".raw"))
								m_pCurrentRawFile = new RawFileThermo(rawfilename + ".raw");
							else if (Directory.Exists(rawfilename + ".d"))
								m_pCurrentRawFile = new RawFileBruker(rawfilename + ".d");
							else
								throw new Exception("Unknown file format");
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
					double mobility_length = Statistics.Max(mobility_lengths.ToArray());

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
					centroids = SpectrumUtils.FilterForBasepeak(centroids, m_pSettings.PercentOfBasepeak);

					// detect isotopes
					IsotopePattern[] isotopes = IsotopePatternDetection.Process(centroids, new IsotopePatternDetection.Settings { MaxCharge = best_precursor.Charge });
					centroids = SpectrumUtils.Deisotope(centroids, isotopes, best_precursor.Charge, true);

					// load the fragmentation model
					PeptideFragment.FragmentModel model = new PeptideFragment.FragmentModel(PeptideFragment.GetFragmentModel(m_pSettings.Fragmentation == Spectrum.FragmentationType.None ? best_precursor.Fragmentation : m_pSettings.Fragmentation));
					model.tolerance = m_pSettings.MatchTolerance;
					if (m_pSettings.ExcludeNeutralLosses == true)
						model.TurnOff(PeptideFragment.MASSSHIFT_WATERLOSS | PeptideFragment.MASSSHIFT_AMMONIALOSS | PeptideFragment.MASSSHIFT_NEUTRALLOSS);

					int[] topxranks;
					centroids = SpectrumUtils.TopX(centroids, model.topx, model.topx_massrange, out topxranks);

					// write the spectrum
					if (centroids.Length < 10)
						continue;

					string title = string.Format("SPECTRA:{0} MOBILITY:{1} MOBILITYLENGTH:{2} CCS:{3} INTENSITY:{4}", all_mzs.Count, mobility, mobility_length, ccs, best_precursor.Intensity);

					PrecursorInfo pinfo = new PrecursorInfo(best_precursor);
					MgfWriter.WriteSpectrum(m_fMgfWriter, title, pinfo, centroids, best_scanheader.Polarity, Spectrum.MassAnalyzer.TimeOfFlight);
				}
			}

			public override double Progress()
			{
				return m_nCurrentGroupedPsm / (double)m_aGroupedPsms.Length;
			}

			private StreamWriter m_fMgfWriter;

			private string m_sPath;
			private string m_sCurrentRawFile;
			private RawFile m_pCurrentRawFile;

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
			List<List<int>[]> sliced_groupedpsms = ArraysA.Slice(groupedpsms.ToArray(), (int)Math.Ceiling(groupedpsms.Count / Math.Max(1, Environment.ProcessorCount - 1.0)));

			// make the separate threads
			List<string> filenames = new List<string>();
			WorkerThreadGroup exportgrp = new WorkerThreadGroup();
			for (int i = 0; i < sliced_groupedpsms.Count; ++i)
			{
				filenames.Add(Path.GetTempFileName());
				exportgrp.AddWorker(new ExportAsMgfThread(filenames.Last(), m_sPath, sliced_groupedpsms[i], psms, settings));
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
		}
		#endregion


		#region data
		private string m_sPath;

		private int m_nNumberPsms;
		private Dictionary<string, List<PeptideSpectrumMatch>> m_lPsms;

		private string m_sCurrentRawFile = null;
		private RawFile m_pCurrentRawFile = null;

		private Dictionary<string, Modification> m_lModifications = Modification.Parse("modifications.xml");
		#endregion
	}
}
