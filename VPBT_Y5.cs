using System;
using System.Collections.Generic;
using Yarhl.IO;

namespace SoundLib
{
    public class VPBT_Y5
    {
        public List<string> VolumePresets = new List<string>();

        /// <summary>
        /// For reading from sound_def
        /// </summary>m>
        public static VPBT_Y5 ReadFromStream(DataReader reader, int usnbCount)
        {
            return ReadVPBTData(reader, usnbCount);
        }

        private static VPBT_Y5 ReadVPBTData(DataReader reader, int vpbtCount)
        {
            VPBT_Y5 vpbt = new VPBT_Y5();

            for (int i = 0; i < vpbtCount; i++)
            {
                int namePtr = reader.ReadInt32();
                reader.Stream.RunInPosition(delegate { vpbt.VolumePresets.Add(reader.ReadString()); }, namePtr);
                reader.Stream.Position += 12;
            }

            return vpbt;
        }
    }
}
