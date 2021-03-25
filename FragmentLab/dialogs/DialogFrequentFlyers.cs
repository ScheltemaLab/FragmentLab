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
using System.Drawing;
using System.Threading;

using System.Collections;
using System.Collections.Generic;

using System.Windows;
using System.Windows.Forms;

// HeckLib
using HeckLib;
using HeckLib.utils;
using HeckLib.masspec;
using HeckLib.backgroundworker;

using HeckLib.visualization;
using HeckLib.objectlistview;
using HeckLib.visualization.objectlistview;





namespace FragmentLab.dialogs
{
	public partial class DialogFrequentFlyers : Form
	{
		private class SignificantEntry
		{
			public double Mz;
			public int Frequency;
			public float Intensity;
			public string Annotation;
		}
		
		
		#region constructor(s)
		public DialogFrequentFlyers(FragmentLabSettings settings, Document document, List<PeptideSpectrumMatch> psms)
		{
			// windows related
			this.StartPosition = FormStartPosition.CenterParent;
			
			// set up the dialog
			InitializeComponent();

			// store the information
			m_pDocument = document;
			m_pSettings = settings;
			m_lPeptideSpectrumMatches = psms;

			// events
			this.Shown += DialogFrequentFlyers_Shown;

			// set up the legends for the graphs
			this.plotFreqMzDiffs.Legend.Name			= "Point types";
			this.plotFreqMzDiffs.Legend.Title			= "Point types";
			this.plotFreqMzDiffs.Legend.BorderColor		= Color.Black;
			this.plotFreqMzDiffs.Legend.BorderWidth		= 1;

			this.plotFreqMzs.Legend.Name				= "Point types";
			this.plotFreqMzs.Legend.Title				= "Point types";
			this.plotFreqMzs.Legend.BorderColor			= Color.Black;
			this.plotFreqMzs.Legend.BorderWidth			= 1;

			// add the columns to the mzdiffs list
			this.listFreqMzDiffs.FullRowSelect = true;
			this.listFreqMzDiffs.ShowGroups = false;

			this.colMzDiff_Mz.Text = "m/z";
			this.colMzDiff_Mz.FilterMenuBuildStrategy = new RangeFilterMenuBuilder();
			this.colMzDiff_Mz.AspectGetter = delegate (Object obj) {
					SignificantEntry entry = (SignificantEntry)obj;
					if (entry == null)
						return "";
					else
						return entry.Mz;
				};
			this.listFreqMzDiffs.AllColumns.Add(this.colMzDiff_Mz);

			this.colMzDiff_Frequency.Text = "frequency";
			this.colMzDiff_Frequency.FilterMenuBuildStrategy = new RangeFilterMenuBuilder();
			this.colMzDiff_Frequency.AspectGetter = delegate (Object obj) {
					SignificantEntry entry = (SignificantEntry)obj;
					if (entry == null)
						return "";
					else
						return entry.Frequency;
				};
			this.listFreqMzDiffs.AllColumns.Add(this.colMzDiff_Frequency);

			this.colMzDiff_Intensity.Text = "intensity";
			this.colMzDiff_Intensity.FilterMenuBuildStrategy = new RangeFilterMenuBuilder();
			this.colMzDiff_Intensity.AspectGetter = delegate (Object obj) {
					SignificantEntry entry = (SignificantEntry)obj;
					if (entry == null)
						return "";
					else
						return entry.Intensity;
				};
			this.listFreqMzDiffs.AllColumns.Add(this.colMzDiff_Intensity);

			this.colMzDiff_Annotation.Text = "annotation";
			this.colMzDiff_Annotation.FilterMenuBuildStrategy = new RangeFilterMenuBuilder();
			this.colMzDiff_Annotation.AspectGetter = delegate (Object obj) {
					SignificantEntry entry = (SignificantEntry)obj;
					if (entry == null)
						return "";
					else
						return entry.Annotation == null ? entry.Mz.ToString("0.#####") : entry.Annotation;
				};
			this.listFreqMzDiffs.AllColumns.Add(this.colMzDiff_Annotation);

			this.listFreqMzDiffs.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
					colMzDiff_Mz,
					colMzDiff_Frequency,
					colMzDiff_Intensity,
					colMzDiff_Annotation
				});

			// add the columns to the mzs list
			this.listFreqMzs.FullRowSelect = true;
			this.listFreqMzs.ShowGroups = false;

