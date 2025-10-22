using System;
using System.Collections.Generic;
using System.Linq;
using Yarhl.IO;

namespace SoundLib
{
    internal static class Extensions
    {
        public static int Align(this DataWriter writer, int alignment)
        {
            int mod = (int)writer.Stream.Position % alignment;

            if (mod == 0)
                return 0;

            int neededBytes = alignment - mod;

            writer.WriteTimes(0, neededBytes);
            return neededBytes;
        }
    }
}
