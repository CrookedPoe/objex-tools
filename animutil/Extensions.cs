using System;
using System.Text;

namespace animutil
{
    public static class Extenstions
    {
        public static dynamic ReadBEBuffer(this byte[] buf, int count, int org)
        {
            dynamic ret = 0;
            for (int index = 0; index < count; index++)
            {
                ret <<= 8;
                ret |= buf[org + index];
            }
            return ret;
        }
        public static dynamic ReadLEBuffer(this byte[] buf, int count, int org)
        {
            dynamic ret = 0;
            for (int index = count - 1; index > -1; index--)
            {
                ret <<= 8;
                ret |= buf[org + index];
            }
            return ret;
        }

        public static byte[] BlockCopy(this byte[] buf, int org, int size)
        {
            byte[] arrayCopied = new byte[size];
            for (int i = org; i < (org + size); i++)
            {
                arrayCopied[i - org] = buf[i];
            }
            return arrayCopied;
        }

        public static byte[] BlockCopy(this byte[] buf, int org, int start, int end)
        {
            byte[] arrayCopied = new byte[(end - start)];
            for (int i = org; i < (org + (end - start)); i++)
            {
                arrayCopied[i - org] = buf[i];
            }
            return arrayCopied;
        }

        public static string ToByteString(this byte[] buf)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < buf.Length; i++)
            {
                sb.Append(buf[i].ToString("X2"));
            }
            return sb.ToString();
        }

        public static Byte ReadUByte(this byte[] buf, int org)
        {
            return (Byte)(buf[org]);
        }

        public static SByte ReadSByte(this byte[] buf, int org)
        {
            return (SByte)(buf[org]);
        }

        public static Int16 BEReadInt16(this byte[] buf, int org)
        {
            return (Int16)(ReadBEBuffer(buf, 2, org));
        }

        public static UInt16 BEReadUInt16(this byte[] buf, int org)
        {
            return (UInt16)(ReadBEBuffer(buf, 2, org));
        }

        public static Int16 LEReadInt16(this byte[] buf, int org)
        {
            return (Int16)(ReadLEBuffer(buf, 2, org));
        }

        public static UInt16 LEReadUInt16(this byte[] buf, int org)
        {
            return (UInt16)(ReadLEBuffer(buf, 2, org));
        }

        public static Int32 BEReadInt32(this byte[] buf, int org)
        {
            return (Int32)(ReadBEBuffer(buf, 4, org));
        }

        public static UInt32 BEReadUInt32(this byte[] buf, int org)
        {
            return (UInt32)(ReadBEBuffer(buf, 4, org));
        }

        public static Int32 LEReadInt32(this byte[] buf, int org)
        {
            return (Int32)(ReadLEBuffer(buf, 4, org));
        }

        public static UInt32 LEReadUInt32(this byte[] buf, int org)
        {
            return (UInt32)(ReadLEBuffer(buf, 4, org));
        }

        public static Int64 BEReadInt64(this byte[] buf, int org)
        {
            return (Int64)(ReadBEBuffer(buf, 8, org));
        }

        public static UInt64 BEReadUInt64(this byte[] buf, int org)
        {
            return (UInt64)(ReadBEBuffer(buf, 8, org));
        }

        public static Int64 LEReadInt64(this byte[] buf, int org)
        {
            return (Int64)(ReadLEBuffer(buf, 8, org));
        }

        public static UInt64 LEReadUInt64(this byte[] buf, int org)
        {
            return (UInt64)(ReadLEBuffer(buf, 8, org));
        }
    }
}