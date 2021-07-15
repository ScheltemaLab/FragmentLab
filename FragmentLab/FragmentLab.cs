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
using System.Windows.Forms;
using System.Collections.Generic;

// HeckLib
using HeckLib;
using HeckLib.masspec;
using HeckLib.chemistry;
using HeckLib.backgroundworker;
using HeckLib.objectlistview;

using HeckLib.io;
using HeckLib.io.database;
using HeckLib.io.fileformats;

using hecklib.graphics;
using hecklib.graphics.editors;

using HeckLib.visualization;
using HeckLib.visualization.propgrid;
using HeckLib.visualization.objectlistview;

using HeckLibWin32;

// object list view
using BrightIdeasSoftware;






namespace FragmentLab
{
	public partial class FragmentLab : Form
	{
		#region constructor(s)
		public FragmentLab()
		{
			InitializeComponent();

			// versioning
			Text = "Fragment LAB (beta) - version " + Application.ProductVersion + "    [© richard scheltema lab]";

			// add the settings
			settings.SignalToNoise = 1.5;
			settings.Peptide = null;
			settings.Charge = 0;
			settings.MatchTolerance = new Tolerance(20, Tolerance.ErrorUnit.PPM);

			settings.Ms2SpectrumSettings = new Ms2SpectrumGraph.Settings();
			settings.Ms2SpectrumSettings.MinTopX = 1;

			settings.NumberCores = 4;
			settings.CombineSpectra = true;

			propertiesSettings.SelectedObject = settings;
			propertiesSettings.CollapseAllGridItems();
			propertiesSettings.ExpandGroup("1. Peptide / protein");

			// add event handlers
			ms2SpectrumGraph1.MouseChartPositionChanged += Ms2SpectrumGraph1_MouseChartPositionChanged;

			// create the columns for the peptide spectrum match selection
			this.PsmBrowser.SelectedIndexChanged += PsmBrowser_SelectedIndexChanged;
			this.PsmBrowser.FilterMenuBuildStrategy = new CustomFilterMenuBuilder();

			this.ChargeColumn.FilterMenuBuildStrategy = new RangeFilterMenuBuilder();
			this.ChargeColumn.AspectGetter = delegate (Object obj) {
					PeptideSpectrumMatch psm = (PeptideSpectrumMatch)obj;
					if (psm == null)
						return "Unknown";
					else
						return psm.Charge;
				};
			this.FragmentationColumn.FilterMenuBuildStrategy = new StringFilterMenuBuilder();
			this.FragmentationColumn.AspectGetter = delegate (Object obj) {
					PeptideSpectrumMatch psm = (PeptideSpectrumMatch)obj;
					if (psm == null)
						return "Unknown";
					else
						return psm.Fragmentation;
				};
			this.RetentionTimeColumn.FilterMenuBuildStrategy = new RangeFilterMenuBuilder();
			this.RetentionTimeColumn.AspectGetter = delegate (Object obj) {
					PeptideSpectrumMatch psm = (PeptideSpectrumMatch)obj;
					if (psm == null)
						return "Unknown";
					else
						return psm.RetentionTime;
				};
			this.MzColumn.FilterMenuBuildStrategy = new RangeFilterMenuBuilder();
			this.MzColumn.AspectGetter = delegate (Object obj) {
					PeptideSpectrumMatch psm = (PeptideSpectrumMatch)obj;
					if (psm == null)
						return "Unknown";
					else
						return psm.Mz;
				};
			this.RawFileColumn.FilterMenuBuildStrategy = new FilterMenuBuilder();
			this.RawFileColumn.AspectGetter = delegate (Object obj) {
					PeptideSpectrumMatch psm = (PeptideSpectrumMatch)obj;
					if (psm == null)
						return "Unknown";
					else
						return psm.RawFile;
				};
			this.MinScanColumn.FilterMenuBuildStrategy = new RangeFilterMenuBuilder();
			this.MinScanColumn.AspectGetter = delegate (Object obj) {
					PeptideSpectrumMatch psm = (PeptideSpectrumMatch)obj;
					if (psm == null)
						return "Unknown";
					else
						return psm.MinScan;
				};
			this.MaxScanColumn.FilterMenuBuildStrategy = new RangeFilterMenuBuilder();
			this.MaxScanColumn.AspectGetter = delegate (Object obj) {
					PeptideSpectrumMatch psm = (PeptideSpectrumMatch)obj;
					if (psm == null)
						return "Unknown";
					else
						return psm.MaxScan;
				};
			this.DescriptionColumn.AspectGetter = delegate (Object obj) {
					PeptideSpectrumMatch psm = (PeptideSpectrumMatch)obj;
					if (psm == null)
						return "Unknown";
					else
						return psm.Description;
				};
			this.RetentionLengthColumn.AspectGetter = delegate (Object obj) {
					PeptideSpectrumMatch psm = (PeptideSpectrumMatch)obj;
					if (psm == null)
						return "Unknown";
					else
						return psm.RetentionLength;
				};
			this.FragmentationEnergyColumn.AspectGetter = delegate (Object obj) {
					PeptideSpectrumMatch psm = (PeptideSpectrumMatch)obj;
					if (psm == null)
						return "Unknown";
					else
						return psm.FragmentationEnergy;
				};
			this.ProteinsColumn.AspectGetter = delegate (Object obj) {
					PeptideSpectrumMatch psm = (PeptideSpectrumMatch)obj;
					if (psm == null)
						return "Unknown";
					else
						return psm.Proteins;
				};
			this.PeptideColumn.FilterMenuBuildStrategy = new StringFilterMenuBuilder();
			this.PeptideColumn.AspectGetter = delegate (Object obj) {
					PeptideSpectrumMatch psm = (PeptideSpectrumMatch)obj;
					if (psm == null)
						return "Unknown";
					else
						return psm.Peptide.Sequence;
				};
			this.IntensityColumn.AspectGetter = delegate (Object obj) {
					PeptideSpectrumMatch psm = (PeptideSpectrumMatch)obj;
					if (psm == null)
						return "Unknown";
					else
						return psm.Intensity;
				};
			this.ModProbColumn.AspectGetter = delegate (Object obj) {
					PeptideSpectrumMatch psm = (PeptideSpectrumMatch)obj;
					if (psm == null)
						return "Unknown";
					else
						return psm.ModificationProbabilities;
				};
			this.ScoreColumn.FilterMenuBuildStrategy = new RangeFilterMenuBuilder();
			this.ScoreColumn.AspectGetter = delegate (Object obj) {
					PeptideSpectrumMatch psm = (PeptideSpectrumMatch)obj;
					if (psm == null)
						return -1;
					else
						return psm.Score;
				};
			this.DeltaScoreColumn.AspectGetter = delegate (Object obj) {
					PeptideSpectrumMatch psm = (PeptideSpectrumMatch)obj;
					if (psm == null)
						return "Unknown";
					else
						return psm.DeltaScore;
				};
			this.ModificationsColumn.FilterMenuBuildStrategy = new StringFilterMenuBuilder();
			this.ModificationsColumn.AspectGetter = delegate (Object obj) {
					PeptideSpectrumMatch psm = (PeptideSpectrumMatch)obj;
					if (psm == null)
						return "Unknown";
					else
						return psm.Peptide.GetUniqueModificationsString();
				};

			// collect events from elements that can alter the sequence
			this.ms2FragmentTable1.PtmScorePeptideChanged += Ms2FragmentTable1_PtmScorePeptideChanged;

			// create the settings database
			string dbfile = HeckLibSettings.CreateAppDataFilename(@"hecklib-settings.sqlite", HeckLibSettings.FolderType.RESOURCE);
			if (!File.Exists(dbfile))
			{
				HeckLib.io.database.DatabaseInformation dbinfo = new HeckLib.io.database.DatabaseInformation {
						Created			= DateTime.Now,
						FileName		= dbfile,
						HashCode		= "",
						LastAccessed	= DateTime.Now
					};
				m_pSettingsDb = new HeckLib.io.database.HeckLibSettingsDatabase(dbfile, Modification.Parse(), dbinfo);
				m_pSettingsDb.Dispose();
			}
			m_pSettingsDb = new HeckLib.io.database.HeckLibSettingsDatabase(dbfile);
			// bit of a workaround for the peptide editor
			settings.AllModifications = m_pSettingsDb.GetModifications();
		}

