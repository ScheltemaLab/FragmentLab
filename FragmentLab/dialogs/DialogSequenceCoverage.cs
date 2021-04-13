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
using System.Drawing;
using System.Threading;
using System.ComponentModel;

using System.Collections;
using System.Collections.Generic;

using System.Windows;
using System.Windows.Forms;

// HeckLib
using HeckLib;
using HeckLib.masspec;
using HeckLib.chemistry;
using HeckLib.objectlistview;
using HeckLib.backgroundworker;

using HeckLib.io;
using HeckLib.io.fasta;
using HeckLib.io.database;

using HeckLib.visualization.propgrid;
using HeckLib.visualization.objectlistview;





namespace FragmentLab.dialogs
{
	public partial class DialogSequenceCoverage : Form
	{
		private class Settings
		{
			[Category("1. General")]
			[DisplayName("FASTA file")]
			[Description("The fasta file used for identifying the PSMs.")]
			[Editor(typeof(FileSelectorTypeEditor), typeof(System.Drawing.Design.UITypeEditor)), FileSelector("FASTA (*.fasta;*.fas)|*.fasta;*.fas", true)]
			public string FastaFile { get; set; }

			[Category("1. General")]
			[DisplayName("Display mode")]
			[Description("Display mode for the sequence (for multi-line the sequence is spread over multiple lines).")]
			public hecklib.graphics.controls.SequenceCoverageView.DisplayMode DisplayMode { get; set; }

			[Category("1. General")]
			[DisplayName("Adaptive spacing")]
			[Description("Adapt the spacing to the numbe of detected PSMs.")]
			public bool AdaptiveSpacing { get; set; }

			[Category("1. General")]
			[DisplayName("Include fragment information")]
			[Description("When set to true the fragment spectra are analyzed on the fly to see which amino acids where actually covered by the underlying fragmentation scans. Please note, this has a speed impact.")]
			public bool IncludeFragmentInfo { get; set; }

			[Category("1. General")]
			[DisplayName("Multi-line font")]
			[Description("When set to true the fragment spectra are analyzed on the fly to see which amino acids where actually covered by the underlying fragmentation scans. Please note, this has a speed impact.")]
			public Font MultiLineFont { get; set; }