			this.colMz_Mz.Text = "m/z";
			this.colMz_Mz.FilterMenuBuildStrategy = new RangeFilterMenuBuilder();
			this.colMz_Mz.AspectGetter = delegate (Object obj) {
					SignificantEntry entry = (SignificantEntry)obj;
					if (entry == null)
						return "";
					else
						return entry.Mz;
				};
			this.listFreqMzDiffs.AllColumns.Add(this.colMzDiff_Frequency);

			this.colMz_Frequency.Text = "frequency";
			this.colMz_Frequency.FilterMenuBuildStrategy = new RangeFilterMenuBuilder();
			this.colMz_Frequency.AspectGetter = delegate (Object obj) {
					SignificantEntry entry = (SignificantEntry)obj;
					if (entry == null)
						return "";
					else
						return entry.Frequency;
				};
			this.listFreqMzDiffs.AllColumns.Add(this.colMz_Frequency);

			this.colMz_Intensity.Text = "intensity";
			this.colMz_Intensity.FilterMenuBuildStrategy = new RangeFilterMenuBuilder();
			this.colMz_Intensity.AspectGetter = delegate (Object obj) {
					SignificantEntry entry = (SignificantEntry)obj;
					if (entry == null)
						return "";
					else
						return entry.Intensity;
				};
			this.listFreqMzDiffs.AllColumns.Add(this.colMz_Intensity);

			this.colMz_Annotation.Text = "annotation";
			this.colMz_Annotation.FilterMenuBuildStrategy = new RangeFilterMenuBuilder();
			this.colMz_Annotation.AspectGetter = delegate (Object obj) {
					SignificantEntry entry = (SignificantEntry)obj;
					if (entry == null)
						return "";
					else
						return entry.Annotation == null ? entry.Mz.ToString("0.#####") : entry.Annotation;
				};
			this.listFreqMzDiffs.AllColumns.Add(this.colMz_Annotation);

