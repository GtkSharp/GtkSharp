using System;
using Gtk;
using Samples;
using Xunit;

namespace GtkSharp.Tests
{
    /// <summary>
    /// Gtk.Window.Destroy() must be idempotent. gtk_window_destroy on an already-destroyed window trips
    /// "gtk_window_destroy: assertion 'GTK_IS_WINDOW (window)' failed" and can crash the process; a second
    /// close path (a stale timer, a DialogFlags.DestroyWithParent auto-destroy, or wrapper finalization)
    /// used to do exactly that, because the binding never cleared the wrapper's handle when the native
    /// window went away. The hand-written Destroy() in Window.cs skips a null handle and Disposes to clear
    /// it after tearing the window down.
    /// </summary>
    public class WindowDestroyTests : GtkTestBase
    {
        public WindowDestroyTests(GtkFixture gtk) : base(gtk) { }

        [Fact]
        public void Destroy_ClearsHandle()
        {
            Run(() =>
            {
                Program.EnsureApplication();

                var window = new Window { Title = "destroy-once" };
                Assert.NotEqual(IntPtr.Zero, window.Handle);

                window.Destroy();

                // The idempotent Destroy disposes the wrapper, which clears the handle.
                Assert.Equal(IntPtr.Zero, window.Handle);
            });
        }

        [Fact]
        public void Destroy_CalledTwice_IsASafeNoOp()
        {
            Run(() =>
            {
                Program.EnsureApplication();

                var window = new Window { Title = "destroy-twice" };
                window.Destroy();

                // A second destroy on a torn-down window must not throw or crash the process.
                var exception = Record.Exception(() => window.Destroy());

                Assert.Null(exception);
                Assert.Equal(IntPtr.Zero, window.Handle);
            });
        }

        [Fact]
        public void Destroy_ThenDispose_IsASafeNoOp()
        {
            Run(() =>
            {
                Program.EnsureApplication();

                var window = new Window { Title = "destroy-then-dispose" };
                window.Destroy();

                // Dispose after Destroy is the finalization path; it must also be a safe no-op.
                var exception = Record.Exception(() => window.Dispose());

                Assert.Null(exception);
            });
        }
    }
}
