using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace GitGameServer
{
    public static class StreamExtension
    {
        public static void Write(this Stream stream, bool value)
        {
            stream.WriteByte((byte)(value ? 1 : 0));
        }
        public static void Write(this Stream stream, bool? value)
        {
            stream.WriteByte((byte)(value.HasValue ? (value.Value ? 1 : 2) : 0));
        }
        public static void Write(this Stream stream, int value)
        {
            stream.Write(BitConverter.GetBytes(value), 0, 4);
        }
        public static void Write(this Stream stream, long value)
        {
            stream.Write(BitConverter.GetBytes(value), 0, 8);
        }
        public static void Write(this Stream stream, string value)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(value);
            stream.Write(buffer.Length);
            stream.Write(buffer, 0, buffer.Length);
        }

        public static void WriteZeros(this Stream stream, int count)
        {
            int bufferSize = 8192;
            byte[] buffer = new byte[bufferSize];

            while (count > bufferSize) { stream.Write(buffer, 0, bufferSize); count -= bufferSize; }
            if (bufferSize > 0) stream.Write(buffer, 0, count);
        }

        public static bool ReadBoolean(this Stream stream)
        {
            return stream.ReadByte() != 0;
        }
        public static bool? ReadNullBoolean(this Stream stream)
        {
            var b = stream.ReadByte();
            if (b == 0)
                return null;
            else
                return b == 1;
        }
        public static int ReadInt32(this Stream stream)
        {
            byte[] buffer = new byte[4];
            stream.Read(buffer, 0, 4);
            return BitConverter.ToInt32(buffer, 0);
        }
        public static long ReadInt64(this Stream stream)
        {
            byte[] buffer = new byte[8];
            stream.Read(buffer, 0, 8);
            return BitConverter.ToInt64(buffer, 0);
        }
        public static string ReadString(this Stream stream)
        {
            int len = ReadInt32(stream);
            byte[] buffer = new byte[len];
            stream.Read(buffer, 0, buffer.Length);
            return Encoding.UTF8.GetString(buffer);
        }
    }
}