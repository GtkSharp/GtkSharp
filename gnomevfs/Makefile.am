SUBDIRS = .

if ENABLE_GNOMEVFS
pkg = gnome-vfs
pkgconfigdir = $(libdir)/pkgconfig
pkgconfig_DATA = gnome-vfs-sharp-2.0.pc
else
pkg=
endif

SYMBOLS = gnomevfs-symbols.xml
METADATA = Gnomevfs.metadata
references = ../glib/glib-sharp.dll

sources = 					\
	Async.cs				\
	AsyncCallback.cs			\
	AsyncCallbackNative.cs			\
	AsyncDirectoryLoadCallback.cs		\
	AsyncDirectoryLoadCallbackNative.cs	\
	AsyncReadCallback.cs			\
	AsyncReadCallbackNative.cs		\
	AsyncWriteCallback.cs			\
	AsyncWriteCallbackNative.cs		\
	Directory.cs				\
	FileInfo.cs				\
	MimeType.cs				\
	ModuleCallback.cs			\
	ModuleCallbackAuthentication.cs		\
	ModuleCallbackFillAuthentication.cs	\
	ModuleCallbackFullAuthentication.cs	\
	ModuleCallbackSaveAuthentication.cs	\
	ModuleCallbackStatusMessage.cs		\
	Monitor.cs				\
	Sync.cs					\
	Vfs.cs					\
	VfsException.cs				\
	VfsStream.cs				\
	VfsStreamAsyncResult.cs			\
	Xfer.cs					\
	XferProgressCallback.cs			\
	XferProgressCallbackNative.cs

customs =				\
	Uri.custom			\
	VolumeMonitor.custom

add_dist = gnome-vfs-sharp-2.0.pc.in

include ../Makefile.include

