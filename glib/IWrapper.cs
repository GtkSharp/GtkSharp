// IWrapper.cs - Common code for GInterfaces and GObjects
//
// Author: Rachel Hestilow <hesitlow@ximian.com> 
//
// (c) 2002 Rachel Hestilow

namespace GLib
{
	using System;

	public interface IWrapper
	{
		IntPtr Handle { get; set; }
	}
}
