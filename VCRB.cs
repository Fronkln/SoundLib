using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Yarhl.IO;

namespace SoundLib
{
    public class VCRB
    {
        public List<string> Voicers = new List<string>();

        public static VCRB Read(string filePath)
        {
            if (!File.Exists(filePath))
                return null;

            byte[] buffer = File.ReadAllBytes(filePath);   

            DataStream readStream = DataStreamFactory.FromArray(buffer, 0, buffer.Length);
            DataReader reader = new DataReader(readStream) { Endianness = EndiannessMode.BigEndian, DefaultEncoding = Encoding.GetEncoding(932) };

            //just reading voicer names only for now
            reader.Stream.Position = 16;

            int voicerCount = reader.ReadInt32();
            reader.Stream.Position = 28;       
            int voicerPtr = reader.ReadInt32();

            VCRB voicer = new VCRB();

            reader.Stream.Position = voicerPtr;

            for(int i = 0; i < voicerCount; i++)
            {
                reader.Stream.RunInPosition(delegate
                {
                    voicer.Voicers.Add(reader.ReadString());
                }, reader.ReadInt32());
            }

            return voicer;

        }
    }
}
