using System;
using System.Collections.Generic;
using Yarhl.IO;

namespace SoundLib
{
    public class SETB
    {
        public List<SETBTable> SETables = new List<SETBTable>();

        /// <summary>
        /// For reading from sound_def
        /// </summary>m>
        public static SETB ReadFromStream(DataReader reader, int seTableCount)
        {
            return ReadSETBData(reader, seTableCount);
        }

        private static SETB ReadSETBData(DataReader reader, int seTableCount)
        {
            SETB setb = new SETB();

            for (int i = 0; i < seTableCount; i++)
            {
                SETBTable seTable = new SETBTable();
                int pointer = reader.ReadInt32();

                reader.Stream.RunInPosition(delegate
                {
                    int count = reader.ReadInt32();

                    for (int k = 0; k < count; k++)
                    {
                        Sound sound = new Sound();
                        sound.CuesheetID = reader.ReadUInt16();
                        sound.SoundID = reader.ReadUInt16();

                        seTable.Sounds.Add(sound);
                    }
                }, pointer);

                setb.SETables.Add(seTable);
            }

            return setb;
        }
    }
}
