using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SoundLib;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data;

namespace CastingCall
{
    internal class Program
    {
        private static string outputDirectory;

        private static string speechDirectory;

        //"E:\SteamLibrary\steamapps\common\Yakuza 5\main\data\soundpar\sound.par.unpack\sound" -copysound -speechdir "C:\Users\orhan_vfigibb\Downloads\BGMTools\Unpack"

        static void Main(string[] args)
        {
            if (args.Length <= 0)
            {
                Console.WriteLine("Please drag this into this tool a soundpar folder");
                Console.ReadKey();
                return;
            }

            if(args.Contains("-speechdir"))
            {
                speechDirectory = args[Array.IndexOf(args, "-speechdir") + 1];
            }

            bool isExport = false;

            string path = args[0];

            //OE
            string voicerFile = Path.Combine(path, "voicer.bin");
            string voiceTableFile = Path.Combine(path, "voice_table.bin");

            //OOE
            string soundDefFile = Path.Combine(path, "sound_def.bin");


            string outVoicerPath = Path.Combine(path, "voicer_list.txt");
            string outVoicersDir = Path.Combine(path, "Voice Table");
            isExport = File.Exists(outVoicerPath);

            bool copySound = false;


            if (!isExport)
            {
                if (args.Length > 1)
                    copySound = args[1] == "-copysound";

                if (File.Exists(soundDefFile) && !path.Contains("sound_def_out"))
                {
                    ImportY5SoundDef(path, soundDefFile);

                    Console.WriteLine("Imported Y5 Sound Def");
                    Console.ReadKey();
                    return;
                }

                if (Directory.Exists(path))
                {
                    ExportY5SoundDef(path, Path.Combine(path, "sound_def.bin"));
                    Console.WriteLine("Exported Y5 Sound Def");
                    Console.ReadKey();
                    return;
                }

                if (!File.Exists(voicerFile))
                {
                    Console.WriteLine("voicer.bin does not exist in the provided directory");
                    Console.ReadKey();
                    return;
                }

                if (!File.Exists(voiceTableFile))
                {
                    Console.WriteLine("voice_table.bin does not exist in the provided directory");
                    Console.ReadKey();
                    return;
                }

                //Y0 sound reading code
                outputDirectory = Path.Combine(path, "out_sound");

                if (!Directory.Exists(outputDirectory))
                    Directory.CreateDirectory(outputDirectory);

                Console.WriteLine("Reading Voicers...");
                VCRB voicer = VCRB.Read(voicerFile);
                Console.WriteLine("Reading Voice Tables...");
                VOTB voiceTable = VOTB.Read(voiceTableFile);

                string cuesheetFile = Path.Combine(path, "cuesheet.bin");
                string categoryFile = Path.Combine(path, "category.bin");

                string voicerOutFile = Path.Combine(outputDirectory, "voicer_list.txt");
                File.WriteAllLines(voicerOutFile, voicer.Voicers);

                string outVoiceTableVoicerDir = Path.Combine(outputDirectory, "Voice Table", "Voicer");

                if (!Directory.Exists(outVoiceTableVoicerDir))
                    Directory.CreateDirectory(outVoiceTableVoicerDir);

                for (int i = 0; i < voicer.Voicers.Count; i++)
                {
                    string voicerName = voicer.Voicers[i];
                    string outVoicerDir = Path.Combine(outVoiceTableVoicerDir, voicerName);

                    if (!Directory.Exists(outVoicerDir))
                        Directory.CreateDirectory(outVoicerDir);

                    var voicerObj = voiceTable.Voicers[i];

                    HashSet<short> copiedCues = new HashSet<short>();

                    for (int k = 0; k < voicerObj.CategorySounds.Length; k++)
                    {
                        if (!voicerObj.ValidCategories[k] || voicerObj.CategorySounds[k].Count <= 0)
                            continue;

                        var categorySounds = voicerObj.CategorySounds[k];
                        string outputVoicerCategoryDir = Path.Combine(outVoicerDir, "Category " + k);

                        if (!Directory.Exists(outputVoicerCategoryDir))
                            Directory.CreateDirectory(outputVoicerCategoryDir);

                        string outputVoicerCategorySoundFile = Path.Combine(outputVoicerCategoryDir, "Sounds.txt");
                        File.WriteAllLines(outputVoicerCategorySoundFile, categorySounds.Select(x => x.Cuesheet.ToString("x") + " " + x.Sound));

                        if (copySound)
                        {
                            foreach (var cue in categorySounds)
                            {
                                if (copiedCues.Contains(cue.Cuesheet))
                                    continue;

                                string cueNameHex = cue.Cuesheet.ToString("x4");
                                string cuenameDec = cue.Cuesheet.ToString("d4");
                                string cueFileName = cueNameHex + ".acb";

                                string cuePath = Path.Combine(path, cueFileName);


                                if (Directory.Exists(speechDirectory))
                                {
                                    if (File.Exists(Path.Combine(speechDirectory, cuenameDec)))
                                    {
                                        ;
                                    }
                                }

                                if (File.Exists(cuePath))
                                {
                                    File.Copy(cuePath, Path.Combine(outputVoicerCategoryDir, new FileInfo(cueFileName).Name), true);
                                }

                                copiedCues.Add(cue.Cuesheet);
                            }
                        }
                    }
                }

                string outVoiceTableCategoryDir = Path.Combine(outputDirectory, "Voice Table");

                if (!Directory.Exists(outVoiceTableCategoryDir))
                    Directory.CreateDirectory(outVoiceTableCategoryDir);

                for (int i = 0; i < voiceTable.Categories.Count; i++)
                {
                    var category = voiceTable.Categories[i];

                    string outCategoryFile = Path.Combine(outVoiceTableCategoryDir, "Sound Category " + i + ".txt");

                    List<string> fileBuf = new List<string>();
                    fileBuf.Add(category.SoundCount.ToString());
                    fileBuf.AddRange(category.UnkDatas.Select(x => x.Unk1 + " " + x.Unk2));

                    File.WriteAllLines(outCategoryFile, fileBuf);
                }


                if(File.Exists(cuesheetFile) && File.Exists(categoryFile))
                {
                    Console.WriteLine("Reading Cuesheets...");
                    CSHB cuesheet = CSHB.Read(cuesheetFile);
                    Console.WriteLine("Reading Categories...");
                    List<CTGRCategoryY0> categories = CTGR.Read(categoryFile);

                    Console.WriteLine("Importing Cuesheets...");
                    ImportCSHBY0(cuesheet, categories);
                    Console.WriteLine("Importing Categories...");
                    ImportCTGRY0(categories);
                }
            }
            else
            {
                VOTB table = new VOTB();

                Console.WriteLine("Exporting Voicer and Voice Table...");

                string[] voicersList = File.ReadAllLines(outVoicerPath);

                string voiceTableDir = Path.Combine(path, "Voice Table");
                string voiceTableVoicerDir = Path.Combine(voiceTableDir, "Voicer");

                string[] categoryFiles = Directory.GetFiles(voiceTableDir, "*.txt");
                VOTBSoundCategory[] categories = new VOTBSoundCategory[categoryFiles.Length];

                foreach (string categoryFile in categoryFiles)
                {
                    string fileName = Path.GetFileNameWithoutExtension(categoryFile);
                    int categoryID = int.Parse(fileName.Replace("Sound Category ", ""));

                    VOTBSoundCategory category = new VOTBSoundCategory();
                    categories[categoryID] = category;

                    string[] fileBuf = File.ReadAllLines(categoryFile);

                    category.SoundCount = int.Parse(fileBuf[0]);

                    for (int i = 1; i < fileBuf.Length; i++)
                    {
                        string[] split = fileBuf[i].Split(' ');
                        category.UnkDatas.Add(new VOTBUnkStructure() { Unk1 = int.Parse(split[0]), Unk2 = int.Parse(split[1]) });
                    }

                    category.IsValid = category.SoundCount > 0;
                }

                table.Categories = categories.ToList();

                List<VOTBVoicer> voicers = new List<VOTBVoicer>();

                for (int i = 0; i < voicersList.Length; i++)
                {
                    string voicerName = voicersList[i];
                    string voicerDir = Path.Combine(voiceTableVoicerDir, voicerName);

                    string[] categoryDirs = Directory.GetDirectories(voicerDir);

                    VOTBVoicer voicer = new VOTBVoicer();
                    voicer.CategorySounds = new List<VOTBSound>[table.Categories.Count];
                    voicer.ValidCategories = new bool[table.Categories.Count];

                    foreach (string categoryDir in categoryDirs)
                    {
                        string categoryName = new DirectoryInfo(categoryDir).Name;
                        int categoryID = int.Parse(categoryName.Replace("Category ", ""));

                        List<VOTBSound> sounds = new List<VOTBSound>();

                        string[] fileBuf = File.ReadAllLines(Path.Combine(categoryDir, "Sounds.txt"));

                        foreach (string file in fileBuf)
                        {
                            string[] split = file.Split(' ');

                            sounds.Add(new VOTBSound(short.Parse(split[0], System.Globalization.NumberStyles.HexNumber), short.Parse(split[1])));
                        }

                        voicer.CategorySounds[categoryID] = sounds;
                    }

                    for (int k = 0; k < voicer.CategorySounds.Length; k++)
                    {
                        voicer.ValidCategories[k] = voicer.CategorySounds[k] != null && voicer.CategorySounds[k].Count > 0;
                    }

                    voicers.Add(voicer);
                }

                table.Voicers = voicers.ToList();

                string outVoiceTableFile = Path.Combine(path, "voice_table.bin");
                VOTB.Write(outVoiceTableFile, table);

                Console.WriteLine("Exported Yakuza 0 sound bins");
            }
        }