			[Category("2. Filters")]
			[DisplayName("Raw files")]
			[EditorBrowsable(EditorBrowsableState.Never)]
			[Editor(typeof(CheckedListEditor.TypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
			[Description(".")]
			public List<Tuple<object, bool>> Rawfiles { get; set; }

			[Category("2. Filters")]
			[DisplayName("Modifications")]
			[EditorBrowsable(EditorBrowsableState.Never)]
			[Editor(typeof(CheckedListEditor.TypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
			[Description(".")]
			public List<Tuple<object, bool>> Modifications { get; set; }
		}

		private class ProteinData
		{
			public string Accession;
			public string Sequence;
			public string Description;
			public double SequenceCoverage;
			public List<PeptideSpectrumMatch> Psms;

			public bool[] CoveredByFragments;
		}


		#region constructor(s)
		public DialogSequenceCoverage(FragmentLabSettings settings, Document document, List<PeptideSpectrumMatch> psms)
		{
			// windows related
			this.StartPosition = FormStartPosition.CenterParent;
			InitializeComponent();

			// save the pointers
			m_pDocument = document;
			m_pFragmentLabSettings = settings;

			// settings
			m_pSettings.MultiLineFont = sequenceCoverageView.LabelFont;
			m_pSettings.AdaptiveSpacing = sequenceCoverageView.AdaptiveSpacing;
			propertySettings.SelectedObject = m_pSettings;
			propertySettings.PropertyValueChanged += PropertySettings_PropertyValueChanged;

			// activate the listPsms columns
			{
				this.colListPsms_accession.Text = "Accession";
				this.colListPsms_accession.Width = 100;
				this.colListPsms_accession.FilterMenuBuildStrategy = new RangeFilterMenuBuilder();
				this.colListPsms_accession.AspectGetter = delegate (Object obj) {
						ProteinData entry = (ProteinData)obj;
						if (entry == null)
							return "";
						else
							return entry.Accession;
					};
				this.listPsms.AllColumns.Add(this.colListPsms_accession);

				this.colListPsms_NumberPsms.Text = "Number PSMs";
				this.colListPsms_NumberPsms.Width = 80;
				this.colListPsms_NumberPsms.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
				this.colListPsms_NumberPsms.FilterMenuBuildStrategy = new RangeFilterMenuBuilder();
				this.colListPsms_NumberPsms.AspectGetter = delegate (Object obj) {
						ProteinData entry = (ProteinData)obj;
						if (entry == null)
							return "";
						else
							return entry.Psms.Count;
					};
				this.listPsms.AllColumns.Add(this.colListPsms_NumberPsms);

				this.colListPsms_SequenceCoverage.Text = "Sequence coverage";
				this.colListPsms_SequenceCoverage.Width = 110;
				this.colListPsms_SequenceCoverage.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
				this.colListPsms_SequenceCoverage.FilterMenuBuildStrategy = new RangeFilterMenuBuilder();
				this.colListPsms_SequenceCoverage.AspectGetter = delegate (Object obj) {
						ProteinData entry = (ProteinData)obj;
					if (entry == null)
						return "";
					else
						return entry.SequenceCoverage;
					};
				this.listPsms.AllColumns.Add(this.colListPsms_SequenceCoverage);

				this.listPsms.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
					colListPsms_accession,
					colListPsms_NumberPsms,
					colListPsms_SequenceCoverage
				});
			}

			// record the information passed along
			m_lPsms = psms;

			// locate the unique raw-files + modifications
			foreach (PeptideSpectrumMatch psm in psms)
			{
				m_hUniqueRawfiles.Add(Path.GetFileName(psm.RawFile));
				for (int i = 0; i < psm.Peptide.Length; ++i)
				{
					if (psm.Peptide.Modifications[i] != null)
						m_hUniqueModifications.Add(psm.Peptide.Modifications[i].Title);
				}
			}

			// fill up the checked lists
			List<Tuple<object, bool>> rawfiles = new List<Tuple<object, bool>>();
			foreach (string rawfile in m_hUniqueRawfiles)
				rawfiles.Add(new Tuple<object, bool>(rawfile, true));
			m_pSettings.Rawfiles = rawfiles;

			List<Tuple<object, bool>> modifications = new List<Tuple<object, bool>>();
			foreach (string mtitle in m_hUniqueModifications)
				modifications.Add(new Tuple<object, bool>(mtitle, mtitle != "Carbamidomethyl (C)"));
			m_pSettings.Modifications = modifications;
		}
		#endregion


		#region calculations
		private class Dummy : HeckLib.io.database.ISearchableSequenceDatabase, IDatabase
		{
			public void Dispose()
			{
				throw new NotImplementedException();
			}

			public DatabaseInformation GetDatabaseInformation()
			{
				throw new NotImplementedException();
			}

			public int GetIsoformFromAccession(string accession) 
			{
				throw new NotImplementedException();
			}

			public IEnumerable<int> GetIteratorModifiedPeptidesInMassRange(double minmass, double maxmass)
			{
				throw new NotImplementedException();
			}

			public DateTime GetLastAccessTimeForDb()
			{
				throw new NotImplementedException();
			}

			public Peptide GetModifiedPeptideFromId(int id)
			{
				throw new NotImplementedException();
			}

			public int[] GetModifiedPeptideIndices(double minmass = double.MinValue, double maxmass = double.MaxValue)
			{
				throw new NotImplementedException();
			}

			public int[] GetModifiedPeptideIndicesForProteinAccessions(string[] accessions)
			{
				throw new NotImplementedException();
			}

			public void GetModifiedPeptideMassRange(out double minmass, out double maxmass)
			{
				throw new NotImplementedException();
			}

			public Peptide[] GetModifiedPeptidesForSequenceTag(string sequence)
			{
				throw new NotImplementedException();
			}

			public Peptide[] GetModifiedPeptidesFromIds(int[] ids)
			{
				throw new NotImplementedException();
			}

			public Peptide[] GetModifiedPeptidesInMassRange(double minmass, double maxmass)
			{
				throw new NotImplementedException();
			}

			public int GetPeptideIdFromSequence(string sequence, bool proteinNterm, bool proteinCterm)
			{
				throw new NotImplementedException();
			}

			public void GetPeptideProteoformInfoForIds(int[] peptideids, out Dictionary<int, int[]> proteoformids, out Dictionary<int, string[]> accessions, out Dictionary<int, int[]> positions)
			{
				throw new NotImplementedException();
			}

			public Tuple<int, string, int>[] GetPositionsForPeptideId(int peptideid)
			{
				throw new NotImplementedException();
			}

			public int GetProteinFromGenename(string genename)
			{
				throw new NotImplementedException();
			}

			public int[] GetProteoformIdsForSequenceTag(string tag)
			{
				throw new NotImplementedException();
			}

			public void InsertIsoform(int id, Isoform isoform)
			{
				throw new NotImplementedException();
			}

			public void InsertModifiedPeptides(Peptide[] modpeptides)
			{
				throw new NotImplementedException();
			}

			public void InsertModifiedSequences(string[] modsequences)
			{
				throw new NotImplementedException();
			}

			public void InsertPeptide(int id, Peptide peptide)
			{
				throw new NotImplementedException();
			}

			public void InsertPeptides(Peptide[] peptides)
			{
				throw new NotImplementedException();
			}

			public void InsertPeptideToProteoform(PeptideToProteoform[] entries)
			{
				throw new NotImplementedException();
			}

			public void InsertProtein(int id, Protein protein)
			{
				throw new NotImplementedException();
			}

			public void InsertProteoform(int id, Proteoform proteoform)
			{
				throw new NotImplementedException();
			}

			public void InsertSequenceTags(Tuple<string, int>[] sequencetagToProteinid)
			{
				throw new NotImplementedException();
			}

			public bool IsComplete()
			{
				throw new NotImplementedException();
			}

			public bool IsModifiedSequenceInserted(string modsequence)
			{
				throw new NotImplementedException();
			}

			public void NotifyDatabaseComplete()
			{
				throw new NotImplementedException();
			}

			public void SetDatabaseInformation(DatabaseInformation dbinfo)
			{
				throw new NotImplementedException();
			}
		}

		private class SequenceCoverageData
		{
			public string FastaFilename;
			public List<PeptideSpectrumMatch> PsmList;
			public Dictionary<string, ProteinData> protein_mapping;
		}

		private void CalculateSequenceCoverages(SequenceCoverageData data, IProgress<int> progress, CancellationToken iscancelled)
		{
			// logically order the psms
			Dictionary<string, List<PeptideSpectrumMatch>> accession_to_psm = new Dictionary<string, List<PeptideSpectrumMatch>>();
			foreach (PeptideSpectrumMatch psm in m_lPsms)
			{
				if (psm.Peptide.ProteinAccessions == null) continue;
				foreach (string accession in psm.Peptide.ProteinAccessions)
				{
					PeptideSpectrumMatch psm_cpy = new PeptideSpectrumMatch(psm);
					if (!accession_to_psm.ContainsKey(accession))
						accession_to_psm[accession] = new List<PeptideSpectrumMatch>();
					accession_to_psm[accession].Add(psm_cpy);
				}
			}
			if (accession_to_psm.Count == 0)
			{
				System.Windows.MessageBox.Show("No protein accessions found; exiting.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}

			// create the protein data
			data.protein_mapping = new Dictionary<string, ProteinData>();
			FastaParser.Parse(m_pSettings.FastaFile, FastaParser.Format.FASTA, null, delegate (string header, string sequence, Modification nterm, Modification cterm, Modification[] modifications, double local_progress) {
					progress.Report((int)Math.Max(100*local_progress, 100));

					string accession = FastaUtils<Dummy>.RetrieveAccession(header);
					string description = FastaUtils <Dummy>.RetrieveDescription(header);
					if (data.protein_mapping.ContainsKey(accession))
						return;
					if (!accession_to_psm.ContainsKey(accession))
						return;
					data.protein_mapping.Add(accession, new ProteinData { Accession = accession, Description = description, Sequence = sequence, Psms = accession_to_psm[accession], SequenceCoverage = -1 });
				});

			foreach (ProteinData p in data.protein_mapping.Values)
			{
				// calculate the protein position where this is not known
				foreach (PeptideSpectrumMatch psm in p.Psms)
					if (psm.Peptide.Position == -1) psm.Peptide.Position = p.Sequence.IndexOf(psm.Peptide.Sequence);

				bool[][] identified_at_level;
				Modification[][] modification_at_level;
				Modification[] unique_modifications;
				int[] covered = hecklib.graphics.controls.SequenceCoverageView.GetCoverageMap(p.Sequence.Length, p.Psms.ToArray(), out identified_at_level, out modification_at_level, out unique_modifications);

				int nridentified = 0;
				for (int i = 0; i < p.Sequence.Length; ++i)
					if (covered[i] > 0) nridentified++;

				p.SequenceCoverage = Math.Round(100 * nridentified / (double)p.Sequence.Length, 2);
			}
		}

		private class FragmentCoverageData
		{
			public ProteinData Protein;
		}

		private void CalculateFragmentCoverages(FragmentCoverageData data, IProgress<int> progress, CancellationToken iscancelled)
		{
			data.Protein.CoveredByFragments = new bool[data.Protein.Sequence.Length];
			for (int i = 0; i < data.Protein.Psms.Count; ++i)
			{
				progress.Report((int)Math.Max(100, 100 * i / (double)data.Protein.Psms.Count));

				PeptideSpectrumMatch psm = data.Protein.Psms[i];

				// load the spectrum
				int[] topxranks;
				ScanHeader scanheader;
				PrecursorInfo precursor;
				INoiseDistribution noise;
				PeptideFragment.FragmentModel model;
				Centroid[] spectrum = m_pDocument.LoadSpectrum(psm, m_pFragmentLabSettings, out topxranks, out model, out noise, out precursor, out scanheader);

				// annotate the spectrum
				PeptideFragment[] matches = SpectrumUtils.MatchFragments(psm.Peptide, psm.Charge, spectrum, model);
				PeptideFragmentAnnotator annotator = new PeptideFragmentAnnotator(psm.Peptide, matches, spectrum);

				// locate the fragments
				for (int peptide_position = 0; peptide_position < psm.Peptide.Length; ++peptide_position)
				{
					if (annotator.Contains(peptide_position))
						data.Protein.CoveredByFragments[peptide_position + psm.Peptide.Position] = true;
				}
			}
			System.Diagnostics.Debug.WriteLine(data.Protein.Accession + " DONE");
		}
		#endregion


		#region events
		private void PropertySettings_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
		{
			if (!File.Exists(m_pSettings.FastaFile)) return;
			try
			{
				if (e.ChangedItem.Label == "FASTA file")
				{
					SequenceCoverageData data = new SequenceCoverageData {
							FastaFilename	= m_pSettings.FastaFile,
							PsmList			= m_lPsms
						};
					m_bgwSequenceCoverage.TriggerBackgroundWorker(CalculateSequenceCoverages, data);

					Cursor.Current = Cursors.WaitCursor;
					listPsms.Objects = data.protein_mapping.Values;
					Cursor.Current = Cursors.Default;
				}
				else if (e.ChangedItem.Label == "Display mode")
				{
					sequenceCoverageView.CurrentDisplayMode = m_pSettings.DisplayMode;
				}
				else if (e.ChangedItem.Label == "Raw files" || e.ChangedItem.Label == "Modifications")
				{
					listPsms_SelectedIndexChanged(null, null);
				}
				else if (e.ChangedItem.Label == "Include fragment information")
				{
					listPsms_SelectedIndexChanged(null, null);
				}
				else if (e.ChangedItem.Label == "Multi-line font")
				{
					sequenceCoverageView.LabelFont = m_pSettings.MultiLineFont;
					listPsms_SelectedIndexChanged(null, null);
				}
				else if (e.ChangedItem.Label == "Adaptive spacing")
				{
					sequenceCoverageView.AdaptiveSpacing = m_pSettings.AdaptiveSpacing;
					listPsms_SelectedIndexChanged(null, null);
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(ex.Message);
				System.Diagnostics.Debug.WriteLine(ex.StackTrace);
				System.Windows.MessageBox.Show("Failed to interpet the fasta file\n   '" + m_pSettings.FastaFile + "'.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void listPsms_SelectedIndexChanged(object sender, EventArgs e)
		{
			ProteinData p = (ProteinData)listPsms.SelectedObject;
			if (p == null)
				return;

			// filter out de-selected raw-files & modifications
			HashSet<string> selected_rawfiles = new HashSet<string>();
			foreach (Tuple<object, bool> rawfile in m_pSettings.Rawfiles)
				if (rawfile.Item2) selected_rawfiles.Add((string)rawfile.Item1);

			List<PeptideSpectrumMatch> psms = new List<PeptideSpectrumMatch>();
			foreach (PeptideSpectrumMatch psm in p.Psms)
				if (selected_rawfiles.Contains(Path.GetFileName(psm.RawFile))) psms.Add(psm);

			List<string> active_modifications = new List<string>();
			foreach (Tuple<object, bool> modinfo in m_pSettings.Modifications)
				if (modinfo.Item2) active_modifications.Add((string)modinfo.Item1);

			// trigger the analysis of the fragmentation spectra
			bool[] covered_by_fragments = null;
			if (m_pSettings.IncludeFragmentInfo)
			{
				if (p.CoveredByFragments == null)
				{
					FragmentCoverageData data = new FragmentCoverageData {
							Protein = p
						};
					m_bgwFragmentCoverage.TriggerBackgroundWorker(CalculateFragmentCoverages, data);
				}
				covered_by_fragments = p.CoveredByFragments;
			}

			this.sequenceCoverageView.SetData(p.Accession, p.Description, p.Sequence, psms.ToArray(), m_hUniqueModifications.ToArray(), active_modifications.ToArray(), covered_by_fragments);
		}

		private void CheckedListRawfiles_MouseUp(object sender, MouseEventArgs e)
		{
			listPsms_SelectedIndexChanged(null, null);
		}
		#endregion


		#region data
		HashSet<string> m_hUniqueRawfiles = new HashSet<string>();
		HashSet<string> m_hUniqueModifications = new HashSet<string>();

		private Document m_pDocument;
		private FragmentLabSettings m_pFragmentLabSettings;

		private Settings m_pSettings = new Settings();
		private List<PeptideSpectrumMatch> m_lPsms = new List<PeptideSpectrumMatch>();
		private HlBackgroundWorker<SequenceCoverageData> m_bgwSequenceCoverage = new HlBackgroundWorker<SequenceCoverageData>();
		private HlBackgroundWorker<FragmentCoverageData> m_bgwFragmentCoverage = new HlBackgroundWorker<FragmentCoverageData>();

		private CustomOLVColumn colListPsms_accession = new CustomOLVColumn();
		private CustomOLVColumn colListPsms_NumberPsms = new CustomOLVColumn();
		private CustomOLVColumn colListPsms_SequenceCoverage = new CustomOLVColumn();
		#endregion
	}
}
