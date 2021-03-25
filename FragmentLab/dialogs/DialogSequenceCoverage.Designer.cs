namespace FragmentLab.dialogs
{
	partial class DialogSequenceCoverage
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
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.splitContainer2 = new System.Windows.Forms.SplitContainer();
			this.propertySettings = new System.Windows.Forms.PropertyGrid();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.listPsms = new BrightIdeasSoftware.ObjectListView();
			this.sequenceCoverageView = new hecklib.graphics.controls.SequenceCoverageView();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
			this.splitContainer2.Panel1.SuspendLayout();
			this.splitContainer2.Panel2.SuspendLayout();
			this.splitContainer2.SuspendLayout();
			this.tableLayoutPanel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.listPsms)).BeginInit();
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
			this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.sequenceCoverageView);
			this.splitContainer1.Size = new System.Drawing.Size(1338, 517);
			this.splitContainer1.SplitterDistance = 268;
			this.splitContainer1.TabIndex = 0;
			// 
			// splitContainer2
			// 
			this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer2.Location = new System.Drawing.Point(0, 0);
			this.splitContainer2.Name = "splitContainer2";
			this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer2.Panel1
			// 
			this.splitContainer2.Panel1.Controls.Add(this.propertySettings);
			// 
			// splitContainer2.Panel2
			// 
			this.splitContainer2.Panel2.Controls.Add(this.tableLayoutPanel1);
			this.splitContainer2.Size = new System.Drawing.Size(268, 517);
			this.splitContainer2.SplitterDistance = 131;
			this.splitContainer2.TabIndex = 0;
			// 
			// propertySettings
			// 
			this.propertySettings.Dock = System.Windows.Forms.DockStyle.Fill;
			this.propertySettings.HelpVisible = false;
			this.propertySettings.Location = new System.Drawing.Point(0, 0);
			this.propertySettings.Name = "propertySettings";
			this.propertySettings.Size = new System.Drawing.Size(268, 131);
			this.propertySettings.TabIndex = 0;
			this.propertySettings.ToolbarVisible = false;
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 1;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(this.listPsms, 0, 0);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 1;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(268, 382);
			this.tableLayoutPanel1.TabIndex = 1;
			// 
			// listPsms
			// 
			this.listPsms.CellEditUseWholeCell = false;
			this.listPsms.Cursor = System.Windows.Forms.Cursors.Default;
			this.listPsms.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listPsms.FullRowSelect = true;
			this.listPsms.HideSelection = false;
			this.listPsms.Location = new System.Drawing.Point(3, 3);
			this.listPsms.Name = "listPsms";
			this.listPsms.ShowGroups = false;
			this.listPsms.Size = new System.Drawing.Size(262, 376);
			this.listPsms.TabIndex = 0;
			this.listPsms.UseCompatibleStateImageBehavior = false;
			this.listPsms.View = System.Windows.Forms.View.Details;
			this.listPsms.SelectedIndexChanged += new System.EventHandler(this.listPsms_SelectedIndexChanged);
			// 
			// sequenceCoverageView
			// 
			this.sequenceCoverageView.AutoScroll = true;
			this.sequenceCoverageView.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.sequenceCoverageView.CurrentDisplayMode = hecklib.graphics.controls.SequenceCoverageView.DisplayMode.Summary;
			this.sequenceCoverageView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.sequenceCoverageView.Location = new System.Drawing.Point(0, 0);
			this.sequenceCoverageView.Name = "sequenceCoverageView";
			this.sequenceCoverageView.Size = new System.Drawing.Size(1066, 517);
			this.sequenceCoverageView.TabIndex = 0;
			// 
			// DialogSequenceCoverage
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1338, 517);
			this.Controls.Add(this.splitContainer1);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "DialogSequenceCoverage";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.Text = "Sequence coverage";
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			this.splitContainer2.Panel1.ResumeLayout(false);
			this.splitContainer2.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
			this.splitContainer2.ResumeLayout(false);
			this.tableLayoutPanel1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.listPsms)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.SplitContainer splitContainer1;
		private System.Windows.Forms.SplitContainer splitContainer2;
		private System.Windows.Forms.PropertyGrid propertySettings;
		private BrightIdeasSoftware.ObjectListView listPsms;
		private hecklib.graphics.controls.SequenceCoverageView sequenceCoverageView;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
	}
}