using System;
using System.ComponentModel;
using System.Windows.Forms;
using GearCamChat.Plugins;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using GearCamChat.Helpers;
using System.Threading;

namespace GearCamChat {
	/// <summary>
	/// Optionsform for the plugins
	/// </summary>
	public partial class MainForm : Form 
    {
        Client c;
        private bool m_Active;
        System.Timers.Timer t;

        private WebCamCapture wc;
        public Image lastImage;
        private Int32 count;

        public WebCamCapture Webcam
        {
            get
            {
                return wc;
            }
        }

        public bool Active
        {
            get
            {
                return m_Active;
            }
            set
            {
                if (value == true)
                {
                    if (c != null && c.IsConnected())
                    {
                        GAISWriter gw = new GAISWriter();
                        gw.Write((Int32)OPCODE.VIDEO_START);
                        gw.Write(txtNick.Text);
                        c.SendData(gw);
                    }
                }
                else 
                {
                    if (c != null && c.IsConnected())
                    {
                        GAISWriter gw = new GAISWriter();
                        gw.Write((Int32)OPCODE.VIDEO_END);
                        gw.Write(txtNick.Text);
                        c.SendData(gw);
                    }
                }
                m_Active = value;
            }
        }

		public MainForm() 
        {
			InitializeComponent();
            c = null;
            Active = false;
            wc = new WebCamCapture();
            wc.ImageCaptured += new WebCamCapture.WebCamEventHandler(wc_ImageCaptured);
            count = 0;
        }

        public void SetStartup()
        {
            t = new System.Timers.Timer(Program.General.TickInterval);
            t.Elapsed += new System.Timers.ElapsedEventHandler(t_Elapsed);
            t.Start();
            wc.CaptureHeight = Program.General.Height;
            wc.CaptureWidth = Program.General.Width;
            wc.TimeToCapture_milliseconds = Program.General.WebCamTickInterval;
        }

        public Mutex m_Mutex = new Mutex();
        public Image GetImage()
        {
            //m_Mutex.WaitOne(1000, false);
            if (lastImage != null)
                return (Image)lastImage.Clone();
            else
                return null;
            //m_Mutex.ReleaseMutex();
        }

        public void SetImage(Image img)
        {
            m_Mutex.WaitOne(1000, false);
            if (lastImage != null)
                lastImage.Dispose();
            lastImage = img;
            m_Mutex.ReleaseMutex();    
        }

        void wc_ImageCaptured(object source, WebcamEventArgs e)
        {
            if (count == 10)
            {
                GC.Collect();
                count = 0;
            }

            count++;

            SetImage(e.WebCamImage);
        }

        void t_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            t.Enabled = false;
            if (Active)
            {
                if (Program.General.WebcamChanged && wc.IsRunning)
                {
                    Program.General.WebcamChanged = false;
                    wc.Start(wc.FrameNumber);
                }
                Image img = GetImage();
                if (img != null)
                {
                    c.SendData(Utils.ImageToByteArray(img));
                    img.Dispose();
                }
            }
            t.Enabled = true;
        }

        public delegate void UpdateTimerCallback();
        public void UpdateTimer()
        {
             //Thread Safe
            if (this.InvokeRequired)
            {
                UpdateTimerCallback d = new UpdateTimerCallback(UpdateTimer);
                this.Invoke(d, new object[] { });
            }
            else
            {
                t.Interval = (Program.General.TickInterval);
            }
        }

		/// <summary>
		/// Closes the program
		/// </summary>
		public void ExitProgram() {
			notifyIcon1.Visible = false; //otherwise there'll be a ghost icon ..
			Application.Exit();
		}

		/// <summary>
		/// Shows the form
		/// </summary>
		public void ShowForm() {
			if (!Visible) {
				WindowState = FormWindowState.Normal;
				ShowDialog();
			}
		}

		/// <summary>
		/// Hides the form
		/// </summary>
		public void HideForm() {
			if (Visible) {
				Hide();
			}
		}

		private void MainForm_FormClosing(object sender, FormClosingEventArgs e) {
			if (e.CloseReason != CloseReason.None) {
				//Hide may call this too, but only there the closereason is none!
                if(c != null)
                    
				ExitProgram();
			}
		}

		private void bClose_Click(object sender, EventArgs e) {
			HideForm();
		}

		private void MainForm_Resize(object sender, EventArgs e) {
			if (WindowState == FormWindowState.Minimized) {
				HideForm();
			}
		}

		private void bApply_Click(object sender, EventArgs e) {
			Program.SavePlugins();
		}

		private void bRestart_Click(object sender, EventArgs e) {
			Application.Restart();
		}

		#region Tray icon

		private void activeToolStripMenuItem_Click(object sender, EventArgs e) {
			Program.General.Active = !Program.General.Active;
		}

		private void optionsToolStripMenuItem_Click(object sender, EventArgs e) {
			ShowForm();
		}

		private void notifyIcon1_DoubleClick(object sender, EventArgs e) {
			ShowForm();
		}

		private void shutdownToolStripMenuItem_Click(object sender, EventArgs e) {
			ExitProgram();
		}

		#endregion

        public delegate void SetVisibilityCallback(bool check);
        public void SetVisibility(bool check)
        {
             //Thread Safe
            if (this.InvokeRequired)
            {
                SetVisibilityCallback d = new SetVisibilityCallback(SetVisibility);
                this.Invoke(d, new object[] { check });
            }
            else
            {
                butConnect.Enabled = check;
                butDisconnect.Enabled = !check;
                txtNick.Enabled = check;
                txtIp.Enabled = check;
                txtPort.Enabled = check;
                if(check)
                    c = null;
            }
        }

        private void butConnect_Click(object sender, EventArgs e)
        {
            SetVisibility(false);
            if(c == null)
                c = new Client(txtNick.Text, txtIp.Text, Convert.ToInt32(txtPort.Text), this);
        }

        private void butDisconnect_Click(object sender, EventArgs e)
        {
            SetVisibility(true);
            if (c != null)
                c.Dispose();
        }
	}
}