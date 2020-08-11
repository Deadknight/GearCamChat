using System.Drawing;

namespace GearCamChat.Helpers {
	/// <summary>
	/// Draw-Helper
	/// Used for drawing on the overlay
	/// Offers some general Methods to have some sort of visual style (e.g. Fonts)
	/// </summary>
	public static class Draw {
		private static Font m_FontSmall;
		private static Font m_FontStandard;
		private static Font m_FontVerySmall;

		/// <summary>
		/// Standard font
		/// </summary>
		public static Font FontStandard {
			get {
				if (m_FontStandard == null) {
					m_FontStandard = new Font("Arial", 18, FontStyle.Bold);
				}

				return m_FontStandard;
			}
		}

		/// <summary>
		/// Small font
		/// </summary>
		public static Font FontSmall {
			get {
				if (m_FontSmall == null) {
					m_FontSmall = new Font("Arial", 12, FontStyle.Bold);
				}

				return m_FontSmall;
			}
		}

		/// <summary>
		/// Very small font
		/// </summary>
		public static Font FontVerySmall {
			get {
				if (m_FontVerySmall == null) {
					m_FontVerySmall = new Font("Arial", 10);
				}

				return m_FontVerySmall;
			}
		}

		/// <summary>
		/// Draws a shaded string on the graphics object
		/// </summary>
		/// <param name="g">Graphics</param>
		/// <param name="f">Font</param>
		/// <param name="x">X</param>
		/// <param name="y">Y</param>
		/// <param name="text">Text</param>
		public static void DrawString(Graphics g, Font f, int x, int y, string text) {
			g.DrawString(text, f, new SolidBrush(Program.General.TextShadowColor), x + 1, y + 1);
			g.DrawString(text, f, new SolidBrush(Program.General.TextColor), x, y);
		}

		/// <summary>
		/// Draws a centered shaded string on the graphics object
		/// </summary>
		/// <param name="g">Graphics</param>
		/// <param name="f">Font</param>
		/// <param name="x">X</param>
		/// <param name="y">Y</param>
		/// <param name="text">Text</param>
		public static void DrawCenteredString(Graphics g, Font f, int x, int y, string text) {
			SizeF s = g.MeasureString(text, f);
			DrawString(g, f, x - ((int) s.Width/2), y - ((int) s.Height/2), text);
		}
	}
}