        private static void ImportY5SoundDef(string dir, string soundDefFile)
        {
            var def = SoundDef.Read(soundDefFile);

            outputDirectory = Path.Combine(dir, "sound_def_out");

            if (!Directory.Exists(outputDirectory))
                Directory.CreateDirectory(outputDirectory);

            Console.WriteLine("Importing Cuesheets...");
            ImportCSHB(def.Cuesheets, def.Categories);
            Console.WriteLine("Importing Voice Tables...");
            ImportVOTBY5(dir, def.VoiceTable);
            Console.WriteLine("Importing Categories...");
            ImportCTGR(def.Categories);
            Console.WriteLine("Importing Unknown Voicer Data...");
            ImportUnknownVoicer(def.UnknownVoicer);
            Console.WriteLine("Importing SE Port Sets...");
            ImportSEPB(def.SEPortSets);
            Console.WriteLine("Importing SE Tables...");
            ImportSETB(def.SETables);
            Console.WriteLine("Importing BGMs...");
            ImportBGMB(def.BGMs, def.Categories);
            Console.WriteLine("Importing BGM Tables...");
            ImportBGMT(def.BGMTables, def.BGMs);
            Console.WriteLine("Importing USEN...");
            ImportUSNB(def.USEN);
            Console.WriteLine("Importing Unknown Volume Preset Data...");
            ImportUnknownVolumePreset(def.UnknownVolumePresetRegion);
            Console.WriteLine("Importing Volume Presets...");
            ImportVPBT(def.VolumePresets);
        }

