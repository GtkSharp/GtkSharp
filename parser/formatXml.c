#include <glib.h>
#include <stdlib.h>

#include <libxml/xmlmemory.h>
#include <libxml/parser.h>

static int
formatFile (const gchar *input, const gchar *output)
{
	xmlDocPtr doc;

	/*
	* build an XML tree from a the file;
	*/
	doc = xmlParseFile (input);
	if (doc == NULL){
		g_warning ("File %s empty or not well-formed.", input);
		return -1;
	}

	if (xmlSaveFormatFile (output, doc, TRUE) == -1){
		g_warning ("Error saving config data to %s", input);
	}

	xmlFreeDoc (doc);
	return 0;
}

int main(int argc, char **argv)
{
	if (argc != 3){
		g_print ("Usage: formatXml inputfile outputfile\n\n");
		return -1;
	}

	xmlKeepBlanksDefault(0);
	formatFile (argv [1], argv [2]);

	return 0;
}

