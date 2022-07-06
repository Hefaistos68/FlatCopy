# FlatCopy
Just a very simple tool to copy files from any folder structure into a single folder without structure.

Supports copying or moving files, automatic renaming of duplicate filenames, optionally with check for unique files.

Duplicate filenames use the default Windows scheme of adding an increasing number in brackets to the filename.

Usage:

    flatcopy [path\\]*.ext [destinationpath] [/m] [/r] [/u]")
    
    path             optional path to copy from, current working directory is default
    *.ext            the file spec to use
    destinationpath  if not specified, current directory is used
    /m               move files instead of copying
    /r               remove empty folders after moving files
    /u               copy only unique files, duplicate files are ignored
