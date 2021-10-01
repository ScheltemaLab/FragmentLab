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
using System.Threading;

using System.Collections;
using System.Collections.Generic;

using System.Windows;
using System.Windows.Forms;

// HeckLib
using HeckLib;
using HeckLib.utils;
using HeckLib.masspec;
using HeckLib.chemistry;
using HeckLib.backgroundworker;

using HeckLib.io;
using HeckLib.io.database;
using HeckLib.io.opensearch;





namespace FragmentLab.dialogs
{
	public partial class DialogPeptideSearch : Form
	{
		#region constructor(s)
		public DialogPeptideSearch(Document document, FragmentLabSettings settings, HeckLibSettingsDatabase hecklab_settings)
		{
			this.ctrlPeptideEditor = new HeckLib.graphics.controls.PeptideEditorControl(hecklab_settings.GetModifications()); // <- this was moved out of the InitializeComponent function

			// windows related
			this.StartPosition = FormStartPosition.CenterParent;
			this.CancelButton = btnCancel;
			InitializeComponent();

			// events
			listProteinModifications.DoubleClickActivation	= true;
			listProteinModifications.SubItemClicked			+= ListProteinModifications_SubItemClicked;
			listProteinModifications.SubItemEndEditing		+= ListProteinModifications_SubItemEndEditing;

			// add the proteases
			m_dProteases = new Dictionary<string, Protease>();
			foreach (Protease protease in Protease.Proteases)
			{
				m_dProteases.Add(protease.Title, protease);
				cmbProteinProtease.Items.Add(protease.Title);
			}
			cmbProteinProtease.SelectedItem = Protease.Proteases[Protease.TrypsinP].Title;

			// fill the editors
			m_dModifications = hecklab_settings.GetModifications();
			foreach (Modification m in m_dModifications.Values)
				editorModification.Items.Add(m.Title);

			editorType.Items.Add("FIXED");
			editorType.Items.Add("VARIABLE");

			// general bookkeeping
			m_pHecklibSettings = hecklab_settings;
			m_pSettings = settings;
			m_pDocument = document;
		}
		#endregion


		#region access
		public List<PeptideSpectrumMatch> Result { get { return m_lResult; } }
		#endregion


