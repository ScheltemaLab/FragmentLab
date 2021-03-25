namespace FragmentLab.dialogs
{
	partial class DialogFrequentFlyers
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DialogFrequentFlyers));
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
			this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
			this.listFreqMzDiffs = new BrightIdeasSoftware.ObjectListView();
			this.listFreqMzs = new BrightIdeasSoftware.ObjectListView();
			this.panel1 = new System.Windows.Forms.Panel();
			this.btnSaveMzDiffs = new System.Windows.Forms.Button();
			this.lblTableFrequentMzDiffs = new System.Windows.Forms.Label();
			this.panel2 = new System.Windows.Forms.Panel();
			this.btnSaveMzs = new System.Windows.Forms.Button();
			this.lblFrequentMzs = new System.Windows.Forms.Label();
			this.grpSettings = new System.Windows.Forms.GroupBox();
			this.numSignificanceMzs = new System.Windows.Forms.NumericUpDown();
			this.lblSignificanceMzs = new System.Windows.Forms.Label();
			this.numSignificanceMzDiffs = new System.Windows.Forms.NumericUpDown();
			this.lblSignificanceMzDiffs = new System.Windows.Forms.Label();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.plotFreqMzs = new HeckLib.visualization.graphs.ScatterPlot();
			this.plotFreqMzDiffs = new HeckLib.visualization.graphs.ScatterPlot();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.tableLayoutPanel2.SuspendLayout();
			this.tableLayoutPanel3.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.listFreqMzDiffs)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.listFreqMzs)).BeginInit();
			this.panel1.SuspendLayout();
			this.panel2.SuspendLayout();
			this.grpSettings.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numSignificanceMzs)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numSignificanceMzDiffs)).BeginInit();
			this.tableLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.Location = new System.Drawing.Point(0, 0);
			this.splitContainer1.Name = "splitContainer1";
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.tableLayoutPanel2);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.tableLayoutPanel1);
			this.splitContainer1.Size = new System.Drawing.Size(1039, 620);
			this.splitContainer1.SplitterDistance = 314;
			this.splitContainer1.TabIndex = 0;
			// 
			// tableLayoutPanel2
			// 
			this.tableLayoutPanel2.ColumnCount = 1;
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel2.Controls.Add(this.tableLayoutPanel3, 0, 1);
			this.tableLayoutPanel2.Controls.Add(this.grpSettings, 0, 0);
			this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel2.Name = "tableLayoutPanel2";
			this.tableLayoutPanel2.RowCount = 2;
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 18.54839F));
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 81.45161F));
			this.tableLayoutPanel2.Size = new System.Drawing.Size(314, 620);
			this.tableLayoutPanel2.TabIndex = 0;
			// 
			// tableLayoutPanel3
			// 
			this.tableLayoutPanel3.ColumnCount = 1;
			this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel3.Controls.Add(this.listFreqMzDiffs, 0, 1);
			this.tableLayoutPanel3.Controls.Add(this.listFreqMzs, 0, 3);
			this.tableLayoutPanel3.Controls.Add(this.panel1, 0, 0);
			this.tableLayoutPanel3.Controls.Add(this.panel2, 0, 2);
			this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel3.Location = new System.Drawing.Point(3, 118);
			this.tableLayoutPanel3.Name = "tableLayoutPanel3";
			this.tableLayoutPanel3.RowCount = 4;
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel3.Size = new System.Drawing.Size(308, 499);
			this.tableLayoutPanel3.TabIndex = 2;
			// 
			// listFreqMzDiffs
			// 
			this.listFreqMzDiffs.AllowColumnReorder = true;
			this.listFreqMzDiffs.CellEditUseWholeCell = false;
			this.listFreqMzDiffs.Cursor = System.Windows.Forms.Cursors.Default;
			this.listFreqMzDiffs.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listFreqMzDiffs.FullRowSelect = true;
			this.listFreqMzDiffs.HideSelection = false;
			this.listFreqMzDiffs.Location = new System.Drawing.Point(3, 33);
			this.listFreqMzDiffs.Name = "listFreqMzDiffs";
			this.listFreqMzDiffs.Size = new System.Drawing.Size(302, 213);
			this.listFreqMzDiffs.TabIndex = 0;
			this.listFreqMzDiffs.UseCompatibleStateImageBehavior = false;
			this.listFreqMzDiffs.View = System.Windows.Forms.View.Details;
			// 
			// listFreqMzs
			// 
			this.listFreqMzs.AllowColumnReorder = true;
			this.listFreqMzs.CellEditUseWholeCell = false;
			this.listFreqMzs.Cursor = System.Windows.Forms.Cursors.Default;
			this.listFreqMzs.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listFreqMzs.FullRowSelect = true;
			this.listFreqMzs.HideSelection = false;
			this.listFreqMzs.Location = new System.Drawing.Point(3, 282);
			this.listFreqMzs.Name = "listFreqMzs";
			this.listFreqMzs.Size = new System.Drawing.Size(302, 214);
			this.listFreqMzs.TabIndex = 1;
			this.listFreqMzs.UseCompatibleStateImageBehavior = false;
			this.listFreqMzs.View = System.Windows.Forms.View.Details;
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.btnSaveMzDiffs);
			this.panel1.Controls.Add(this.lblTableFrequentMzDiffs);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel1.Location = new System.Drawing.Point(3, 3);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(302, 24);
			this.panel1.TabIndex = 2;
			// 
			// btnSaveMzDiffs
			// 
			this.btnSaveMzDiffs.Image = ((System.Drawing.Image)(resources.GetObject("btnSaveMzDiffs.Image")));
			this.btnSaveMzDiffs.Location = new System.Drawing.Point(161, 0);
			this.btnSaveMzDiffs.Name = "btnSaveMzDiffs";
			this.btnSaveMzDiffs.Size = new System.Drawing.Size(25, 25);
			this.btnSaveMzDiffs.TabIndex = 3;
			this.btnSaveMzDiffs.UseVisualStyleBackColor = true;
			this.btnSaveMzDiffs.Click += new System.EventHandler(this.btnSaveMzDiffs_Click);
			// 
			// lblTableFrequentMzDiffs
			// 
			this.lblTableFrequentMzDiffs.AutoSize = true;
			this.lblTableFrequentMzDiffs.Location = new System.Drawing.Point(-3, 6);
			this.lblTableFrequentMzDiffs.Name = "lblTableFrequentMzDiffs";
			this.lblTableFrequentMzDiffs.Size = new System.Drawing.Size(125, 13);
			this.lblTableFrequentMzDiffs.TabIndex = 2;
			this.lblTableFrequentMzDiffs.Text = "Frequent m/z differences";
			// 
			// panel2
			// 
			this.panel2.Controls.Add(this.btnSaveMzs);
			this.panel2.Controls.Add(this.lblFrequentMzs);
			this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel2.Location = new System.Drawing.Point(3, 252);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(302, 24);
			this.panel2.TabIndex = 3;
			// 
			// btnSaveMzs
			// 
			this.btnSaveMzs.Image = ((System.Drawing.Image)(resources.GetObject("btnSaveMzs.Image")));
			this.btnSaveMzs.Location = new System.Drawing.Point(161, 0);
			this.btnSaveMzs.Name = "btnSaveMzs";
			this.btnSaveMzs.Size = new System.Drawing.Size(25, 25);
			this.btnSaveMzs.TabIndex = 4;
			this.btnSaveMzs.UseVisualStyleBackColor = true;
			this.btnSaveMzs.Click += new System.EventHandler(this.btnSaveMzs_Click);
			// 
			// lblFrequentMzs
			// 
			this.lblFrequentMzs.AutoSize = true;
			this.lblFrequentMzs.Location = new System.Drawing.Point(-3, 4);
			this.lblFrequentMzs.Name = "lblFrequentMzs";
			this.lblFrequentMzs.Size = new System.Drawing.Size(77, 13);
			this.lblFrequentMzs.TabIndex = 3;
			this.lblFrequentMzs.Text = "Frequent m/z\'s";
			// 
			// grpSettings
			// 
			this.grpSettings.Controls.Add(this.numSignificanceMzs);
			this.grpSettings.Controls.Add(this.lblSignificanceMzs);
			this.grpSettings.Controls.Add(this.numSignificanceMzDiffs);
			this.grpSettings.Controls.Add(this.lblSignificanceMzDiffs);
			this.grpSettings.Dock = System.Windows.Forms.DockStyle.Fill;
			this.grpSettings.Location = new System.Drawing.Point(3, 3);
			this.grpSettings.Name = "grpSettings";
			this.grpSettings.Size = new System.Drawing.Size(308, 109);
			this.grpSettings.TabIndex = 3;
			this.grpSettings.TabStop = false;
			this.grpSettings.Text = "Settings";
			// 
			// numSignificanceMzs
			// 
			this.numSignificanceMzs.DecimalPlaces = 5;
			this.numSignificanceMzs.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
			this.numSignificanceMzs.Location = new System.Drawing.Point(164, 54);
			this.numSignificanceMzs.Name = "numSignificanceMzs";
			this.numSignificanceMzs.Size = new System.Drawing.Size(120, 20);
			this.numSignificanceMzs.TabIndex = 3;
			this.numSignificanceMzs.Value = new decimal(new int[] {
            1,
            0,
            0,
            131072});
			this.numSignificanceMzs.ValueChanged += new System.EventHandler(this.numSignificanceMzs_ValueChanged);
			// 
			// lblSignificanceMzs
			// 
			this.lblSignificanceMzs.AutoSize = true;
			this.lblSignificanceMzs.Location = new System.Drawing.Point(9, 56);
			this.lblSignificanceMzs.Name = "lblSignificanceMzs";
			this.lblSignificanceMzs.Size = new System.Drawing.Size(93, 13);
			this.lblSignificanceMzs.TabIndex = 2;
			this.lblSignificanceMzs.Text = "Significance m/z\'s";
			// 
			// numSignificanceMzDiffs
			// 
			this.numSignificanceMzDiffs.DecimalPlaces = 5;
			this.numSignificanceMzDiffs.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
			this.numSignificanceMzDiffs.Location = new System.Drawing.Point(164, 28);
			this.numSignificanceMzDiffs.Name = "numSignificanceMzDiffs";
			this.numSignificanceMzDiffs.Size = new System.Drawing.Size(120, 20);
			this.numSignificanceMzDiffs.TabIndex = 1;
			this.numSignificanceMzDiffs.Value = new decimal(new int[] {
            1,
            0,
            0,
            327680});
			this.numSignificanceMzDiffs.ValueChanged += new System.EventHandler(this.numSignificanceMzDiffs_ValueChanged);
			// 
			// lblSignificanceMzDiffs
			// 
			this.lblSignificanceMzDiffs.AutoSize = true;
			this.lblSignificanceMzDiffs.Location = new System.Drawing.Point(9, 30);
			this.lblSignificanceMzDiffs.Name = "lblSignificanceMzDiffs";
			this.lblSignificanceMzDiffs.Size = new System.Drawing.Size(141, 13);
			this.lblSignificanceMzDiffs.TabIndex = 0;
			this.lblSignificanceMzDiffs.Text = "Significance m/z differences";
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 1;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel1.Controls.Add(this.plotFreqMzs, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.plotFreqMzDiffs, 0, 0);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 2;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(721, 620);
			this.tableLayoutPanel1.TabIndex = 0;
			// 
			// plotFreqMzs
			// 
			this.plotFreqMzs.Dock = System.Windows.Forms.DockStyle.Fill;
			this.plotFreqMzs.Location = new System.Drawing.Point(3, 313);
			this.plotFreqMzs.Name = "plotFreqMzs";
			this.plotFreqMzs.Size = new System.Drawing.Size(715, 304);
			this.plotFreqMzs.TabIndex = 1;
			// 
			// plotFreqMzDiffs
			// 
			this.plotFreqMzDiffs.Dock = System.Windows.Forms.DockStyle.Fill;
			this.plotFreqMzDiffs.Location = new System.Drawing.Point(3, 3);
			this.plotFreqMzDiffs.Name = "plotFreqMzDiffs";
			this.plotFreqMzDiffs.Size = new System.Drawing.Size(715, 304);
			this.plotFreqMzDiffs.TabIndex = 2;
			// 
			// DialogFrequentFlyers
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1039, 620);
			this.Controls.Add(this.splitContainer1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "DialogFrequentFlyers";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.Text = "Frequent flyers";
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			this.tableLayoutPanel2.ResumeLayout(false);
			this.tableLayoutPanel3.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.listFreqMzDiffs)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.listFreqMzs)).EndInit();
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			this.panel2.ResumeLayout(false);
			this.panel2.PerformLayout();
			this.grpSettings.ResumeLayout(false);
			this.grpSettings.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.numSignificanceMzs)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numSignificanceMzDiffs)).EndInit();
			this.tableLayoutPanel1.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.SplitContainer splitContainer1;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private HeckLib.visualization.graphs.ScatterPlot plotFreqMzs;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
		private HeckLib.visualization.graphs.ScatterPlot plotFreqMzDiffs;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
		private BrightIdeasSoftware.ObjectListView listFreqMzDiffs;
		private BrightIdeasSoftware.ObjectListView listFreqMzs;
		private System.Windows.Forms.Label lblTableFrequentMzDiffs;
		private System.Windows.Forms.Label lblFrequentMzs;
		private System.Windows.Forms.GroupBox grpSettings;
		private System.Windows.Forms.NumericUpDown numSignificanceMzDiffs;
		private System.Windows.Forms.Label lblSignificanceMzDiffs;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Button btnSaveMzDiffs;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.Button btnSaveMzs;
		private System.Windows.Forms.NumericUpDown numSignificanceMzs;
		private System.Windows.Forms.Label lblSignificanceMzs;
	}
}