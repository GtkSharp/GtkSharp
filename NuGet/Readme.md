# Readme

## Overview

This is a series of python scripts to generate the binaries and NuGet Packages for GtkSharp and Gtk

  * NuGet Packages for the GtkSharp .Net libraries
  * NuGet Packages for the Windows 32bit / 64bit Gtk dll's

## Windows

### Depends

The following is used as part of the build process

  * MSYS2 - Windows install

Also it's assumed that you've already installed the 32bit and 64bit binaries for gtk within the MSYS environment
if your going to generate the Win32 / Win64 Nuget Packages for windows

```
pacman -S mingw-w64-i686-pango mingw-w64-i686-atk mingw-w64-i686-gtk3
pacman -S mingw-w64-x86_64-pango mingw-w64-x86_64-atk mingw-w64-x86_64-gtk3
```

And installed the executor python module
```
C:\Python35\Scripts\pip.exe install executor
```

### Running Build

To run the build
```
cd gtk-sharp\NuGet
build.py all
```

## Linux

### Depends

For Ubuntu we need to install pip for python 3 and a few other packages
```
sudo apt-get install python3-pip autoconf libtool libtool-bin mono-complete 
sudo apt-get install libglib2.0-dev libpango1.0-dev libatk1.0-dev libgtk-3-dev
```

Then install the executor python module
```
pip3 install executor
```

The version of Nuget needs to be the latest for linux
```
sudo wget https://dist.nuget.org/win-x86-commandline/v3.5.0-rc1/NuGet.exe -O /usr/local/bin/nuget.exe
sudo chmod +x /usr/local/bin/nuget.exe
```

### Running Build

To run the build
```
cd gtk-sharp/NuGet
chmod +x build.py
./build.py all
```
