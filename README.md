# mdkExtractor
Extracts mdk assets for reading into known formats.

# Running
To run, simply load solution into any C# editor (Visual studio/VSCode), edit Program.cs to point at your base game folder, and run program. Extractions will be done into "extractions" folder under current EXE location (so normally into bin/Debug).

# Progress
The tool can decode:
 - Textures (almost all textures are supported, in most cases correct palette is detected). RLE textures are supported. Some extra texture formats are unsupoorted. Overall 95% done.
 - Models (all single models are supported, all multi models are supported, all levels are supported). Extracts *geometry only*. 100% done for geometry.
 - Sounds, in WAV format. 100% done.
 
Further it can detect (but not decode) following assets:
 - Game script - appears to be custom opcode based script, might look into that :).
 - Text assets (duh)
 - Animations
