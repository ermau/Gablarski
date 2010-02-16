using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Gablarski.Audio;
using Gablarski.Clients.Windows.Properties;

namespace Gablarski.Clients.Windows
{
	public partial class MusicForm : Form
	{
		public MusicForm ()
		{
			this.Icon = Resources.MusicImage.ToIcon ();
			InitializeComponent ();
		}

		public bool File
		{
			get { return this.fileMusic.Checked; }
		}

		public IAudioDeviceProvider Provider
		{
			get { return deviceSelector.Provider; }
		}

		public IAudioDevice CaptureDevice
		{
			get { return deviceSelector.Device; }
		}

		public string FilePath
		{
			get { return this.filename.Text; }
		}

		private void MusicForm_Load (object sender, EventArgs e)
		{
			this.deviceSelector.ProviderSource = Modules.Capture.Cast<IAudioDeviceProvider>();
		}

		private void browseButton_Click (object sender, EventArgs e)
		{
			this.captureMusic.Checked = false;
			this.fileMusic.Checked = true;

			var ofd = new OpenFileDialog();
			ofd.InitialDirectory = Environment.GetFolderPath (Environment.SpecialFolder.MyMusic);
			ofd.Filter = "Wav Files (*.wav)|*.txt|All Files (*.*)|*.*";
			ofd.CheckFileExists = true;
			ofd.CheckPathExists = true;

			if (ofd.ShowDialog() == DialogResult.OK)
				this.filename.Text = ofd.FileName;  
		}

		private void filename_TextChanged (object sender, EventArgs e)
		{
			this.captureMusic.Checked = false;
			this.fileMusic.Checked = true;
		}
	
		private void deviceSelector_Enter (object sender, EventArgs e)
		{
			this.captureMusic.Checked = true;
			this.fileMusic.Checked = false;
		}

		private void okButton_Click (object sender, EventArgs e)
		{
			if (File && !System.IO.File.Exists (FilePath))
			{
				this.filename.BackColor = Color.Red;
				return;
			}
			else
				this.filename.BackColor = Color.Empty;

			DialogResult = System.Windows.Forms.DialogResult.OK;
			Close ();
		}
	}		
}
