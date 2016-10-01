#!/usr/bin/python3
"""Script to build out the .Net dll's and package them into a Nuget Package for gtksharp3"""
import os, sys
from pybuild.GtkSharp_Builder import GtkSharp_Builder
from pybuild.Gtk_Builder import Gtk_Builder
from pybuild.Helper import Helper as helpers

# Ideally I'd like to see the GtkSharp Build system redone via .Net Core or something other than make
# For now though we rely on the use of make to build the .Net dll's
# under linux we run this natively, under windows we can use MSYS2

class Build(object):

    # Class Init
    def __init__(self):
        self.GtkSharp_Builder = GtkSharp_Builder()
        self.Gtk_Builder = Gtk_Builder()

    # Clean the Build directory
    def clean(self):
        """Clean the build dir"""
        helpers.emptydir('./build')
        print ("Clean finished")

    # Print Usage
    def usage(self):
        print ("Please use GtkSharp3_Build.py <target> where <target> is one of")
        print ("  clean                 to clean the output directory: ./build")
        print ("  gtksharp_net45        to build ,Net libs for GtkSharp, via .Net 4.5")
        print ("  gtksharp_nuget_net45  to build Nuget Packages for GtkSharp, via .Net 4.5")
        print ("  gtk_nuget_win32       to build the Nuget package for GtkSharp.Win32")
        print ("  gtk_nuget_win64     to build the Nuget package for GtkSharp.Win64")
        print ("  all                 to make all")

    def main(self):
        if len(sys.argv) != 2:
            self.usage()
            return

        if sys.argv[1] == "gtksharp_net45":
            self.GtkSharp_Builder.build_net45()
        if sys.argv[1] == "gtksharp_nuget_net45":
            self.GtkSharp_Builder.build_nuget_net45()
        if sys.argv[1] == "gtk_nuget_win32":
            self.Gtk_Builder.build_nuget_win32()
        if sys.argv[1] == "gtk_nuget_win64":
            self.Gtk_Builder.build_nuget_win64()

        if sys.argv[1] == "all":
            self.GtkSharp_Builder.build_net45()
            self.GtkSharp_Builder.build_nuget_net45()
            self.Gtk_Builder.build_nuget_win32()
            self.Gtk_Builder.build_nuget_win64()

        if sys.argv[1] == "clean":
            self.clean()

if __name__ == "__main__":
    Build().main()
