//
// Result.cs: GnomeVFSResult enum.
//
// Author:
//   Jeroen Zwartepoorte <jeroen@xs4all.nl>
//
// (C) Copyright Jeroen Zwartepoorte 2004
//

namespace Gnome.Vfs {
	public enum Result {
		Ok,
		NotFound,
		Generic,
		Internal,
		BadParameters,
		NotSupported,
		Io,
		CorruptedData,
		WrongFormat,
		BadFile,
		TooBig,
		NoSpace,
		ReadOnly,
		InvalidUri,
		NotOpen,
		InvalidOpenMode,
		AccessDenied,
		TooManyOpenFiles,
		Eof,
		NotADirectory,
		InProgress,
		Interrupted,
		FileExists,
		Loop,
		NotPermitted,
		IsDirectory,
		NoMemory,
		HostNotFound,
		InvalidHostName,
		HostHasNoAddress,
		LoginFailed,
		Cancelled,
		DirectoryBusy,
		DirectoryNotEmpty,
		TooManyLinks,
		ReadOnlyFileSystem,
		NotSameFileSystem,
		NameTooLong,
		ServiceNotAvailable,
		ServiceObsolete,
		ProtocolError,
		NoMasterBrowser,
		NoDefault,
		NoHandler,
		Parse,
		Launch,
		NumErrors
	}
}