		public new void Dispose()
		{
			m_pSettingsDb.Dispose();
			base.Dispose();
		}
		#endregion constructor(s)


		#region events
		private void openToolStripMenuItem_Click(object sender, EventArgs e)
		{
			openToolStripMenuItem_helper_LoadData(false);
		}

		private void openIntoToolStripMenuItem_Click(object sender, EventArgs e)
		{
			openToolStripMenuItem_helper_LoadData(true);
		}

		private void openToolStripMenuItem_helper_LoadData(bool add)
		{
			OpenFileDialog dlg = new OpenFileDialog();
			dlg.Filter = "Search Result Files|*.txt;*.tsv;*.csv|PepXML|*.pepXML|mzIdentML|*.mzid|" + HeckLibRawFiles.HeckLibRawFile.FileDialogExtensions;
			dlg.FilterIndex = 1;
			dlg.Multiselect = true;

			if (dlg.ShowDialog() == DialogResult.OK)
			{
				string[] filenames = dlg.FileNames;

				Document.ImportType type = Document.ImportType.ERROR;
				if (dlg.FilterIndex == 1) type = Document.ImportType.GENERIC;
				if (dlg.FilterIndex == 2) type = Document.ImportType.PEPXML;
				if (dlg.FilterIndex == 3) type = Document.ImportType.MZIDENTML;
				if (dlg.FilterIndex == 4) type = Document.ImportType.RAW;

				if (type == Document.ImportType.ERROR)
					MessageBox.Show("Unknown import type, exiting.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

				List<PeptideSpectrumMatch> old_psms = m_pDocument != null && add == true 
						? m_pDocument.GetPsms() 
						: null;
				m_pDocument = new Document(m_pSettingsDb, dlg.FileNames, type, m_pBackgroundWorker, old_psms);

				// if available, open the spectral predictions
				if (!string.IsNullOrEmpty(settings.EncyclopeDIA))
				{
					Document.BackgroundWorkerData data = new Document.BackgroundWorkerData();
					data = new Document.BackgroundWorkerData {
							Filename	= settings.EncyclopeDIA,
							Settings	= settings,
							PsmList		= GetCurrentSetOfPsms()
						};
					m_pBackgroundWorker.TriggerBackgroundWorker(m_pDocument.LoadSpectralPredictions, data);
				}
				
				// fill the tree
				Cursor.Current = Cursors.WaitCursor;
				FillTree();
				Cursor.Current = Cursors.Default;

				// empty out the visualizationa
				ms2SpectrumGraph1.Clear();
				boxPlot1.Clear();
				ms2FragmentTable1.Clear();
			}
		}

		private void saveToolStripMenuItem_Click(object sender, EventArgs e)
		{
			SaveFileDialog dlg = new SaveFileDialog();
			dlg.Filter = "Search Result Files| *.txt";
			if (dlg.ShowDialog() != DialogResult.OK)
				return;

			Document.BackgroundWorkerData data = new Document.BackgroundWorkerData();
			data = new Document.BackgroundWorkerData {
					Filename	= dlg.FileName,
					Settings	= settings,
					PsmList		= GetCurrentSetOfPsms()
				};
			m_pBackgroundWorker.TriggerBackgroundWorker(m_pDocument.SavePsms, data);
		}

		private void FillTree()
		{
			List<PeptideSpectrumMatch> filtered_psms = m_pDocument.GetPsms();
			if (filtered_psms == null)
			{
				MessageBox.Show("No PSM entries found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}
			PsmBrowser.BeginUpdate();
			PsmBrowser.SetObjects(filtered_psms);
			PsmBrowser.EndUpdate();
			toolNrPsms.Text = "#psms: " + filtered_psms.Count;
		}

		private void propertiesSettings_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
		{
			// if available, open the spectral predictions
			if (string.IsNullOrEmpty(settings.EncyclopeDIA) || !File.Exists(settings.EncyclopeDIA))
			{
				m_pDocument.ClearSpectralPredictions();
				settings.EncyclopeDIA = "";
			}
			else
			{
				Document.BackgroundWorkerData data = new Document.BackgroundWorkerData();
				data = new Document.BackgroundWorkerData {
						Filename	= settings.EncyclopeDIA,
						Settings	= settings,
						PsmList		= GetCurrentSetOfPsms()
					};
				m_pBackgroundWorker.TriggerBackgroundWorker(m_pDocument.LoadSpectralPredictions, data);
			}

			// done, trigger repaint
			PsmBrowser_SelectedIndexChanged(null, null);
		}

		private void exportMassListsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			FolderBrowserDialog dlg = new FolderBrowserDialog();
			dlg.SelectedPath = m_pDocument.GetPsmPath();
			if (dlg.ShowDialog() != DialogResult.OK)
				return;

			// trigger the process
			Document.BackgroundWorkerData data = new Document.BackgroundWorkerData();
			data = new Document.BackgroundWorkerData {
					Filename	= dlg.SelectedPath,
					Settings	= settings,
					PsmList		= GetCurrentSetOfPsms()
				};
			m_pBackgroundWorker.TriggerBackgroundWorker(m_pDocument.ExportMassLists, data);
		}

		private void exportPeptidePropertiesToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// get a filename
			SaveFileDialog dlg = new SaveFileDialog();
			dlg.DefaultExt = "peprops";
			dlg.Filter = "Peptide property files (*.peprops)|*.peprops|All files (*.*)|*.*"; ;
			if (dlg.ShowDialog() != System.Windows.Forms.DialogResult.OK)
				return;

			Document.BackgroundWorkerData data = new Document.BackgroundWorkerData();
			data = new Document.BackgroundWorkerData {
					Filename	= dlg.FileName,
					Settings	= settings,
					PsmList		= GetCurrentSetOfPsms()
				};
			m_pBackgroundWorker.TriggerBackgroundWorker(m_pDocument.ExportPeptideProperties, data);
		}

		private void exportFragmentReportToolStripMenuItem1_Click(object sender, EventArgs e)
		{
			// locate the file ouput
			SaveFileDialog dlg = new SaveFileDialog();
			dlg.DefaultExt = ".fragreport";
			dlg.Filter = "Fragmentation Report (*.fragreport)|*.fragreport";
			if (dlg.ShowDialog() != System.Windows.Forms.DialogResult.OK)
				return;

			Document.BackgroundWorkerData data = new Document.BackgroundWorkerData();
			data = new Document.BackgroundWorkerData {
					Filename	= dlg.FileName,
					Settings	= settings,
					PsmList		= GetCurrentSetOfPsms()
				};
			m_pBackgroundWorker.TriggerBackgroundWorker(m_pDocument.ExportFragmentReport, data);
		}

		private void exportGlycoScoresToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// locate the file ouput
			SaveFileDialog dlg = new SaveFileDialog();
			dlg.DefaultExt = ".mscore";
			dlg.Filter = "Mscore (*.mscore)|*.mscore";
			if (dlg.ShowDialog() != System.Windows.Forms.DialogResult.OK)
				return;

			// trigger the process
			Document.BackgroundWorkerData data = new Document.BackgroundWorkerData();
				data = new Document.BackgroundWorkerData {
					Filename	= dlg.FileName,
					Settings	= settings,
					PsmList		= GetCurrentSetOfPsms()
				};
			m_pBackgroundWorker.TriggerBackgroundWorker(m_pDocument.ExportGlycoScores, data);
		}

