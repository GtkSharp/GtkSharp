// Opaquetest.cs: GLib.Opaque regression test
//
// Copyright (c) 2005 Novell, Inc.

using Gtksharp;
using System;

public class OpaqueTest {

	static int errors = 0;

	static void GC ()
	{
		System.GC.Collect ();
		System.GC.WaitForPendingFinalizers ();
		System.GC.Collect ();
		System.GC.WaitForPendingFinalizers ();
	}

	public static int Main ()
	{
		Gtk.Application.Init ();

		TestOpaque ();
		Console.WriteLine ();
		TestRefcounted ();
		Console.WriteLine ();

		Console.WriteLine ("{0} errors", errors);

		return errors;
	}

	static Opaque ret_op;

	static Opaque ReturnOpaque ()
	{
		return ret_op;
	}

	static void TestOpaque ()
	{
		Opaque op, op1;
		IntPtr handle;

		Console.WriteLine ("Testing Opaque new/free");
		op = new Opaque ();
		if (!op.Owned)
			Error ("Newly-created Opaque is not Owned");
		handle = op.Handle;
		op.Dispose ();
		op = new Opaque (handle);
		if (op.Owned)
			Error ("IntPtr-created Opaque is Owned");
		if (Opaquetest.Error)
			Error ("Memory error after initial new/free.");
		Opaquetest.ExpectError = true;
		if (op.Serial != Opaque.LastSerial)
			Error ("Serial mismatch. Expected {0}, Got {1}", Opaque.LastSerial, op.Serial);
		if (!Opaquetest.Error)
			Error ("Opaque not properly freed.");
		op.Dispose ();
		if (Opaquetest.Error)
			Error ("Opaque created from IntPtr was freed by gtk#");

		Console.WriteLine ("Testing Opaque copy/free");
		op = new Opaque ();
		op1 = op.Copy ();
		handle = op1.Handle;
		op.Dispose ();
		op1.Dispose ();
		if (Opaquetest.Error)
			Error ("Memory error after initial copy/free.");
		op = new Opaque (handle);
		Opaquetest.ExpectError = true;
		if (op.Serial != Opaque.LastSerial)
			Error ("Serial mismatch. Expected {0}, Got {1}", Opaque.LastSerial, op.Serial);

		if (!Opaquetest.Error)
			Error ("Opaque not properly freed.");
		op.Dispose ();
		if (Opaquetest.Error)
			Error ("Opaque created from IntPtr was freed by gtk#");

		Console.WriteLine ("Testing non-owned return.");
		op = new Opaque ();
		op1 = new Opaque ();
		op.Friend = op1;
		if (Opaquetest.Error)
			Error ("Memory error after setting op.Friend.");
		op1 = op.Friend;
		if (op1.Serial != Opaque.LastSerial || Opaquetest.Error)
			Error ("Error reading op.Friend. Expected {0}, Got {1}", Opaque.LastSerial, op1.Serial);
		if (!op1.Owned)
			Error ("op1 not Owned after being read off op.Friend");
		op.Dispose ();
		op1.Dispose ();
		if (Opaquetest.Error)
			Error ("Memory error after freeing op and op1.");

		Console.WriteLine ("Testing returning a Gtk#-owned opaque from C# to C");
		ret_op = new Opaque ();
		op = Opaque.Check (new Gtksharp.OpaqueReturnFunc (ReturnOpaque), new Gtksharp.GCFunc (GC));
		if (op.Serial != Opaque.LastSerial || Opaquetest.Error)
			Error ("Error during Opaque.Check. Expected {0}, Got {1}", Opaque.LastSerial, op.Serial);
		op.Dispose ();
		if (Opaquetest.Error)
			Error ("Memory error after clearing op.");

		Console.WriteLine ("Testing returning a Gtk#-owned opaque to a C method that will free it");
		ret_op = new Opaque ();
		op = Opaque.CheckFree (new Gtksharp.OpaqueReturnFunc (ReturnOpaque), new Gtksharp.GCFunc (GC));
		if (Opaquetest.Error)
			Error ("Error during Opaque.CheckFree.");
		Opaquetest.ExpectError = true;
		if (op.Serial != Opaque.LastSerial)
			Error ("Error during Opaque.CheckFree. Expected {0}, Got {1}", Opaque.LastSerial, op.Serial);
		if (!Opaquetest.Error)
			Error ("Didn't get expected error accessing op.Serial!");
		Opaquetest.ExpectError = true;
		op.Dispose ();
		if (!Opaquetest.Error)
			Error ("Didn't get expected double free on op after CheckFree!");

		Console.WriteLine ("Testing leaking a C-owned opaque");
		ret_op = new Opaque ();
		ret_op.Owned = false;
		op = Opaque.Check (new Gtksharp.OpaqueReturnFunc (ReturnOpaque), new Gtksharp.GCFunc (GC));
		if (op.Serial != Opaque.LastSerial || Opaquetest.Error)
			Error ("Error during Opaque.Check. Expected {0}, Got {1}", Opaque.LastSerial, op.Serial);
		handle = op.Handle;
		op.Dispose ();
		if (Opaquetest.Error)
			Error ("Memory error after disposing op.");
		op = new Opaque (handle);
		if (op.Serial != Opaque.LastSerial || Opaquetest.Error)
			Error ("Failed to leak op. Expected {0}, Got {1}", Opaque.LastSerial, op.Serial);

		Console.WriteLine ("Testing handing over a C-owned opaque to a C method that will free it");
		ret_op = new Opaque ();
		ret_op.Owned = false;
		op = Opaque.CheckFree (new Gtksharp.OpaqueReturnFunc (ReturnOpaque), new Gtksharp.GCFunc (GC));
		if (Opaquetest.Error)
			Error ("Error during Opaque.CheckFree.");
		Opaquetest.ExpectError = true;
		if (op.Serial != Opaque.LastSerial)
			Error ("Error during Opaque.CheckFree. Expected {0}, Got {1}", Opaque.LastSerial, op.Serial);
		if (!Opaquetest.Error)
			Error ("Didn't get expected error accessing op.Serial!");
		op.Dispose ();
		if (Opaquetest.Error)
			Error ("Double free on op!");
	}

