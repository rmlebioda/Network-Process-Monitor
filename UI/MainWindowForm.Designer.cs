namespace NetworkProcessMonitor
{
    partial class MainWindowForm
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            this.panel1 = new System.Windows.Forms.Panel();
            this.TransferStatusStrip = new System.Windows.Forms.StatusStrip();
            this.TotalDownloadedDescription = new System.Windows.Forms.ToolStripStatusLabel();
            this.TotalDownloaded = new System.Windows.Forms.ToolStripStatusLabel();
            this.TotalUploadedDescription = new System.Windows.Forms.ToolStripStatusLabel();
            this.TotalUploaded = new System.Windows.Forms.ToolStripStatusLabel();
            this.ShowProcessesDescription = new System.Windows.Forms.ToolStripStatusLabel();
            this.ShowingProcesses = new System.Windows.Forms.ToolStripStatusLabel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.HideDeadProcesses = new System.Windows.Forms.CheckBox();
            this.HideActiveProcessesCheck = new System.Windows.Forms.CheckBox();
            this.ProcessDataGridView = new NetworkProcessMonitor.DataGridViewWithProcessDataListSource();
            this.panel1.SuspendLayout();
            this.TransferStatusStrip.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ProcessDataGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.ProcessDataGridView);
            this.panel1.Controls.Add(this.TransferStatusStrip);
            this.panel1.Controls.Add(this.flowLayoutPanel1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1384, 661);
            this.panel1.TabIndex = 2;
            // 
            // TransferStatusStrip
            // 
            this.TransferStatusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.TotalDownloadedDescription,
            this.TotalDownloaded,
            this.TotalUploadedDescription,
            this.TotalUploaded,
            this.ShowProcessesDescription,
            this.ShowingProcesses});
            this.TransferStatusStrip.Location = new System.Drawing.Point(0, 637);
            this.TransferStatusStrip.Name = "TransferStatusStrip";
            this.TransferStatusStrip.Size = new System.Drawing.Size(1384, 24);
            this.TransferStatusStrip.TabIndex = 3;
            // 
            // TotalDownloadedDescription
            // 
            this.TotalDownloadedDescription.Name = "TotalDownloadedDescription";
            this.TotalDownloadedDescription.Size = new System.Drawing.Size(105, 19);
            this.TotalDownloadedDescription.Text = "Total downloaded:";
            // 
            // TotalDownloaded
            // 
            this.TotalDownloaded.AutoSize = false;
            this.TotalDownloaded.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this.TotalDownloaded.Name = "TotalDownloaded";
            this.TotalDownloaded.Size = new System.Drawing.Size(75, 19);
            this.TotalDownloaded.Text = "0 bytes";
            this.TotalDownloaded.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // TotalUploadedDescription
            // 
            this.TotalUploadedDescription.Name = "TotalUploadedDescription";
            this.TotalUploadedDescription.Size = new System.Drawing.Size(89, 19);
            this.TotalUploadedDescription.Text = "Total uploaded:";
            // 
            // TotalUploaded
            // 
            this.TotalUploaded.AutoSize = false;
            this.TotalUploaded.Name = "TotalUploaded";
            this.TotalUploaded.Size = new System.Drawing.Size(75, 19);
            this.TotalUploaded.Text = "0 bytes";
            this.TotalUploaded.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // ShowProcessesDescription
            // 
            this.ShowProcessesDescription.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Left;
            this.ShowProcessesDescription.Name = "ShowProcessesDescription";
            this.ShowProcessesDescription.Size = new System.Drawing.Size(114, 19);
            this.ShowProcessesDescription.Text = "Showing processes:";
            this.ShowProcessesDescription.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // ShowingProcesses
            // 
            this.ShowingProcesses.Name = "ShowingProcesses";
            this.ShowingProcesses.Size = new System.Drawing.Size(13, 19);
            this.ShowingProcesses.Text = "0";
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.HideDeadProcesses);
            this.flowLayoutPanel1.Controls.Add(this.HideActiveProcessesCheck);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(1384, 21);
            this.flowLayoutPanel1.TabIndex = 2;
            // 
            // HideDeadProcesses
            // 
            this.HideDeadProcesses.AutoSize = true;
            this.HideDeadProcesses.Location = new System.Drawing.Point(3, 3);
            this.HideDeadProcesses.Name = "HideDeadProcesses";
            this.HideDeadProcesses.Size = new System.Drawing.Size(126, 17);
            this.HideDeadProcesses.TabIndex = 0;
            this.HideDeadProcesses.Text = "Hide dead processes";
            this.HideDeadProcesses.UseVisualStyleBackColor = true;
            this.HideDeadProcesses.CheckedChanged += new System.EventHandler(this.HideDeadProcesses_CheckedChanged);
            // 
            // HideActiveProcessesCheck
            // 
            this.HideActiveProcessesCheck.AutoSize = true;
            this.HideActiveProcessesCheck.Location = new System.Drawing.Point(135, 3);
            this.HideActiveProcessesCheck.Name = "HideActiveProcessesCheck";
            this.HideActiveProcessesCheck.Size = new System.Drawing.Size(131, 17);
            this.HideActiveProcessesCheck.TabIndex = 1;
            this.HideActiveProcessesCheck.Text = "Hide active processes";
            this.HideActiveProcessesCheck.UseVisualStyleBackColor = true;
            this.HideActiveProcessesCheck.CheckedChanged += new System.EventHandler(this.HideDeadProcesses_CheckedChanged);
            // 
            // ProcessDataGridView
            // 
            this.ProcessDataGridView.AllowUserToAddRows = false;
            this.ProcessDataGridView.AllowUserToDeleteRows = false;
            this.ProcessDataGridView.AllowUserToOrderColumns = true;
            this.ProcessDataGridView.AllowUserToResizeRows = false;
            this.ProcessDataGridView.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SunkenHorizontal;
            this.ProcessDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Times New Roman", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.ProcessDataGridView.DefaultCellStyle = dataGridViewCellStyle1;
            this.ProcessDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ProcessDataGridView.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.ProcessDataGridView.Location = new System.Drawing.Point(0, 21);
            this.ProcessDataGridView.Name = "ProcessDataGridView";
            this.ProcessDataGridView.ReadOnly = true;
            this.ProcessDataGridView.RowHeadersVisible = false;
            this.ProcessDataGridView.RowHeadersWidth = 4;
            this.ProcessDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.ProcessDataGridView.Size = new System.Drawing.Size(1384, 616);
            this.ProcessDataGridView.TabIndex = 1;
            this.ProcessDataGridView.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.ProcessDataGridView_CellFormatting);
            this.ProcessDataGridView.ColumnHeaderMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.ProcessDataGridView_ColumnHeaderMouseClick);
            // 
            // MainWindowForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1384, 661);
            this.Controls.Add(this.panel1);
            this.Name = "MainWindowForm";
            this.Text = "Network Process Monitor";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.TransferStatusStrip.ResumeLayout(false);
            this.TransferStatusStrip.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ProcessDataGridView)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private DataGridViewWithProcessDataListSource ProcessDataGridView;
        private System.Windows.Forms.CheckBox HideDeadProcesses;
        private System.Windows.Forms.StatusStrip TransferStatusStrip;
        private System.Windows.Forms.ToolStripStatusLabel TotalDownloadedDescription;
        private System.Windows.Forms.ToolStripStatusLabel TotalUploadedDescription;
        private System.Windows.Forms.ToolStripStatusLabel TotalDownloaded;
        private System.Windows.Forms.ToolStripStatusLabel TotalUploaded;
        private System.Windows.Forms.ToolStripStatusLabel ShowProcessesDescription;
        private System.Windows.Forms.ToolStripStatusLabel ShowingProcesses;
        private System.Windows.Forms.CheckBox HideActiveProcessesCheck;
    }
}

