using System;
using System.Collections.Generic;
using Yarhl.IO;

namespace SoundLib
{
    public class StringTable
    {
        private Dictionary<string, long> table = new Dictionary<string, long>();
        public long StartPosition { get; private set; }
        public long Position { get; private set; }

        private DataWriter m_writer;


        public StringTable(DataWriter writer, long pos)
        {
            StartPosition = pos;
            Position = pos;
            m_writer = writer;
        }

        public long GetPosition(string value)
        {
            if (!table.ContainsKey(value))
                return -1;

            return table[value];
        }

        public long Write(string str, bool allowDuplicate = false)
        {
            if (table.ContainsKey(str) && !allowDuplicate)
                return table[str];

            long strStart = 0;

            m_writer.Stream.RunInPosition(delegate
            {
                strStart = m_writer.Stream.Position;
                m_writer.Write(str, true);
                table[str] = strStart;

                Position = m_writer.Stream.Position;
            }, Position);


            return strStart;
        }


        public static string[] Read(DataReader reader, long position, int count)
        {
            string[] strings = new string[count];

            reader.Stream.RunInPosition(delegate
            {
                int[] pointers = new int[count];

                for (int i = 0; i < count; i++)
                    pointers[i] = reader.ReadInt32();

                for (int i = 0; i < count; i++)
                {
                    int addr = pointers[i];
                    reader.Stream.Seek(addr, SeekMode.Start);
                    string str = reader.ReadString();

                    strings[i] = str;

                }
            }, position, SeekMode.Start);

            return strings;
        }
    }
}