	static Refcounted ret_ref;

	static Refcounted ReturnRefcounted ()
	{
		return ret_ref;
	}

	static void TestRefcounted ()
	{
		Refcounted ref1, ref2;
		IntPtr handle;

		Console.WriteLine ("Testing Refcounted new/free");
		ref1 = new Refcounted ();
		if (!ref1.Owned)
			Error ("Newly-created Refcounted is not Owned");
		handle = ref1.Handle;
		ref1.Dispose ();
		Opaquetest.ExpectError = true;
		ref1 = new Refcounted (handle);
		if (!Opaquetest.Error)
			Error ("Didn't get expected ref error resurrecting ref1.");
		if (!ref1.Owned)
			Error ("IntPtr-created Refcounted is not Owned");
		Opaquetest.ExpectError = true;
		if (ref1.Serial != Refcounted.LastSerial)
			Error ("Serial mismatch. Expected {0}, Got {1}", Refcounted.LastSerial, ref1.Serial);
		// We caused it to take a ref on the "freed" underlying object, so
		// undo that now so it doesn't cause an error later when we're not
		// expecting it.
		Opaquetest.ExpectError = true;
		ref1.Dispose ();
		Opaquetest.Error = false;

		Console.WriteLine ("Testing Refcounted leak/non-free");
		ref1 = new Refcounted ();
		ref1.Owned = false;
		handle = ref1.Handle;
		ref1.Dispose ();
		ref1 = new Refcounted (handle);
		if (Opaquetest.Error)
			Error ("Non-owned ref was freed by gtk#");
		if (ref1.Serial != Refcounted.LastSerial)
			Error ("Serial mismatch. Expected {0}, Got {1}", Refcounted.LastSerial, ref1.Serial);
		if (Opaquetest.Error)
			Error ("Non-owned ref was freed by gtk#");

		Console.WriteLine ("Testing non-owned return.");
		ref1 = new Refcounted ();
		ref2 = new Refcounted ();
		ref1.Friend = ref2;
		if (Opaquetest.Error)
			Error ("Memory error after setting ref1.Friend.");
		if (ref2.Refcount != 2)
			Error ("Refcount wrong for ref2 after setting ref1.Friend. Expected 2, Got {0}", ref2.Refcount);
		ref2.Dispose ();
		ref2 = ref1.Friend;
		if (ref2.Serial != Refcounted.LastSerial || Opaquetest.Error)
			Error ("Error reading ref1.Friend. Expected {0}, Got {1}", Refcounted.LastSerial, ref2.Serial);
		if (ref2.Refcount != 2 || Opaquetest.Error)
			Error ("Refcount wrong for ref2 after reading ref1.Friend. Expected 2, Got {0}", ref2.Refcount);
		if (!ref2.Owned)
			Error ("ref2 not Owned after being read off ref1.Friend");
		ref1.Dispose ();
		ref2.Dispose ();
		if (Opaquetest.Error)
			Error ("Memory error after freeing ref1 and ref2.");

		Console.WriteLine ("Testing returning a Gtk#-owned refcounted from C# to C");
		ret_ref = new Refcounted ();
		ref1 = Refcounted.Check (new Gtksharp.RefcountedReturnFunc (ReturnRefcounted), new Gtksharp.GCFunc (GC));
		if (ref1.Serial != Refcounted.LastSerial || Opaquetest.Error)
			Error ("Error during Refcounted.Check. Expected {0}, Got {1}", Refcounted.LastSerial, ref1.Serial);
		ref1.Dispose ();
		if (Opaquetest.Error)
			Error ("Memory error after clearing ref1.");

		Console.WriteLine ("Testing returning a Gtk#-owned refcounted to a C method that will free it");
		ret_ref = new Refcounted ();
		ref1 = Refcounted.CheckUnref (new Gtksharp.RefcountedReturnFunc (ReturnRefcounted), new Gtksharp.GCFunc (GC));
		if (Opaquetest.Error)
			Error ("Error during Refcounted.CheckUnref.");
		Opaquetest.ExpectError = true;
		if (ref1.Serial != Refcounted.LastSerial)
			Error ("Error during Refcounted.CheckUnref. Expected {0}, Got {1}", Refcounted.LastSerial, ref1.Serial);
		if (!Opaquetest.Error)
			Error ("Didn't get expected error accessing ref1.Serial!");
		Opaquetest.ExpectError = true;
		ref1.Dispose ();
		if (!Opaquetest.Error)
			Error ("Didn't get expected double free on ref1 after CheckUnref!");

		Console.WriteLine ("Testing leaking a C-owned refcounted");
		ret_ref = new Refcounted ();
		ret_ref.Owned = false;
		ref1 = Refcounted.Check (new Gtksharp.RefcountedReturnFunc (ReturnRefcounted), new Gtksharp.GCFunc (GC));
		if (ref1.Serial != Refcounted.LastSerial || Opaquetest.Error)
			Error ("Error during Refcounted.Check. Expected {0}, Got {1}", Refcounted.LastSerial, ref1.Serial);
		handle = ref1.Handle;
		ref1.Dispose ();
		if (Opaquetest.Error)
			Error ("Memory error after disposing ref1.");
		ref1 = new Refcounted (handle);
		if (ref1.Serial != Refcounted.LastSerial || Opaquetest.Error)
			Error ("Failed to leak ref1. Expected {0}, Got {1}", Refcounted.LastSerial, ref1.Serial);

		Console.WriteLine ("Testing handing over a C-owned refcounted to a C method that will free it");
		ret_ref = new Refcounted ();
		ret_ref.Owned = false;
		ref1 = Refcounted.CheckUnref (new Gtksharp.RefcountedReturnFunc (ReturnRefcounted), new Gtksharp.GCFunc (GC));
		if (Opaquetest.Error)
			Error ("Error during Refcounted.CheckUnref.");
		Opaquetest.ExpectError = true;
		if (ref1.Serial != Refcounted.LastSerial)
			Error ("Error during Refcounted.CheckUnref. Expected {0}, Got {1}", Refcounted.LastSerial, ref1.Serial);
		if (!Opaquetest.Error)
			Error ("Didn't get expected error accessing ref1.Serial!");
		ref1.Dispose ();
		if (Opaquetest.Error)
			Error ("Double free on ref1!");
	}

	static void Error (string message, params object[] args)
	{
		Console.Error.WriteLine ("  MANAGED ERROR: " + message, args);
		errors++;
	}
}
