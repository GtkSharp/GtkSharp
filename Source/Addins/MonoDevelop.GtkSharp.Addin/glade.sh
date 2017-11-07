#!/bin/bash
if [ -x "$(which glade)" ]; then
    glade "$@"
elif [ -x "$(which flatpak)" ] && [ ! -z "$(flatpak list | grep org.gnome.Glade)" ]; then
    flatpak run org.gnome.Glade "$@"
fi