			this.listFreqMzs.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
					colMz_Mz,
					colMz_Frequency,
					colMz_Intensity,
					colMz_Annotation
				});
		}
		#endregion


		#region events
		private void DialogFrequentFlyers_Shown(object sender, EventArgs e)
		{
			Analyze();
		}

		private void btnSaveMzDiffs_Click(object sender, EventArgs e)
		{
			SaveFileDialog dlg = new SaveFileDialog();
			dlg.DefaultExt = "freqmzdiffs";
			dlg.Filter = "Frequent mzdiffs files (*.freqmzdiffs)|*.freqmzdiffs|All files (*.*)|*.*"; ;
			if (dlg.ShowDialog() != System.Windows.Forms.DialogResult.OK)
				return;

			Document.BackgroundWorkerData data = new Document.BackgroundWorkerData();
			data = new Document.BackgroundWorkerData {
					Filename		= dlg.FileName,
					FreqFlyerData	= this.data_mzdiffs
				};
			m_pBackgroundWorker.TriggerBackgroundWorker(ExportMAnnotatedData, data);
		}

		private void btnSaveMzs_Click(object sender, EventArgs e)
		{
			SaveFileDialog dlg = new SaveFileDialog();
			dlg.DefaultExt = "freqmzs";
			dlg.Filter = "Frequent mzs files (*.freqmzs)|*.freqmzs|All files (*.*)|*.*"; ;
			if (dlg.ShowDialog() != System.Windows.Forms.DialogResult.OK)
				return;

			Document.BackgroundWorkerData data = new Document.BackgroundWorkerData();
			data = new Document.BackgroundWorkerData {
					Filename		= dlg.FileName,
					FreqFlyerData	= this.data_peaks
				};
			m_pBackgroundWorker.TriggerBackgroundWorker(ExportMAnnotatedData, data);
		}

		private void ExportMAnnotatedData(Document.BackgroundWorkerData data, IProgress<int> progress, CancellationToken iscancelled)
		{
			using (StreamWriter writer = new StreamWriter(data.Filename))
			{
				writer.WriteLine("Mz\tCount\tIntensity\tAnnotations\tSignificance");
				int processed = 0;
				for (int i = 0; i < data.FreqFlyerData.Mzs.Length; ++i)
				{
					if (iscancelled.IsCancellationRequested)
						break;
					if (data.FreqFlyerData.Frequencies[i] == 0)
						continue;

					writer.WriteLine(
							data.FreqFlyerData.Mzs[i]
							+ "\t" + 
							data.FreqFlyerData.Frequencies[i]
							+ "\t" + 
							data.FreqFlyerData.Intensities[i] 
							+ "\t" + 
							data.FreqFlyerData.Annotations[i]
							+ "\t" +
							data.FreqFlyerData.Significances[i]
						);

					// update progress
					processed++;
					progress.Report((int)(100.0 * processed / data.FreqFlyerData.Mzs.Length));
				}
			}
		}

		private void numSignificanceMzDiffs_ValueChanged(object sender, EventArgs e)
		{
			FillTables();
		}

		private void numSignificanceMzs_ValueChanged(object sender, EventArgs e)
		{
			FillTables();
		}
		#endregion


		#region local helpers
		private void Analyze()
		{
			double[] ratios = new double[] { 1,-2,-1,2,100,0,2,-1 };
			double[] intensities = new double[] { 1,2,3,4,5,6,7,8 };

			// sanity check 
			int nr_with_peptide = 0;
			foreach (PeptideSpectrumMatch psm in m_lPeptideSpectrumMatches)
			{
				if (psm.Peptide != null && psm.Peptide.Length > 0)
					nr_with_peptide++;
			}
			if (nr_with_peptide < 10)
			{
				System.Windows.MessageBox.Show("Need at least 10 annotated spectra", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}
			
			// calculate the frequent flyers
			Document.BackgroundWorkerData data = new Document.BackgroundWorkerData {
					PsmList = m_lPeptideSpectrumMatches,
					Settings = m_pSettings
				};
			m_pBackgroundWorker.TriggerBackgroundWorker(m_pDocument.CalculateFrequentFlyers, data);
			FrequentFlyers freqflyers = data.FreqFyerResult;

			data_peaks = freqflyers.InterpretPeaks();
			data_mzdiffs = freqflyers.InterpretMzDifferences();

			FillTables();
		}

		private void FillTables()
		{
			if (data_mzdiffs.Mzs.Length == 0)
				return;

			double threshold_mzdiffs = (double)numSignificanceMzDiffs.Value;
			double threshold_mzs = (double)numSignificanceMzs.Value;

			List<SignificantEntry> mzs_sign_entries = new List<SignificantEntry>();
			List<SignificantEntry> mzdiffs_sign_entries = new List<SignificantEntry>();
			Cursor.Current = Cursors.WaitCursor;
			{
				// interpret the frequent m/z diffs
				float mzdiffs_minintensity, mzdiffs_maxintensity;
				Statistics.MinMax(data_mzdiffs.Intensities, out mzdiffs_minintensity, out mzdiffs_maxintensity);

				double[] mzdiffs_freqs		= new double[data_mzdiffs.Mzs.Length];
				double[] mzdiffs			= new double[data_mzdiffs.Mzs.Length];
				int[] mzdiffs_sizes			= new int[data_mzdiffs.Mzs.Length];
				Color[] mzdiffs_colors		= new Color[data_mzdiffs.Mzs.Length];
				string[] mzdiffs_labels		= new string[data_mzdiffs.Mzs.Length];
				for (int i = 0; i < data_mzdiffs.Mzs.Length; ++i)
				{
					int size = 2;
					Color color = Color.LightGray;
					string label = null;
					if (data_mzdiffs.Significances[i] < threshold_mzdiffs)
					{
						size = (int)Math.Round(((data_mzdiffs.Intensities[i] - mzdiffs_minintensity) / mzdiffs_maxintensity) * 7);
						if (size < 3)
							size = 3;
						color = data_mzdiffs.Frequencies[i] < 0 ? Color.Blue : Color.Red;

						label = data_mzdiffs.Annotations[i];
						if (label == null)
							label = data_mzdiffs.Mzs[i].ToString("0.#####");

						// create the entry for the list
						mzdiffs_sign_entries.Add(new SignificantEntry {
								Mz				= data_mzdiffs.Mzs[i],
								Frequency		= data_mzdiffs.Frequencies[i],
								Intensity		= data_mzdiffs.Intensities[i],
								Annotation		= label
							});
					}

					mzdiffs_freqs[i]		= data_mzdiffs.Frequencies[i];
					mzdiffs[i]				= data_mzdiffs.Mzs[i];
					mzdiffs_sizes[i]		= size;
					mzdiffs_colors[i]		= color;
					mzdiffs_labels[i]		= label;
				}
				plotFreqMzDiffs.SetData("Frequent mass differences", mzdiffs, "Mass difference [Da]", mzdiffs_freqs, "Counts", mzdiffs_colors, mzdiffs_sizes, mzdiffs_labels);

				// interpret the frequent m/z's
				float mzs_minintensity, mzs_maxintensity;
				Statistics.MinMax(data_peaks.Intensities, out mzs_minintensity, out mzs_maxintensity);

				double[] mzs_freqs	= new double[data_peaks.Mzs.Length];
				double[] mzs		= new double[data_peaks.Mzs.Length];
				int[] mzs_sizes		= new int[data_peaks.Mzs.Length];
				Color[] mzs_colors	= new Color[data_peaks.Mzs.Length];
				string[] mzs_labels	= new string[data_peaks.Mzs.Length];
				for (int i = 0; i < data_peaks.Mzs.Length; ++i)
				{
					int size = 2;
					Color color = Color.LightGray;
					string label = null;
					if (data_peaks.Significances[i] < threshold_mzs)
					{
						size = (int)Math.Round(((data_peaks.Intensities[i] - mzs_minintensity) / mzs_maxintensity) * 7);
						if (size < 3)
							size = 3;
						color = Color.Red;

						label = data_peaks.Annotations[i];
						if (label == null)
							label = data_peaks.Mzs[i].ToString("0.#####");

						// create the entry for the list
						mzs_sign_entries.Add(new SignificantEntry {
								Mz				= data_peaks.Mzs[i],
								Frequency		= data_peaks.Frequencies[i],
								Intensity		= data_peaks.Intensities[i],
								Annotation		= label
							});
					}

					mzs_freqs[i]			= data_peaks.Frequencies[i];
					mzs[i]					= data_peaks.Mzs[i];
					mzs_sizes[i]			= size;
					mzs_colors[i]			= color;
					mzs_labels[i]			= label;
				}

				plotFreqMzs.SetData("Frequent m/z's", mzs, "m/z [Th]", mzs_freqs, "Counts", mzs_colors, mzs_sizes, mzs_labels);

				// set up the legends
				this.plotFreqMzDiffs.Legend.CustomItems.Clear();
				this.plotFreqMzDiffs.Legend.CustomItems.Add(Color.Gray, "ns");
				this.plotFreqMzDiffs.Legend.CustomItems.Add(Color.Red, "sign. c-term");
				this.plotFreqMzDiffs.Legend.CustomItems.Add(Color.Blue, "sign. n-term");
				this.plotFreqMzDiffs.Legend.CustomItems.Add(Color.Black, "size = intensity");

				this.plotFreqMzs.Legend.CustomItems.Clear();
				this.plotFreqMzs.Legend.CustomItems.Add(Color.Gray, "ns");
				this.plotFreqMzs.Legend.CustomItems.Add(Color.Red, "sign.");
				this.plotFreqMzs.Legend.CustomItems.Add(Color.Black, "size = intensity");

				// dump the significant entries into the lists
				this.listFreqMzs.SetObjects(mzs_sign_entries);
				this.listFreqMzDiffs.SetObjects(mzdiffs_sign_entries);
			}
			Cursor.Current = Cursors.Default;
		}
		#endregion


		#region data
		private FrequentFlyers.InterpretedData data_peaks;
		private FrequentFlyers.InterpretedData data_mzdiffs;

		private Document m_pDocument;
		private FragmentLabSettings m_pSettings;
		private List<PeptideSpectrumMatch> m_lPeptideSpectrumMatches;

		private HlBackgroundWorker<Document.BackgroundWorkerData> m_pBackgroundWorker = new HlBackgroundWorker<Document.BackgroundWorkerData>();

		private CustomOLVColumn colMzDiff_Mz = new CustomOLVColumn();
		private CustomOLVColumn colMzDiff_Frequency = new CustomOLVColumn();
		private CustomOLVColumn colMzDiff_Intensity = new CustomOLVColumn();
		private CustomOLVColumn colMzDiff_Annotation = new CustomOLVColumn();

		private CustomOLVColumn colMz_Mz = new CustomOLVColumn();
		private CustomOLVColumn colMz_Frequency = new CustomOLVColumn();
		private CustomOLVColumn colMz_Intensity = new CustomOLVColumn();
		private CustomOLVColumn colMz_Annotation = new CustomOLVColumn();
		#endregion
	}
}