        private static void ImportCSHB(CSHB cuesheets, List<CTGRCategory> categories)
        {
            string cuesheetDir = Path.Combine(outputDirectory, "Cuesheets");

            if (!Directory.Exists(cuesheetDir))
                Directory.CreateDirectory(cuesheetDir);

            string orderFilePath = Path.Combine(cuesheetDir, "_CUESHEET ORDER.txt");
            File.WriteAllLines(orderFilePath, cuesheets.Cuesheets.Select(x => x.Cuesheet.ToString("x4")));

            foreach (var cuesheet in cuesheets.Cuesheets)
            {
                string filePath = Path.Combine(cuesheetDir, cuesheet.Cuesheet.ToString("x4") + ".json");

                JObject cuesheetObject = new JObject();

                if (cuesheet.Category >= 0)
                    cuesheetObject["Category"] = categories[cuesheet.Category].Name;
                else
                    cuesheetObject["Category"] = "(INVALID)";

                cuesheetObject["Flags"] = cuesheet.Flags;

                File.WriteAllText(filePath, cuesheetObject.ToString(Formatting.Indented));
            }
        }

        private static void ImportCSHBY0(CSHB cuesheets, List<CTGRCategoryY0> categories)
        {
            string cuesheetDir = Path.Combine(outputDirectory, "Cuesheets");

            if (!Directory.Exists(cuesheetDir))
                Directory.CreateDirectory(cuesheetDir);

            string orderFilePath = Path.Combine(cuesheetDir, "_CUESHEET ORDER.txt");
            File.WriteAllLines(orderFilePath, cuesheets.Cuesheets.Select(x => x.Cuesheet.ToString("x4")));

            foreach (var cuesheet in cuesheets.Cuesheets)
            {
                string filePath = Path.Combine(cuesheetDir, cuesheet.Cuesheet.ToString("x4") + ".json");

                JObject cuesheetObject = new JObject();

                if (cuesheet.Category >= 0)
                    cuesheetObject["Category"] = categories[cuesheet.Category].Name;
                else
                    cuesheetObject["Category"] = "(INVALID)";

                cuesheetObject["Flags"] = cuesheet.Flags;

                File.WriteAllText(filePath, cuesheetObject.ToString(Formatting.Indented));
            }
        }


        private static void ImportCTGR(List<CTGRCategory> ctgr)
        {
            string categoryDir = Path.Combine(outputDirectory, "Sound Category");

            if (!Directory.Exists(categoryDir))
                Directory.CreateDirectory(categoryDir);

            string orderFilePath = Path.Combine(categoryDir, "_CATEGORY ORDER.txt");
            File.WriteAllLines(orderFilePath, ctgr.Select(x => x.Name));

            foreach (var category in ctgr)
            {
                string filePath = Path.Combine(categoryDir, category.Name + ".json");

                JObject categoryObj = new JObject();
                categoryObj["Size Limit"] = category.SizeLimit;
                categoryObj["Unknown 1"] = category.Unknown;
                categoryObj["Unknown 2"] = category.Unknown2;
                categoryObj["Unknown 3"] = category.Unknown3;
                categoryObj["Unknown 4"] = category.Unknown4;
                categoryObj["Unknown 5"] = category.Unknown5;
                categoryObj["Unknown 6"] = category.Unknown6;
                categoryObj["Unknown 7"] = category.Unknown7;
                categoryObj["Unknown 8"] = category.Unknown8;
                categoryObj["Unknown 9"] = category.Unknown9;
                categoryObj["Unknown 10"] = category.Unknown10;
                categoryObj["Unknown 11"] = category.Unknown11;
                categoryObj["Unknown 12"] = category.Unknown12;
                categoryObj["Unknown 13"] = category.Unknown13;
                categoryObj["Unknown 14"] = category.Unknown14;

                File.WriteAllText(filePath, categoryObj.ToString(Formatting.Indented));
            }
        }

