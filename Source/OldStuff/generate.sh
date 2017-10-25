#!/bin/bash

set -e 
cd "$(dirname "$0")"

genstub () 
{
    DLLNAME=$1
    TARGET=$2

    rm -f empty.c
    touch empty.c
    gcc -shared -o ${TARGET} empty.c    
    gcc -Wl,--no-as-needed -shared -o ${DLLNAME}.so -fPIC -L. -l:${TARGET}
    rm -f ${TARGET}
    rm -f empty.c

    echo "Mapped ${DLLNAME}.dll ==> ${TARGET}"
}

#genstub libglib-2.0-0 libglib-2.0.so.0
#genstub libgobject-2.0-0 libgobject-2.0.so.0
#genstub libgthread-2.0-0 libgthread-2.0.so.0
#genstub libgio-2.0-0 libgio-2.0.so.0
#genstub libatk-1.0-0 libatk-1.0.so.0
#genstub libcairo-2 libcairo.so.2
#genstub libgtk-3-0 libgtk-3.so.0
#genstub libgdk-3-0 libgdk-3.so.0
#genstub libgdk_pixbuf-2.0-0 libgdk_pixbuf-2.0.so.0
#genstub libgtk-3-0 libgtk-3.so.0
genstub libpango-1.0-0 libpango-1.0.so.0
