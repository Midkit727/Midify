# Midify
My Mp3-Player

A customizable music player written in C#. Built with Windows Forms and packed with features, themes, my favourite characters (Astolfo & Miku!), playlist management, and queue-based playback.

# Features include

- Music Playback: Supports MP3, WAV, MP4, MIDI, and MID formats.

- Custom Playlists: Create, edit, and delete playlists.

- Queue System: Play songs in order or shuffle them.

- Loop & Shuffle Modes: Customize playback.

- Color Schemes: Light, dark, pink, purple, blue, and green.
 
- Persistent Config: Saves all your settings.

- Automatic Resource Setup: Downloads missing assets on launch.

- Volume Slider + Seek Bar: Control your music.

- Settings Menu: customizable from within the app.

- Uninstaller Included

# Installation

Midify is composed of multiple scripts and executables. 
This repo includes the core source for documentation, not for direct compilation.

To use the compiled version:

1. Download the installer from:                           
https://drive.google.com/uc?export=download&id=1oh43zS4my4fkIqPw3lcbuPieFBn6rv-q
3. Unzip the Zipfile
4. Run "midify installer.exe"
5. Pick preferred settings and version
6. Selected Midify version will be installed

**Note: Running Midify for the first time will create a cfg directory to store settings and download some additional resources.**

# Technologies

Language: C#

UI: Windows Forms

Framework: .Net 4.8.1

Audio: MediaPlayer, NAudio

Resources: Custom images & sounds

Config: Plain text files

# Known issues

Known issues include:
1. My code being bad
2. Thread.Abort() can lead to problems in niche situations (will probably never happen)
3. Not compatible with Linux/Wine (Windows-only)
4. Midify Legacy might break if songs are removed outside the app
5. Might need reinstallation if one tampers with any save files

# License

This project is not currently open-source. Code provided is for documentation purposes only.

# Credits

Libraries: Uses NAudio for getting song durations and converting files

Anime assets: Astolfo & Miku belong to their respective creators.

Code: Written by me

# Why I Made This

Midify started as a small project I wanted to use to learn how Dictionaries work in C#,
but over the almost 3 months and many hours I put into it, it grew to the size that it is now.
This is the biggest, most complex program I have written and by far my greatest achievement as a programmer,
which is the reason why I decided to make this repository. I have been actively coding for not even 1.5 years
now and am only 16 years old. This program is unironically the single most impressive thing I have made.
And even though the plan was not to use any libraries in the making of this, I am still proud on the final 
product, as it actually kinda serves a use. 
Thanks for downloading!
