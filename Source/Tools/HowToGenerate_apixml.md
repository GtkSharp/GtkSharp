get libgirepository1.0-dev

eg. on debian: 

apt install libgirepository1.0-dev

you have gir files then in 

/usr/share/gir-1.0/

run:

/usr/bin/xsltproc -o <sharp-api> /usr/local/lib/bindinator/gir2gapi.xslt /usr/share/gir-1.0/<gir-api-file>

where
<sharp-api> : output file for gtksharp-api, eg. ../WebkitGtkSharp/WebkitGtkSharp-api.xml
<gir-api-file>: gir file containing the api-definitions, eg. WebKit2-4.0.gir

example:

/usr/bin/xsltproc -o ../Libs/WebkitGtkSharp/WebkitGtkSharp-api.xml /usr/local/lib/bindinator/gir2gapi.xslt /usr/share/gir-1.0/WebKit2-4.0.gir
