using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yarhl.IO;

namespace SoundLib
{
    public class SoundDef
    {
        public VOTB VoiceTable = new VOTB();
        public CSHB Cuesheets = new CSHB();
        public List<CTGRCategory> Categories = new List<CTGRCategory>();
        public UnknownVoicerRegion UnknownVoicer = new UnknownVoicerRegion();
        public SEPB SEPortSets = new SEPB();
        public SETB SETables = new SETB();
        public BGMB BGMs = new BGMB();
        public BGMT BGMTables = new BGMT();
        public USNB USEN = new USNB();
        public UnknownVolumePresetRegion UnknownVolumePresetRegion = new UnknownVolumePresetRegion();
        public VPBT_Y5 VolumePresets = new VPBT_Y5();

        public static SoundDef Read(string filePath)
        {
            if (!File.Exists(filePath))
                return null;

            byte[] buffer = File.ReadAllBytes(filePath);

            DataStream readStream = DataStreamFactory.FromArray(buffer, 0, buffer.Length);
            DataReader reader = new DataReader(readStream) { Endianness = EndiannessMode.BigEndian, DefaultEncoding = Encoding.GetEncoding(932) };

            string magic = reader.ReadString(4);

            if (magic == "TDNS")
                throw new Exception("Yakuza 3/4 sound_def.bin not supported");

            SoundDef def = new SoundDef();

            int endian = reader.ReadInt32();
            int unknown = reader.ReadInt32();

            reader.Stream.Position += 4;

            int cuesheetCount = reader.ReadInt32();
            int soundCategoryCount = reader.ReadInt32();
            int sePortCount = reader.ReadInt32();
            int voicerCount = reader.ReadInt32();
            int voicerCategoryCount = reader.ReadInt32();

            long voicerCategoryPtr = reader.Stream.Position;

            reader.Stream.Position += voicerCategoryCount * 4;
            reader.Stream.Position += 184;

            int soundCategoriesPtr = reader.ReadInt32();
            int sePortSetPointer = reader.ReadInt32();
            int unknown2 = reader.ReadInt32();
            int unknownVoicerRelatedPtr = reader.ReadInt32();
            int unknown3 = reader.ReadInt32();
            int unknown4 = reader.ReadInt32();
            int seTableCount = reader.ReadInt32();
            int seTablePointer = reader.ReadInt32();
            int bgmCount = reader.ReadInt32();
            int bgmPointer = reader.ReadInt32();
            int bgmTableCount = reader.ReadInt32();
            int bgmTablePointer = reader.ReadInt32();
            int usenSoundCount = reader.ReadInt32();
            int usenSoundPointer = reader.ReadInt32();
            int volumePresetCount = reader.ReadInt32();
            int unknownVolumePresetRelatedPointer = reader.ReadInt32();
            int unk5 = reader.ReadInt32();
            int volumePresetPointer = reader.ReadInt32();

            reader.Stream.Position += 8;

            long cuesheetsPointer = reader.Stream.Position;

            reader.Stream.Position = voicerCategoryPtr;
            def.VoiceTable = VOTB.ReadFromStream(reader, voicerCount, voicerCategoryCount);

            reader.Stream.Position = cuesheetsPointer;
            def.Cuesheets = CSHB.ReadFromStream(reader, cuesheetCount);

            reader.Stream.Position = soundCategoriesPtr;
            def.Categories = CTGR.ReadFromStream(reader, soundCategoryCount);

            reader.Stream.Position = unknownVoicerRelatedPtr;
            def.UnknownVoicer = UnknownVoicerRegion.ReadFromStream(reader, voicerCount);

            reader.Stream.Position = sePortSetPointer;
            def.SEPortSets = SEPB.ReadFromStream(reader, sePortCount);

            reader.Stream.Position = seTablePointer;
            def.SETables = SETB.ReadFromStream(reader, seTableCount);

            reader.Stream.Position = bgmPointer;
            def.BGMs = BGMB.ReadFromStream(reader, bgmCount);

            reader.Stream.Position = bgmTablePointer;
            def.BGMTables = BGMT.ReadFromStream(reader, bgmTableCount);

            reader.Stream.Position = usenSoundPointer;
            def.USEN = USNB.ReadFromStream(reader, usenSoundCount);

            reader.Stream.Position = unknownVolumePresetRelatedPointer;
            def.UnknownVolumePresetRegion = UnknownVolumePresetRegion.ReadFromStream(reader, volumePresetCount);

            reader.Stream.Position = volumePresetPointer;
            def.VolumePresets = VPBT_Y5.ReadFromStream(reader, volumePresetCount);

            return def;
        }

