using Xunit;

// Gtk is not thread-safe, and every test body is marshalled onto the single
// thread GtkFixture initialises it on. Running collections in parallel would
// queue work from several threads onto that one thread and interleave tests
// that share global Gtk state, so it is disabled outright.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
