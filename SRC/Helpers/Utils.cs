using System;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;

namespace GearCamChat.Helpers {
	/// <summary>
	/// Helpers
	/// </summary>
	public static class Utils {
		/// <summary>
		/// Min's and Max'es a value
		/// </summary>
		/// <param name="val">Value</param>
		/// <param name="min">Min Value</param>
		/// <param name="max">Max Value</param>
		/// <returns></returns>
		public static int Range(int val, int min, int max) {
			return Math.Min(max, Math.Max(min, val));
		}

		/// <summary>
		/// Min/Maxes a color to the range 0-255
		/// </summary>
		/// <param name="c">Color component</param>
		/// <returns>Ranged color component</returns>
		public static int Color(int c) {
			return Range(c, 0, 255);
		}

        public static Byte[] ImageToByteArray(Image imageIn)
        {
            MemoryStream ms = new MemoryStream();
            imageIn.Save(ms, ImageFormat.Gif);
            return ms.ToArray();
        }

        public static Image ByteArrayToImage(Byte[] byteArrayIn)
        {
            MemoryStream ms = new MemoryStream(byteArrayIn);
            Image returnImage = Image.FromStream(ms);
            return returnImage;
        }
	}
}