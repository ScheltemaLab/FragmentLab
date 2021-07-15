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

using HeckLib.io;
using HeckLib.io.fileformats;

using hecklib.graphics;
using hecklib.graphics.csvimport;





namespace FragmentLab
{
	/// <summary>
	/// House of horrors where we try to convert all file formats in existence...
	/// </summary>
	[CsvFile("")]
	public class GenericFileFormat
	{
		public class ModificationException : Exception
		{
			public ModificationException()
			{
			}

			public ModificationException(string message) : base(message)
			{
			}

			public ModificationException(string message, Exception inner) : base(message, inner)
			{
			}
		}

		
		#region csv file interface
		[CsvFileField(true, "Spectrum File", "Raw File")]
		public string SpectrumFile;
		[CsvFileField(true, "Scan Number", "First Scan", "MS/MS Scan Number", "Spectrum")]
		public string ScanNumberString;
		public int ScanNumber;
		[CsvFileField(false, "RT [min]", "RT in min", "Retention Time", "Retention")]
		public float RetentionTime;
		[CsvFileField(false, "M/z [Da]", "Mz in Da", "mz", "Observed mz")]
		public double Mz;
		[CsvFileField(true, "Charge", "Charge State")]
		public short Charge;
		[CsvFileField(true, "Sequence", "Peptide Sequence", "Annotated Sequence", "Peptide")]
		public string Sequence;
		[CsvFileField(false, "Score", "Hyperscore")]
		public double Score;
		[CsvFileField(true, "Modified Sequence")]
		public string ModifiedSequence;
		[CsvFileField(true, "Modifications", "Assigned Modifications")]
		public string Modifications;
		[CsvFileField(false, "Proteins", "Protein ID", "Protein Accessions")]
		public string Proteins;
		[CsvFileField(false, "Gene names")]
		public string GeneNames;
		[CsvFileField(false, "Activation Type", "Fragmentation")]
		public string FragmentationType;
		[CsvFileField(false, "Activation Energy", "Fragmentation Energy", "Energy")]
		public string FragmentationEnergy;

		// solely used for file format detection
		[CsvFileField(false, "PeptideProphet Probability")]
		public double FragPipe = 0;
		#endregion


		#region helpers
		public enum Format
		{
			UNKNOWN,
			MAXQUANT,
			PROTEOMEDISCOVERER,
			PROFORMA,
			FRAGPIPE
		}

		public static Peptide CreatePeptide(GenericFileFormat record, Dictionary<string, Modification> allmodifications, Format format, out Format detected_format)
		{
			// attempt to resolve open cases
			int.TryParse(record.ScanNumberString, out record.ScanNumber);

			// parse the peptide
			detected_format = format;
			if (format == Format.UNKNOWN)
			{
				if (record.FragPipe != 0)
				{
					detected_format = Format.FRAGPIPE;
					return CreatePeptideFragPipe(record, allmodifications);
				}

				try
				{
					Peptide p = CreatePeptideMQ(record, allmodifications);
					detected_format = Format.MAXQUANT;
					return p;
				}
				catch (ModificationException e)
				{
					throw e;
				}
				catch (Exception)
				{
					try
					{
						Peptide p = CreatePeptidePD(record, allmodifications);
						detected_format = Format.PROTEOMEDISCOVERER;
						return p;
					}
					catch (ModificationException e)
					{
						throw e;
					}
					catch (Exception)
					{
						Peptide p = CreatePeptideProForma(record, allmodifications);
						detected_format = Format.PROFORMA;
						return p;
					}
				}
			}
			else if (format == Format.MAXQUANT)
			{
				return CreatePeptideMQ(record, allmodifications);
			}
			else if (format == Format.PROTEOMEDISCOVERER)
			{
				return CreatePeptidePD(record, allmodifications);
			}
			else if (format == Format.PROFORMA)
			{
				return CreatePeptideProForma(record, allmodifications);
			}
			else if (format == Format.FRAGPIPE)
			{
				return CreatePeptideFragPipe(record, allmodifications);
			}
			throw new Exception("Unexpected behavior with format=" + format);
		}

