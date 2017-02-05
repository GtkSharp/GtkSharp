#!/usr/bin/python3
"""Helper Functions"""

import os
from os.path import join

from executor import ExternalCommand


class Helper(object):
    @staticmethod
    def emptydir(top):
        """Empty a Directory"""
        if top == '/' or top == "\\":
            return
        else:
            for root, dirs, files in os.walk(top, topdown=False):
                for name in files:
                    os.remove(os.path.join(root, name))
                for name in dirs:
                    os.rmdir(os.path.join(root, name))

    @staticmethod
    def run_cmd(cmdarray, workdir):
        """Run a command on the shell"""
        cmd = ExternalCommand(*cmdarray, capture=True, capture_stderr=True, async=True, shell=False, directory=workdir)
        cmd.start()
        last_out = ''
        last_err = ''
        while cmd.is_running:
            new_out = cmd.stdout.decode(cmd.encoding, 'ignore').replace(last_out, '')
            new_err = cmd.stderr.decode(cmd.encoding, 'ignore').replace(last_err, '')

            last_out += new_out
            last_err += new_err
            new_out = new_out.replace(u"\u2018", "'").replace(u"\u2019", "'")
            new_err = new_err.replace(u"\u2018", "'").replace(u"\u2019", "'")
            if new_out != '':
                print(new_out, end='')
            if new_err != '':
                print(new_err, end='')

        if cmd.returncode != 0:
            raise RuntimeError('Failure to run command')
        return cmd

    @staticmethod
    def winpath_to_msyspath(winpath):
        """Convert a Windows path to a Msys type path"""
        winpath = '/' + winpath[0] + winpath[2:].replace('\\', '/')
        return winpath

    @staticmethod
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

    @staticmethod
    def install_pacman_deps():
        Helper.run_pacman_cmd(['-Sy'])

        args = ['--needed', '--nodeps', '--noprogressbar', '--noconfirm', '-S']
        args += 'unzip autoconf automake libtool pkg-config make'.split(' ')

        deps_arch = 'gcc glib2 pango atk gtk3 zlib libiconv'
        args += ['mingw-w64-i686-{0}'.format(i) for i in deps_arch.split(' ')]
        args += ['mingw-w64-x86_64-{0}'.format(i) for i in deps_arch.split(' ')]

        Helper.run_pacman_cmd(args)

    @staticmethod
    def run_pacman_cmd(pacman_args):
        msyspath = 'C:\\msys64'
        pacman_path = join(msyspath, 'usr\\bin\\pacman.exe')
        return Helper.run_cmd([pacman_path] + pacman_args, msyspath)

    @staticmethod
    def get_gtk_version_msys(msyspath):
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
