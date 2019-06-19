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
using System.ComponentModel.DataAnnotations;

// HeckLib
using HeckLib;
using HeckLib.masspec;
using HeckLib.chemistry;

using HeckLib.visualization;
using HeckLib.visualization.ui;
using HeckLib.visualization.propgrid;





namespace FragmentLab
{
	public class FragmentLabSettings
	{
		#region Peptide / protein
		[Category("1. Peptide / protein")]
		[Editor(typeof(PeptideEditor.TypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
		public Peptide Peptide { get; set; }

		[Category("1. Peptide / protein")]
		public short Charge { get; set; }
		#endregion


		#region Spectral processing
		[Category("2. Spectral processing")]
		[DisplayName("Signal to noise")]
		[Editor(typeof(ValueSliderEditor.TypeEditor), typeof(System.Drawing.Design.UITypeEditor)), ValueSlider(0, 1000, 0.01, 0.5)]
		public double SignalToNoise { get; set; }

		[Category("2. Spectral processing")]
		[DisplayName("Minimum intensity of basepeak [%]")]
		[Editor(typeof(ValueSliderEditor.TypeEditor), typeof(System.Drawing.Design.UITypeEditor)), ValueSlider(0, 100, 0.01, 0.5)]
		public double PercentOfBasepeak { get; set; }

		[Category("2. Spectral processing")]
		[DisplayName("Peak match tolerance")]
		public Tolerance MatchTolerance { get; set; }

		[Category("2. Spectral processing")]
		[DisplayName("Fragmentation")]
		public Spectrum.FragmentationType Fragmentation { get; set; }

		[Category("2. Spectral processing")]
		[DisplayName("Exclude neutral losses")]
		public bool ExcludeNeutralLosses { get; set; }

		[Category("2. Spectral processing")]
		[DisplayName("BackgroundIons")]
		[Editor(typeof(FileSelectorTypeEditor), typeof(System.Drawing.Design.UITypeEditor)), FileSelector("Mascot Generic Format Files (*.mgf)|*.mgf", true)]
		public string BackgroundIons { get; set; }
		#endregion


		#region export settings
		[Category("3. Export options")]
		[DisplayName("Combine spectra of same precursor")]
		public bool CombineSpectra { get; set; }
		#endregion


		#region Spectral visualization
		[Category("4. Spectral visualization")]
		[DisplayName("MS2 settings")]
		public Ms2SpectrumGraph.Settings Ms2SpectrumSettings { get; set; }
		#endregion
	}
}