        public static void Write(string path, SoundDef def)
        {
            DataWriter writer = new DataWriter(new DataStream()) { Endianness = EndiannessMode.BigEndian };

            //Magic
            writer.Write(new byte[] { 0x73, 0x6E, 0x64, 0x64 });
            writer.Write(33619968);
            writer.Write(262144);
            writer.Write(0);

            int voicerCategoriesCount = def.VoiceTable.Voicers[0].CategorySounds.Length;

            writer.Write(def.Cuesheets.Cuesheets.Count);
            writer.Write(def.Categories.Count);
            writer.Write(def.SEPortSets.SEPortSets.Count);
            writer.Write(def.VoiceTable.Voicers.Count);
            writer.Write(voicerCategoriesCount);

            long voicerCategoriesPointersStart = writer.Stream.Position;
            writer.WriteTimes(0, voicerCategoriesCount * 4);
            writer.WriteTimes(0, 184);

            long header2Start = writer.Stream.Position;
            writer.WriteTimes(0, 80);

            long cshbStart = WriteCSHB(writer, def.Cuesheets);
            writer.Align(16);
            long ctgrStart = WriteCTGR(writer, def.Categories);
            writer.Align(16);
            long sepbStart = WriteSEPB(writer, def.SEPortSets);
            writer.Align(16);
            long unknownVoiceRegStart = WriteUnknownVoicerRegion(writer, def.UnknownVoicer);
            writer.Align(16);
            List<long> votbPointers = WriteVOTB(writer, def.VoiceTable);
            writer.Align(16);

            writer.Stream.RunInPosition(
                delegate
                {
                    foreach (long ptr in votbPointers)
                        writer.Write((int)ptr);
                }, voicerCategoriesPointersStart);


            long setbStart = WriteSETB(writer, def.SETables);
            writer.Align(16);
            long bgmbStart = WriteBGMB(writer, def.BGMs);
            writer.Align(16);
            long bgmtStart = WriteBGMT(writer, def.BGMTables);
            writer.Align(16);
            long usnbStart = WriteUSNB(writer, def.USEN);
            writer.Align(16);
            long unkVolumePresetStart = WriteUnknownVolumePreset(writer, def.UnknownVolumePresetRegion);
            writer.Align(16);
            long vpbtStart = WriteVPBT(writer, def.VolumePresets);
            writer.Align(16);


            writer.Stream.Position = header2Start;
            writer.Write((int)ctgrStart);
            writer.Write((int)sepbStart);
            writer.Write(4);
            writer.Write((int)unknownVoiceRegStart);
            writer.Write(12890);
            writer.Write(0);
            writer.Write(def.SETables.SETables.Count);
            writer.Write((int)setbStart);
            writer.Write(def.BGMs.Files.Count);
            writer.Write((int)bgmbStart);
            writer.Write(def.BGMTables.Tables.Count);
            writer.Write((int)bgmtStart);
            writer.Write(def.USEN.Entries.Count);
            writer.Write((int)usnbStart);
            writer.Write(def.VolumePresets.VolumePresets.Count);
            writer.Write((int)unkVolumePresetStart);
            writer.Write(17);
            writer.Write((int)vpbtStart);

            writer.Stream.WriteTo(path);
        }

        private static long WriteCSHB(DataWriter writer, CSHB cshb)
        {
            List<long> pointersList = new List<long>();

            long cshbStart = writer.Stream.Position;

            foreach (var cuesheet in cshb.Cuesheets)
            {
                pointersList.Add(writer.Stream.Position);

                writer.Write(cuesheet.Cuesheet);
                writer.Write(cuesheet.Category);
                writer.Write(cuesheet.Flags);
            }

            return cshbStart;
        }