        private static void ImportCTGRY0(List<CTGRCategoryY0> ctgr)
        {
            string categoryDir = Path.Combine(outputDirectory, "Sound Category");

            if (!Directory.Exists(categoryDir))
                Directory.CreateDirectory(categoryDir);

            string orderFilePath = Path.Combine(categoryDir, "_CATEGORY ORDER.txt");
            File.WriteAllLines(orderFilePath, ctgr.Select(x => x.Name));

            foreach (var category in ctgr)
            {
                string filePath = Path.Combine(categoryDir, category.Name + ".json");

                JObject categoryObj = new JObject();
                categoryObj["Size Limit"] = category.SizeLimit;
                categoryObj["Unknown 1"] = category.Unknown;
                categoryObj["Unknown 2"] = category.Unknown2;
                categoryObj["Unknown 3"] = category.Unknown3;
                categoryObj["Unknown 4"] = category.Unknown4;
                categoryObj["Unknown 5"] = category.Unknown5;
                categoryObj["Unknown 6"] = category.Unknown6;
                categoryObj["Unknown 7"] = category.Unknown7;
                categoryObj["Unknown 8"] = category.Unknown8;
                categoryObj["Unknown 9"] = category.Unknown9;
                categoryObj["Unknown 10"] = category.Unknown10;
                categoryObj["Unknown 11"] = category.Unknown11;
                categoryObj["Unknown 12"] = category.Unknown12;
                categoryObj["Unknown 13"] = category.Unknown13;
                categoryObj["Unknown 14"] = category.Unknown14;
                categoryObj["Unknown 15"] = category.Unknown15;

                File.WriteAllText(filePath, categoryObj.ToString(Formatting.Indented));
            }
        }

        private static void ImportUnknownVoicer(UnknownVoicerRegion unkVoicerReg)
        {
            string unkVoicerDir = Path.Combine(outputDirectory, "Unknown Voicer Data");

            if (!Directory.Exists(unkVoicerDir))
                Directory.CreateDirectory(unkVoicerDir);

            for (int i = 0; i < unkVoicerReg.VoicerData.Count; i++)
            {
                string voicerName = ((Y5Voicer)i).ToString();
                string filePath = Path.Combine(unkVoicerDir, voicerName + ".bin");

                File.WriteAllBytes(filePath, unkVoicerReg.VoicerData[i].Data);
            }
        }

        private static void ImportSEPB(SEPB sePortSets)
        {
            string sePortSetDir = Path.Combine(outputDirectory, "SE Port Set");

            if (!Directory.Exists(sePortSetDir))
                Directory.CreateDirectory(sePortSetDir);

            for (int i = 0; i < sePortSets.SEPortSets.Count; i++)
            {
                var portSet = sePortSets.SEPortSets[i];

                string filePath = Path.Combine(sePortSetDir, i.ToString() + ".json");
                File.WriteAllText(filePath, JsonConvert.SerializeObject(portSet.Ports, Formatting.Indented));
            }
        }

        private static void ImportSETB(SETB seTables)
        {
            string seTableDir = Path.Combine(outputDirectory, "SE Table");

            if (!Directory.Exists(seTableDir))
                Directory.CreateDirectory(seTableDir);

            for (int i = 0; i < seTables.SETables.Count; i++)
            {
                var table = seTables.SETables[i];

                string filePath = Path.Combine(seTableDir, i.ToString() + ".json");

                string[] convertedSounds = new string[table.Sounds.Count];

                for (int k = 0; k < convertedSounds.Length; k++)
                    convertedSounds[k] = $"{table.Sounds[k].CuesheetID.ToString("x")}-" + table.Sounds[k].SoundID;

                File.WriteAllText(filePath, JsonConvert.SerializeObject(convertedSounds, Formatting.Indented));
            }
        }

        private static void ImportBGMB(BGMB bgms, List<CTGRCategory> categories)
        {
            string bgmDir = Path.Combine(outputDirectory, "BGM");

            if (!Directory.Exists(bgmDir))
                Directory.CreateDirectory(bgmDir);

            List<string> order = new List<string>();

            foreach (var bgmFile in bgms.Files)
            {
                if (string.IsNullOrEmpty(bgmFile.Name))
                {
                    order.Add("NULL");
                    continue;
                }

                string bgmName = Path.GetFileNameWithoutExtension(bgmFile.Name);
                string bgmJsonPath = Path.Combine(bgmDir, bgmName + ".json");
                JObject bgmObject = new JObject();
                bgmObject["Category"] = categories[bgmFile.SoundCategory].Name;
                bgmObject["Unknown"] = bgmFile.Unknown;

                File.WriteAllText(bgmJsonPath, bgmObject.ToString(Formatting.Indented));

                order.Add(bgmName + " " + new FileInfo(bgmFile.Name).Extension);
            }

            string orderFilePath = Path.Combine(bgmDir, "_BGM ORDER.txt");

            File.WriteAllLines(orderFilePath, order);
        }

