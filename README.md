This repository contains SoundLib and the project that uses it, **Casting Call**, sound_def.bin and sound bin files editor for Yakuza 5 and Yakuza 0

With this tool you can:
<br>-Adjust voicers of characters and give them new sounds
<br>-Add new cuesheets to the game
<br>-Add new music (Y5 only for now)
<br>-And many more (Y5 only for now)
<br>
### Supported
<br>**Yakuza 5 and 0**
<br>Cuesheets (CSHB)
<br>Sound Categories (CTGR)
<br>Voicer Table (VOTB)
<br><br>**Yakuza 5 only**
<br>Sound Port (SEPB)
<br>Sound Table (SETB)
<br>BGM (BGMB)
<br>BGM Table (BGMT)
<br>USEN (USNB)
<br>Volume Preset (VPBT)
<br>Unknown Voicer Related Data
<br>
<br>**Yakuza 0 only**
<br>-Voicer names (VCRB)
<br>
### Usage
<br>Drag and drop the folder in which sound_def.bin or the sound bin files reside.
<br><br>
Alternatively, it can be used through commandline

`CastingCall.exe (directory)`
<br><br>Addittional arguments for importing:
<br>`-copysound` copies acbs of the sounds referenced in voicer tables
<br>`-speechdir (directory)` path to unpacked se.cpk with BGMTools. Copies awbs that acbs may use in voicer tables
