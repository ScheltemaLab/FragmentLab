namespace FragmentLab.dialogs
{
	partial class DialogPeptideSearch
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.panel1 = new System.Windows.Forms.Panel();
			this.numMinScore = new System.Windows.Forms.NumericUpDown();
			this.lblMinScore = new System.Windows.Forms.Label();
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.tabPeptide = new System.Windows.Forms.TabPage();
			//this.ctrlPeptideEditor = new HeckLib.graphics.controls.PeptideEditorControl();
			this.tabProtein = new System.Windows.Forms.TabPage();
			this.editorType = new System.Windows.Forms.ComboBox();
			this.editorModification = new System.Windows.Forms.ComboBox();
			this.btnProteinRmMod = new System.Windows.Forms.Button();
			this.btnProteinAddMod = new System.Windows.Forms.Button();
			this.lblProteinModifications = new System.Windows.Forms.Label();
			this.lblProteinProtease = new System.Windows.Forms.Label();
			this.cmbProteinProtease = new System.Windows.Forms.ComboBox();
			this.listProteinModifications = new HeckLib.visualization.ui.ListViewEx();
			this.colModification = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.colType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.txtProteinSequence = new System.Windows.Forms.TextBox();
			this.lblProteinSequence = new System.Windows.Forms.Label();
			this.btnOk = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.lblParseFullFile = new System.Windows.Forms.Label();
			this.chkParseFullFiles = new System.Windows.Forms.CheckBox();
			this.panel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numMinScore)).BeginInit();
			this.tabControl1.SuspendLayout();
			this.tabPeptide.SuspendLayout();
			this.tabProtein.SuspendLayout();
			this.SuspendLayout();
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.chkParseFullFiles);
			this.panel1.Controls.Add(this.lblParseFullFile);
			this.panel1.Controls.Add(this.numMinScore);
			this.panel1.Controls.Add(this.lblMinScore);
			this.panel1.Controls.Add(this.tabControl1);
			this.panel1.Controls.Add(this.btnOk);
			this.panel1.Controls.Add(this.btnCancel);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel1.Location = new System.Drawing.Point(0, 0);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(347, 326);
			this.panel1.TabIndex = 0;
			// 
			// numMinScore
			// 
			this.numMinScore.DecimalPlaces = 1;
			this.numMinScore.Location = new System.Drawing.Point(91, 247);
			this.numMinScore.Name = "numMinScore";
			this.numMinScore.Size = new System.Drawing.Size(64, 20);
			this.numMinScore.TabIndex = 6;
			this.numMinScore.Value = new decimal(new int[] {
            25,
            0,
            0,
            0});
			// 
			// lblMinScore
			// 
			this.lblMinScore.AutoSize = true;
			this.lblMinScore.Location = new System.Drawing.Point(4, 250);
			this.lblMinScore.Name = "lblMinScore";
			this.lblMinScore.Size = new System.Drawing.Size(53, 13);
			this.lblMinScore.TabIndex = 5;
			this.lblMinScore.Text = "Min score";
			// 
			// tabControl1
			// 
			this.tabControl1.Controls.Add(this.tabPeptide);
			this.tabControl1.Controls.Add(this.tabProtein);
			this.tabControl1.Location = new System.Drawing.Point(0, 0);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(347, 239);
			this.tabControl1.TabIndex = 4;
			// 
			// tabPeptide
			// 
			this.tabPeptide.Controls.Add(this.ctrlPeptideEditor);
			this.tabPeptide.Location = new System.Drawing.Point(4, 22);
			this.tabPeptide.Name = "tabPeptide";
			this.tabPeptide.Padding = new System.Windows.Forms.Padding(3);
			this.tabPeptide.Size = new System.Drawing.Size(339, 213);
			this.tabPeptide.TabIndex = 0;
			this.tabPeptide.Text = "Peptide";
			this.tabPeptide.UseVisualStyleBackColor = true;
			// 
			// ctrlPeptideEditor
			// 
			this.ctrlPeptideEditor.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ctrlPeptideEditor.Location = new System.Drawing.Point(3, 3);
			this.ctrlPeptideEditor.Name = "ctrlPeptideEditor";
			this.ctrlPeptideEditor.Size = new System.Drawing.Size(333, 207);
			this.ctrlPeptideEditor.TabIndex = 1;
			// 
			// tabProtein
			// 
			this.tabProtein.Controls.Add(this.editorType);
			this.tabProtein.Controls.Add(this.editorModification);
			this.tabProtein.Controls.Add(this.btnProteinRmMod);
			this.tabProtein.Controls.Add(this.btnProteinAddMod);
			this.tabProtein.Controls.Add(this.lblProteinModifications);
			this.tabProtein.Controls.Add(this.lblProteinProtease);
			this.tabProtein.Controls.Add(this.cmbProteinProtease);
			this.tabProtein.Controls.Add(this.listProteinModifications);
			this.tabProtein.Controls.Add(this.txtProteinSequence);
			this.tabProtein.Controls.Add(this.lblProteinSequence);
			this.tabProtein.Location = new System.Drawing.Point(4, 22);
			this.tabProtein.Name = "tabProtein";
			this.tabProtein.Padding = new System.Windows.Forms.Padding(3);
			this.tabProtein.Size = new System.Drawing.Size(339, 213);
			this.tabProtein.TabIndex = 1;
			this.tabProtein.Text = "Protein";
			this.tabProtein.UseVisualStyleBackColor = true;
			// 
			// editorType
			// 
			this.editorType.FormattingEnabled = true;
			this.editorType.Location = new System.Drawing.Point(97, 148);
			this.editorType.Name = "editorType";
			this.editorType.Size = new System.Drawing.Size(121, 21);
			this.editorType.TabIndex = 9;
			this.editorType.Visible = false;
			// 
			// editorModification
			// 
			this.editorModification.FormattingEnabled = true;
			this.editorModification.Location = new System.Drawing.Point(97, 121);
			this.editorModification.Name = "editorModification";
			this.editorModification.Size = new System.Drawing.Size(121, 21);
			this.editorModification.TabIndex = 8;
			this.editorModification.Visible = false;
			// 
			// btnProteinRmMod
			// 
			this.btnProteinRmMod.Location = new System.Drawing.Point(305, 117);
			this.btnProteinRmMod.Name = "btnProteinRmMod";
			this.btnProteinRmMod.Size = new System.Drawing.Size(26, 26);
			this.btnProteinRmMod.TabIndex = 7;
			this.btnProteinRmMod.Text = "-";
			this.btnProteinRmMod.UseVisualStyleBackColor = true;
			this.btnProteinRmMod.Click += new System.EventHandler(this.btnProteinRmMod_Click);
			// 
			// btnProteinAddMod
			// 
			this.btnProteinAddMod.Location = new System.Drawing.Point(305, 85);
			this.btnProteinAddMod.Name = "btnProteinAddMod";
			this.btnProteinAddMod.Size = new System.Drawing.Size(26, 26);
			this.btnProteinAddMod.TabIndex = 6;
			this.btnProteinAddMod.Text = "+";
			this.btnProteinAddMod.UseVisualStyleBackColor = true;
			this.btnProteinAddMod.Click += new System.EventHandler(this.btnProteinAddMod_Click);
			// 
			// lblProteinModifications
			// 
			this.lblProteinModifications.AutoSize = true;
			this.lblProteinModifications.Location = new System.Drawing.Point(8, 85);
			this.lblProteinModifications.Name = "lblProteinModifications";
			this.lblProteinModifications.Size = new System.Drawing.Size(69, 13);
			this.lblProteinModifications.TabIndex = 5;
			this.lblProteinModifications.Text = "Modifications";
			// 
			// lblProteinProtease
			// 
			this.lblProteinProtease.AutoSize = true;
			this.lblProteinProtease.Location = new System.Drawing.Point(8, 51);
			this.lblProteinProtease.Name = "lblProteinProtease";
			this.lblProteinProtease.Size = new System.Drawing.Size(49, 13);
			this.lblProteinProtease.TabIndex = 4;
			this.lblProteinProtease.Text = "Protease";
			// 
			// cmbProteinProtease
			// 
			this.cmbProteinProtease.FormattingEnabled = true;
			this.cmbProteinProtease.Location = new System.Drawing.Point(85, 46);
			this.cmbProteinProtease.Name = "cmbProteinProtease";
			this.cmbProteinProtease.Size = new System.Drawing.Size(246, 21);
			this.cmbProteinProtease.TabIndex = 3;
			// 
			// listProteinModifications
			// 
			this.listProteinModifications.AllowColumnReorder = true;
			this.listProteinModifications.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colModification,
            this.colType});
			this.listProteinModifications.DoubleClickActivation = false;
			this.listProteinModifications.FullRowSelect = true;
			this.listProteinModifications.HideSelection = false;
			this.listProteinModifications.Location = new System.Drawing.Point(85, 85);
			this.listProteinModifications.Name = "listProteinModifications";
			this.listProteinModifications.Size = new System.Drawing.Size(214, 122);
			this.listProteinModifications.TabIndex = 2;
			this.listProteinModifications.UseCompatibleStateImageBehavior = false;
			this.listProteinModifications.View = System.Windows.Forms.View.Details;
			// 
			// colModification
			// 
			this.colModification.Text = "Modification";
			this.colModification.Width = 132;
			// 
			// colType
			// 
			this.colType.Text = "Type";
			this.colType.Width = 77;
			// 
			// txtProteinSequence
			// 
			this.txtProteinSequence.Location = new System.Drawing.Point(85, 19);
			this.txtProteinSequence.Name = "txtProteinSequence";
			this.txtProteinSequence.Size = new System.Drawing.Size(246, 20);
			this.txtProteinSequence.TabIndex = 1;
			// 
			// lblProteinSequence
			// 
			this.lblProteinSequence.AutoSize = true;
			this.lblProteinSequence.Location = new System.Drawing.Point(8, 22);
			this.lblProteinSequence.Name = "lblProteinSequence";
			this.lblProteinSequence.Size = new System.Drawing.Size(56, 13);
			this.lblProteinSequence.TabIndex = 0;
			this.lblProteinSequence.Text = "Sequence";
			// 
			// btnOk
			// 
			this.btnOk.Location = new System.Drawing.Point(187, 291);
			this.btnOk.Name = "btnOk";
			this.btnOk.Size = new System.Drawing.Size(75, 23);
			this.btnOk.TabIndex = 2;
			this.btnOk.Text = "&OK";
			this.btnOk.UseVisualStyleBackColor = true;
			this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
			// 
			// btnCancel
			// 
			this.btnCancel.Location = new System.Drawing.Point(268, 291);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(75, 23);
			this.btnCancel.TabIndex = 3;
			this.btnCancel.Text = "&Cancel";
			this.btnCancel.UseVisualStyleBackColor = true;
			this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
			// 
			// lblParseFullFile
			// 
			this.lblParseFullFile.AutoSize = true;
			this.lblParseFullFile.Location = new System.Drawing.Point(4, 275);
			this.lblParseFullFile.Name = "lblParseFullFile";
			this.lblParseFullFile.Size = new System.Drawing.Size(77, 13);
			this.lblParseFullFile.TabIndex = 7;
			this.lblParseFullFile.Text = "Parse full file(s)";
			// 
			// chkParseFullFiles
			// 
			this.chkParseFullFiles.AutoSize = true;
			this.chkParseFullFiles.Location = new System.Drawing.Point(91, 275);
			this.chkParseFullFiles.Name = "chkParseFullFiles";
			this.chkParseFullFiles.Size = new System.Drawing.Size(15, 14);
			this.chkParseFullFiles.TabIndex = 8;
			this.chkParseFullFiles.UseVisualStyleBackColor = true;
			// 
			// DialogPeptideSearch
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(347, 326);
			this.Controls.Add(this.panel1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "DialogPeptideSearch";
			this.Text = "Open search";
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.numMinScore)).EndInit();
			this.tabControl1.ResumeLayout(false);
			this.tabPeptide.ResumeLayout(false);
			this.tabProtein.ResumeLayout(false);
			this.tabProtein.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Button btnOk;
		private System.Windows.Forms.Button btnCancel;
		private HeckLib.graphics.controls.PeptideEditorControl ctrlPeptideEditor;
		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage tabPeptide;
		private System.Windows.Forms.TabPage tabProtein;
		private System.Windows.Forms.Label lblProteinProtease;
		private System.Windows.Forms.ComboBox cmbProteinProtease;
		private HeckLib.visualization.ui.ListViewEx listProteinModifications;
		private System.Windows.Forms.TextBox txtProteinSequence;
		private System.Windows.Forms.Label lblProteinSequence;
		private System.Windows.Forms.Label lblProteinModifications;
		private System.Windows.Forms.Button btnProteinRmMod;
		private System.Windows.Forms.Button btnProteinAddMod;
		private System.Windows.Forms.ColumnHeader colModification;
		private System.Windows.Forms.ColumnHeader colType;
		private System.Windows.Forms.ComboBox editorType;
		private System.Windows.Forms.ComboBox editorModification;
		private System.Windows.Forms.NumericUpDown numMinScore;
		private System.Windows.Forms.Label lblMinScore;
		private System.Windows.Forms.CheckBox chkParseFullFiles;
		private System.Windows.Forms.Label lblParseFullFile;
	}
}