        private static void ImportBGMT(BGMT bgmTables, BGMB bgms)
        {
            string bgmtDir = Path.Combine(outputDirectory, "BGM Table");

            if (!Directory.Exists(bgmtDir))
                Directory.CreateDirectory(bgmtDir);

            for (int i = 0; i < bgmTables.Tables.Count; i++)
            {
                var table = bgmTables.Tables[i];

                string filePath = Path.Combine(bgmtDir, i.ToString() + ".json");
                string[] convertedTable = new string[table.BGMIDs.Count];

                for (int k = 0; k < convertedTable.Length; k++)
                    convertedTable[k] = bgms.Files[table.BGMIDs[k]].Name;

                File.WriteAllText(filePath, JsonConvert.SerializeObject(convertedTable, Formatting.Indented));
            }
        }

        private static void ImportVPBT(VPBT_Y5 vpbt)
        {
            string listPath = Path.Combine(outputDirectory, "Volume Preset.txt");
            File.WriteAllLines(listPath, vpbt.VolumePresets);
        }

        private static void ImportUnknownVolumePreset(UnknownVolumePresetRegion unkVolumePresetRegion)
        {
            string vpdDir = Path.Combine(outputDirectory, "Unknown Volume Preset Data");

            if (!Directory.Exists(vpdDir))
                Directory.CreateDirectory(vpdDir);

            for (int i = 0; i < unkVolumePresetRegion.Datas.Count; i++)
            {
                string filePath = Path.Combine(vpdDir, i + ".bin");

                File.WriteAllBytes(filePath, unkVolumePresetRegion.Datas[i].Data);
            }
        }

        private static void ImportUSNB(USNB usen)
        {
            string usnDir = Path.Combine(outputDirectory, "USEN");

            if (!Directory.Exists(usnDir))
                Directory.CreateDirectory(usnDir);

            string orderFilePath = Path.Combine(usnDir, "_USEN ORDER.txt");
            File.WriteAllLines(orderFilePath, usen.Entries.Select(x => x.Name));

            foreach (var entry in usen.Entries)
            {
                string filePath = Path.Combine(usnDir, entry.Name + ".bin");
                File.WriteAllBytes(filePath, entry.Data);
            }
        }

        private static void ImportVOTBY5(string dir, VOTB voiceTable)
        {
            string votbDir = Path.Combine(outputDirectory, "Voice Table");

            if (!Directory.Exists(votbDir))
                Directory.CreateDirectory(votbDir);

            JObject voicerSettingsObj = new JObject();
            voicerSettingsObj["Category Count"] = voiceTable.Categories.Count;

            string outVoiceTableVoicerDir = Path.Combine(votbDir, "Voicer");

            for (int i = 0; i < voiceTable.Voicers.Count; i++)
            {
                string voicerName = ((Y5Voicer)i).ToString();
                string outVoicerDir = Path.Combine(outVoiceTableVoicerDir, voicerName);

                if (!Directory.Exists(outVoicerDir))
                    Directory.CreateDirectory(outVoicerDir);

                VOTBVoicer voicerObj = voiceTable.Voicers[i];


                for (int k = 0; k < voicerObj.CategorySounds.Length; k++)
                {
                    if (!voicerObj.ValidCategories[k] || voicerObj.CategorySounds[k].Count <= 0)
                        continue;

                    HashSet<short> copiedCues = new HashSet<short>();

                    var categorySounds = voicerObj.CategorySounds[k];
                    string outputVoicerCategoryDir = Path.Combine(outVoicerDir, "Category " + k);

                    if (!Directory.Exists(outputVoicerCategoryDir))
                        Directory.CreateDirectory(outputVoicerCategoryDir);

                    string outputVoicerCategorySoundFile = Path.Combine(outputVoicerCategoryDir, "Sounds.txt");
                    File.WriteAllLines(outputVoicerCategorySoundFile, categorySounds.Select(x => x.Cuesheet.ToString("x") + " " + x.Sound));

                    bool copySound = true;

                    if (copySound)
                    {
                        foreach (var cue in categorySounds)
                        {
                            if (copiedCues.Contains(cue.Cuesheet))
                                continue;
                           
                            string cueNameHex = cue.Cuesheet.ToString("x4");
                            string cuenameDec = cue.Cuesheet.ToString("d4");
                            string cueFileName = cueNameHex + ".acb";

                            string cuePath = Path.Combine(dir, cueFileName);


                            if (Directory.Exists(speechDirectory))
                            {
                                string awbPath = Path.Combine(speechDirectory, cuenameDec);

                                if (File.Exists(awbPath))
                                {
                                    File.Copy(awbPath, Path.Combine(outputVoicerCategoryDir, cueNameHex+ ".awb"), true);
                                }
                            }

                            if (File.Exists(cuePath))
                            {
                                File.Copy(cuePath, Path.Combine(outputVoicerCategoryDir, new FileInfo(cueFileName).Name), true);
                            }

                            copiedCues.Add(cue.Cuesheet);
                        }
                    }
                }

            }


            for (int i = 0; i < voiceTable.Categories.Count; i++)
            {
                var category = voiceTable.Categories[i];

                string outCategoryFile = Path.Combine(votbDir, "Sound Category " + i + ".txt");

                List<string> fileBuf = new List<string>();
                fileBuf.Add(category.SoundCount.ToString());
                fileBuf.AddRange(category.UnkDatas.Select(x => x.Unk1 + " " + x.Unk2));

                File.WriteAllLines(outCategoryFile, fileBuf);
            }
        }


