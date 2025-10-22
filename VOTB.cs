using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using Yarhl.IO;

namespace SoundLib
{
    public class VOTB
    {
        public List<VOTBSoundCategory> Categories = new List<VOTBSoundCategory>();
        public List<VOTBVoicer> Voicers = new List<VOTBVoicer>();

        public static VOTB Read(string filePath)
        {
            if (!File.Exists(filePath))
                return null;

            byte[] buffer = File.ReadAllBytes(filePath);

            DataStream readStream = DataStreamFactory.FromArray(buffer, 0, buffer.Length);
            DataReader reader = new DataReader(readStream) { Endianness = EndiannessMode.BigEndian, DefaultEncoding = Encoding.GetEncoding(932) };

            reader.Stream.Position = 16;

            int soundCategoryCount = reader.ReadInt32();
            int voicerCount = reader.ReadInt32();

            reader.Stream.Position = 32;

            return ReadVOTBData(reader, voicerCount, soundCategoryCount);
        }

        /// <summary>
        /// For reading from sound_def
        /// </summary>m>
        public static VOTB ReadFromStream(DataReader reader, int voicerCount, int soundCategoryCount)
        {
            return ReadVOTBData(reader, voicerCount, soundCategoryCount);
        }

        private static VOTB ReadVOTBData(DataReader reader, int voicerCount, int soundCategoryCount)
        {
            VOTB table = new VOTB();

            VOTBVoicer[] voicers = new VOTBVoicer[voicerCount];

            for (int i = 0; i < soundCategoryCount; i++)
            {
                int soundCategoryPtr = reader.ReadInt32();

                reader.Stream.RunInPosition(delegate
                {
                    VOTBSoundCategory category = new VOTBSoundCategory();
                    category.SoundCount = reader.ReadInt32();

                    table.Categories.Add(category);

                    if (category.SoundCount <= 0)
                    {
                        category.IsValid = false;
                        return;
                    }

                    category.IsValid = true;

                    reader.Stream.Position += 12;

                    for (int k = 0; k < category.SoundCount; k++)
                    {
                        VOTBUnkStructure dat = new VOTBUnkStructure();
                        dat.Unk1 = reader.ReadInt32();
                        dat.Unk2 = reader.ReadInt32();

                        category.UnkDatas.Add(dat);
                    }

                    for (int k = 0; k < voicerCount; k++)
                    {
                        if (voicers[k] == null)
                        {
                            voicers[k] = new VOTBVoicer();
                            voicers[k].ValidCategories = new bool[soundCategoryCount];
                            voicers[k].CategorySounds = new List<VOTBSound>[soundCategoryCount];
                        }

                        var tableVoicer = voicers[k];
                        int soundTablePointer = reader.ReadInt32();

                        tableVoicer.CategorySounds[i] = new List<VOTBSound>();

                        if (soundTablePointer < 0)
                        {
                            tableVoicer.ValidCategories[i] = false;
                            continue;
                        }
                        else
                        {
                            tableVoicer.ValidCategories[i] = true;

                            reader.Stream.RunInPosition(delegate
                            {
                                for (int s = 0; s < category.SoundCount; s++)
                                {
                                    VOTBSound sound = new VOTBSound(reader.ReadInt16(), reader.ReadInt16());
                                    tableVoicer.CategorySounds[i].Add(sound);
                                }
                            }, soundTablePointer);
                        }
                    }

                }, soundCategoryPtr);
            }

            table.Voicers = voicers.ToList();

            return table;
        }

        public static void Write(string path, VOTB table)
        {
            DataWriter writer = new DataWriter(new DataStream()) { Endianness = EndiannessMode.BigEndian };

            //Magic
            writer.Write(new byte[] { 0x76, 0x6f, 0x74, 0x62 });
            //Endian
            writer.Write(new byte[] { 0x2, 0x1, 0x0, 0x0 });
            //Unknown
            writer.Write(new byte[] { 0x0, 0x4, 0x0, 0x0 });
            //Padding
            writer.Write(0);

            writer.Write(table.Categories.Count);
            writer.Write(table.Voicers.Count);
            writer.Write(0);
            writer.Write(0);

            long categoriesPtrStart = writer.Stream.Position;

            //Pointers
            writer.WriteTimes(0, table.Categories.Count * 4);

            //Padding
            writer.WriteTimes(0, 144);

            Dictionary<VOTBSoundCategory, long> categoryLocations = new Dictionary<VOTBSoundCategory, long>();

            for(int i = 0; i < table.Categories.Count; i++)
            {
                var category = table.Categories[i];

                categoryLocations[category] = writer.Stream.Position;

                writer.Write(category.SoundCount);
                writer.WriteTimes(0, 12);

                if (!category.IsValid)
                {
                    writer.WriteTimes(0xFF, table.Voicers.Count * 4);
                    continue;
                }

                Dictionary<VOTBVoicer, long> soundTableLocations = new Dictionary<VOTBVoicer, long>();
               
                foreach(var unkData in category.UnkDatas)
                {
                    writer.Write(unkData.Unk1);
                    writer.Write(unkData.Unk2);
                }

                long categoryVoicerPtrsStart = writer.Stream.Position;

                writer.WriteTimes(0, table.Voicers.Count * 4);

                foreach (var voicer in table.Voicers)
                {
                    long soundTableLocation = writer.Stream.Position;
                    soundTableLocations[voicer] = soundTableLocation;

                    if (!voicer.ValidCategories[i])
                    {
                        writer.Write(-1);
                        soundTableLocations[voicer] = -1;
                        continue;
                    }
                    else
                    {
                        foreach(var soundTableVal in voicer.CategorySounds[i])
                        {
                            writer.Write(soundTableVal.Cuesheet);
                            writer.Write(soundTableVal.Sound);
                        }
                    }
                }

                writer.Stream.RunInPosition(delegate
                {
                    foreach (var val in soundTableLocations.Values)
                        writer.Write((int)val);
                }, categoryVoicerPtrsStart);
            }

            writer.Stream.Position = categoriesPtrStart;

            foreach(long val in categoryLocations.Values)
            {
                writer.Write((int)val);
            }

            writer.Stream.WriteTo(path);
        }
    }
}
