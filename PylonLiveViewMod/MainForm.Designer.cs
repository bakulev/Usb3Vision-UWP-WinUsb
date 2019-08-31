namespace PylonLiveView
{
    partial class MainForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.splitContainerImageView = new System.Windows.Forms.SplitContainer();
            this.splitContainerConfiguration = new System.Windows.Forms.SplitContainer();
            this.deviceListView = new System.Windows.Forms.ListView();
            this.imageListForDeviceList = new System.Windows.Forms.ImageList(this.components);
            this.exposureTimeSliderControl = new PylonLiveViewControl.FloatSliderUserControl();
            this.gainSliderControl = new PylonLiveViewControl.FloatSliderUserControl();
            this.heightSliderControl = new PylonLiveViewControl.IntSliderUserControl();
            this.widthSliderControl = new PylonLiveViewControl.IntSliderUserControl();
            this.pixelFormatControl = new PylonLiveViewControl.EnumerationComboBoxUserControl();
            this.testImageControl = new PylonLiveViewControl.EnumerationComboBoxUserControl();
            this.toolStrip = new System.Windows.Forms.ToolStrip();
            this.toolStripButtonOneShot = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonContinuousShot = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonStop = new System.Windows.Forms.ToolStripButton();
            this.pictureBox = new System.Windows.Forms.PictureBox();
            this.updateDeviceListTimer = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerImageView)).BeginInit();
            this.splitContainerImageView.Panel1.SuspendLayout();
            this.splitContainerImageView.Panel2.SuspendLayout();
            this.splitContainerImageView.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerConfiguration)).BeginInit();
            this.splitContainerConfiguration.Panel1.SuspendLayout();
            this.splitContainerConfiguration.Panel2.SuspendLayout();
            this.splitContainerConfiguration.SuspendLayout();
            this.toolStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainerImageView
            // 
            this.splitContainerImageView.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.splitContainerImageView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerImageView.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainerImageView.Location = new System.Drawing.Point(0, 0);
            this.splitContainerImageView.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.splitContainerImageView.Name = "splitContainerImageView";
            // 
            // splitContainerImageView.Panel1
            // 
            this.splitContainerImageView.Panel1.Controls.Add(this.splitContainerConfiguration);
            this.splitContainerImageView.Panel1.Controls.Add(this.toolStrip);
            // 
            // splitContainerImageView.Panel2
            // 
            this.splitContainerImageView.Panel2.AutoScroll = true;
            this.splitContainerImageView.Panel2.Controls.Add(this.pictureBox);
            this.splitContainerImageView.Size = new System.Drawing.Size(1045, 692);
            this.splitContainerImageView.SplitterDistance = 226;
            this.splitContainerImageView.SplitterWidth = 5;
            this.splitContainerImageView.TabIndex = 0;
            this.splitContainerImageView.TabStop = false;
            // 
            // splitContainerConfiguration
            // 
            this.splitContainerConfiguration.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.splitContainerConfiguration.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerConfiguration.Location = new System.Drawing.Point(0, 39);
            this.splitContainerConfiguration.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.splitContainerConfiguration.Name = "splitContainerConfiguration";
            this.splitContainerConfiguration.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainerConfiguration.Panel1
            // 
            this.splitContainerConfiguration.Panel1.Controls.Add(this.deviceListView);
            // 
            // splitContainerConfiguration.Panel2
            // 
            this.splitContainerConfiguration.Panel2.Controls.Add(this.exposureTimeSliderControl);
            this.splitContainerConfiguration.Panel2.Controls.Add(this.gainSliderControl);
            this.splitContainerConfiguration.Panel2.Controls.Add(this.heightSliderControl);
            this.splitContainerConfiguration.Panel2.Controls.Add(this.widthSliderControl);
            this.splitContainerConfiguration.Panel2.Controls.Add(this.pixelFormatControl);
            this.splitContainerConfiguration.Panel2.Controls.Add(this.testImageControl);
            this.splitContainerConfiguration.Size = new System.Drawing.Size(226, 653);
            this.splitContainerConfiguration.SplitterDistance = 203;
            this.splitContainerConfiguration.SplitterWidth = 5;
            this.splitContainerConfiguration.TabIndex = 1;
            this.splitContainerConfiguration.TabStop = false;
            // 
            // deviceListView
            // 
            this.deviceListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.deviceListView.HideSelection = false;
            this.deviceListView.LargeImageList = this.imageListForDeviceList;
            this.deviceListView.Location = new System.Drawing.Point(0, 0);
            this.deviceListView.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.deviceListView.MultiSelect = false;
            this.deviceListView.Name = "deviceListView";
            this.deviceListView.ShowItemToolTips = true;
            this.deviceListView.Size = new System.Drawing.Size(222, 199);
            this.deviceListView.TabIndex = 0;
            this.deviceListView.UseCompatibleStateImageBehavior = false;
            this.deviceListView.View = System.Windows.Forms.View.Tile;
            this.deviceListView.SelectedIndexChanged += new System.EventHandler(this.deviceListView_SelectedIndexChanged);
            this.deviceListView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.deviceListView_KeyDown);
            // 
            // imageListForDeviceList
            // 
            this.imageListForDeviceList.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.imageListForDeviceList.ImageSize = new System.Drawing.Size(32, 32);
            this.imageListForDeviceList.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // exposureTimeSliderControl
            // 
            this.exposureTimeSliderControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.exposureTimeSliderControl.DefaultName = "N/A";
            this.exposureTimeSliderControl.Location = new System.Drawing.Point(0, 325);
            this.exposureTimeSliderControl.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.exposureTimeSliderControl.MinimumSize = new System.Drawing.Size(300, 62);
            this.exposureTimeSliderControl.Name = "exposureTimeSliderControl";
            this.exposureTimeSliderControl.Size = new System.Drawing.Size(300, 62);
            this.exposureTimeSliderControl.TabIndex = 6;
            // 
            // gainSliderControl
            // 
            this.gainSliderControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gainSliderControl.DefaultName = "N/A";
            this.gainSliderControl.Location = new System.Drawing.Point(0, 263);
            this.gainSliderControl.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.gainSliderControl.MinimumSize = new System.Drawing.Size(300, 62);
            this.gainSliderControl.Name = "gainSliderControl";
            this.gainSliderControl.Size = new System.Drawing.Size(300, 62);
            this.gainSliderControl.TabIndex = 5;
            // 
            // heightSliderControl
            // 
            this.heightSliderControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.heightSliderControl.DefaultName = "N/A";
            this.heightSliderControl.Location = new System.Drawing.Point(0, 202);
            this.heightSliderControl.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.heightSliderControl.MinimumSize = new System.Drawing.Size(300, 62);
            this.heightSliderControl.Name = "heightSliderControl";
            this.heightSliderControl.Size = new System.Drawing.Size(300, 62);
            this.heightSliderControl.TabIndex = 4;
            // 
            // widthSliderControl
            // 
            this.widthSliderControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.widthSliderControl.DefaultName = "N/A";
            this.widthSliderControl.Location = new System.Drawing.Point(0, 140);
            this.widthSliderControl.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.widthSliderControl.MinimumSize = new System.Drawing.Size(300, 62);
            this.widthSliderControl.Name = "widthSliderControl";
            this.widthSliderControl.Size = new System.Drawing.Size(300, 62);
            this.widthSliderControl.TabIndex = 3;
            // 
            // pixelFormatControl
            // 
            this.pixelFormatControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pixelFormatControl.DefaultName = "N/A";
            this.pixelFormatControl.Location = new System.Drawing.Point(16, 70);
            this.pixelFormatControl.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.pixelFormatControl.Name = "pixelFormatControl";
            this.pixelFormatControl.Size = new System.Drawing.Size(194, 70);
            this.pixelFormatControl.TabIndex = 1;
            // 
            // testImageControl
            // 
            this.testImageControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.testImageControl.DefaultName = "N/A";
            this.testImageControl.Location = new System.Drawing.Point(16, 0);
            this.testImageControl.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.testImageControl.Name = "testImageControl";
            this.testImageControl.Size = new System.Drawing.Size(194, 70);
            this.testImageControl.TabIndex = 0;
            // 
            // toolStrip
            // 
            this.toolStrip.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonOneShot,
            this.toolStripButtonContinuousShot,
            this.toolStripButtonStop});
            this.toolStrip.Location = new System.Drawing.Point(0, 0);
            this.toolStrip.Name = "toolStrip";
            this.toolStrip.Size = new System.Drawing.Size(226, 39);
            this.toolStrip.TabIndex = 0;
            this.toolStrip.Text = "toolStrip";
            // 
            // toolStripButtonOneShot
            // 
            this.toolStripButtonOneShot.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonOneShot.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonOneShot.Image")));
            this.toolStripButtonOneShot.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonOneShot.Name = "toolStripButtonOneShot";
            this.toolStripButtonOneShot.Size = new System.Drawing.Size(36, 36);
            this.toolStripButtonOneShot.Text = "One Shot";
            this.toolStripButtonOneShot.ToolTipText = "One Shot";
            this.toolStripButtonOneShot.Click += new System.EventHandler(this.toolStripButtonOneShot_Click);
            // 
            // toolStripButtonContinuousShot
            // 
            this.toolStripButtonContinuousShot.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonContinuousShot.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonContinuousShot.Image")));
            this.toolStripButtonContinuousShot.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonContinuousShot.Name = "toolStripButtonContinuousShot";
            this.toolStripButtonContinuousShot.Size = new System.Drawing.Size(36, 36);
            this.toolStripButtonContinuousShot.Text = "Continuous Shot";
            this.toolStripButtonContinuousShot.ToolTipText = "Continuous Shot";
            this.toolStripButtonContinuousShot.Click += new System.EventHandler(this.toolStripButtonContinuousShot_Click);
            // 
            // toolStripButtonStop
            // 
            this.toolStripButtonStop.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonStop.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonStop.Image")));
            this.toolStripButtonStop.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonStop.Name = "toolStripButtonStop";
            this.toolStripButtonStop.Size = new System.Drawing.Size(36, 36);
            this.toolStripButtonStop.Text = "Stop Grab";
            this.toolStripButtonStop.ToolTipText = "Stop Grab";
            this.toolStripButtonStop.Click += new System.EventHandler(this.toolStripButtonStop_Click);
            // 
            // pictureBox
            // 
            this.pictureBox.Location = new System.Drawing.Point(0, 0);
            this.pictureBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.pictureBox.Name = "pictureBox";
            this.pictureBox.Size = new System.Drawing.Size(480, 480);
            this.pictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox.TabIndex = 0;
            this.pictureBox.TabStop = false;
            // 
            // updateDeviceListTimer
            // 
            this.updateDeviceListTimer.Enabled = true;
            this.updateDeviceListTimer.Interval = 5000;
            this.updateDeviceListTimer.Tick += new System.EventHandler(this.updateDeviceListTimer_Tick);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1045, 692);
            this.Controls.Add(this.splitContainerImageView);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MinimumSize = new System.Drawing.Size(1061, 728);
            this.Name = "MainForm";
            this.Text = "Pylon Live View";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.splitContainerImageView.Panel1.ResumeLayout(false);
            this.splitContainerImageView.Panel1.PerformLayout();
            this.splitContainerImageView.Panel2.ResumeLayout(false);
            this.splitContainerImageView.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerImageView)).EndInit();
            this.splitContainerImageView.ResumeLayout(false);
            this.splitContainerConfiguration.Panel1.ResumeLayout(false);
            this.splitContainerConfiguration.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerConfiguration)).EndInit();
            this.splitContainerConfiguration.ResumeLayout(false);
            this.toolStrip.ResumeLayout(false);
            this.toolStrip.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainerImageView;
        private System.Windows.Forms.PictureBox pictureBox;
        private System.Windows.Forms.ToolStrip toolStrip;
        private System.Windows.Forms.ToolStripButton toolStripButtonOneShot;
        private System.Windows.Forms.ToolStripButton toolStripButtonContinuousShot;
        private System.Windows.Forms.ToolStripButton toolStripButtonStop;
        private System.Windows.Forms.SplitContainer splitContainerConfiguration;
        private System.Windows.Forms.ListView deviceListView;
        private System.Windows.Forms.Timer updateDeviceListTimer;
        private System.Windows.Forms.ImageList imageListForDeviceList;
        private PylonLiveViewControl.EnumerationComboBoxUserControl testImageControl;
        private PylonLiveViewControl.EnumerationComboBoxUserControl pixelFormatControl;
        private PylonLiveViewControl.IntSliderUserControl widthSliderControl;
        private PylonLiveViewControl.IntSliderUserControl heightSliderControl;
        private PylonLiveViewControl.FloatSliderUserControl gainSliderControl;
        private PylonLiveViewControl.FloatSliderUserControl exposureTimeSliderControl;
    }
}