		private static Peptide CreatePeptideFragPipe(GenericFileFormat record, Dictionary<string, Modification> allmodifications)
		{
			// interpret the rawfilename and scannumber
			Match m = regex_fragpipe_spectrum.Match(record.ScanNumberString);
			record.Charge			= short.Parse(m.Groups["charge"].Value);
			record.SpectrumFile		= m.Groups["rawfile"].Value;
			record.ScanNumber		= int.Parse(m.Groups["firstscan"].Value);

			// interpret the peptide
			string sequence = record.Sequence.ToUpper();

			Modification nterm = null, cterm = null;
			Modification[] modifications = new Modification[sequence.Length];

			if (record.Modifications != null)
			{
				string[] modsbasic = record.Modifications.Split(',');
				foreach (string modbasic in modsbasic)
				{
					m = regex_fragpipe_peptide.Match(modbasic);

					double mass = double.Parse(m.Groups["mass"].ToString());
					string aminoacid = m.Groups["aminoacid"].ToString();

					Modification mod = new Modification {
							Title = mass.ToString(), Delta = mass, Formula = "", Description = "",
							Sites = new Modification.Site[0]
						};

					try
					{
						if (aminoacid == "N-term")
						{
							nterm = mod;
						}
						else if (aminoacid == "C-term")
						{
							cterm = mod;
						}
						else
						{
							int position = int.Parse(m.Groups["position"].ToString()) - 1;
							modifications[position] = mod;
						}
					}
					catch (Exception e)
					{
						System.Diagnostics.Debug.WriteLine(e.Message);
						System.Diagnostics.Debug.WriteLine(e.StackTrace);
					}
				}
			}

			string[] proteins = !string.IsNullOrEmpty(record.Proteins) ? new string[] { record.Proteins } : null;

			return new Peptide { Sequence = sequence, Modifications = modifications, Nterm = nterm, Cterm = cterm, ProteinAccessions = proteins };
		}

		private static Peptide CreatePeptidePD(GenericFileFormat record, Dictionary<string, Modification> allmodifications)
		{
			ModificationTranslator modtranslator = new ModificationTranslator(allmodifications);
			
			string sequence = record.Sequence.ToUpper();
			if (sequence.Contains("."))
			{
				int idx1 = sequence.IndexOf('.');
				int idx2 = sequence.LastIndexOf('.');
				sequence = sequence.Substring(idx1 + 1, idx2 - idx1 - 1);
			}

			if (record.Modifications == null)
				record.Modifications = "";

			Modification nterm = null, cterm = null;
			Modification[] modifications = new Modification[sequence.Length];
			foreach (string mascotmod in record.Modifications.Split(';'))
			{
				if (string.IsNullOrEmpty(mascotmod))
					continue;

				Modification modification;
				int position;
				char aminoacid;
				float probability;
				TranslateModificationPD(mascotmod, modtranslator, out modification, out position, out aminoacid, out probability);

				if (position >= 0) // we hit an amino acid
				{
					if (sequence[position] != aminoacid)
						throw new IOException("Error in mascot file: " + sequence[position] + " [" + sequence + " | " + record.Modifications + "]");
					modifications[position] = modification;
				}
				else // it's a terminal modification
				{
					if (modification.Terminus == Modification.TerminusType.cterm || modification.Position == Modification.PositionType.proteinCterm)
						cterm = modification;
					else if (modification.Terminus == Modification.TerminusType.nterm || modification.Position == Modification.PositionType.proteinNterm)
						nterm = modification;
				}
			}

			Peptide p = new Peptide { Sequence = sequence, Modifications = modifications, Nterm = nterm, Cterm = cterm };
			p.ProteinAccessions = record.Proteins.Split(';');
			return p;
		}

		private static void TranslateModificationPD(string mascotmod, ModificationTranslator modtranslator, out Modification modification, out int position, out char aminoacid, out float probability)
		{
			mascotmod = mascotmod.Trim();

			// std values
			probability = float.NaN;
			position = -1;
			aminoacid = '\0';
			modification = null;

			//
			if (mascotmod.IndexOf('(') - 1 < 0) // too many isoforms
				return;

			// is it an n/c terminal modification
			Modification modterm = modtranslator.GetModification(mascotmod);
			if (modterm != null)
			{
				modification = modterm;
				return;
			}

			// locate the amino acid and position
			try
			{
				aminoacid = mascotmod[0];
				position = int.Parse(mascotmod.Substring(1, mascotmod.IndexOf('(') - 1)) - 1;
			}
			catch (Exception e)
			{
				System.Diagnostics.Debug.WriteLine(e.Message);
			}
			if (position == -1)
				throw new Exception("Unknown modification '" + mascotmod + "'");

			// locate the modification
			int i1 = mascotmod.IndexOf('(') + 1;
			int i2 = mascotmod.IndexOf(')');
			while (i2 + 1 < mascotmod.Length && mascotmod[i2 + 1] == ')')
				i2++;
			string name = mascotmod.Substring(i1, i2 - i1);

			modification = modtranslator.GetModification(name);
			if (modification == null)
				throw new ModificationException("Unknown modification '" + name + "'. Please define this in the editor found in Edit -> Modifications.");
		}

