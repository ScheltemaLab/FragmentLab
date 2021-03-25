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

using HeckLib.objectlistview;
using HeckLib.visualization.objectlistview;

// LiveCharts
using LiveCharts;
using LiveCharts.Wpf;
using LiveCharts.Defaults;





namespace FragmentLab.dialogs
{
	public partial class DialogFragmentReport : Form
	{
		#region constructor(s) / destructor
		public DialogFragmentReport(FragmentLabSettings settings, Document document, List<PeptideSpectrumMatch> psms)
		{
			InitializeComponent();

			// store the information
			m_pDocument = document;
			m_pSettings = settings;

			m_dRawfilesToPsms = new Dictionary<string, List<PeptideSpectrumMatch>>();
			foreach (PeptideSpectrumMatch psm in psms)
			{
				string rawfile = Path.GetFileName(psm.RawFile);
				if (!m_dRawfilesToPsms.ContainsKey(rawfile))
					m_dRawfilesToPsms[rawfile] = new List<PeptideSpectrumMatch>();
				m_dRawfilesToPsms[rawfile].Add(psm);
			}

			// windows related
			this.StartPosition = FormStartPosition.CenterParent;

			// events
			this.Shown += DialogFragmentReport_Shown;

			// setup the graphs
			chartSequenceCoverage.Series.Clear();

			// add the columns to the raw-file list
			this.listRawFiles.FullRowSelect = true;
			this.listRawFiles.ShowGroups = false;

			this.colRawFileName.Text = "Filename";
			this.colRawFileName.Width = 350;
			this.colRawFileName.FilterMenuBuildStrategy = new RangeFilterMenuBuilder();
			this.colRawFileName.AspectGetter = delegate (Object obj) {
					FragmentStatistics stats = (FragmentStatistics)obj;
					if (stats == null)
						return "";
					else
						return stats.RawFile;
				};
			this.listRawFiles.AllColumns.Add(this.colRawFileName);

			this.colRawFilePsms.Text = "Number PSMs";
			this.colRawFilePsms.Width = 100;
			this.colRawFilePsms.FilterMenuBuildStrategy = new RangeFilterMenuBuilder();
			this.colRawFilePsms.AspectGetter = delegate (Object obj) {
					FragmentStatistics stats = (FragmentStatistics)obj;
					if (stats == null)
						return "";
					else
						return stats.NumberPsms;
				};
			this.listRawFiles.AllColumns.Add(this.colRawFilePsms);

			this.listRawFiles.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
					colRawFileName,
					colRawFilePsms,
				});
		}

		private CustomOLVColumn colRawFileName = new CustomOLVColumn();
		private CustomOLVColumn colRawFilePsms = new CustomOLVColumn();
		#endregion


		#region events
		private void DialogFragmentReport_Shown(object sender, EventArgs e)
		{
			UpdateStatistics();
		}
		#endregion


		#region local helpers
		private void UpdateStatistics()
		{
			// calculate the statistics
			Dictionary<string, FragmentStatistics> data = new Dictionary<string, FragmentStatistics>();
			m_pBackgroundWorker.TriggerBackgroundWorker(CalculateStatistics, data);

			// dump the raw-file info in the table
			listRawFiles.Objects = data.Values;

			// update the graphs
			foreach (string rawfile in data.Keys)
			{
				FillSequenceCoveragePlot(data, rawfile);
				FillIntensityPlot(data, rawfile);
				FillModifications(data, rawfile);
			}
		}

		private void FillModifications(Dictionary<string, FragmentStatistics> allstats, string rawfile)
		{
			Func<ChartPoint, string> labelPoint = chartPoint =>
				string.Format("{0} ({1:P})", chartPoint.Y, chartPoint.Participation);

			SeriesCollection series = new SeriesCollection();
			foreach (string modstring in allstats[rawfile].Modifications.Keys)
			{
				series.Add(new PieSeries {
						Title = modstring,
						Values = new ChartValues<double> { allstats[rawfile].Modifications[modstring] },
						DataLabels = true,
						LabelPoint = labelPoint
					});
			}
			chartModifications.Series = series;
			chartModifications.LegendLocation = LegendLocation.Right;
		}

		private void FillIntensityPlot(Dictionary<string, FragmentStatistics> allstats, string rawfile)
		{
			FragmentStatistics stats = allstats[allstats.Keys.ToArray()[0]];

			// create the series
			ChartValues<OhlcPoint> vals_boxplot = new ChartValues<OhlcPoint>();
			vals_boxplot.Add(CreateBoxPlotEntry(stats.FragmentationEfficiency));
			vals_boxplot.Add(CreateBoxPlotEntry(stats.AssignedIonLoad));

			chartIntensities.Series.Add(new CandleSeries { Values = vals_boxplot, Title = rawfile });

			// set up the title
			chartIntensities.Name = "Intensity";

			// set up the axes
			chartIntensities.AxisY.Clear();
			chartIntensities.AxisY.Add(new Axis {
					FontSize = 15,
					Title = "Intensity [%]",
					MinValue = 0,
					MaxValue = 100
				});

			chartIntensities.AxisX.Clear();
			chartIntensities.AxisX.Add(new Axis {
					FontSize = 15,
					Labels = new[] {
						"Fragmentation efficiency",
						"Assigned ion load"
					}
			});
		}

		private void FillSequenceCoveragePlot(Dictionary<string, FragmentStatistics> allstats, string rawfile)
		{
			FragmentStatistics stats = allstats[rawfile];

			// create the series
			ChartValues<OhlcPoint> vals_boxplot = new ChartValues<OhlcPoint>();
			vals_boxplot.Add(CreateBoxPlotEntry(stats.SequenceCoverage));
			vals_boxplot.Add(CreateBoxPlotEntry(stats.SequenceCoverageNterm));
			vals_boxplot.Add(CreateBoxPlotEntry(stats.SequenceCoverageCterm));

			chartSequenceCoverage.Series.Add(new CandleSeries { Values = vals_boxplot, Title = rawfile });

			// set up the title
			chartSequenceCoverage.Name = "Sequence coverage";

			// set up the axes
			chartSequenceCoverage.AxisY.Clear();
			chartSequenceCoverage.AxisY.Add(new Axis {
					FontSize = 15,
					Title = "Sequence Coverage [%]",
					MinValue = 0,
					MaxValue = 100
				});

			chartSequenceCoverage.AxisX.Clear();
			chartSequenceCoverage.AxisX.Add(new Axis {
					FontSize = 15,
					Labels = new[] {
						"All",
						"N-term",
						"C-term",
						"Fragmentation efficiency"
					}
				});
		}

		private OhlcPoint CreateBoxPlotEntry(double[] values)
		{
			double[] boxplot = BoxPlot(values);
			return new OhlcPoint (
					100 * boxplot[BOXPLOT_LOWER_QUART],
					100 * boxplot[BOXPLOT_95PERCENT],
					100 * boxplot[BOXPLOT_5PERCENT],
					100 * boxplot[BOXPLOT_UPPER_QUART]
				);
		}
		#endregion


		#region to be transferred to Statistics.cs
		public static readonly int BOXPLOT_5PERCENT = 0;
		public static readonly int BOXPLOT_LOWER_QUART = 1;
		public static readonly int BOXPLOT_MEDIAN = 2;
		public static readonly int BOXPLOT_UPPER_QUART = 3;
		public static readonly int BOXPLOT_95PERCENT = 4;

		public static double[] BoxPlot(double[] values)
		{
			// sort the values
			Double[] sorted = (Double[])values.Clone();
			Array.Sort(sorted);

			// race conditions
			if (values.Length == 0 || values.Length == 1)
				return null;

			double[] boxplot = new double[5];
			if (values.Length == 2)
			{
				boxplot[BOXPLOT_5PERCENT] = sorted[0];
				boxplot[BOXPLOT_LOWER_QUART] = sorted[0];
				boxplot[BOXPLOT_MEDIAN] = (sorted[0] + sorted[1]) / 2.0f;
				boxplot[BOXPLOT_UPPER_QUART] = sorted[1];
				boxplot[BOXPLOT_95PERCENT] = sorted[1];
				return boxplot;
			}

			// calculate the quantiles on the sorted values
			boxplot[BOXPLOT_5PERCENT] = sorted[(int)Math.Ceiling(0.05 * values.Length)];
			boxplot[BOXPLOT_LOWER_QUART] = sorted[(int)Math.Round(0.25f * sorted.Length)];
			if (sorted.Length % 2 != 0)
				boxplot[BOXPLOT_MEDIAN] = sorted[(int)Math.Round(sorted.Length / 2.0f)];
			else
				boxplot[BOXPLOT_MEDIAN] = (sorted[(int)Math.Round(sorted.Length / 2.0f) - 1] + sorted[(int)Math.Round(sorted.Length / 2.0f)]) / 2.0f;
			boxplot[BOXPLOT_UPPER_QUART] = sorted[(int)Math.Round(0.75f * sorted.Length)];
			boxplot[BOXPLOT_95PERCENT] = sorted[(int)Math.Floor(0.95 * values.Length)];

			return boxplot;
		}
		#endregion


		#region calculations
		private class FragmentStatistics
		{
			public FragmentStatistics(string rawfile, int nrpsms)
			{
				RawFile						= rawfile;
				NumberPsms					= nrpsms;

				SequenceCoverage			= new double[nrpsms];
				SequenceCoverageNterm		= new double[nrpsms];
				SequenceCoverageCterm		= new double[nrpsms];
				FragmentationEfficiency		= new double[nrpsms];
				AssignedIonLoad				= new double[nrpsms];

				Modifications				= new Dictionary<string, int>();
			}

			public string RawFile;
			public int NumberPsms;

			public double[] SequenceCoverage;
			public double[] SequenceCoverageNterm;
			public double[] SequenceCoverageCterm;
			public double[] FragmentationEfficiency;
			public double[] AssignedIonLoad;

			public Dictionary<string, int> Modifications;
		}

		private void CalculateStatistics(Dictionary<string, FragmentStatistics> data, IProgress<int> progress, CancellationToken iscancelled)
		{
			List<FragmentStatistics> allstats = new List<FragmentStatistics>();
			foreach (string rawfile in m_dRawfilesToPsms.Keys)
			{
				List<PeptideSpectrumMatch> psms = m_dRawfilesToPsms[rawfile];

				// create our stats object and process the data
				FragmentStatistics stats = new FragmentStatistics(rawfile, psms.Count);
				allstats.Add(stats);

				// process all the 
				for (int i = 0; i < psms.Count; ++i)
				{
					PeptideSpectrumMatch psm = psms[i];
					if (iscancelled.IsCancellationRequested)
						return;

					// load the spectrum
					int[] topxranks;
					PeptideFragment.FragmentModel model;
					INoiseDistribution noise;
					PrecursorInfo precursor;
					ScanHeader scanheader;
					Centroid[] spectrum = m_pDocument.LoadSpectrum(psm, m_pSettings, out topxranks, out model, out noise, out precursor, out scanheader);
					if (spectrum == null)
						continue;
					model.tolerance = m_pSettings.MatchTolerance;

					// create the fragments
					short charge = m_pSettings.Charge == 0 ? psm.Charge : m_pSettings.Charge;
					Peptide peptide = m_pSettings.Peptide != null ? m_pSettings.Peptide : psm.Peptide;
					PeptideFragment[] fragments = SpectrumUtils.MatchFragments(peptide, charge, spectrum, model);
					PeptideFragmentAnnotator annotator = new PeptideFragmentAnnotator(psm.Peptide, fragments, spectrum);

					// collect the stats
					stats.SequenceCoverage[i]			= annotator.SequenceCoverage;
					stats.SequenceCoverageNterm[i]		= annotator.SequenceCoverageNterm;
					stats.SequenceCoverageCterm[i]		= annotator.SequenceCoverageCterm;
					stats.FragmentationEfficiency[i]	= annotator.FragmentationEfficiency;
					stats.AssignedIonLoad[i]			= annotator.AssignedIonLoad;

					// analyze the modifications
					string modstring = psm.Peptide.GetModificationsString();
					if (!stats.Modifications.ContainsKey(modstring))
						stats.Modifications.Add(modstring, 0);
					stats.Modifications[modstring] += 1;
				}
			}

			foreach (FragmentStatistics stats in allstats)
				data.Add(stats.RawFile, stats);
		}
		#endregion


		#region data
		private Document m_pDocument;
		private FragmentLabSettings m_pSettings;
		private Dictionary<string, List<PeptideSpectrumMatch>> m_dRawfilesToPsms;

		private HlBackgroundWorker<Dictionary<string, FragmentStatistics>> m_pBackgroundWorker = new HlBackgroundWorker<Dictionary<string, FragmentStatistics>>();
		#endregion
	}
}
