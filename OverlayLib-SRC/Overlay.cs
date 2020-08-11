using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using Microsoft.DirectX;
using Microsoft.DirectX.DirectDraw;
using PixelFormat=Microsoft.DirectX.DirectDraw.PixelFormat;

namespace OverlayLib {
	/// <summary>
	/// A DirectDraw Overlay
	/// (c) 2007 Metty
	/// </summary>
	public class Overlay : IDisposable {
		#region ePixelFormat enum

		/// <summary>
		/// The PixelFormat of the render surface
		/// </summary>
		public enum ePixelFormat {
			RGB32, //Defines a RGB32 (R,G,B, -) pixel format
			YUY2, //Defines a YUY2 (16 bit) pixel format
		}

		#endregion

		private bool m_AlphaEnabled = false;
		private Surface m_BackBuffer;
		private Surface m_Buffer; //Render Source
		private Device m_Device;
		private OverlayEffects m_Effects;
		private OverlayFlags m_Flags;
		private ePixelFormat m_PixelFormat = ePixelFormat.RGB32;
		private Point m_Position = Point.Empty;
		private Surface m_Primary; //Display
		private RenderDelegate m_Renderer;
		private Bitmap m_RenderTarget; //Rendertarget

		private Size m_Size = new Size(100, 100);

		/// <summary>
		/// The position of the overlay on the screen
		/// </summary>
		public Point Position {
			get { return m_Position; }
			set {
				if (m_Device != null) {
					Hide();
					m_Position = value;
					Update();
				}
				else {
					m_Position = value;
				}
			}
		}

		/// <summary>
		/// The Size of the overlay
		/// </summary>
		public Size Size {
			get { return m_Size; }
			set {
				m_Size = value;

				if (m_Device != null) {
					Dispose();
					Initialise();
				}
			}
		}

		/// <summary>
		/// Boundings of the overlay
		/// Combins Position and Size
		/// </summary>
		public Rectangle Boundings {
			get { return new Rectangle(Position, Size); }
		}

		/// <summary>
		/// Called when the overlay shall be redrawn
		/// </summary>
		public RenderDelegate Renderer {
			get { return m_Renderer; }
			set { m_Renderer = value; }
		}

		/// <summary>
		/// The pixelformat currently used
		/// Either let this be determined default, or set it to try using the specified value as a start value
		/// </summary>
		public ePixelFormat PixelFormat {
			get { return m_PixelFormat; }
			set { m_PixelFormat = value; }
		}

		/// <summary>
		/// Determines whether alpha is enabled or not
		/// Beware: Is not supported on many (all?) nvidia graphic cards
		/// </summary>
		public bool AlphaEnabled {
			get { return m_AlphaEnabled; }
			set {
				m_AlphaEnabled = value;

				if (m_Device != null) {
					CreateFlags();
				}
			}
		}

		#region IDisposable Members

		/// <summary>
		/// Disposes the overlay and frees resources
		/// </summary>
		public void Dispose() {
			if (m_Primary != null) {
				m_Primary.Dispose();
				m_Primary = null;
			}

			if (m_Buffer != null) {
				m_Buffer.Dispose();
				m_Buffer = null;
			}

			if (m_BackBuffer != null) {
				m_BackBuffer.Dispose();
				m_BackBuffer = null;
			}

			if (m_RenderTarget != null) {
				m_RenderTarget.Dispose();
				m_RenderTarget = null;
			}

			if (m_Device != null) {
				m_Device.Dispose();
				m_Device = null;
			}
		}

		#endregion

		/// <summary>
		/// Gets the directX pixelformat for the specified pixelformat
		/// </summary>
		/// <param name="e">ePixelFormat</param>
		/// <returns>DirectX PixelFormat</returns>
		public static PixelFormat GetPixelFormat(ePixelFormat e) {
			PixelFormat pixelFormat = new PixelFormat();

			switch (e) {
				case ePixelFormat.RGB32:
					pixelFormat.Rgb = true;
					pixelFormat.RgbBitCount = 32;
					pixelFormat.RBitMask = 0xFF0000;
					pixelFormat.GBitMask = 0x00FF00;
					pixelFormat.BBitMask = 0x0000FF;
					break;
				case ePixelFormat.YUY2:
					pixelFormat.FourCC = 0x32595559;
					pixelFormat.FourCcIsValid = true;
					break;
			}

			return pixelFormat;
		}