		private static Peptide CreatePeptideMQ(GenericFileFormat record, Dictionary<string, Modification> allmodifications)
		{
			Regex modifiedPeptideRegex = new Regex(@".(?:\((.*?\)*)\))?");
			Regex modificationFieldRegex = new Regex(@"(?:\d+ )?(.*)"); // old which failed when amino acid is not included (i.e. 2 Oxidation, Methyl): @"(?:\d+ )?(.* \(.*?\))"

			//
			if (record.Modifications == null)
				record.Modifications = "";
			string modification = record.Modifications.Trim();
			if (string.IsNullOrEmpty(modification))
				modification = "Unmodified";

			Dictionary<string, string> reportedModifications = new Dictionary<string, string>();
			foreach (string mod in modification.Split(new char[] { ',' }))
			{
				Match m = modificationFieldRegex.Match(mod);
				if (m.Success)
				{
					string modstring = m.Groups[1].Value;
					reportedModifications[modstring] = modstring;
					reportedModifications[modstring.Substring(0, 2).ToLower()] = modstring;
				}
				else
					throw new Exception("Malformed modification string: " + modification);
			}

			List<Modification> modifications = new List<Modification>();
			foreach (Match match in modifiedPeptideRegex.Matches(record.ModifiedSequence))
			{
				if (!string.IsNullOrEmpty(match.Groups[1].Value))
					modifications.Add(TranslateModificationMQ(allmodifications, reportedModifications[match.Groups[1].Value]));
				else
					modifications.Add(null);
			}

			for (int loc = 0; loc < record.Sequence.Length; loc++)
			{
				if (record.Sequence[loc] == 'C' && modifications[loc + 1] == null)
					modifications[loc + 1] = TranslateModificationMQ(allmodifications, "Carbamidomethyl (C)");
			}

			string[] proteins = null;
			if (!string.IsNullOrEmpty(record.Proteins))
				proteins = record.Proteins.Split(';');

			Peptide peptide = new Peptide(record.Sequence, modifications[0], modifications[modifications.Count - 1], modifications.GetRange(1, modifications.Count - 2).ToArray());
			peptide.ProteinAccessions = proteins;
			return peptide;
		}

		private static Modification TranslateModificationMQ(Dictionary<string, Modification> allmodifications, string modificationString)
		{
			Modification mod = null;
			if (!modificationLUT.TryGetValue(modificationString, out mod))
				if (allmodifications.TryGetValue(modificationString, out mod))
				{
					string searchstring = modificationString.Contains(' ')
						? modificationString.Substring(0, modificationString.IndexOf(' ') - 1).ToLower()
						: modificationString.ToLower();

					List<string> candidates = new List<string>();
					foreach (string modname in allmodifications.Keys)
					{
						if (modname.ToLower().StartsWith(searchstring))
							candidates.Add(modname);
					}

					int lowestDistance = int.MaxValue;
					string closestString = modificationString;

					foreach (string candidate in candidates)
					{
						int distance = BKTree.LevenshteinDistance(candidate.ToLower(), modificationString.ToLower());
						if (distance < lowestDistance)
						{
							closestString = candidate;
							lowestDistance = distance;
						}
						mod = allmodifications[closestString];
					}
				}

			if (mod == null)
				throw new ModificationException("Unknown modification '" + modificationString + "'. Please define this in the editor found in Edit -> Modifications.");

			if (mod != null)
				modificationLUT[modificationString] = mod;
			return mod;
		}

		private static Peptide CreatePeptideProForma(GenericFileFormat record, Dictionary<string, Modification> allmodifications)
		{
			Modification nterm, cterm;
			Modification[] modifications;

			string sequence = HeckLib.io.fasta.FastaParser.ParseProForma(
						record.ModifiedSequence,
						allmodifications,
						out nterm, out cterm, out modifications
					);

			return new Peptide(sequence, nterm, cterm, modifications);
		}
		#endregion


		#region data
		private static Regex regex_fragpipe_peptide = new Regex(@"(?<position>\d+)?(?<aminoacid>.-term|\p{L})\((?<mass>\d+\.\d+)\)");
		private static Regex regex_fragpipe_spectrum = new Regex(@"(?<rawfile>.*?(?=\.)).(?<firstscan>\d+).(?<lastscan>\d+).(?<charge>\d+)");

		private static ModificationTranslator m_sModificationTranslator;
		private static Dictionary<string, Modification> modificationLUT = new Dictionary<string, Modification>();
		#endregion
	}
}
