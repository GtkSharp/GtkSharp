include makefile.common

MCS=mcs
RUNTIME=mono

$(stage2_files): %stage2.xml: %stage1.xml introspect.exe
	$(RUNTIME) introspect.exe $< $@

introspect.exe: introspect.cs
	$(MCS) -o introspect.exe -r System.Xml introspect.cs
