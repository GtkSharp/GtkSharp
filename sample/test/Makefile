MCS=mcs

all:
	$(MCS) -o WidgetViewer.exe -r glib-sharp -r gdk-sharp -r gtk-sharp \
	TestCheckButton.cs TestColorSelection.cs TestFileSelection.cs TestRadioButton.cs \
	TestRange.cs TestStatusbar.cs TestToolbar.cs TestDialog.cs WidgetViewer.cs
clean:
	rm -f *.exe *~
