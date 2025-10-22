using System;
using System.Collections.Generic;
using Yarhl.IO;

namespace SoundLib
{
    public class BGMT
    {
        public List<BGMTTable> Tables = new List<BGMTTable>();


        /// <summary>
        /// For reading from sound_def
        /// </summary>m>
        public static BGMT ReadFromStream(DataReader reader, int tableCount)
        {
            return ReadBGMTData(reader, tableCount);
        }

        private static BGMT ReadBGMTData(DataReader reader, int tableCount)
        {
            BGMT bgmt = new BGMT();

            for (int i = 0; i < tableCount; i++)
            {
                int ptr = reader.ReadInt32();

                reader.Stream.RunInPosition(delegate
                {
                    BGMTTable table = new BGMTTable();
                    int count = reader.ReadInt32();

                    for(int k = 0; k < count; k++)
                        table.BGMIDs.Add(reader.ReadInt32());

                    bgmt.Tables.Add(table);
                }, ptr);
            }

            return bgmt;
        }
    }
}