        private static void ExportY5SoundDef(string dir, string outputPath)
        {
            SoundDef def = new SoundDef();
            Console.WriteLine("Exporting Categories...");
            def.Categories = ExportCTGR(dir);
            Console.WriteLine("Exporting Cuesheets...");
            def.Cuesheets = ExportCSHB(dir, def.Categories);
            Console.WriteLine("Exporting Unknown Voicer Data...");
            def.UnknownVoicer = ExportY5UnknownVoicer(dir);
            Console.WriteLine("Exporting SE Port Sets...");
            def.SEPortSets = ExportSEPB(dir);
            Console.WriteLine("Exporting SE Tables...");
            def.SETables = ExportSETB(dir);
            Console.WriteLine("Exporting Voicer Table...");
            def.VoiceTable = ExportY5VOTB(dir);
            Console.WriteLine("Exporting BGMs...");
            def.BGMs = ExportBGMB(dir, def.Categories);
            Console.WriteLine("Exporting BGM Tables...");
            def.BGMTables = ExportBGMT(dir, def.BGMs);
            Console.WriteLine("Exporting USEN...");
            def.USEN = ExportUSNB(dir);
            Console.WriteLine("Exporting Unknown Volume Preset Data...");
            def.UnknownVolumePresetRegion = ExportUnknownVolumePresetData(dir);
            Console.WriteLine("Exporting Volume Presets...");
            def.VolumePresets = ExportY5VPBT(dir);

            SoundDef.Write(outputPath, def);
        }

        private static CSHB ExportCSHB(string dir, List<CTGRCategory> categories)
        {
            string dataPath = Path.Combine(dir, "Cuesheets");
            string orderFilePath = Path.Combine(dataPath, "_CUESHEET ORDER.txt");
            string[] orderFileBuf = File.ReadAllLines(orderFilePath);

            CSHB cshb = new CSHB();

            foreach(string cuesheet in orderFileBuf)
            {
                string filePath = Path.Combine(dataPath, cuesheet + ".json");
                JObject cueObj = JObject.Parse(File.ReadAllText(filePath));

                CSHBCuesheet cshbCuesheet = new CSHBCuesheet();
                cshbCuesheet.Cuesheet = short.Parse(cuesheet, System.Globalization.NumberStyles.HexNumber);
                cshbCuesheet.Category = (short)categories.FindIndex(x => x.Name == (string)cueObj["Category"]);
                cshbCuesheet.Flags = (int)cueObj["Flags"];

                cshb.Cuesheets.Add(cshbCuesheet);
            }

            return cshb;
        }

        private static CSHB ExportCSHBY0(string dir, List<CTGRCategoryY0> categories)
        {
            string dataPath = Path.Combine(dir, "Cuesheets");
            string orderFilePath = Path.Combine(dataPath, "_CUESHEET ORDER.txt");
            string[] orderFileBuf = File.ReadAllLines(orderFilePath);

            CSHB cshb = new CSHB();

            foreach (string cuesheet in orderFileBuf)
            {
                string filePath = Path.Combine(dataPath, cuesheet + ".json");
                JObject cueObj = JObject.Parse(File.ReadAllText(filePath));

                CSHBCuesheet cshbCuesheet = new CSHBCuesheet();
                cshbCuesheet.Cuesheet = short.Parse(cuesheet, System.Globalization.NumberStyles.HexNumber);

                string category = (string)cueObj["Category"];

                if (category == "(INVALID)")
                    cshbCuesheet.Category = -1;
                else
                    cshbCuesheet.Category = (short)categories.FindIndex(x => x.Name == (string)cueObj["Category"]);
                
                cshbCuesheet.Flags = (int)cueObj["Flags"];

                cshb.Cuesheets.Add(cshbCuesheet);
            }

            return cshb;
        }

        private static List<CTGRCategory> ExportCTGR(string dir)
        {
            string dataPath = Path.Combine(dir, "Sound Category");
            string orderFilePath = Path.Combine(dataPath, "_CATEGORY ORDER.txt");
            string[] orderFileBuf = File.ReadAllLines(orderFilePath);

            List<CTGRCategory> categories = new List<CTGRCategory>();
   
            foreach (string category in orderFileBuf)
            {
                string filePath = Path.Combine(dataPath, category + ".json");
                JObject categoryObj = JObject.Parse(File.ReadAllText(filePath));

                CTGRCategory ctgrCategory = new CTGRCategory();
                ctgrCategory.Name = category;
                ctgrCategory.SizeLimit = (int)categoryObj["Size Limit"];
                ctgrCategory.Unknown = (int)categoryObj["Unknown 1"];
                ctgrCategory.Unknown2 = (int)categoryObj["Unknown 2"];
                ctgrCategory.Unknown3 = (int)categoryObj["Unknown 3"];
                ctgrCategory.Unknown4 = (int)categoryObj["Unknown 4"];
                ctgrCategory.Unknown5 = (int)categoryObj["Unknown 5"];
                ctgrCategory.Unknown6 = (int)categoryObj["Unknown 6"];
                ctgrCategory.Unknown7 = (float)categoryObj["Unknown 7"];
                ctgrCategory.Unknown8 = (float)categoryObj["Unknown 8"];
                ctgrCategory.Unknown9 = (int)categoryObj["Unknown 9"];
                ctgrCategory.Unknown10 = (int)categoryObj["Unknown 10"];
                ctgrCategory.Unknown11 = (float)categoryObj["Unknown 11"];
                ctgrCategory.Unknown12 = (float)categoryObj["Unknown 12"];
                ctgrCategory.Unknown13 = (float)categoryObj["Unknown 13"];
                ctgrCategory.Unknown14 = (float)categoryObj["Unknown 14"];

                categories.Add(ctgrCategory);
            }

            return categories;
        }

