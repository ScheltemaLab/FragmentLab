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
using System.Threading;
using System.Windows.Forms;
using System.Collections.Generic;

// HeckLib
using HeckLib;
using HeckLib.masspec;
using HeckLib.chemistry;
using HeckLib.backgroundworker;

using HeckLib.visualization;
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

			// add the settings
			settings.SignalToNoise = 1.5;
			settings.Peptide = null;
			settings.Charge = 0;
			settings.MatchTolerance = new Tolerance(20, Tolerance.ErrorUnit.PPM);

			settings.Ms2SpectrumSettings = new Ms2SpectrumGraph.Settings();
			settings.Ms2SpectrumSettings.MinTopX = 1;

			settings.CombineSpectra = true;

			propertiesSettings.SelectedObject = settings;

			// create the columns for the peptide spectrum match selection
			this.PsmBrowser.SelectedIndexChanged += PsmBrowser_SelectedIndexChanged;

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
						return psm.FragmentationEnergy;
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
			this.ScoreColumn.AspectGetter = delegate (Object obj) {
					PeptideSpectrumMatch psm = (PeptideSpectrumMatch)obj;
					if (psm == null)
						return "Unknown";
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
						return psm.Peptide.GetAnnotatedModifications();
				};
		}
		#endregion constructor(s)


		#region events
		private void openToolStripMenuItem_Click(object sender, EventArgs e)
		{
			OpenFileDialog dlg = new OpenFileDialog();
			dlg.Filter = "PD PSM Files|*.txt|MQ Evidence Files|*.txt|MQ MsmsScans Files|*.txt|Vendor RAW Files|*.raw;*.tdf";
			dlg.FilterIndex = 1;
			dlg.Multiselect = true;

			if (dlg.ShowDialog() == DialogResult.OK)
			{
				string[] filenames = dlg.FileNames;
				Document.ImportType type = Document.ImportType.ERROR;

				if (dlg.FilterIndex == 1) type = Document.ImportType.PD;
				if (dlg.FilterIndex == 2) type = Document.ImportType.MQ_EVIDENCE;
				if (dlg.FilterIndex == 3) type = Document.ImportType.MQ_MSMS;
				if (dlg.FilterIndex == 4) type = Document.ImportType.RAW;

				if (type != Document.ImportType.ERROR)
					m_pDocument = new Document(dlg.FileNames, type);

				// fill the tree
				FillTree();
			}
		}

		private void FillTree()
		{
			PsmBrowser.SetObjects(m_pDocument.GetPsms());
		}

		private void propertiesSettings_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
		{
			FillTree();
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

		private void exportFrequentFlyersToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// get a filename
			SaveFileDialog dlg = new SaveFileDialog();
			dlg.DefaultExt = "freqflyers";
			dlg.Filter = "Frequent flyers files (*.freqflyers)|*.freqflyers|All files (*.*)|*.*"; ;
			if (dlg.ShowDialog() != System.Windows.Forms.DialogResult.OK)
				return;
			Document.BackgroundWorkerData data = new Document.BackgroundWorkerData();
			data = new Document.BackgroundWorkerData {
					Filename	= dlg.FileName,
					Settings	= settings,
					PsmList		= GetCurrentSetOfPsms()
				};
			m_pBackgroundWorker.TriggerBackgroundWorker(m_pDocument.ExportFrequentFlyers, data);
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

			// trigger the process
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

		private void PsmBrowser_SelectedIndexChanged(object sender, EventArgs e)
		{
			PeptideSpectrumMatch psm = (PeptideSpectrumMatch)PsmBrowser.SelectedObject;
			if (psm == null)
				return;

			if (settings.Peptide != null)
				psm.Peptide = settings.Peptide;

			int[] topxranks;
			PeptideFragment.FragmentModel model;
			INoiseDistribution noise;
			ScanHeader scanheader;
			PrecursorInfo precursor;
			Centroid[] spectrum = m_pDocument.LoadSpectrum(psm, settings, out topxranks, out model, out noise, out precursor, out scanheader);

			PeptideFragment[] matches = SpectrumUtils.MatchFragments(psm.Peptide, psm.Charge, spectrum, model);

			model.tolerance = settings.MatchTolerance;
			ms2SpectrumGraph1.SetSpectrum(spectrum, topxranks, psm, model, settings.Ms2SpectrumSettings, noiseband: noise);

			ms2FragmentTable1.SetPeptide(psm.Peptide, spectrum, matches, model, psm.Charge);

			// set the intensities for the different classes of fragments
			Dictionary<int, List<double>> iontypeintensities = new Dictionary<int, List<double>>();
			foreach (int iontype in PeptideFragment.ION_ALL)
				if (model.IonTypeActive(iontype)) iontypeintensities.Add(iontype, new List<double>());

			for (int i = 0; i < spectrum.Length; ++i)
			{
				if (matches[i] == null)
					continue;
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

		private void extractMgfMetaInfo_Click(object sender, EventArgs e)
		{
			OpenFileDialog dlgOpen = new OpenFileDialog();
			dlgOpen.DefaultExt = "MGF";
			dlgOpen.Filter = "Mascot Generic Format (*.mgf)|*.mgf|All files (*.*)|*.*"; ;
			if (dlgOpen.ShowDialog() != System.Windows.Forms.DialogResult.OK)
				return;

			SaveFileDialog dlgSave = new SaveFileDialog();
			dlgSave.DefaultExt = "MGFMeta";
			dlgSave.Filter = "MGF meta (*.mgfmeta)|*.mgfmeta|All files (*.*)|*.*"; ;
			if (dlgSave.ShowDialog() != System.Windows.Forms.DialogResult.OK)
				return;

			Document.BackgroundWorkerData data = new Document.BackgroundWorkerData();
			data = new Document.BackgroundWorkerData {
					Filename	= dlgOpen.FileName + "\t" + dlgSave.FileName,
					Settings	= null,
					PsmList		= null
				};
			m_pBackgroundWorker.TriggerBackgroundWorker(m_pDocument.ConvertMgfToMeta, data);
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
						psm.Peptide = settings.Peptide;

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

					currentSpectrum = new Ms2SpectrumGraph();
					model.tolerance = settings.MatchTolerance;
					currentSpectrum.SetSpectrum(spectrum, topxranks, psm, model, settings.Ms2SpectrumSettings, noiseband: noise);

					maker.InsertGDIPLUSDrawing(CreateGraphics(), currentSpectrum.PrintPaint);
				}
			}
		}
		#endregion actions


		#region data
		private Document m_pDocument = new Document();
		private FragmentLabSettings settings = new FragmentLabSettings();
		private Ms2SpectrumGraph currentSpectrum;
		private HlBackgroundWorker<Document.BackgroundWorkerData> m_pBackgroundWorker = new HlBackgroundWorker<Document.BackgroundWorkerData>();
		#endregion data
	}
}