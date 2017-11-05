/*
 * Copyright 2002 - 2007 Quintity, LLC.  All Rights Reserved.
 * Use is subject to license terms.
 * 
*/
using System;
using System.Reflection;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using Microsoft.VisualBasic.ApplicationServices;


namespace Quintity.TestFramework.TestEngineer
{
	/// <summary>
	/// Summary description for SplashDlg.
	/// </summary>
	public class SplashDialog : System.Windows.Forms.Form
    {
		private System.Windows.Forms.Timer m_timerSplash;
        private Label m_copyright;
        private PictureBox m_pbSplash;
        private Label m_version;
		private System.ComponentModel.IContainer components;

		public SplashDialog( bool bSetTimer )
		{
			// Initialize setup.
			InitializeComponent();

			// Ensure that dialog contains splash bitmap.
            AssemblyInfo info = new AssemblyInfo(Assembly.GetAssembly(this.GetType()));
            this.m_copyright.Text = info.Copyright;
            this.m_version.Text = "Version " + info.Version;

			// Set timer for splash screen.
			this.m_timerSplash.Enabled = bSetTimer;
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SplashDialog));
            this.m_timerSplash = new System.Windows.Forms.Timer(this.components);
            this.m_copyright = new System.Windows.Forms.Label();
            this.m_pbSplash = new System.Windows.Forms.PictureBox();
            this.m_version = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.m_pbSplash)).BeginInit();
            this.SuspendLayout();
            // 
            // m_timerSplash
            // 
            this.m_timerSplash.Interval = 3000;
            this.m_timerSplash.Tick += new System.EventHandler(this.m_timerSplash_Tick);
            // 
            // m_copyright
            // 
            this.m_copyright.AutoSize = true;
            this.m_copyright.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.m_copyright.ForeColor = System.Drawing.Color.RoyalBlue;
            this.m_copyright.Location = new System.Drawing.Point(93, 321);
            this.m_copyright.Name = "m_copyright";
            this.m_copyright.Size = new System.Drawing.Size(86, 13);
            this.m_copyright.TabIndex = 1;
            this.m_copyright.Text = "Copyright Info";
            // 
            // m_pbSplash
            // 
            this.m_pbSplash.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.m_pbSplash.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.m_pbSplash.Image = ((System.Drawing.Image)(resources.GetObject("m_pbSplash.Image")));
            this.m_pbSplash.Location = new System.Drawing.Point(0, 0);
            this.m_pbSplash.Name = "m_pbSplash";
            this.m_pbSplash.Size = new System.Drawing.Size(457, 364);
            this.m_pbSplash.TabIndex = 0;
            this.m_pbSplash.TabStop = false;
            this.m_pbSplash.Click += new System.EventHandler(this.m_pbSplash_Click);
            // 
            // m_version
            // 
            this.m_version.AutoSize = true;
            this.m_version.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.m_version.ForeColor = System.Drawing.Color.RoyalBlue;
            this.m_version.Location = new System.Drawing.Point(171, 338);
            this.m_version.Name = "m_version";
            this.m_version.Size = new System.Drawing.Size(71, 13);
            this.m_version.TabIndex = 2;
            this.m_version.Text = "VersionInfo";
            // 
            // SplashDialog
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.ClientSize = new System.Drawing.Size(458, 364);
            this.ControlBox = false;
            this.Controls.Add(this.m_version);
            this.Controls.Add(this.m_copyright);
            this.Controls.Add(this.m_pbSplash);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SplashDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "SplashDlg";
            ((System.ComponentModel.ISupportInitialize)(this.m_pbSplash)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

		private void m_timerSplash_Tick(object sender, System.EventArgs e)
		{
			this.Close();
		}

		private void m_pbSplash_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}
	}
}