		private void exportPDFToolStripMenuItem1_Click(object sender, EventArgs e)
		{
			FileDialog dlg = new SaveFileDialog();
			dlg.DefaultExt = ".pdf";
			dlg.Filter = "Adobe PDF|*.pdf";
			if (dlg.ShowDialog() != System.Windows.Forms.DialogResult.OK)
				return;

			// trigger the process
			Document.BackgroundWorkerData data = new Document.BackgroundWorkerData();
			data = new Document.BackgroundWorkerData {
					Filename	= dlg.FileName,
					Settings	= settings,
					PsmList		= GetCurrentSetOfPsms()
			};
			m_pBackgroundWorker.TriggerBackgroundWorker(CreatePDF, data);
		}

		private void exitToolStripMenuItem_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		private void exportMGFToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// locate the file ouput
			SaveFileDialog dlg = new SaveFileDialog();
			dlg.DefaultExt = ".mgf";
			dlg.Filter = "Mascot General Format (*.mgf)|*.mgf";
			if (dlg.ShowDialog() != System.Windows.Forms.DialogResult.OK)
				return;

			// trigger the process
			Document.BackgroundWorkerData data = new Document.BackgroundWorkerData();
			data = new Document.BackgroundWorkerData {
					Filename	= dlg.FileName,
					Settings	= settings,
					PsmList		= GetCurrentSetOfPsms()
				};
			m_pBackgroundWorker.TriggerBackgroundWorker(m_pDocument.ExportAsMgfFile, data);
		}