        private static UnknownVoicerRegion ExportY5UnknownVoicer(string dir)
        {
            UnknownVoicerRegion unknownVoicerRegion = new UnknownVoicerRegion();

            string dataPath = Path.Combine(dir, "Unknown Voicer Data");

            int curDat = 0;

            while (true)
            {
                string voicerName = ((Y5Voicer)curDat).ToString();
                string filePath = Path.Combine(dataPath, voicerName + ".bin");

                if (File.Exists(filePath))
                {
                    UnknownVoicerData dat = new UnknownVoicerData() { Data = File.ReadAllBytes(filePath) };
                    unknownVoicerRegion.VoicerData.Add(dat);
                    curDat++;
                }
                else
                    break;
            }

            return unknownVoicerRegion;
        }

        private static SEPB ExportSEPB(string dir)
        {
            string dataPath = Path.Combine(dir, "SE Port Set");

            SEPB sepb = new SEPB();

            List<SEPBSEPortSet> sets = new List<SEPBSEPortSet>();
            int curDat = 0;

            while (true)
            {
                string filePath = Path.Combine(dataPath, curDat.ToString() + ".json");

                if (File.Exists(filePath))
                {
                    sepb.SEPortSets.Add(new SEPBSEPortSet() { Ports = JsonConvert.DeserializeObject<List<SEPBSEPort>>(File.ReadAllText(filePath)) });
                    curDat++;
                }
                else
                    break;
            }

            return sepb;
        }

        private static SETB ExportSETB(string dir)
        {
            string dataPath = Path.Combine(dir, "SE Table");

            SETB setb = new SETB();
            int curDat = 0;

            string[] files = Directory.GetFiles(dataPath, "*json");
            files = files.OrderBy(x => int.Parse(Path.GetFileNameWithoutExtension(x))).ToArray();

            foreach(string file in  files)
            {
                string[] table = JsonConvert.DeserializeObject<string[]>(File.ReadAllText(file));

                List<Sound> sounds = new List<Sound>();

                for (int i = 0; i < table.Length; i++)
                {
                    string[] split = table[i].Split('-');

                    if (split.Length < 2)
                        continue;

                    Sound sound = new Sound();
                    sound.CuesheetID = ushort.Parse(split[0], System.Globalization.NumberStyles.HexNumber);
                    sound.SoundID = ushort.Parse(split[1]);

                    sounds.Add(sound);
                }

                setb.SETables.Add(new SETBTable() { Sounds = sounds });

                curDat++;
            }

            return setb;
        }

        private static VOTB ExportY5VOTB(string dir)
        {
            VOTB table = new VOTB();

            string[] voicersList = Enum.GetNames(typeof(Y5Voicer));

            string voiceTableDir = Path.Combine(dir, "Voice Table");
            string voiceTableVoicerDir = Path.Combine(voiceTableDir, "Voicer");

            string[] categoryFiles = Directory.GetFiles(voiceTableDir, "*.txt");
            VOTBSoundCategory[] categories = new VOTBSoundCategory[categoryFiles.Length];

            foreach (string categoryFile in categoryFiles)
            {
                string fileName = Path.GetFileNameWithoutExtension(categoryFile);
                int categoryID = int.Parse(fileName.Replace("Sound Category ", ""));

                VOTBSoundCategory category = new VOTBSoundCategory();
                categories[categoryID] = category;

                string[] fileBuf = File.ReadAllLines(categoryFile);

                category.SoundCount = int.Parse(fileBuf[0]);

                for (int i = 1; i < fileBuf.Length; i++)
                {
                    string[] split = fileBuf[i].Split(' ');
                    category.UnkDatas.Add(new VOTBUnkStructure() { Unk1 = int.Parse(split[0]), Unk2 = int.Parse(split[1]) });
                }

                category.IsValid = category.SoundCount > 0;
            }

            table.Categories = categories.ToList();

            List<VOTBVoicer> voicers = new List<VOTBVoicer>();

            for (int i = 0; i < voicersList.Length; i++)
            {
                string voicerName = voicersList[i];
                string voicerDir = Path.Combine(voiceTableVoicerDir, voicerName);

                string[] categoryDirs = Directory.GetDirectories(voicerDir);

                VOTBVoicer voicer = new VOTBVoicer();
                voicer.CategorySounds = new List<VOTBSound>[table.Categories.Count];
                voicer.ValidCategories = new bool[table.Categories.Count];

                foreach (string categoryDir in categoryDirs)
                {
                    string categoryName = new DirectoryInfo(categoryDir).Name;
                    int categoryID = int.Parse(categoryName.Replace("Category ", ""));

                    List<VOTBSound> sounds = new List<VOTBSound>();

                    string[] fileBuf = File.ReadAllLines(Path.Combine(categoryDir, "Sounds.txt"));

                    foreach (string file in fileBuf)
                    {
                        string[] split = file.Split(' ');

                        sounds.Add(new VOTBSound(short.Parse(split[0], System.Globalization.NumberStyles.HexNumber), short.Parse(split[1])));
                    }

                    voicer.CategorySounds[categoryID] = sounds;
                }

                for (int k = 0; k < voicer.CategorySounds.Length; k++)
                {
                    voicer.ValidCategories[k] = voicer.CategorySounds[k] != null && voicer.CategorySounds[k].Count > 0;
                }

                voicers.Add(voicer);
            }

            table.Voicers = voicers.ToList();

            return table;
        }

