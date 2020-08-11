/*
 * Bu sýnýflar Foole'nin sýnýflarý taban alýnarak hazýrlanmýþtýr.
 */
using System.IO;
using System.Text;

namespace GearCamChat
{
    public class GAISReader : BinaryReader
    {
        public GAISReader(byte[] Data)
            : base(new MemoryStream(Data))
        {
        }

        public GAISReader(Stream fs)
            : base(fs)
        {
        }

        public override string ReadString()
        {
            StringBuilder sb = new StringBuilder();
            while (true)
            {
                byte b;
                //if (Remaining > 0)
                b = ReadByte();
                //else
                //   b = 0;

                if (b == 0)
                    break;
                sb.Append((char)b);
            }
            return sb.ToString();
        }

        public byte[] ReadRemaining()
        {
            MemoryStream ms = (MemoryStream)BaseStream;
            int Remaining = (int)(ms.Length - ms.Position);
            return ReadBytes(Remaining);
        }

        public int Remaining
        {
            get
            {
                MemoryStream ms = (MemoryStream)BaseStream;
                return (int)(ms.Length - ms.Position);
            }
            set
            {
                MemoryStream ms = (MemoryStream)BaseStream;
                if (value <= (ms.Length - ms.Position))
                    ms.Position = value;
            }
        }
        public float ReadFloat()
        {
            return System.BitConverter.ToSingle(ReadBytes(4), 0);
        }


        public byte[] ToArray()
        {
            return ((MemoryStream)BaseStream).ToArray();
        }
    }
}
