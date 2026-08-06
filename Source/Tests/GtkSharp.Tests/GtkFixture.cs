using System;
using System.Collections.Concurrent;
using System.Runtime.ExceptionServices;
using System.Threading;
using Gtk;
using Xunit;

namespace GtkSharp.Tests
{
    /// <summary>
    /// Owns the single thread that Gtk is initialised on, and marshals test
    /// bodies onto it.
    /// </summary>
    /// <remarks>
    /// Gtk may only be used from the thread that called gtk_init, and xunit
    /// makes no promise about which thread a test body runs on. Disabling
    /// parallelism is necessary but not sufficient: sequential tests can still
    /// land on different pool threads. So the fixture starts one thread,
    /// initialises Gtk there, and runs every test body on it through
    /// <see cref="Invoke"/>.
    ///
    /// Each work item is followed by draining the pending main-loop work, so
    /// that a failure in layout, a draw function or an idle callback is
    /// attributed to the test that caused it rather than to whichever test
    /// happens to run next.
    /// </remarks>
    public sealed class GtkFixture : IDisposable
    {
        private readonly BlockingCollection<Action> _queue = new BlockingCollection<Action>();
        private readonly Thread _thread;

        public GtkFixture()
        {
            var ready = new ManualResetEventSlim();
            ExceptionDispatchInfo startupError = null;

            _thread = new Thread(() =>
            {
                try
                {
                    Application.Init();
                }
                catch (Exception e)
                {
                    startupError = ExceptionDispatchInfo.Capture(e);
                    ready.Set();
                    return;
                }

                ready.Set();

                foreach (var work in _queue.GetConsumingEnumerable())
                {
                    work();

                    // Bounded: a test that queues work indefinitely should fail
                    // by timing out rather than by hanging the whole run.
                    for (int i = 0; i < 200 && Application.EventsPending(); i++)
                        Application.RunIteration(false);
                }
            });

            _thread.IsBackground = true;
            _thread.Name = "Gtk";
            if (OperatingSystem.IsWindows())
                _thread.SetApartmentState(ApartmentState.STA);

            _thread.Start();
            ready.Wait();

            startupError?.Throw();
        }

        /// <summary>Runs <paramref name="action"/> on the Gtk thread, rethrowing anything it throws.</summary>
        public void Invoke(Action action)
        {
            ExceptionDispatchInfo error = null;
            using (var done = new ManualResetEventSlim())
            {
                _queue.Add(() =>
                {
                    try
                    {
                        action();
                    }
                    catch (Exception e)
                    {
                        error = ExceptionDispatchInfo.Capture(e);
                    }
                    finally
                    {
                        done.Set();
                    }
                });

                done.Wait();
            }

            error?.Throw();
        }

        public T Invoke<T>(Func<T> func)
        {
            T result = default;
            Invoke(() => { result = func(); });
            return result;
        }

        public void Dispose()
        {
            _queue.CompleteAdding();
            _thread.Join(TimeSpan.FromSeconds(10));
        }
    }

    [CollectionDefinition(Name)]
    public sealed class GtkCollection : ICollectionFixture<GtkFixture>
    {
        public const string Name = "Gtk";
    }

    /// <summary>Base class that routes each test body onto the Gtk thread.</summary>
    [Collection(GtkCollection.Name)]
    public abstract class GtkTestBase
    {
        protected GtkTestBase(GtkFixture gtk)
        {
            Gtk = gtk;
        }

        protected GtkFixture Gtk { get; }

        /// <summary>Runs a test body on the Gtk thread.</summary>
        protected void Run(Action body) => Gtk.Invoke(body);

        protected T Run<T>(Func<T> body) => Gtk.Invoke(body);
    }
}