        private static long WriteCTGR(DataWriter writer, List<CTGRCategory> ctgr)
        {
            long ctgrStart = writer.Stream.Position;

            writer.WriteTimes(0, ctgr.Count * 64);

            StringTable table = new StringTable(writer, writer.Stream.Position);

            foreach (var category in ctgr)
                table.Write(category.Name);


            writer.Stream.Position = ctgrStart;

            foreach (var category in ctgr)
            {
                writer.Write(category.SizeLimit);
                writer.Write(category.Unknown);
                writer.Write(category.Unknown2);
                writer.Write(category.Unknown3);
                writer.Write((int)table.GetPosition(category.Name));
                writer.Write(category.Unknown4);
                writer.Write(category.Unknown5);
                writer.Write(category.Unknown6);
                writer.Write(category.Unknown7);
                writer.Write(category.Unknown8);
                writer.Write(category.Unknown9);
                writer.Write(category.Unknown10);
                writer.Write(category.Unknown11);
                writer.Write(category.Unknown12);
                writer.Write(category.Unknown13);
                writer.Write(category.Unknown14);
            }

            writer.Stream.Position = table.Position;

            return ctgrStart;
        }

        private static long WriteSEPB(DataWriter writer, SEPB sepb)
        {
            List<long> pointers = new List<long>();

            long sepbStart = writer.Stream.Position;

            writer.WriteTimes(0, sepb.SEPortSets.Count * 16);

            foreach (var portSet in sepb.SEPortSets)
            {
                pointers.Add(writer.Stream.Position);

                foreach (SEPBSEPort port in portSet.Ports)
                {
                    writer.Write(port.Unknown1);
                    writer.Write(port.Unknown2);
                    writer.Write(port.Unknown3);
                    writer.Write(0);
                }
            }

            writer.Stream.RunInPosition
                (
                delegate
                {
                    for (int i = 0; i < pointers.Count; i++)
                    {
                        var portSet = sepb.SEPortSets[i];

                        writer.Write(portSet.Ports.Count);
                        writer.Write((int)pointers[i]);
                        writer.WriteTimes(0, 8);
                    }
                }, sepbStart);

            return sepbStart;
        }

        private static long WriteUnknownVoicerRegion(DataWriter writer, UnknownVoicerRegion reg)
        {
            long regStart = writer.Stream.Position;

            foreach (var unkVoicerDat in reg.VoicerData)
            {
                writer.Write(unkVoicerDat.Data);
            }

            return regStart;
        }

        private static List<long> WriteVOTB(DataWriter writer, VOTB votb)
        {
            List<long> pointers = new List<long>();

            for (int i = 0; i < votb.Categories.Count; i++)
            {
                var category = votb.Categories[i];

                long categoryStart = writer.Stream.Position;
                pointers.Add(categoryStart);

                writer.Write(category.SoundCount);
                writer.WriteTimes(0, 12);

                if (!category.IsValid)
                {
                    writer.WriteTimes(0xFF, votb.Voicers.Count * 4);
                    continue;
                }

                Dictionary<VOTBVoicer, long> soundTableLocations = new Dictionary<VOTBVoicer, long>();

                foreach (var unkData in category.UnkDatas)
                {
                    writer.Write(unkData.Unk1);
                    writer.Write(unkData.Unk2);
                }

                long categoryVoicerPtrsStart = writer.Stream.Position;

                writer.WriteTimes(0, votb.Voicers.Count * 4);
                writer.Align(16);

                foreach (var voicer in votb.Voicers)
                {
                    long soundTableLocation = writer.Stream.Position;
                    soundTableLocations[voicer] = soundTableLocation;

                    if (!voicer.ValidCategories[i])
                    {
                        //writer.Write(-1);
                        soundTableLocations[voicer] = -1;
                        continue;
                    }
                    else
                    {
                        foreach (var soundTableVal in voicer.CategorySounds[i])
                        {
                            writer.Write(soundTableVal.Cuesheet);
                            writer.Write(soundTableVal.Sound);
                        }
                    }
                }

                writer.Align(16);

                writer.Stream.RunInPosition(delegate
                {
                    foreach (var val in soundTableLocations.Values)
                        writer.Write((int)val);
                }, categoryVoicerPtrsStart);
            }

            return pointers;
        }

