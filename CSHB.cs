using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Yarhl.IO;

namespace SoundLib
{
    public class CSHB
    {
        public List<CSHBCuesheet> Cuesheets = new List<CSHBCuesheet>();


        public static CSHB Read(string filePath)
        {
            if (!File.Exists(filePath))
                return null;

            byte[] buffer = File.ReadAllBytes(filePath);

            DataStream readStream = DataStreamFactory.FromArray(buffer, 0, buffer.Length);
            DataReader reader = new DataReader(readStream) { Endianness = EndiannessMode.BigEndian, DefaultEncoding = Encoding.GetEncoding(932) };

            reader.Stream.Position = 16;
            int count = reader.ReadInt32();
            reader.Stream.Position = 32;

            return ReadCSHBData(reader, count);
        }

        public static void Write(string filePath, CSHB cshb)
        {
            DataWriter writer = new DataWriter(new DataStream()) { Endianness = EndiannessMode.BigEndian };
            writer.Write(new byte[] { 0x63, 0x73, 0x68, 0x62 }); //CSHB
            writer.Write(0x33619968); //Endian
            writer.Write((short)4); //Flag?
            writer.WriteTimes(0, 6);
            writer.Write(cshb.Cuesheets.Count);
            writer.WriteTimes(0, 12);

            foreach (var cuesheet in cshb.Cuesheets)
            {
                writer.Write(cuesheet.Cuesheet);
                writer.Write(cuesheet.Category);
                writer.Write(cuesheet.Flags);
            }

            writer.Align(16);
        }


        /// <summary>
        /// For reading from sound_def
        /// </summary>m>
        public static CSHB ReadFromStream(DataReader reader, int cuesheetCount)
        {
            return ReadCSHBData(reader, cuesheetCount);
        }

        private static CSHB ReadCSHBData(DataReader reader, int cuesheetCount)
        {
            CSHB cshb = new CSHB();

            for(int i = 0; i < cuesheetCount; i++)
            {
                cshb.Cuesheets.Add(new CSHBCuesheet() 
                { 
                    Cuesheet = reader.ReadInt16(),
                    Category = reader.ReadInt16(), 
                    Flags = reader.ReadInt32() 
                });
            }

            return cshb;
        }
    }
}
