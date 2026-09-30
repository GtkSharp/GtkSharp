using System;
using System.Runtime.InteropServices;
using Gtk;
using Samples;
using Xunit;

namespace GtkSharp.Tests
{
    /// <summary>
    /// GtkSharp holds a toggle ref on every wrapped GObject, so the native object normally outlives its
    /// managed wrapper. But some paths free the native object behind the wrapper's back -- Widget.Destroy,
    /// or an unbalanced unref -- and the wrapper is then left holding a dangling handle. A later operation on
    /// that wrapper (a leaked GLib timer calling Present / get_realized, say) dereferences freed -- and, once
    /// the address is reused, unrelated -- memory, which shows up as "GTK_IS_WINDOW/GTK_IS_WIDGET failed"
    /// followed by a 0xC0000005 access violation that aborts the whole host.
    ///
    /// The toggle ref is now paired with a g_object_weak_ref whose notify fires when the native object is
    /// finalized without going through the wrapper's Dispose. It zeros the wrapper's handle so those stale
    /// operations hit IntPtr.Zero (a harmless GTK CRITICAL) instead of freed memory.
    /// </summary>
    public class ToggleRefWeakNotifyTests : GtkTestBase
    {
        public ToggleRefWeakNotifyTests(GtkFixture gtk) : base(gtk) { }

        // g_object_unref, resolved the same way GtkSharp resolves it (candidate names per platform). Used to
        // drop the object's last native ref out from under the wrapper, reproducing the "freed behind our
        // back" case directly and deterministically.
        private delegate void UnrefDelegate(IntPtr o);

        private static readonly UnrefDelegate GObjectUnref = LoadUnref();

        private static UnrefDelegate LoadUnref()
        {
            string[] candidates =
            {
                "gobject-2.0-0.dll", "libgobject-2.0.so.0", "libgobject-2.0.0.dylib", "libgobject-2.0-0.dll",
            };

            foreach (var name in candidates)
            {
                if (NativeLibrary.TryLoad(name, out var lib) &&
                    NativeLibrary.TryGetExport(lib, "g_object_unref", out var proc))
                {
                    return Marshal.GetDelegateForFunctionPointer<UnrefDelegate>(proc);
                }
            }

            throw new InvalidOperationException("Could not resolve g_object_unref from libgobject.");
        }

        [Fact]
        public void NativeFinalizeBehindWrappersBack_ZerosHandle()
        {
            Run(() =>
            {
                Program.EnsureApplication();

                // A fresh, unparented widget: its only remaining native ref is the toggle ref GtkSharp took.
                var obj = new Label("weak-notify");
                IntPtr h = obj.Handle;
                Assert.NotEqual(IntPtr.Zero, h);

                // Drop that last ref behind the wrapper's back, exactly as a stray unref / Widget.Destroy
                // would. The native object finalizes synchronously; the weak notify must zero the wrapper's
                // handle rather than leave it dangling at the (now freed) address.
                GObjectUnref(h);

                Assert.Equal(IntPtr.Zero, obj.Handle);

                // And the wrapper must still dispose cleanly afterwards: its toggle ref knows the object is
                // gone, so Dispose does no native call on the freed pointer (no double free, no crash).
                var ex = Record.Exception(() => obj.Dispose());
                Assert.Null(ex);

                GC.KeepAlive(obj);
            });
        }

        [Fact]
        public void NormalDispose_StillZerosHandle_AndDoesNotFault()
        {
            Run(() =>
            {
                Program.EnsureApplication();

                // The weak ref must be a no-op for ordinary lifetimes: Dispose unregisters it before dropping
                // the toggle ref, so the notify never fires spuriously and disposal behaves exactly as before.
                var obj = new Label("normal-dispose");
                Assert.NotEqual(IntPtr.Zero, obj.Handle);

                var ex = Record.Exception(() => obj.Dispose());
                Assert.Null(ex);

                Assert.Equal(IntPtr.Zero, obj.Handle);
            });
        }
    }
}
