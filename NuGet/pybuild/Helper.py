#! python3
"""Helper Functions"""

import os, subprocess, shutil, sys
from os.path import join
from glob import iglob
from executor import ExternalCommand
import ntpath

class Helper(object):

    def emptydir(top):
        """Empty a Directory"""
        if(top == '/' or top == "\\"): return
        else:
            for root, dirs, files in os.walk(top, topdown=False):
                for name in files:
                    os.remove(os.path.join(root, name))
                for name in dirs:
                    os.rmdir(os.path.join(root, name))

    def run_cmd(cmdarray, workdir, comms = None):
        """Run a command on the shell"""      
        cmd = ExternalCommand(*cmdarray, capture=True, capture_stderr=True, async=True, shell=False, directory=workdir)
        cmd.start()
        last_out = ''
        last_err = ''
        while cmd.is_running:
            new_out = cmd.decoded_stdout.replace(last_out, '')
            new_err = cmd.decoded_stderr.replace(last_err, '')
            last_out += new_out
            last_err += new_err
            new_out = new_out.replace(u"\u2018", "'").replace(u"\u2019", "'")
            new_err = new_err.replace(u"\u2018", "'").replace(u"\u2019", "'")
            if new_out != '': print(new_out, end='')
            if new_err != '': print(new_err, end='')

        if cmd.returncode != 0:
            raise RuntimeError('Failure to run command')
        return cmd

    def winpath_to_msyspath(winpath):
        """Convert a Windows path to a Msys type path"""
        winpath = '/' + winpath[0] + winpath[2:].replace('\\', '/')
        return winpath

    def get_gtksharp_version(srcdir):
       """Get the Version of GTK Sharp in use from the source directory"""
       ret = None
       with open(join(srcdir, 'configure.ac')) as f:
           for line in f:
               if line.startswith('AC_INIT'):
                   ret = line
                   ret = ret.replace('AC_INIT(gtk-sharp,', '')
                   ret = ret.replace(' ', '')
                   ret = ret.replace(')\n', '')
                   break
       return ret

    def get_gtk_version(msyspath):
        ret = ''
        pacman_path = join(msyspath, 'usr\\bin\\pacman.exe')
        # pull version from msys2 / pacman
        # pacman -Qi mingw-w64-i686-gtk3
        cmd = Helper.run_cmd([pacman_path, '-Qi', 'mingw-w64-i686-gtk3'], msyspath)

        for line in cmd.output.split('\n'):
            if 'Version' in line:
                ret = line.replace('Version', '')
                ret = ret.replace(' ', '').replace(':', '')
                if '-' in ret:
                    ret = ret[:-2]
                break
        return ret

    def copy_files(src_glob, dst_folder):
        for fname in iglob(src_glob):
            shutil.copy(fname, join(dst_folder, ntpath.basename(fname)))