        private static BGMB ExportBGMB(string dir, List<CTGRCategory> categories)
        {
            string dataPath = Path.Combine(dir, "BGM");
            string orderFilePath = Path.Combine(dataPath, "_BGM ORDER.txt");
            string[] orderFileBuf = File.ReadAllLines(orderFilePath);

            BGMB bgmb = new BGMB();

            foreach (string bgm in orderFileBuf)
            {
                string[] bgmSplit = bgm.Split(' ');

                if(bgmSplit.Length < 2)
                {
                    var invEntry = new BGMBEntry();
                    invEntry.Name = "";
                    invEntry.IsValid = 0;
                    bgmb.Files.Add(invEntry);
                    continue;
                }

                string filePath = Path.Combine(dataPath, bgmSplit[0] + ".json");
                JObject bgmObj = JObject.Parse(File.ReadAllText(filePath));

                BGMBEntry entry = new BGMBEntry();
                entry.Name = bgmSplit[0] + bgmSplit[1];
                entry.SoundCategory = categories.FindIndex(x => x.Name == (string)bgmObj["Category"]);
                entry.IsValid = 1;
                entry.Unknown = (int)bgmObj["Unknown"];

                bgmb.Files.Add(entry);

            }

            return bgmb;
        }

        private static BGMT ExportBGMT(string dir, BGMB bgms)
        {
            string dataPath = Path.Combine(dir, "BGM Table");

            BGMT bgmt = new BGMT();

            int curDat = 0;

            while (true)
            {
                string filePath = Path.Combine(dataPath, curDat.ToString() + ".json");

                if (File.Exists(filePath))
                {
                    string[] bgmsList = JsonConvert.DeserializeObject<string[]>(File.ReadAllText(filePath));
                    int[] convertedBgms = new int[bgmsList.Length];


                    for (int i = 0; i < bgmsList.Length; i++)
                        convertedBgms[i] = bgms.Files.FindIndex(x => x.Name == bgmsList[i]);

                    BGMTTable table = new BGMTTable();
                    table.BGMIDs = convertedBgms.ToList();
                    bgmt.Tables.Add(table);
                    curDat++;
                }
                else
                    break;
            }

            return bgmt;
        }

        private static USNB ExportUSNB(string dir)
        {
            string dataPath = Path.Combine(dir, "USEN");
            string orderFilePath = Path.Combine(dataPath, "_USEN ORDER.txt");
            string[] orderFileBuf = File.ReadAllLines(orderFilePath);

            USNB usnb = new USNB();

            foreach(string usen in orderFileBuf)
            {
                string filePath = Path.Combine(dataPath, usen + ".bin");
                USNBEntry entry = new USNBEntry();
                entry.Data = File.ReadAllBytes(filePath);
                entry.Name = usen;

                usnb.Entries.Add(entry);
            }

            return usnb;
        }

        private static UnknownVolumePresetRegion ExportUnknownVolumePresetData(string dir)
        {
            string dataPath = Path.Combine(dir, "Unknown Volume Preset Data");

            List<UnknownVolumePresetData> datas = new List<UnknownVolumePresetData>();

            int curDat = 0;

            while (true)
            {
                string filePath = Path.Combine(dataPath, curDat.ToString() + ".bin");

                if (File.Exists(filePath))
                {
                    datas.Add(new UnknownVolumePresetData() { Data = File.ReadAllBytes(filePath) });
                    curDat++;
                }
                else
                    break;
            }

            return new UnknownVolumePresetRegion() { Datas = datas };
        }

        private static VPBT_Y5 ExportY5VPBT(string dir)
        {
            string filePath = Path.Combine(dir, "Volume Preset.txt");

            return new VPBT_Y5() { VolumePresets = File.ReadAllLines(filePath).ToList() };
        }
    }
}
