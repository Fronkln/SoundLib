using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Yarhl.IO;


namespace SoundLib
{
    public class CTGR
    {
        /// <summary>
        /// For reading from category.bin, Y0
        /// </summary>
        public static List<CTGRCategoryY0> Read(string filePath)
        {
            byte[] buffer = File.ReadAllBytes(filePath);

            DataStream readStream = DataStreamFactory.FromArray(buffer, 0, buffer.Length);
            DataReader reader = new DataReader(readStream) { Endianness = EndiannessMode.BigEndian, DefaultEncoding = Encoding.GetEncoding(932) };

            reader.Stream.Position = 16;
            int categoryCount = reader.ReadInt32();
            int dataPtr = reader.ReadInt32();

            reader.Stream.Position = dataPtr;
            return ReadCSHBDataY0(reader, categoryCount);
        }

        /// <summary>
        /// For reading from sound_def
        /// </summary>m>
        public static List<CTGRCategory> ReadFromStream(DataReader reader, int categoryCount)
        {
            return ReadCSHBData(reader, categoryCount);
        }

        private static List<CTGRCategory> ReadCSHBData(DataReader reader, int categoryCount)
        {
            List<CTGRCategory> categories = new List<CTGRCategory>();

            for (int i = 0; i < categoryCount; i++)
            {
                CTGRCategory category = new CTGRCategory();

                category.SizeLimit = reader.ReadInt32();
                category.Unknown = reader.ReadInt32();
                category.Unknown2 = reader.ReadInt32();
                category.Unknown3 = reader.ReadInt32();
                int namePtr = reader.ReadInt32();
                category.Unknown4 = reader.ReadInt32();
                category.Unknown5 = reader.ReadInt32();
                category.Unknown6 = reader.ReadInt32();
                category.Unknown7 = reader.ReadSingle();
                category.Unknown8 = reader.ReadSingle();
                category.Unknown9 = reader.ReadInt32();
                category.Unknown10 = reader.ReadInt32();
                category.Unknown11 = reader.ReadSingle();
                category.Unknown12 = reader.ReadSingle();
                category.Unknown13 = reader.ReadSingle();
                category.Unknown14 = reader.ReadSingle();

                reader.Stream.RunInPosition(delegate
                {
                    category.Name = reader.ReadString();
                }, namePtr);

                categories.Add(category);
            }

            return categories;
        }

        private static List<CTGRCategoryY0> ReadCSHBDataY0(DataReader reader, int categoryCount)
        {
            List<CTGRCategoryY0> categories = new List<CTGRCategoryY0>();

            for (int i = 0; i < categoryCount; i++)
            {
                CTGRCategoryY0 category = new CTGRCategoryY0();

                category.SizeLimit = reader.ReadInt32();
                category.Unknown = reader.ReadInt32();
                category.Unknown2 = reader.ReadInt32();
                category.Unknown3 = reader.ReadInt32();
                int namePtr = reader.ReadInt32();
                category.Unknown4 = reader.ReadInt32();
                category.Unknown5 = reader.ReadInt32();
                category.Unknown6 = reader.ReadInt32();
                category.Unknown7 = reader.ReadInt32();
                category.Unknown8 = reader.ReadInt32();
                category.Unknown9 = reader.ReadInt32();
                category.Unknown10 = reader.ReadInt32();
                category.Unknown11 = reader.ReadSingle();
                category.Unknown12 = reader.ReadSingle();
                category.Unknown13 = reader.ReadSingle();
                category.Unknown14 = reader.ReadSingle();
                category.Unknown15 = reader.ReadInt32();

                reader.Stream.RunInPosition(delegate
                {
                    category.Name = reader.ReadString();
                }, namePtr);

                categories.Add(category);
            }

            return categories;
        }
    }
}