        private static long WriteSETB(DataWriter writer, SETB setb)
        {
            long setbStart = writer.Stream.Position;

            List<long> pointers = new List<long>();

            writer.WriteTimes(0, setb.SETables.Count * 4);

            foreach (var table in setb.SETables)
            {
                pointers.Add(writer.Stream.Position);

                writer.Write(table.Sounds.Count);

                foreach (var sound in table.Sounds)
                {
                    writer.Write(sound.CuesheetID);
                    writer.Write(sound.SoundID);
                }
            }

            writer.Stream.RunInPosition(delegate
            {
                foreach (long ptr in pointers)
                    writer.Write((int)ptr);
            }, setbStart);

            return setbStart;
        }

        private static long WriteBGMB(DataWriter writer, BGMB bgmb)
        {
            long bgmbStart = writer.Stream.Position;

            writer.WriteTimes(0, bgmb.Files.Count * 16);
            writer.Align(16);

            StringTable table = new StringTable(writer, writer.Stream.Position);

            foreach (var bgmFile in bgmb.Files)
                table.Write(bgmFile.Name, true);

            long tableEnd = table.Position;
            writer.Align(16);

            writer.Stream.Position = bgmbStart;

            foreach (var bgmFile in bgmb.Files)
            {
                writer.Write(bgmFile.Unknown);
                writer.Write((int)table.GetPosition(bgmFile.Name));
                writer.Write(bgmFile.SoundCategory);
                writer.Write(bgmFile.IsValid);
            }

            writer.Stream.Position = tableEnd;

            return bgmbStart;
        }

        private static long WriteBGMT(DataWriter writer, BGMT bgmt)
        {
            long bgmtStart = writer.Stream.Position;

            List<long> pointers = new List<long>();

            writer.WriteTimes(0, bgmt.Tables.Count * 4);

            foreach(var bgmTable in bgmt.Tables)
            {
                pointers.Add(writer.Stream.Position);
                writer.Write(bgmTable.BGMIDs.Count);

                foreach(int bgmIndex in  bgmTable.BGMIDs)
                    writer.Write(bgmIndex);
            }

            writer.Stream.RunInPosition(delegate
            {
                foreach (var ptr in pointers)
                    writer.Write((int)ptr);

            }, bgmtStart);

            return bgmtStart;
        }

        private static long WriteUSNB(DataWriter writer, USNB usnb)
        {
            long usnbStart = writer.Stream.Position;
                    
            writer.WriteTimes(0, usnb.Entries.Count * 80);
            writer.Align(16);

            StringTable table = new StringTable(writer, writer.Stream.Position);

            foreach (var usen in usnb.Entries)
                table.Write(usen.Name);

            long tableEnd = table.Position;

            writer.Stream.Position = usnbStart;

            foreach(var usen in usnb.Entries)
            {
                writer.Write(usen.Data);
                writer.Write((int)table.GetPosition(usen.Name));
                writer.Write(0);
            }

            writer.Stream.Position = tableEnd;
            writer.Align(16);

            return usnbStart;
        }

        private static long WriteUnknownVolumePreset(DataWriter writer, UnknownVolumePresetRegion unkVolumePresetData)
        {
            long unkVolumePresetDataStart = writer.Stream.Position;

            foreach (var data in unkVolumePresetData.Datas)
                writer.Write(data.Data);

            return unkVolumePresetDataStart;
        }

        private static long WriteVPBT(DataWriter writer, VPBT_Y5 vpbt)
        {
            long vpbtStart = writer.Stream.Position;

            writer.WriteTimes(0, vpbt.VolumePresets.Count * 16);

            StringTable table = new StringTable(writer, writer.Stream.Position);

            foreach (string volumePreset in vpbt.VolumePresets)
                table.Write(volumePreset);

            writer.Stream.Position = table.Position;
            writer.Align(16);

            long tableEnd = writer.Stream.Position;

            writer.Stream.Position = vpbtStart;

            foreach(string volumePreset in  vpbt.VolumePresets)
            {
                writer.Write((int)table.GetPosition(volumePreset));
                writer.WriteTimes(0, 12);
            }

            writer.Stream.Position = tableEnd;

            return vpbtStart;
        }
    }
}
