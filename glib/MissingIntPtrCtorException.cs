// MissingIntPtrCtorException.cs : Exception for missing IntPtr ctors
//
// Authors: Mike Kestner  <mkestner@ximian.com>
//
// Copyright (c) 2004 Novell, Inc.

namespace GLib {

	using System;
	using System.Runtime.InteropServices;
	
	public class MissingIntPtrCtorException : Exception
	{
		public MissingIntPtrCtorException (string msg) : base (msg)
		{
		}

	}
}

