MCS=mcs
REFERENCES= glib-sharp gdk-sharp gtk-sharp gnome-sharp gda-sharp gnomedb-sharp System.Data System.Drawing

###

REFS= $(addprefix /r:, $(REFERENCES))

all:
	$(MCS) $(REFS) client.cs
	$(MCS) $(REFS) GnomeDbClient.cs

clean:
	rm -f *.exe *.pdb
