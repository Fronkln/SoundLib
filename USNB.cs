using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yarhl.IO;

namespace SoundLib
{
    public class USNB
    {
        public List<USNBEntry> Entries = new List<USNBEntry>();

        /// <summary>
        /// For reading from sound_def
        /// </summary>m>
        public static USNB ReadFromStream(DataReader reader, int usnbCount)
        {
            return ReadUSNBData(reader, usnbCount);
        }

        private static USNB ReadUSNBData(DataReader reader, int usnbCount)
        {
            USNB usnb = new USNB();

            for (int i = 0; i < usnbCount; i++)
            {
                USNBEntry entry = new USNBEntry();
                entry.Data = reader.ReadBytes(72);
                int namePtr = reader.ReadInt32();
                reader.Stream.Position += 4;

                reader.Stream.RunInPosition(delegate { entry.Name = reader.ReadString(); }, namePtr);
                usnb.Entries.Add(entry);
            }

            return usnb;
        }
    }
}
