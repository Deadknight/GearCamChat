/*
 * Bu sýnýflar Foole'nin sýnýflarý taban alýnarak hazýrlanmýþtýr.
 */

using System;
using System.IO;
using System.Text;

namespace GearCamChatServer
{
    public class GAISWriter : BinaryWriter
    {
        public GAISWriter()
            : base(new MemoryStream())
        {
        }

        public GAISWriter(Stream fs)
            : base(fs)
        {
        }

        public override void Write(string Text)
        {
            if (Text != null) Write(Encoding.Default.GetBytes(Text));
            Write((byte)0); // String terminator
        }

        public void Write(GAISWriter ww)
        {
            this.Write(ww.ToArray());
        }

        public byte[] ToArray()
        {
            return ((MemoryStream)BaseStream).ToArray();
        }

        public static implicit operator byte[](GAISWriter ww)
        {
            return ww.ToArray();
        }
    }
}
