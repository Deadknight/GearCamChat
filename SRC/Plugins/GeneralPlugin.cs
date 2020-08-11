using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using OverlayLib;
using System;
using GearCamChat.Helpers;

namespace GearCamChat.Plugins {
	/// <summary>
	/// General Plugin
	/// </summary>
	public class GeneralPlugin : APlugin {
		private int m_BackgroundColor = Color.Black.ToArgb();
		private int m_Height = 240;
		private int m_TextColor = Color.Orange.ToArgb();
		private int m_TextShadowColor = Color.FromArgb(16, 16, 16).ToArgb();
		private int m_Width = 320;
        private Image lastImage;
        Int32 count;
        MainForm main;

        public GeneralPlugin()
        {
            count = 0;
        }

        public void SetMain(MainForm mainP)
        {
            main = mainP;
        }

		public override bool Active {
			get { return base.Active; }
			set {
				base.Active = value;

				if (!value) {
					Program.Overlay.Hide();
				}
			}
		}

		public override string BaseName {
			get { return "general"; }
		}

		public override string Name {
			get { return "General"; }
		}

		#region General

		private int m_RenderInterval = 500;
		private int m_TickInterval = 1000;
        private int m_WebcamTickInterval = 30;

        private bool m_WebcamTickChanged = false;
        public bool WebcamChanged
        {
            get
            {
                return m_WebcamTickChanged;
            }
            set
            {
                m_WebcamTickChanged = value;
            }
        }


		[Save, Category("General")]
		public int Width {
			get { return m_Width; }
			set {
				m_Width = value;

				if (Program.Overlay != null) {
					Program.UpdateSize();
				}
			}
		}

		[Save, Category("General")]
		public int Height {
			get { return m_Height; }
			set {
				m_Height = value;

				if (Program.Overlay != null) {
					Program.UpdateSize();
				}
			}
		}

		[Save, Category("General")]
		public override int X {
			get { return base.X; }
			set {
				base.X = value;

				if (Program.Overlay != null) {
					Program.UpdatePosition();
				}
			}
		}

		[Save, Category("General")]
		public override int Y {
			get { return base.Y; }
			set {
				base.Y = value;

				if (Program.Overlay != null) {
					Program.UpdatePosition();
				}
			}
		}

		[Save, Category("General")]
		public int TickInterval {
			get { return m_TickInterval; }
			set 
            {
                main.UpdateTimer();
                m_TickInterval = value; 
            }
		}

        [Save, Category("General")]
        public int WebCamTickInterval
        {
            get { return m_WebcamTickInterval; }
            set { m_WebcamTickInterval = value; }
        }

		[Save, Category("General")]
		public int RenderInterval {
			get { return m_RenderInterval; }
			set { m_RenderInterval = value; }
		}

		#endregion

		#region Visual

		private bool m_FakeAlpha = false;

		[Category("Visual")]
		public Color TextColor {
			get { return Color.FromArgb(m_TextColor); }
			set { m_TextColor = value.ToArgb(); }
		}

		[Save, Browsable(false)]
		public int xTextColor {
			get { return m_TextColor; }
			set { m_TextColor = value; }
		}

		[Category("Visual")]
		public Color TextShadowColor {
			get { return Color.FromArgb(m_TextShadowColor); }
			set { m_TextShadowColor = value.ToArgb(); }
		}

		[Save, Browsable(false)]
		public int xTextShadowColor {
			get { return m_TextShadowColor; }
			set { m_TextShadowColor = value; }
		}

		[Category("Visual")]
		public Color BackgroundColor {
			get { return Color.FromArgb(m_BackgroundColor); }
			set { m_BackgroundColor = value.ToArgb(); }
		}

		[Save, Browsable(false)]
		public int xBackgroundColor {
			get { return m_BackgroundColor; }
			set { m_BackgroundColor = value; }
		}

		[Save, Category("Visual")]
		public bool AlphaEnabled {
			get { return Program.Overlay.AlphaEnabled; }
			set { Program.Overlay.AlphaEnabled = value; }
		}

		[Save, Category("Visual")]
		public bool FakeAlpha {
			get { return m_FakeAlpha; }
			set { m_FakeAlpha = value; }
		}

		[Category("Visual")]
		public Overlay.ePixelFormat PixelFormat {
			get { return Program.Overlay.PixelFormat; }
		}

		#endregion

		#region Shortcuts

		private Keys m_ChangePageKey = Keys.F11;
		private Keys m_ChangeVisibleKey = Keys.V;

		[Save, Category("Shortcuts")]
		public Keys ChangeVisibleKey {
			get { return m_ChangeVisibleKey; }
			set { m_ChangeVisibleKey = value; }
		}

		[Save, Category("Shorcuts")]
		public Keys ChangePageKey {
			get { return m_ChangePageKey; }
			set { m_ChangePageKey = value; }
		}

		#endregion

		public override void Tick() 
        {
        }

        Point p = new Point(0, 0);
		public override void Render(Graphics g)
        {
            if (lastImage != null)
            {
                g.DrawImageUnscaled(lastImage, p);
            }
        }

        public void SetImage(Byte[] arr)
        {
            if(lastImage != null)
                lastImage.Dispose();

            if(count > 10)
            {
                GC.Collect();
                count = 0;
            }

            count++;

            lastImage = Utils.ByteArrayToImage(arr);
        }
	}
}