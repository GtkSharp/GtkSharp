MCS=mcs
REFERENCES= glib-sharp gdk-sharp gtk-sharp gnome-sharp System.Data

###

REFS= $(addsuffix .dll, $(addprefix /r:, $(REFERENCES)))

all:
	$(MCS) $(REFS) DbClient.cs

clean:
	rm -f *.exe *.pdb
