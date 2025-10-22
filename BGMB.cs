using System;
using System.Collections.Generic;
using Yarhl.IO;

namespace SoundLib
{
    public class BGMB
    {
        public List<BGMBEntry> Files = new List<BGMBEntry>();


        /// <summary>
        /// For reading from sound_def
        /// </summary>m>
        public static BGMB ReadFromStream(DataReader reader, int fileCount)
        {
            return ReadBGMBData(reader, fileCount);
        }

        private static BGMB ReadBGMBData(DataReader reader, int fileCount)
        {
            BGMB bgmb = new BGMB();

            for (int i = 0; i < fileCount; i++)
            {
                BGMBEntry entry = new BGMBEntry();
                entry.Unknown = reader.ReadInt32();
                int namePtr = reader.ReadInt32();

                if (namePtr > 0)
                    reader.Stream.RunInPosition(delegate { entry.Name = reader.ReadString(); }, namePtr);

                entry.SoundCategory = reader.ReadInt32();
                entry.IsValid = reader.ReadInt32();

                bgmb.Files.Add(entry);
            }

            return bgmb;
        }
    }
}
