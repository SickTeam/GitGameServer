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
        public static void Write(this Stream stream, int value)
        {
            stream.Write(BitConverter.GetBytes(value), 0, 4);
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

        public static int ReadInt32(this Stream stream)
        {
            byte[] buffer = new byte[4];
            stream.Read(buffer, 0, 4);
            return BitConverter.ToInt32(buffer, 0);
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