		#region events
		private void btnOk_Click(object sender, EventArgs e)
		{
			PeptideSearchData data;
			if (tabControl1.SelectedTab == tabPeptide)
			{
				// extract the information from the UI
				Peptide peptide = ctrlPeptideEditor.SelectedPeptide;
				if (peptide == null || peptide.Length == 0)
				{
					System.Windows.MessageBox.Show("No peptide defined, exiting.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
					DialogResult = DialogResult.Abort; Close();
					return;
				}

				data = new PeptideSearchData {
						MinScore				= (double)numMinScore.Value,
						SelectedPeptide			= peptide,
					};
			}
			else if (tabControl1.SelectedTab == tabProtein)
			{
				if (string.IsNullOrEmpty(txtProteinSequence.Text) || !m_dProteases.ContainsKey(cmbProteinProtease.Text))
				{
					System.Windows.MessageBox.Show("No protein information defined, exiting.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
					DialogResult = DialogResult.Abort; Close();
					return;
				}

				string sequence = txtProteinSequence.Text;
				Protease protease = m_dProteases[cmbProteinProtease.Text];
				List<Modification> fixmods = new List<Modification>(), varmods = new List<Modification>();
				foreach (ListViewItem item in listProteinModifications.Items)
				{
					string title = item.SubItems[0].Text;
					string type = item.SubItems[1].Text;

					if (!m_dModifications.ContainsKey(title))
					{
						System.Windows.MessageBox.Show("Selected modification '" + title + "' not known, exiting.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
						DialogResult = DialogResult.Abort; Close();
						return;
					}

					if (type == "FIXED")
						fixmods.Add(m_dModifications[title]);
					else
						varmods.Add(m_dModifications[title]);
				}

				data = new PeptideSearchData {
						MinScore				= (double)numMinScore.Value,
						ProteinSequence			= sequence,
						Protease				= protease,
						FixMods					= fixmods.ToArray(),
						VarMods					= varmods.ToArray()
					};
			}
			else
				throw new Exception("Unexpected tab for open search ?");

			// process the file
			m_pBackgroundWorker.TriggerBackgroundWorker(Process, data);

			// we're done
			DialogResult = DialogResult.OK;
			this.Close();
		}

		private void btnCancel_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			this.Close();
		}

		private void ListProteinModifications_SubItemClicked(object sender, HeckLib.visualization.ui.SubItemEventArgs e)
		{
			Control ctrl = null;
			switch (e.SubItem)
			{
				case 0:
					ctrl = editorModification;
					break;
				case 1:
					ctrl = editorType;
					break;
			};
			if (ctrl == null)
				return;

			listProteinModifications.StartEditing(ctrl, e.Item, e.SubItem);
		}

		private void ListProteinModifications_SubItemEndEditing(object sender, HeckLib.visualization.ui.SubItemEndEditingEventArgs e)
		{
			if (e.Cancel)
				return;

			// grab the data from the editor
			if (e.SubItem == 0)
			{
			}
			else if (e.SubItem == 1)
			{
			}
		}

		private void btnProteinAddMod_Click(object sender, EventArgs e)
		{
			listProteinModifications.Items.Add(new ListViewItem(new string[] { editorModification.Items[0].ToString(), editorType.Items[0].ToString() }));
		}

		private void btnProteinRmMod_Click(object sender, EventArgs e)
		{
			if (listProteinModifications.SelectedIndices.Count == 0)
				return;
			int index = listProteinModifications.SelectedIndices[0];
			if (index < 0 || index >= listProteinModifications.Items.Count)
				return;
			listProteinModifications.Items.RemoveAt(index);
		}
		#endregion


		#region processor
		private class PeptideSearchData
		{
			public double MinScore;

			// protein search
			public string ProteinSequence	= null;
			public Modification[] VarMods	= null;
			public Modification[] FixMods	= null;
			public Protease Protease		= null;

			// peptide search
			public Peptide SelectedPeptide	= null;
		}

		private void CreatePeptideIndex(string dbpath, PeptideSearchData data, out OpenSearchDatabase targetdb, out FragmentIndex fragmentindex)
		{
			OpenSearch.DbSettings settings = new HeckLib.io.opensearch.OpenSearch.DbSettings {
						Format					= HeckLib.io.fasta.FastaParser.Format.FASTA,
						MinPeptideLength		= 5,
						MaxPeptideLength		= 50,
						Processing				= HeckLib.io.opensearch.OpenSearch.SequenceProcessing.TARGET,
						MinMass					= 500,
						MaxMass					= 10000,
						MaxFragmentCharge		= 2,
						MinFragmentMz			= 100,
						MaxFragmentMz			= 1500,
						MaxNrMods				= 3,
						FixedMods				= data.SelectedPeptide.GetUniqueModifications(),
						VarMods					= new HeckLib.chemistry.Modification[0],
						ProteaseCascade			= new HeckLib.io.opensearch.OpenSearch.ProteaseSettings[0]
					};
			OpenSearch.GenerateDbFromPeptideDefinition(data.SelectedPeptide, Path.Combine(dbpath, "peptide.db"), settings, null);

			targetdb = new OpenSearchDatabase(
					Path.Combine(dbpath, "peptide.masses"),
					Path.Combine(dbpath, "peptide.modpeps"),
					Path.Combine(dbpath, "peptide.modificationdescriptions"),
					Path.Combine(dbpath, "peptide.proteins")
				);
			fragmentindex = new FragmentIndex(Path.Combine(dbpath, "peptide.fragmentindex"));
		}

		private void CreateProteinIndex(string dbpath, PeptideSearchData data, out OpenSearchDatabase targetdb, out FragmentIndex fragmentindex)
		{
			OpenSearch.DbSettings settings = new HeckLib.io.opensearch.OpenSearch.DbSettings {
						Format					= HeckLib.io.fasta.FastaParser.Format.FASTA,
						MinPeptideLength		= 5,
						MaxPeptideLength		= 50,
						Processing				= HeckLib.io.opensearch.OpenSearch.SequenceProcessing.TARGET,
						MinMass					= 500,
						MaxMass					= 10000,
						MaxFragmentCharge		= 2,
						MinFragmentMz			= 100,
						MaxFragmentMz			= 1500,
						MaxNrMods				= 3,
						FixedMods				= data.FixMods,
						VarMods					= data.VarMods,
						ProteaseCascade			= new OpenSearch.ProteaseSettings[]{
								new OpenSearch.ProteaseSettings { Protease = data.Protease, MaxMissedCleavages = 3 }
							}
					};
			OpenSearch.GenerateDbFromProteinSequence(data.ProteinSequence, Path.Combine(dbpath, "protein.db"), settings, null);

			targetdb = new OpenSearchDatabase(
					Path.Combine(dbpath, "protein.masses"),
					Path.Combine(dbpath, "protein.modpeps"),
					Path.Combine(dbpath, "protein.modificationdescriptions"),
					Path.Combine(dbpath, "protein.proteins")
				);
			fragmentindex = new FragmentIndex(Path.Combine(dbpath, "protein.fragmentindex"));
		}

		private class PeptideMatchInfo
		{
			public uint PeptideId;
			public int N_b = 0;
			public int N_y = 0;
			public int sumI_b = 0;
			public int sumI_y = 0;
		}

		private void Process(PeptideSearchData data, IProgress<int> progress, CancellationToken iscancelled)
		{
			// generate the database
			string dbpath = FilesA.CreateTemporaryDirectory();

			OpenSearchDatabase targetdb; FragmentIndex fragmentindex;
			if (data.SelectedPeptide != null)
				CreatePeptideIndex(dbpath, data, out targetdb, out fragmentindex);
			else if (data.ProteinSequence != null)
				CreateProteinIndex(dbpath, data, out targetdb, out fragmentindex);
			else
				throw new Exception("Unexpected sequence of events for OpenSearch.");
			
			// process the spectra
			List<PeptideSpectrumMatch> base_psms = m_pDocument.GetPsms();
			List<PeptideSpectrumMatch> resulting_psms = new List<PeptideSpectrumMatch>();

			int nr_psms = 0;
			if (chkParseFullFiles.Checked)
			{
				// detect the different raw-files
				HashSet<string> rawfilenames = new HashSet<string>();
				foreach (PeptideSpectrumMatch psm in base_psms)
					rawfilenames.Add(psm.RawFile);

				// check how many files we need to plow through
				int scancount = 0;
				foreach (string rawfilename in rawfilenames)
				{
					HeckLibRawFiles.HeckLibRawFile rawfile = m_pDocument.GetRawFile(rawfilename);
					if (rawfile == null)
						continue;
					scancount += rawfile.LastSpectrumNumber() - rawfile.FirstSpectrumNumber() + 1;
				}

				// go through the files
				foreach (string rawfilename in rawfilenames)
				{
					HeckLibRawFiles.HeckLibRawFile rawfile = m_pDocument.GetRawFile(rawfilename);
					if (rawfile == null)
						continue;

					int minscannumber = rawfile.FirstSpectrumNumber();
					int maxscannumber = rawfile.LastSpectrumNumber();
					for (int scannumber = minscannumber; scannumber <= maxscannumber; ++scannumber)
					{
						progress.Report((int)(100.0 * nr_psms++ / scancount));
						if (iscancelled.IsCancellationRequested)
							break;
						ScanHeader header = rawfile.GetScanHeader(scannumber);
						if (header == null || header.ScanType != Spectrum.ScanType.MSn)
							continue;
						PrecursorInfo precursorinfo = rawfile.GetPrecursorInfo(scannumber);

						PeptideSpectrumMatch base_psm = new PeptideSpectrumMatch { RawFile = rawfilename, MinScan = scannumber, Charge = precursorinfo.Charge == 0 ? (short)3 : precursorinfo.Charge };

						int[] topxranks;
						PeptideFragment.FragmentModel model;
						INoiseDistribution noise; PrecursorInfo precursor; ScanHeader scanheader;
						Centroid[] spectrum = m_pDocument.LoadSpectrum(base_psm, m_pSettings, out topxranks, out model, out noise, out precursor, out scanheader);
						if (spectrum.Length < 5)
							continue;
						Process_analyze_spectrum(data, base_psm, precursor, spectrum, targetdb, fragmentindex, resulting_psms);
					}
					if (iscancelled.IsCancellationRequested)
						break;
				}
			}
			else
			{
				foreach (PeptideSpectrumMatch base_psm in base_psms)
				{
					progress.Report((int)(100.0 * nr_psms++ / base_psms.Count));
					if (iscancelled.IsCancellationRequested)
						break;

					int[] topxranks;
					PeptideFragment.FragmentModel model;
					INoiseDistribution noise; PrecursorInfo precursor; ScanHeader scanheader;
					Centroid[] spectrum = m_pDocument.LoadSpectrum(base_psm, m_pSettings, out topxranks, out model, out noise, out precursor, out scanheader);
					if (spectrum.Length < 5)
						continue;
					Process_analyze_spectrum(data, base_psm, precursor, spectrum, targetdb, fragmentindex, resulting_psms);
				}
			}
			m_lResult = iscancelled.IsCancellationRequested ? null : resulting_psms;

			// destroy the temporary database
			targetdb.Dispose();
			FilesA.DeleteDirectory(dbpath);
		}

		private void Process_analyze_spectrum(PeptideSearchData data, PeptideSpectrumMatch base_psm, PrecursorInfo precursor, Centroid[] spectrum, OpenSearchDatabase targetdb, FragmentIndex fragmentindex, List<PeptideSpectrumMatch> resulting_psms)
		{
			// de-de-isotope
			for (int i = 0; i < spectrum.Length; ++i)
				if (spectrum[i].Charge != 0 && spectrum[i].Charge != 1) spectrum[i].Mz = MassSpectrometry.Recharge(spectrum[i].Mz, 1, spectrum[i].Charge);

			double[] mzs; float[] intensities;
			Centroid.GetMzsIntensities(spectrum, out mzs, out intensities);
			float maxintensity = Statistics.Max(intensities);
			for (int i = 0; i < intensities.Length; ++i)
				intensities[i] = 1 + (float)Math.Round(99 * intensities[i] / maxintensity);

			// match our peptide against the spectrum
			Dictionary<uint, PeptideMatchInfo> matches = new Dictionary<uint, PeptideMatchInfo>();
			for (int i = 0; i < spectrum.Length; ++i)
			{
				uint[] matching_peptideids; byte[] matched_fragmenttype;
				fragmentindex.MatchFragment(m_pSettings.MatchTolerance, spectrum[i].Mz, spectrum[i].Charge, null, out matching_peptideids, out matched_fragmenttype);

				for (int idx = 0; idx < matching_peptideids.Length; ++idx)
				{
					if (!matches.ContainsKey(matching_peptideids[idx]))
						matches.Add(matching_peptideids[idx], new PeptideMatchInfo { PeptideId = matching_peptideids[idx] });
						
					if (matched_fragmenttype[idx] == PeptideFragment.ION_B)
					{
						matches[matching_peptideids[idx]].N_b++;
						matches[matching_peptideids[idx]].sumI_b += (int)intensities[i];
					}
					else if (matched_fragmenttype[idx] == PeptideFragment.ION_Y)
					{
						matches[matching_peptideids[idx]].N_y++;
						matches[matching_peptideids[idx]].sumI_y += (int)intensities[i];
					}
				}
			}

			List<PeptideSpectrumMatch> psms = new List<PeptideSpectrumMatch>();
			foreach (PeptideMatchInfo match in matches.Values)
			{
				if (match.N_b < 2 || match.N_y < 2)
					continue;
				double score = SpectrumUtils.CalcHyperScore_by(match.N_b, match.N_y, match.sumI_b, match.sumI_y);
				if (score < data.MinScore)
					continue;

				// store this as a psm
				Peptide[] p = targetdb.GetModifiedPeptidesFromIds(new int[] { (int)match.PeptideId });

				PeptideSpectrumMatch psm = new PeptideSpectrumMatch {
						RawFile				= base_psm.RawFile,
						Mz					= precursor.Mz,
						Charge				= precursor.Charge,
						MinScan				= precursor.ScanNumber,
						RetentionTime		= precursor.RetentionTime,
						Peptide				= p[0],
						Score				= score,
						Fragmentation		= precursor.Fragmentation
					};
				psms.Add(psm);
			}
			if (psms.Count > 0)
			{
				psms.Sort((a, b) => -a.Score.CompareTo(b.Score));
				resulting_psms.Add(psms[0]);
			}
		}
		#endregion


		#region data
		private Document m_pDocument;
		private FragmentLabSettings m_pSettings;
		private HeckLibSettingsDatabase m_pHecklibSettings;

		private Dictionary<string, Protease> m_dProteases;
		private Dictionary<string, Modification> m_dModifications;

		private HlBackgroundWorker<PeptideSearchData> m_pBackgroundWorker = new HlBackgroundWorker<PeptideSearchData>();

		private List<PeptideSpectrumMatch> m_lResult = new List<PeptideSpectrumMatch>();
		#endregion
	}
}
