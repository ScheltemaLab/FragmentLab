/* Copyright (C) 2015, Biomolecular Mass Spectrometry and Proteomics (http://hecklab.com)
 * This file is part of HeckLibPd.
 * 
 * HeckLibPd is free software; you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as published by
 * the Free Software Foundation; either version 2.1 of the License, or
 * (at your option) any later version.
 * 
 * HeckLibPd is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser General Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser General Public License
 * along with HeckLibPd; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
 */


// C#
using System;
using System.Text;
using System.ComponentModel;

using System.Collections;
using System.Collections.Generic;

// HeckLib
using HeckLib;
using HeckLib.masspec;
using HeckLib.chemistry;

using HeckLib.visualization;
using HeckLib.visualization.ui;
using HeckLib.visualization.propgrid;





namespace FragmentLab
{
	public class FragmentLabSettings : PeptideEditor.MinimalInfomation
	{
		#region Peptide / protein
		[Category("1. Peptide / protein")]
		[DisplayName("Peptide")]
		[Editor(typeof(PeptideEditor.TypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
		[Description("Central location for defining the peptide to use to annotate the spectra. Full definitions, including the PTMs, can be made here.")]
		public Peptide Peptide { get; set; }

		[Category("1. Peptide / protein")]
		[DisplayName("Charge state")]
		[Description("The charge of the peptide. If set, this will override the value detected in the raw-file / search results.")]
		public short Charge { get; set; }

		[Category("1. Peptide / protein")]
		[DisplayName("EncyclopeDIA database")]
		[Description("Peptide fragmentation spectrum preditions / DIA data dump.")]
		[Editor(typeof(FileSelectorTypeEditor), typeof(System.Drawing.Design.UITypeEditor)), FileSelector("EncyclopeDIA (*.dlib)|*.dlib", true)]
		public string EncyclopeDIA { get; set; }
		#endregion


		#region Spectral processing
		[Category("2. Spectral processing")]
		[DisplayName("Signal to noise")]
		[Editor(typeof(ValueSliderEditor.TypeEditor), typeof(System.Drawing.Design.UITypeEditor)), ValueSlider(0, 1000, 0.01, 0.5)]
		[Description("The minimum signel-to-noise to use when filtering peaks from the fragmentation spectra. The higher this value, the fewer peaks will survive; these are however the high(er) quality peaks.")]
		public double SignalToNoise { get; set; }

		[Category("2. Spectral processing")]
		[DisplayName("Minimum intensity of basepeak [%]")]
		[Editor(typeof(ValueSliderEditor.TypeEditor), typeof(System.Drawing.Design.UITypeEditor)), ValueSlider(0, 100, 0.01, 0.5)]
		[Description("The minimum intensity to use when filtering peaks from the fragmentation spectra. The higher this value, the fewer peaks will survive; these are however the high(er) quality peaks.")]
		public double PercentOfBasepeak { get; set; }

		[Category("2. Spectral processing")]
		[DisplayName("Peak match tolerance")]
		[Description("The tolerance to use when matching theoretical masses to the recorded masses.")]
		public Tolerance MatchTolerance { get; set; }

		[Category("2. Spectral processing")]
		[DisplayName("Fragmentation")]
		[Description("The fragmentation mode of the scan. If set, this will override the value detected in the raw-file / search results.")]
		public Spectrum.FragmentationType Fragmentation { get; set; }

		[Category("2. Spectral processing")]
		[DisplayName("Exclude neutral losses")]
		[Description("When set to true, commonly occursing neutral losses are removed from the spectrum.")]
		public bool ExcludeNeutralLosses { get; set; }

		[Category("2. Spectral processing")]
		[DisplayName("BackgroundIons")]
		[Editor(typeof(FileSelectorTypeEditor), typeof(System.Drawing.Design.UITypeEditor)), FileSelector("Mascot Generic Format Files (*.mgf)|*.mgf", true)]
		[Description("MGF file containing background ions that should be removed from the spectrum.")]
		public string BackgroundIons { get; set; }
		#endregion


		#region export settings
		[Category("3. Export options")]
		[DisplayName("Combine spectra of same precursor")]
		[Description("When set to true, spectra from the same precursor will be combined into a single spectrum for improved spectral quality.")]
		public bool CombineSpectra { get; set; }

		[Category("3. Export options")]
		[DisplayName("Number of cores")]
		[Description("The number of processor cores to use during export to speed up the process.")]
		public int NumberCores { get; set; }
		#endregion


		#region Spectral visualization
		[Category("4. Spectral visualization")]
		[DisplayName("MS2 settings")]
		[Description("General settings for fragmentation spectrum visualization / annotation.")]
		public Ms2SpectrumGraph.Settings Ms2SpectrumSettings { get; set; }
		#endregion
	}
}
