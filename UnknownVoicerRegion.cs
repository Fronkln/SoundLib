using System;
using System.Collections.Generic;
using Yarhl.IO;

namespace SoundLib
{
    public class UnknownVoicerRegion
    {
        public List<UnknownVoicerData> VoicerData = new List<UnknownVoicerData>();

        /// <summary>
        /// For reading from sound_def
        /// </summary>m>
        public static UnknownVoicerRegion ReadFromStream(DataReader reader, int voicerCount)
        {
            return ReadUnknownVoicerData(reader, voicerCount);
        }

        private static UnknownVoicerRegion ReadUnknownVoicerData(DataReader reader, int voicerCount)
        {
            UnknownVoicerRegion unkVoicerReg = new UnknownVoicerRegion();

            for (int i = 0; i < voicerCount; i++)
            {
                unkVoicerReg.VoicerData.Add(new UnknownVoicerData() { Data = reader.ReadBytes(48) });
            }

            return unkVoicerReg;
        }
    }
}