		/// <summary>
		/// Initialises the overlay with the specified Boundings
		/// </summary>
		public void Initialise() {
			m_Device = new Device();
			m_Device.SetCooperativeLevel(null, CooperativeLevelFlags.Normal); //Only a overlay..

			//Create Primary
			{
				SurfaceDescription desc = new SurfaceDescription();
				desc.SurfaceCaps.PrimarySurface = true;
				m_Primary = new Surface(desc, m_Device);
			}

			//Create buffer
			{
				SurfaceDescription desc = new SurfaceDescription();
				desc.Width = Boundings.Width;
				desc.Height = Boundings.Height;
				desc.BackBufferCount = 1;
				desc.SurfaceCaps.Flip = true;
				desc.SurfaceCaps.Overlay = true;
				desc.SurfaceCaps.Complex = true;
				desc.SurfaceCaps.VideoMemory = true;

				//Try which pixelformat works
				do {
					try {
						desc.PixelFormatStructure = GetPixelFormat(m_PixelFormat);
						m_Buffer = new Surface(desc, m_Device);
					}
					catch (DirectXException) {
						m_PixelFormat++;
					}
				} while (m_Buffer == null && Enum.IsDefined(typeof(ePixelFormat), m_PixelFormat)); //Stop if a valid format is found or no one is left

				if (m_Buffer == null) {
					//Bad!
					throw new GraphicsException("Could not create overlay - Either your graphic card does not support any of the available pixelformats or overlays in general.");
				}
			}

			//Create backbuffer
			{
				SurfaceCaps caps = new SurfaceCaps();
				caps.BackBuffer = true;
				m_BackBuffer = m_Buffer.GetAttachedSurface(caps);
			}

			//Create rendertarget
			{
				//We use a Bitmap, as it works on every graphicscard, a RGB-Surface wont
				m_RenderTarget = new Bitmap(Boundings.Width, Boundings.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
			}

			CreateFlags();
		}

		/// <summary>
		/// Creates the render flags
		/// </summary>
		public void CreateFlags() {
			//Create Flags
			{
				m_Flags = OverlayFlags.Show;

				if (AlphaEnabled) {
					m_Flags |= OverlayFlags.Effects | OverlayFlags.KeySourceOverride;
				}
			}

			//Create Effects
			{
				m_Effects = new OverlayEffects();

				//Transparency
				if (AlphaEnabled) {
					ColorKey key = new ColorKey();
					key.ColorSpaceHighValue = key.ColorSpaceLowValue = 0; //Make Black (0,0,0) transparent
					m_Effects.DestinationColorKey = key;
				}
			}
		}

		/// <summary>
		/// Renders (Updates) the overlay
		/// * Calls the RenderDelegate
		/// </summary>
		public void Update() {
			if (m_Device == null) {
				return;
			}

			try {
				if (m_Renderer != null) {
					//Draw on the backbuffer
					Graphics g = Graphics.FromImage(m_RenderTarget);
					//g.Clear(Color.Black);
					m_Renderer(g);
					Blit(m_RenderTarget, m_BackBuffer);
				}

				//Flip maps
				m_Buffer.Flip(null, FlipFlags.Wait);

				//Render
				m_Buffer.UpdateOverlay(m_Primary, Boundings, m_Flags, m_Effects);
			}
			catch (SurfaceLostException) {
				//May occur, but not bad - TestCooperativeLevel wont work in some D3D apps, but overlay will still render
				if (m_Device.TestCooperativeLevel()) {
					m_Device.RestoreAllSurfaces();
				}
			}
		}

		/// <summary>
		/// Hides the overlay from screen
		/// Can be undone by calling Update()
		/// </summary>
		public void Hide() {
			if (m_Device == null) {
				return;
			}

			m_Buffer.UpdateOverlay(m_Primary, Boundings, OverlayFlags.Hide);
		}

		/// <summary>
		/// Blits the RGB Bitmap to the specified surface
		/// </summary>
		/// <param name="src">RGB Bitmap</param>
		/// <param name="dest">Surface</param>
		public void Blit(Bitmap src, Surface dest) {
			switch (m_PixelFormat) {
				case ePixelFormat.RGB32:
					BlitRGB32(src, dest);
					break;
				case ePixelFormat.YUY2:
					BlitYUY2(src, dest);
					break;
			}
		}

		/// <summary>
		/// Blits the RGB Bitmap to a YUY2 surface
		/// </summary>
		/// <param name="src">RGB Bitmap</param>
		/// <param name="dest">YUY2 Surface</param>
		public unsafe void BlitYUY2(Bitmap src, Surface dest) {
			BitmapData ds =
				src.LockBits(new Rectangle(0, 0, src.Width, src.Height), ImageLockMode.ReadOnly,
				             System.Drawing.Imaging.PixelFormat.Format24bppRgb);
			try
			{
				LockedData dd = dest.Lock(LockFlags.WriteOnly);

				try {
					int ps = ds.Stride - (ds.Width*3);
					byte[] pd = new byte[dd.Pitch - (dd.Width*2)];

					byte* ptr = (byte*) ds.Scan0;

					for (int h = 0; h < ds.Height; h++) {
						for (int w = 0; w < ds.Width; w += 2) {
							byte[] dbuf = new byte[4]; //2 pixel (2x16bit)

							byte r1 = ptr[0];
							byte g1 = ptr[1];
							byte b1 = ptr[2];

							ptr += 3;

							byte r2 = ptr[0];
							byte g2 = ptr[1];
							byte b2 = ptr[2];

							ptr += 3;

							//Dont ask me for the conversion formulas - They are from FourCC and a bit of own modifications to match colors better
							dbuf[0] = (byte) Math.Min(255, (0.230*r1) + (0.600*g1) + (0.170*b1)); //Yo - luminescent 1
							dbuf[2] = (byte) Math.Min(255, (0.230*r2) + (0.600*g2) + (0.170*b2)); //Y1 - luminescent 2
							dbuf[1] = (byte) Math.Min(255, +(0.439*r1) - (0.368*g1) - (0.071*b1) + 128); //Ux - same for both
							dbuf[3] = (byte) Math.Min(255, -(0.148*r1) - (0.291*g1) + (0.439*b1) + 128); //Vx - same for both

							dd.Data.Write(dbuf, 0, dbuf.Length);
						}
						ptr += ps;

						if (pd.Length > 0) {
							dd.Data.Write(pd, 0, pd.Length);
						}
					}
				}
				catch (Exception e)
				{
					MessageBox.Show(e.ToString());
				}
				finally {
					dest.Unlock();
				}
			}
			catch (Exception e) {
				MessageBox.Show(e.ToString());
			}
			finally {
				src.UnlockBits(ds);
			}
		}

		/// <summary>
		/// Blits the RGB-Bitmap to a RGB32 Surface
		/// </summary>
		/// <param name="src">RGB Bitmap</param>
		/// <param name="dest">RGB32 Surface</param>
		public unsafe void BlitRGB32(Bitmap src, Surface dest) {
			BitmapData ds =
				src.LockBits(new Rectangle(0, 0, src.Width, src.Height), ImageLockMode.ReadOnly,
				             System.Drawing.Imaging.PixelFormat.Format24bppRgb);
			try {
				LockedData dd = dest.Lock(LockFlags.WriteOnly);

				try
				{
					int ps = ds.Stride - (ds.Width * 3);
					byte[] pd = new byte[dd.Pitch - (dd.Width * 4)];

					byte* ptr = (byte*)ds.Scan0;

					for (int h = 0; h < ds.Height; h++)
					{
						for (int w = 0; w < ds.Width; w += 1)
						{
							byte[] dbuf = new byte[4]; //2 pixel (2x16)

							byte r = ptr[0];
							byte g = ptr[1];
							byte b = ptr[2];

							ptr += 3;

							dbuf[0] = r;
							dbuf[1] = g;
							dbuf[2] = b;

							dd.Data.Write(dbuf, 0, dbuf.Length);
						}

						ptr += ps;

						if (pd.Length > 0)
						{
							dd.Data.Write(pd, 0, pd.Length);
						}
					}
				}
				finally {
					dest.Unlock();
				}
			}
			finally {
				src.UnlockBits(ds);
			}
		}
	}

	/// <summary>
	/// Delegate for rendering
	/// </summary>
	/// <param name="g">Graphics Object</param>
	public delegate void RenderDelegate(Graphics g);
}