using System;

namespace SoundLib
{
    public class VOTBSound
    {
        public short Cuesheet;
        public short Sound;

        public VOTBSound(short cuesheet, short sound)
        {
            this.Cuesheet = cuesheet;
            this.Sound = sound;
        }
    }
}
