using System;
using System.Collections.Generic;
using Yarhl.IO;

namespace SoundLib
{
    public class SEPB
    {
        public List<SEPBSEPortSet> SEPortSets = new List<SEPBSEPortSet>();

        /// <summary>
        /// For reading from sound_def
        /// </summary>m>
        public static SEPB ReadFromStream(DataReader reader, int sePortCount)
        {
            return ReadSEBPData(reader, sePortCount);
        }

        private static SEPB ReadSEBPData(DataReader reader, int sePortCount)
        {
            SEPB sepb = new SEPB();
 
            for (int i = 0; i < sePortCount; i++)
            {
                SEPBSEPortSet seport = new SEPBSEPortSet();
                int count = reader.ReadInt32();
                int pointer = reader.ReadInt32();
                reader.Stream.Position += 8;

                reader.Stream.RunInPosition(delegate
                {
                    for(int k = 0; k < count; k++)
                    {
                        SEPBSEPort port = new SEPBSEPort();
                        port.Unknown1 = reader.ReadInt32();
                        port.Unknown2 = reader.ReadInt32();
                        port.Unknown3 = reader.ReadInt32();
                        reader.Stream.Position += 4;

                        seport.Ports.Add(port);
                    }
                }, pointer);

                sepb.SEPortSets.Add(seport);
;            }

            return sepb;
        }
    }
}
