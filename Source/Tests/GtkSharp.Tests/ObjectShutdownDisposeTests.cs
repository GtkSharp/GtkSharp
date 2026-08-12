using System;
using System.Reflection;
using Gtk;
using Samples;
using Xunit;

namespace GtkSharp.Tests
{
    /// <summary>
    /// A GLib.Object finalizer runs on the GC finalizer thread, and at process/AppDomain shutdown the
    /// native GTK/GLib libraries may already be torn down. Touching native objects from a finalizer then
    /// (unref, toggle-ref removal, queuing a GLib timeout) crashes the host with an access violation as it
    /// exits. GLib.Object.Dispose(false) now short-circuits when the runtime is shutting down, dropping the
    /// managed bookkeeping without any native call.
    /// </summary>
    public class ObjectShutdownDisposeTests : GtkTestBase
    {
        public ObjectShutdownDisposeTests(GtkFixture gtk) : base(gtk) { }

        [Fact]
        public void FinalizerPath_DuringShutdown_ClearsHandle_WithoutNativeCalls()
        {
            Run(() =>
            {
                Program.EnsureApplication();

                var obj = new Label("shutdown-dispose");
                Assert.NotEqual(IntPtr.Zero, obj.Handle);

                var dispose = typeof(GLib.Object).GetMethod(
                    "Dispose",
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    null,
                    new[] { typeof(bool) },
                    null);
                Assert.NotNull(dispose);

                var previous = GLib.Object.RuntimeShuttingDown;
                try
                {
                    GLib.Object.RuntimeShuttingDown = () => true;

                    // The finalizer path (disposing:false) must not fault and must clear the handle.
                    var ex = Record.Exception(() => dispose!.Invoke(obj, new object[] { false }));
                    Assert.Null(ex);
                }
                finally
                {
                    GLib.Object.RuntimeShuttingDown = previous;
                }

                Assert.Equal(IntPtr.Zero, obj.Handle);
            });
        }

        [Fact]
        public void NotShuttingDown_StillDisposesNormally()
        {
            Run(() =>
            {
                Program.EnsureApplication();

                // Guard is inactive by default: ordinary Dispose() still tears the object down and clears it.
                var obj = new Label("normal-dispose");
                Assert.NotEqual(IntPtr.Zero, obj.Handle);

                obj.Dispose();

                Assert.Equal(IntPtr.Zero, obj.Handle);
            });
        }
    }
}
