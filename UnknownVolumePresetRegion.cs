using System;
using System.Collections.Generic;
using Yarhl.IO;

namespace SoundLib
{
    public class UnknownVolumePresetRegion
    {
        public List<UnknownVolumePresetData> Datas = new List<UnknownVolumePresetData>();


        /// <summary>
        /// For reading from sound_def
        /// </summary>m>
        public static UnknownVolumePresetRegion ReadFromStream(DataReader reader, int volumePresetCount)
        {
            return ReadUnknownVolumePresetData(reader, volumePresetCount);
        }

        private static UnknownVolumePresetRegion ReadUnknownVolumePresetData(DataReader reader, int volumePresetCount)
        {
            UnknownVolumePresetRegion unknownVolumeReg = new UnknownVolumePresetRegion();

            for (int i = 0; i < volumePresetCount; i++)
            {
                UnknownVolumePresetData dat = new UnknownVolumePresetData();
                dat.Data = reader.ReadBytes(2624);

                unknownVolumeReg.Datas.Add(dat);
            }

            return unknownVolumeReg;
        }
    }
}