		private void exportPrositToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// locate the file ouput
			SaveFileDialog dlg = new SaveFileDialog();
			dlg.DefaultExt = ".mgf";
			dlg.Filter = "PROSIT CSV (*.csv)|*.csv";
			if (dlg.ShowDialog() != System.Windows.Forms.DialogResult.OK)
				return;

			// trigger the process
			Document.BackgroundWorkerData data = new Document.BackgroundWorkerData();
			data = new Document.BackgroundWorkerData {
					Filename	= dlg.FileName,
					Settings	= settings,
					PsmList		= GetCurrentSetOfPsms()
				};
			m_pBackgroundWorker.TriggerBackgroundWorker(m_pDocument.ExportProsit, data);
		}

		private void exportEncyclopeDIAToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// locate the file ouput
			SaveFileDialog dlg = new SaveFileDialog();
			dlg.DefaultExt = ".dlib";
			dlg.Filter = "EncyclopeDIA (*.dlib)|*.dlib";
			if (dlg.ShowDialog() != System.Windows.Forms.DialogResult.OK)
				return;

			// trigger the process
			Document.BackgroundWorkerData data = new Document.BackgroundWorkerData();
			data = new Document.BackgroundWorkerData {
					Filename	= dlg.FileName,
					Settings	= settings,
					PsmList		= GetCurrentSetOfPsms()
				};
			m_pBackgroundWorker.TriggerBackgroundWorker(m_pDocument.ExportEncyclopeDIA, data);
		}

		private void PsmBrowser_SelectedIndexChanged(object sender, EventArgs e)
		{
			PeptideSpectrumMatch psm = (PeptideSpectrumMatch)PsmBrowser.SelectedObject;
			if (psm == null)
				return;
			if (settings.Peptide != null)
			{
				psm = new PeptideSpectrumMatch(psm);
				psm.Peptide = settings.Peptide;
			}

			SetFragmentationSpectrum(psm, false);
		}

		private void Ms2FragmentTable1_PtmScorePeptideChanged(object sender, SpectrumUtils.PtmScoreInfo e)
		{
			PeptideSpectrumMatch psm = (PeptideSpectrumMatch)PsmBrowser.SelectedObject;
			if (psm == null)
				return;
			psm.Peptide = e.Peptide;

			SetFragmentationSpectrum(psm, true);
		}

		private void Ms2SpectrumGraph1_MouseChartPositionChanged(object sender, EventArgs e)
		{
			Ms2SpectrumGraph.MouseChartPositionChangedArgs args = (Ms2SpectrumGraph.MouseChartPositionChangedArgs)e;
			toolGraphPosition.Text = string.Format("m/z={0:0.00}; intensity={1:0.0e0}", args.X, args.Y);
		}

		private void frequentflyersToolStripMenuItem_Click(object sender, EventArgs e)
		{
			dialogs.DialogFrequentFlyers dlg = new dialogs.DialogFrequentFlyers(settings, m_pDocument, GetCurrentSetOfPsms());
			dlg.ShowDialog();
		}

		private void fragmentreportToolStripMenuItem_Click(object sender, EventArgs e)
		{
			dialogs.DialogFragmentReport dlg = new dialogs.DialogFragmentReport(settings, m_pDocument, m_pDocument.GetPsms());
			dlg.ShowDialog();
		}

		private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
		{
			dialogs.AboutBox dlg = new dialogs.AboutBox();
			dlg.ShowDialog();
		}

		private void modificationsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			ModificationEditor editor = new ModificationEditor(m_pSettingsDb);
			editor.ShowDialog();
			// bit of a work-around for the peptide editor
			settings.AllModifications = m_pSettingsDb.GetModifications();
		}

		private void fragmentationSettingsToolStripMenuItem_Click(object sender, EventArgs e)
		{

		}

		private void sequenceCoverageToolStripMenuItem_Click(object sender, EventArgs e)
		{
			dialogs.DialogSequenceCoverage dlg = new dialogs.DialogSequenceCoverage(settings, m_pDocument, this.GetCurrentSetOfPsms());
			dlg.ShowDialog();
		}

		private void openSearchToolStripMenuItem_Click(object sender, EventArgs e)
		{
			dialogs.DialogPeptideSearch dlg = new dialogs.DialogPeptideSearch(m_pDocument, settings, m_pSettingsDb);
			if (dlg.ShowDialog() == DialogResult.OK)
			{
				m_pDocument = new Document(m_pSettingsDb, dlg.Result, m_pDocument.GetPsmPath());

				// fill the tree
				Cursor.Current = Cursors.WaitCursor;
				FillTree();
				Cursor.Current = Cursors.Default;

				// empty out the visualizationa
				ms2SpectrumGraph1.Clear();
				boxPlot1.Clear();
				ms2FragmentTable1.Clear();
			}
		}

		private void testToolStripMenuItem_Click(object sender, EventArgs e)
		{
			ProteaseEditor dlg = new ProteaseEditor();
			dlg.ShowDialog();
		}
		#endregion events


		#region helpers
		private List<PeptideSpectrumMatch> GetCurrentSetOfPsms()
		{
			if (PsmBrowser.FilteredObjects.GetType() == new List<PeptideSpectrumMatch>().GetType())
				return (List<PeptideSpectrumMatch>)PsmBrowser.FilteredObjects;
			List<PeptideSpectrumMatch> psms = new List<PeptideSpectrumMatch>();
			foreach (var psm in PsmBrowser.FilteredObjects)
				psms.Add((PeptideSpectrumMatch)psm);
			return psms;
		}

		private void SetFragmentationSpectrum(PeptideSpectrumMatch psm, bool fragmenttable_event)
		{
			if (psm.MinScan == -1)
			{
				MessageBox.Show("Missing scannumber information; potentially due to\na match-between-runs id.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}
			
			// extract the experimental spectrum
			int[] topxranks;
			PeptideFragment.FragmentModel model;
			INoiseDistribution noise;
			ScanHeader scanheader;
			PrecursorInfo precursor;
			Centroid[] spectrum = m_pDocument.LoadSpectrum(psm, settings, out topxranks, out model, out noise, out precursor, out scanheader);
			if (spectrum == null)
				return;
			PeptideFragment[] matches = SpectrumUtils.MatchFragments(psm.Peptide, psm.Charge, spectrum, model);

			// check if spectral predictions are active
			PeptideFragment[] prediction_matches;
			Centroid[] prediction_spectrum;
			m_pDocument.GetSpectralPrediction(psm.Peptide, psm.Charge, out prediction_spectrum, out prediction_matches);

			// supply the visualization with the info
			model.tolerance = settings.MatchTolerance;
			ms2SpectrumGraph1.SetSpectrum(spectrum, topxranks, psm, model, settings.Ms2SpectrumSettings, noiseband: noise);

			if (prediction_spectrum != null)
			{
				prediction_matches = SpectrumUtils.MatchFragments(psm.Peptide, psm.Charge, prediction_spectrum, model);
				ms2SpectrumGraph1.SetMirrorSpectrum(prediction_spectrum, prediction_matches, model, settings.Ms2SpectrumSettings);
			}
			else
				ms2SpectrumGraph1.SetMirrorSpectrum(null, null, model, settings.Ms2SpectrumSettings);

			ms2FragmentTable1.SetPeptide(psm.Peptide, precursor, spectrum, topxranks, matches, model, psm.Charge, !fragmenttable_event);

			// set the intensities for the different classes of fragments
			Dictionary<int, List<double>> iontypeintensities = new Dictionary<int, List<double>>();
			iontypeintensities.Add(PeptideFragment.ION_UNASSIGNED, new List<double>());
			foreach (int iontype in PeptideFragment.ION_ALL)
				if (model.IonTypeActive(iontype)) iontypeintensities.Add(iontype, new List<double>());

			for (int i = 0; i < spectrum.Length; ++i)
			{
				if (matches[i] == null)
					iontypeintensities[PeptideFragment.ION_UNASSIGNED].Add(spectrum[i].Intensity);
				else if (iontypeintensities.ContainsKey(matches[i].FragmentType))
					iontypeintensities[matches[i].FragmentType].Add(spectrum[i].Intensity);
			}

			boxPlot1.Clear();
			foreach (int iontype in PeptideFragment.ION_ALL)
			{
				if (!iontypeintensities.ContainsKey(iontype))
					continue;
				boxPlot1.AddClass(PeptideFragment.IonToString(iontype), iontypeintensities[iontype].ToArray(), PeptideFragmentVisualUtilities.GetFragmentColor(iontype, PeptideFragment.MASSSHIFT_NONE));
			}
			boxPlot1.Invalidate();
		}
		#endregion


		#region actions
		private void CreatePDF(Document.BackgroundWorkerData data, IProgress<int> progress, CancellationToken iscancelled)
		{
			using (PDFMaker maker = new PDFMaker(data.Filename))
			{
				int PSMProgress = 0;
				int PSMPercentage = 0;

				List<PeptideSpectrumMatch> psms = GetCurrentSetOfPsms();
				psms.Sort((a, b) => a.RawFile.CompareTo(b.RawFile));
				foreach (PeptideSpectrumMatch _psm in psms)
				{
					if (iscancelled.IsCancellationRequested)
						break;

					PeptideSpectrumMatch psm = new PeptideSpectrumMatch(_psm);
					if (settings.Peptide != null)
					{
						psm = new PeptideSpectrumMatch(psm);
						psm.Peptide = settings.Peptide;
					}

					int tempProgress = (++PSMProgress * 100) / psms.Count;
					if (tempProgress > PSMPercentage)
					{
						PSMPercentage = tempProgress;
						progress.Report(PSMPercentage);
					}

					int[] topxranks;
					PeptideFragment.FragmentModel model;
					INoiseDistribution noise;
					ScanHeader scanheader;
					PrecursorInfo precursor;
					Centroid[] spectrum = m_pDocument.LoadSpectrum(psm, settings, out topxranks, out model, out noise, out precursor, out scanheader);
					if (spectrum == null)
						continue;

					currentSpectrum = new Ms2SpectrumGraph();
					currentSpectrum.Font = new System.Drawing.Font(currentSpectrum.Font.FontFamily, 25);
					model.tolerance = settings.MatchTolerance;
					currentSpectrum.SetSpectrum(spectrum, topxranks, psm, model, settings.Ms2SpectrumSettings, noiseband: noise);

					maker.InsertGDIPLUSDrawing(CreateGraphics(), currentSpectrum.PrintPaint);
				}
			}
		}
		#endregion actions


		#region data
		private HeckLibSettingsDatabase m_pSettingsDb;

		private Document m_pDocument = new Document(null);

		private FragmentLabSettings settings = new FragmentLabSettings();
		private Ms2SpectrumGraph currentSpectrum;
		private HlBackgroundWorker<Document.BackgroundWorkerData> m_pBackgroundWorker = new HlBackgroundWorker<Document.BackgroundWorkerData>();
		#endregion data
	}
}