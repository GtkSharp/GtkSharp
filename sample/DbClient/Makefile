MCS=mcs
REFERENCES= glib-sharp gdk-sharp gtk-sharp gnome-sharp System.Data System.Drawing

###

REFS= $(addprefix /r:, $(REFERENCES))

all:
	$(MCS) $(REFS) client.cs

clean:
	rm -f *.exe *.pdb
