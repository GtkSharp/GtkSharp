MCS=mcs
REFERENCES= glib-sharp gdk-sharp gtk-sharp gnome-sharp gda-sharp System.Data System.Drawing

###

REFS= $(addprefix /r:, $(REFERENCES))

all:
	$(MCS) $(REFS) client.cs
	$(MCS) $(REFS) /r:gnomedb-sharp GnomeDbClient.cs

clean:
	rm -f *.exe *.pdb
