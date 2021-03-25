namespace FragmentLab.dialogs
{
	partial class DialogFragmentReport
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
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.lblRawFiles = new System.Windows.Forms.Label();
			this.lblSequenceCoverage = new System.Windows.Forms.Label();
			this.chartSequenceCoverage = new LiveCharts.WinForms.CartesianChart();
			this.listRawFiles = new BrightIdeasSoftware.ObjectListView();
			this.chartIntensities = new LiveCharts.WinForms.CartesianChart();
			this.lblIntensity = new System.Windows.Forms.Label();
			this.chartModifications = new LiveCharts.WinForms.PieChart();
			this.lblModifications = new System.Windows.Forms.Label();
			this.panel1.SuspendLayout();
			this.tableLayoutPanel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.listRawFiles)).BeginInit();
			this.SuspendLayout();
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.tableLayoutPanel1);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel1.Location = new System.Drawing.Point(0, 0);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(997, 522);
			this.panel1.TabIndex = 0;
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 2;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel1.Controls.Add(this.lblModifications, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.lblRawFiles, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.lblSequenceCoverage, 1, 0);
			this.tableLayoutPanel1.Controls.Add(this.chartSequenceCoverage, 1, 1);
			this.tableLayoutPanel1.Controls.Add(this.listRawFiles, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.chartIntensities, 1, 3);
			this.tableLayoutPanel1.Controls.Add(this.lblIntensity, 1, 2);
			this.tableLayoutPanel1.Controls.Add(this.chartModifications, 0, 3);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 4;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(997, 522);
			this.tableLayoutPanel1.TabIndex = 1;
			// 
			// lblRawFiles
			// 
			this.lblRawFiles.AutoSize = true;
			this.lblRawFiles.BackColor = System.Drawing.SystemColors.Window;
			this.lblRawFiles.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblRawFiles.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblRawFiles.Location = new System.Drawing.Point(3, 0);
			this.lblRawFiles.Name = "lblRawFiles";
			this.lblRawFiles.Size = new System.Drawing.Size(492, 50);
			this.lblRawFiles.TabIndex = 5;
			this.lblRawFiles.Text = "Raw Files";
			this.lblRawFiles.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// lblSequenceCoverage
			// 
			this.lblSequenceCoverage.AutoSize = true;
			this.lblSequenceCoverage.BackColor = System.Drawing.SystemColors.Window;
			this.lblSequenceCoverage.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblSequenceCoverage.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblSequenceCoverage.Location = new System.Drawing.Point(501, 0);
			this.lblSequenceCoverage.Name = "lblSequenceCoverage";
			this.lblSequenceCoverage.Size = new System.Drawing.Size(493, 50);
			this.lblSequenceCoverage.TabIndex = 3;
			this.lblSequenceCoverage.Text = "Sequence Coverage";
			this.lblSequenceCoverage.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// chartSequenceCoverage
			// 
			this.chartSequenceCoverage.BackColor = System.Drawing.SystemColors.Window;
			this.chartSequenceCoverage.Dock = System.Windows.Forms.DockStyle.Fill;
			this.chartSequenceCoverage.Location = new System.Drawing.Point(501, 53);
			this.chartSequenceCoverage.Name = "chartSequenceCoverage";
			this.chartSequenceCoverage.Size = new System.Drawing.Size(493, 205);
			this.chartSequenceCoverage.TabIndex = 0;
			this.chartSequenceCoverage.Text = "cartesianChart1";
			// 
			// listRawFiles
			// 
			this.listRawFiles.CellEditUseWholeCell = false;
			this.listRawFiles.Cursor = System.Windows.Forms.Cursors.Default;
			this.listRawFiles.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listRawFiles.GridLines = true;
			this.listRawFiles.HideSelection = false;
			this.listRawFiles.Location = new System.Drawing.Point(3, 53);
			this.listRawFiles.Name = "listRawFiles";
			this.listRawFiles.Size = new System.Drawing.Size(492, 205);
			this.listRawFiles.TabIndex = 6;
			this.listRawFiles.UseCompatibleStateImageBehavior = false;
			this.listRawFiles.View = System.Windows.Forms.View.Details;
			// 
			// chartIntensities
			// 
			this.chartIntensities.Dock = System.Windows.Forms.DockStyle.Fill;
			this.chartIntensities.Location = new System.Drawing.Point(501, 314);
			this.chartIntensities.Name = "chartIntensities";
			this.chartIntensities.Size = new System.Drawing.Size(493, 205);
			this.chartIntensities.TabIndex = 7;
			this.chartIntensities.Text = "cartesianChart1";
			// 
			// lblIntensity
			// 
			this.lblIntensity.AutoSize = true;
			this.lblIntensity.BackColor = System.Drawing.SystemColors.Window;
			this.lblIntensity.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblIntensity.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblIntensity.Location = new System.Drawing.Point(501, 261);
			this.lblIntensity.Name = "lblIntensity";
			this.lblIntensity.Size = new System.Drawing.Size(493, 50);
			this.lblIntensity.TabIndex = 8;
			this.lblIntensity.Text = "Intensity";
			this.lblIntensity.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// chartModifications
			// 
			this.chartModifications.Dock = System.Windows.Forms.DockStyle.Fill;
			this.chartModifications.Location = new System.Drawing.Point(3, 314);
			this.chartModifications.Name = "chartModifications";
			this.chartModifications.Size = new System.Drawing.Size(492, 205);
			this.chartModifications.TabIndex = 9;
			this.chartModifications.Text = "pieChart1";
			// 
			// lblModifications
			// 
			this.lblModifications.AutoSize = true;
			this.lblModifications.BackColor = System.Drawing.SystemColors.Window;
			this.lblModifications.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblModifications.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblModifications.Location = new System.Drawing.Point(3, 261);
			this.lblModifications.Name = "lblModifications";
			this.lblModifications.Size = new System.Drawing.Size(492, 50);
			this.lblModifications.TabIndex = 10;
			this.lblModifications.Text = "Modifications";
			this.lblModifications.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// DialogFragmentReport
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.Window;
			this.ClientSize = new System.Drawing.Size(997, 522);
			this.Controls.Add(this.panel1);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "DialogFragmentReport";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.Text = "Fragmentation report";
			this.panel1.ResumeLayout(false);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.listRawFiles)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Panel panel1;
		private LiveCharts.WinForms.CartesianChart chartSequenceCoverage;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.Label lblSequenceCoverage;
		private System.Windows.Forms.Label lblRawFiles;
		private BrightIdeasSoftware.ObjectListView listRawFiles;
		private LiveCharts.WinForms.CartesianChart chartIntensities;
		private System.Windows.Forms.Label lblIntensity;
		private LiveCharts.WinForms.PieChart chartModifications;
		private System.Windows.Forms.Label lblModifications;
	}
}