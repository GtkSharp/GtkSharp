using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Gtk;
using Xunit;

namespace GtkSharp.Tests
{
    /// <summary>
    /// <c>GtkExpression</c>: how the Gtk 4 list stack gets a value out of an
    /// item without a cell renderer, and how a property is kept in step with one
    /// that lives on another object entirely.
    /// </summary>
    /// <remarks>
    /// The port bound three fundamental-type hierarchies at once and this is the
    /// one nothing called. Three separate mistakes were sitting in it, each
    /// invisible to a build:
    ///
    ///   - <c>gtk_expression_evaluate</c> and <c>gtk_expression_watch_evaluate</c>
    ///     fill a GValue the <em>caller</em> owns. Codegen copied a Value out to
    ///     unmanaged memory, called, and freed it again without reading it back,
    ///     so every evaluation returned <c>true</c> and no value.
    ///   - <c>gtk_expression_bind</c> takes its receiver <c>(transfer full)</c>.
    ///     The wrapper went on owning a reference the watch had eaten, and the
    ///     second unref is deferred onto the main loop by the generated
    ///     finalizer -- so it lands wherever the GC happens to run.
    ///   - <c>gtk_closure_expression_new</c> and <c>gtk_try_expression_new</c>
    ///     take an array plus a count, and came out taking a single Expression
    ///     whose own first machine word GTK then read as element zero.
    ///     <c>gtk_cclosure_expression_new</c> was not emitted at all.
    ///
    /// The oracles below are arithmetic and orderings chosen here -- the length
    /// of a word, an alphabet, a set of ages -- rather than anything read back
    /// out of Gtk.
    /// </remarks>
    public class ExpressionTests : GtkTestBase
    {
        public ExpressionTests(GtkFixture fixture) : base(fixture) { }

        // --------------------------------------------------------- the subjects

        /// <summary>A place, so that a property chain has somewhere to go.</summary>
        [GLib.TypeName("GtkSharpTestsExpressionPlace")]
        public class Place : GLib.Object
        {
            private string city = "";

            public Place() { }
            public Place(IntPtr raw) : base(raw) { }

            [GLib.Property("city")]
            public string City
            {
                get { return city; }
                set { city = value; Notify("city"); }
            }
        }

        /// <summary>
        /// A row. Every setter announces itself: a [GLib.Property] setter is an
        /// ordinary C# setter and emits no notify of its own, so a watch or a
        /// binding would never see an assignment made in C#. <see cref="Nickname"/>
        /// deliberately does not, which is what
        /// <see cref="A_binding_cannot_follow_a_setter_that_does_not_notify"/> pins.
        /// </summary>
        [GLib.TypeName("GtkSharpTestsExpressionPerson")]
        public class Person : GLib.Object
        {
            private string name = "";
            private string nickname = "";
            private int age;
            private Place home;

            public Person() { }
            public Person(IntPtr raw) : base(raw) { }

            [GLib.Property("name")]
            public string Name
            {
                get { return name; }
                set { name = value; Notify("name"); }
            }

            [GLib.Property("nickname")]
            public string Nickname
            {
                get { return nickname; }
                set { nickname = value; }
            }

            [GLib.Property("age")]
            public int Age
            {
                get { return age; }
                set { age = value; Notify("age"); }
            }

            [GLib.Property("home")]
            public Place Home
            {
                get { return home; }
                set { home = value; Notify("home"); }
            }
        }

        private static GLib.GType PersonType
        {
            get { return (GLib.GType) typeof(Person); }
        }

        private static GLib.GType PlaceType
        {
            get { return (GLib.GType) typeof(Place); }
        }

        /// <summary>"name" read off the this-object.</summary>
        private static Expression NameOf()
        {
            return new PropertyExpression(PersonType, null, "name");
        }

        /// <summary>"home.city" -- two property expressions, the outer one rooted on the inner.</summary>
        private static Expression HomeCityOf()
        {
            return new PropertyExpression(PlaceType, new PropertyExpression(PersonType, null, "home"), "city");
        }

        // ----------------------------------------------------- ConstantExpression

        [Fact]
        public void A_constant_expression_evaluates_to_the_value_it_was_built_from()
        {
            // The test that caught the discarded result: the generated Evaluate
            // returned true and left the caller's Value untouched, so this
            // asserted 0 against 42 while the call itself "succeeded".
            Run(() =>
            {
                using var expression = new ConstantExpression(new GLib.Value(42));

                Assert.Equal(GLib.GType.Int.Val, expression.ValueType.Val);

                GLib.Value value;
                Assert.True(expression.Evaluate(null, out value));
                Assert.Equal(42, (int) value.Val);
                value.Dispose();
            });
        }

        [Fact]
        public void A_constant_string_expression_survives_the_round_trip_through_a_GValue()
        {
            // A string is the case a memcpy of the GValue struct cannot fake:
            // the answer is a pointer into unmanaged memory that has to still be
            // valid after the native GValue block has been freed.
            Run(() =>
            {
                using var expression = new ConstantExpression(new GLib.Value("Ada Lovelace"));

                GLib.Value value;
                Assert.True(expression.Evaluate(null, out value));
                Assert.Equal("Ada Lovelace", (string) value.Val);
                value.Dispose();

                // The expression keeps its own copy; reading it a second time
                // must give the same answer, not freed memory.
                Assert.Equal("Ada Lovelace", (string) expression.Value.Val);
            });
        }

        [Fact]
        public void A_constant_expression_is_static_and_a_property_expression_is_not()
        {
            // IsStatic is what tells a sorter it may cache: an expression that
            // reports static and is not silently stops updating. Constant is the
            // only one of the two that can never change.
            Run(() =>
            {
                using var constant = new ConstantExpression(new GLib.Value(1));
                using var property = NameOf();

                Assert.True(constant.IsStatic);
                Assert.False(property.IsStatic);
            });
        }

        // ----------------------------------------------------- PropertyExpression

        [Fact]
        public void A_property_expression_reads_the_property_off_the_this_object()
        {
            Run(() =>
            {
                var person = new Person { Name = "Grace" };
                using var expression = NameOf();

                Assert.Equal(GLib.GType.String.Val, expression.ValueType.Val);

                GLib.Value value;
                Assert.True(expression.Evaluate(person, out value));
                Assert.Equal("Grace", (string) value.Val);
                value.Dispose();
            });
        }

        [Fact]
        public void An_int_property_evaluates_to_an_int_and_not_to_its_text()
        {
            // The value type comes from the GParamSpec, so a binding that quietly
            // stringified would still "work" everywhere a label is the target.
            Run(() =>
            {
                var person = new Person { Age = 36 };
                using var expression = new PropertyExpression(PersonType, null, "age");

                Assert.Equal(GLib.GType.Int.Val, expression.ValueType.Val);

                GLib.Value value;
                Assert.True(expression.Evaluate(person, out value));
                Assert.Equal(36, (int) value.Val);
                value.Dispose();
            });
        }

        [Fact]
        public void A_chained_property_expression_walks_to_the_far_object()
        {
            Run(() =>
            {
                var person = new Person { Name = "Ada", Home = new Place { City = "London" } };
                using var expression = HomeCityOf();

                GLib.Value value;
                Assert.True(expression.Evaluate(person, out value));
                Assert.Equal("London", (string) value.Val);
                value.Dispose();
            });
        }

        [Fact]
        public void A_chained_property_expression_fails_rather_than_throwing_when_a_link_is_null()
        {
            // This is the whole reason GtkTryExpression exists, and the reason
            // Evaluate returns a bool at all: a chain through a null is a normal
            // outcome, not an error.
            Run(() =>
            {
                var person = new Person { Name = "Ada" };
                using var expression = HomeCityOf();

                GLib.Value missing;
                Assert.False(expression.Evaluate(person, out missing));

                // ... and the false means something, because filling the link in
                // makes the same expression succeed.
                person.Home = new Place { City = "Paris" };

                GLib.Value found;
                Assert.True(expression.Evaluate(person, out found));
                Assert.Equal("Paris", (string) found.Val);
                found.Dispose();
            });
        }

        [Fact]
        public void A_property_expression_exposes_the_sub_expression_it_was_given()
        {
            Run(() =>
            {
                using var inner = new PropertyExpression(PersonType, null, "home");
                var outer = new PropertyExpression(PlaceType, inner, "city");

                Assert.Equal(inner.Handle, outer.Expression.Handle);
                Assert.NotEqual(IntPtr.Zero, outer.Pspec);

                // The constructor is (transfer full) in its expression argument,
                // and the binding hands over a reference of its own -- so the
                // caller's wrapper is still usable rather than dangling.
                Assert.False(inner.IsStatic);
                outer.Dispose();
            });
        }

        [Fact]
        public void A_property_expression_with_no_sub_expression_reports_no_sub_expression()
        {
            Run(() =>
            {
                using var expression = new PropertyExpression(PersonType, null, "name");

                Assert.Null(expression.Expression);
            });
        }

        // ------------------------------------------------------- ObjectExpression

        [Fact]
        public void An_object_expression_ignores_the_this_object_entirely()
        {
            // GtkObjectExpression is how a template escapes the item it is bound
            // to: the answer is the object it was built with, whatever is passed
            // in as this.
            Run(() =>
            {
                var pinned = new Place { City = "Turin" };
                using var expression = new ObjectExpression(pinned);

                GLib.Value value;
                Assert.True(expression.Evaluate(null, out value));
                Assert.Equal(pinned.Handle, ((GLib.Object) value.Val).Handle);
                value.Dispose();

                Assert.Equal(pinned.Handle, expression.Object.Handle);
            });
        }

        [Fact]
        public void A_property_expression_rooted_at_an_object_expression_needs_no_this()
        {
            Run(() =>
            {
                var pinned = new Place { City = "Turin" };
                using var expression = new PropertyExpression(PlaceType, new ObjectExpression(pinned), "city");

                GLib.Value value;
                Assert.True(expression.Evaluate(null, out value));
                Assert.Equal("Turin", (string) value.Val);
                value.Dispose();
            });
        }

        // ---------------------------------------------------------------- watches

        [Fact]
        public void A_watch_fires_when_the_property_it_reads_changes_and_not_otherwise()
        {
            Run(() =>
            {
                var person = new Person { Name = "Ada" };
                using var expression = NameOf();

                int fired = 0;
                var watch = expression.Watch(person, () => fired++);

                Assert.Equal(0, fired);

                // A property the expression does not read must not wake it.
                person.Age = 99;
                Assert.Equal(0, fired);

                person.Name = "Grace";
                Assert.Equal(1, fired);

                watch.Unwatch();
            });
        }

        [Fact]
        public void A_watch_re_evaluates_to_the_new_value()
        {
            // gtk_expression_watch_evaluate is the second caller-allocates
            // GValue, with the same defect and the same symptom: a watch that
            // says "something changed" and cannot say to what.
            Run(() =>
            {
                var person = new Person { Name = "Ada" };
                using var expression = NameOf();

                var watch = expression.Watch(person, () => { });

                GLib.Value before;
                Assert.True(watch.Evaluate(out before));
                Assert.Equal("Ada", (string) before.Val);
                before.Dispose();

                person.Name = "Grace";

                GLib.Value after;
                Assert.True(watch.Evaluate(out after));
                Assert.Equal("Grace", (string) after.Val);
                after.Dispose();

                watch.Unwatch();
            });
        }

        [Fact]
        public void Unwatching_stops_the_notifications()
        {
            Run(() =>
            {
                var person = new Person { Name = "Ada" };
                using var expression = NameOf();

                int fired = 0;
                var watch = expression.Watch(person, () => fired++);

                person.Name = "Grace";
                Assert.Equal(1, fired);

                watch.Unwatch();

                person.Name = "Hedy";
                Assert.Equal(1, fired);
            });
        }

        [Fact]
        public void A_watch_on_a_chain_fires_when_the_intermediate_object_is_replaced()
        {
            // The point of a watch rather than a notify handler: it re-hooks
            // itself onto whatever object the middle of the chain now names.
            Run(() =>
            {
                var person = new Person { Home = new Place { City = "London" } };
                using var expression = HomeCityOf();

                int fired = 0;
                var watch = expression.Watch(person, () => fired++);

                // A change to the far end of the chain.
                person.Home.City = "Paris";
                Assert.Equal(1, fired);

                // ... and a change to the middle of it.
                person.Home = new Place { City = "Rome" };
                Assert.True(fired >= 2);

                GLib.Value value;
                Assert.True(watch.Evaluate(out value));
                Assert.Equal("Rome", (string) value.Val);
                value.Dispose();

                watch.Unwatch();
            });
        }

        // ------------------------------------------------------------------- Bind

        [Fact]
        public void Bind_pushes_the_value_into_the_target_before_anything_changes()
        {
            Run(() =>
            {
                var person = new Person { Name = "Ada" };
                var label = new Label("placeholder");

                var watch = NameOf().Bind(label, "label", person);

                Assert.Equal("Ada", label.Text);
                watch.Unwatch();
            });
        }

        [Fact]
        public void Bind_keeps_the_target_in_step_with_the_source()
        {
            Run(() =>
            {
                var person = new Person { Name = "Ada" };
                var label = new Label("");

                var watch = NameOf().Bind(label, "label", person);

                person.Name = "Grace";
                Assert.Equal("Grace", label.Text);

                person.Name = "Hedy";
                Assert.Equal("Hedy", label.Text);

                watch.Unwatch();
            });
        }

        [Fact]
        public void Bind_is_one_way()
        {
            // gtk_expression_bind has no bidirectional mode -- unlike
            // g_object_bind_property, which is the API it resembles. Writing the
            // target does not write the source, and does not even stick if the
            // expression re-evaluates afterwards.
            Run(() =>
            {
                var person = new Person { Name = "Ada" };
                var label = new Label("");

                var watch = NameOf().Bind(label, "label", person);

                label.Text = "typed by hand";
                Assert.Equal("Ada", person.Name);

                person.Name = "Grace";
                Assert.Equal("Grace", label.Text);

                watch.Unwatch();
            });
        }

        [Fact]
        public void Bind_leaves_the_target_alone_when_the_expression_cannot_be_evaluated()
        {
            // Documented, and the reason TryExpression exists: a failed
            // evaluation is not an error and not a reset, it is a no-op. So a
            // stale value on screen is the failure mode to expect.
            Run(() =>
            {
                var person = new Person { Home = new Place { City = "London" } };
                var label = new Label("");

                var watch = HomeCityOf().Bind(label, "label", person);
                Assert.Equal("London", label.Text);

                person.Home = null;

                Assert.Equal("London", label.Text);

                watch.Unwatch();
            });
        }

        [Fact]
        public void Unwatching_a_binding_stops_it()
        {
            Run(() =>
            {
                var person = new Person { Name = "Ada" };
                var label = new Label("");

                var watch = NameOf().Bind(label, "label", person);
                watch.Unwatch();

                person.Name = "Grace";

                Assert.Equal("Ada", label.Text);
            });
        }

        [Fact]
        public void A_binding_can_drive_a_managed_property_of_a_different_type()
        {
            // The target's property is written with g_object_set, so it goes
            // through the managed set_property trampoline rather than through
            // the C# setter -- the other direction of the binding from
            // everything else here.
            Run(() =>
            {
                var source = new Person { Age = 41 };
                var target = new Person { Age = 0 };

                var watch = new PropertyExpression(PersonType, null, "age").Bind(target, "age", source);

                Assert.Equal(41, target.Age);

                source.Age = 42;
                Assert.Equal(42, target.Age);

                watch.Unwatch();
            });
        }

        [Fact]
        public void A_binding_cannot_follow_a_setter_that_does_not_notify()
        {
            // Nickname's setter is an ordinary C# setter with no Notify call,
            // which is what a [GLib.Property] gets by default. Nothing errors:
            // the binding simply never fires again, and the target keeps the
            // value it was initialised with.
            Run(() =>
            {
                var person = new Person { Nickname = "Ada" };
                var label = new Label("");

                var watch = new PropertyExpression(PersonType, null, "nickname").Bind(label, "label", person);
                Assert.Equal("Ada", label.Text);

                person.Nickname = "Grace";
                Assert.Equal("Grace", person.Nickname);
                Assert.Equal("Ada", label.Text);

                // Writing through GObject does notify, so the binding is alive.
                person.SetProperty("nickname", new GLib.Value("Hedy"));
                Assert.Equal("Hedy", label.Text);

                watch.Unwatch();
            });
        }

        [Fact]
        public void Binding_fifty_expressions_and_collecting_them_upsets_nothing()
        {
            // gtk_expression_bind's instance parameter is transfer-full: the
            // watch owns the expression afterwards. The generated binding passed
            // Handle and kept its own reference, so both unreffed -- and the
            // second free is queued onto the main loop by the generated
            // finalizer, landing on whatever test the GC happens to interrupt.
            // No single call reproduces it; fifty wrappers, a forced collection
            // and a drained loop do.
            Run(() =>
            {
                var complaints = new List<string>();
                GLib.LogFunc record = (domain, level, message) => complaints.Add(domain + ": " + message);
                var gtk = GLib.Log.SetLogHandler("Gtk", GLib.LogLevelFlags.Critical | GLib.LogLevelFlags.Warning, record);
                var glib = GLib.Log.SetLogHandler("GLib-GObject", GLib.LogLevelFlags.Critical | GLib.LogLevelFlags.Warning, record);

                try
                {
                    for (int i = 0; i < 50; i++)
                    {
                        var person = new Person { Name = "person " + i };
                        var label = new Label("");

                        // Deliberately not kept: the expression wrapper becomes
                        // garbage while the watch is still using the expression.
                        NameOf().Bind(label, "label", person);

                        Assert.Equal("person " + i, label.Text);
                    }

                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    GC.Collect();

                    var deadline = Stopwatch.StartNew();
                    while (deadline.ElapsedMilliseconds < 400)
                        if (Application.EventsPending())
                            Application.RunIteration(false);
                }
                finally
                {
                    GLib.Log.RemoveLogHandler("Gtk", gtk);
                    GLib.Log.RemoveLogHandler("GLib-GObject", glib);
                }

                Assert.Empty(complaints);

                // And an expression built afterwards still works, which is the
                // half of the oracle that a mere absence of criticals is not.
                var later = new Person { Name = "still here" };
                using var expression = NameOf();
                GLib.Value value;
                Assert.True(expression.Evaluate(later, out value));
                Assert.Equal("still here", (string) value.Val);
                value.Dispose();
            });
        }

        // ------------------------------------------------------ sorters and filters

        private static GLib.ListStore PeopleNamed(params string[] names)
        {
            var store = new GLib.ListStore(PersonType);
            foreach (var name in names)
                store.Append(new Person { Name = name }.Handle);
            return store;
        }

        private static string[] NamesOf(GLib.IListModel model)
        {
            var names = new string[model.NItems];
            for (uint i = 0; i < model.NItems; i++)
                names[i] = ((Person) model.GetObject(i)).Name;
            return names;
        }

        [Fact]
        public void A_StringSorter_orders_a_list_model_by_the_expression()
        {
            // The whole point of an expression: the sorter knows nothing about
            // Person and reads "name" through the expression alone.
            Run(() =>
            {
                var store = PeopleNamed("Hedy", "Ada", "Grace");
                var sorted = new SortListModel(store, new StringSorter(NameOf()));

                Assert.Equal(new[] { "Ada", "Grace", "Hedy" }, NamesOf(sorted));
            });
        }

        [Fact]
        public void Replacing_a_sorters_expression_re_sorts_what_is_already_there()
        {
            Run(() =>
            {
                var store = new GLib.ListStore(PersonType);
                store.Append(new Person { Name = "Ada", Home = new Place { City = "Turin" } }.Handle);
                store.Append(new Person { Name = "Grace", Home = new Place { City = "Rome" } }.Handle);
                store.Append(new Person { Name = "Hedy", Home = new Place { City = "Paris" } }.Handle);

                var sorter = new StringSorter(NameOf());
                var sorted = new SortListModel(store, sorter);
                Assert.Equal(new[] { "Ada", "Grace", "Hedy" }, NamesOf(sorted));

                // Paris < Rome < Turin, which is the reverse of the names.
                sorter.Expression = HomeCityOf();

                Assert.Equal(new[] { "Hedy", "Grace", "Ada" }, NamesOf(sorted));
            });
        }

        [Fact]
        public void A_sorted_model_does_not_notice_a_property_changing_under_it()
        {
            // The trap, and the opposite of what Bind does: a GtkSorter is a
            // comparison function, not an observer. It reads "name" through the
            // expression but installs no watch on any item, so renaming one
            // leaves the model in an order that is now wrong and says nothing.
            //
            // Worse, the obvious remedy is not one. gtk_sorter_changed
            // (GTK_SORTER_CHANGE_DIFFERENT) re-runs the sort over the sort keys
            // GtkSortListModel cached when the item arrived, so it reorders
            // nothing -- even though the sorter itself, asked directly, already
            // gives the new answer. What actually works is telling the *model*
            // the item changed, which is what makes it recompute that item's key.
            Run(() =>
            {
                var store = PeopleNamed("Ada", "Grace");
                var sorter = new StringSorter(NameOf());
                var sorted = new SortListModel(store, sorter);

                Assert.Equal(new[] { "Ada", "Grace" }, NamesOf(sorted));

                ((Person) store.GetObject(0)).Name = "Zoe";

                Assert.Equal(new[] { "Zoe", "Grace" }, NamesOf(sorted));

                // The sorter is not the stale part: it compares the new names
                // correctly the moment it is asked.
                Assert.Equal(Ordering.Larger, sorter.Compare(store.GetItem(0), store.GetItem(1)));

                sorter.EmitChanged(SorterChange.Different);
                while (Application.EventsPending())
                    Application.RunIteration(false);
                Assert.Equal(new[] { "Zoe", "Grace" }, NamesOf(sorted));

                store.EmitItemsChanged(0, 1, 1);

                Assert.Equal(new[] { "Grace", "Zoe" }, NamesOf(sorted));
            });
        }

        [Fact]
        public void A_NumericSorter_orders_by_an_int_expression_in_both_directions()
        {
            Run(() =>
            {
                var store = new GLib.ListStore(PersonType);
                store.Append(new Person { Name = "Ada", Age = 36 }.Handle);
                store.Append(new Person { Name = "Grace", Age = 85 }.Handle);
                store.Append(new Person { Name = "Hedy", Age = 20 }.Handle);

                var sorter = new NumericSorter(new PropertyExpression(PersonType, null, "age"));
                var sorted = new SortListModel(store, sorter);

                Assert.Equal(new[] { "Hedy", "Ada", "Grace" }, NamesOf(sorted));

                sorter.SortOrder = SortType.Descending;

                Assert.Equal(new[] { "Grace", "Ada", "Hedy" }, NamesOf(sorted));
            });
        }

        [Fact]
        public void A_StringFilter_keeps_only_the_items_whose_expression_matches()
        {
            Run(() =>
            {
                var store = PeopleNamed("Ada", "Grace", "Hedy", "Radia");
                var filter = new StringFilter(NameOf())
                {
                    MatchMode = StringFilterMatchMode.Substring,
                    Search = "ad"
                };
                var filtered = new FilterListModel(store, filter);

                // Substring, case-insensitive by default: "Ada" and "Radia".
                Assert.Equal(new[] { "Ada", "Radia" }, NamesOf(filtered));

                filter.MatchMode = StringFilterMatchMode.Prefix;
                Assert.Equal(new[] { "Ada" }, NamesOf(filtered));

                // Case matters now, and only "Radia" spells "ad" in lower case --
                // "Ada" spells "Ad". Which is the reminder that IgnoreCase is on
                // by default and a filter that looks right is not necessarily
                // matching what the user typed.
                filter.IgnoreCase = false;
                filter.MatchMode = StringFilterMatchMode.Substring;
                Assert.Equal(new[] { "Radia" }, NamesOf(filtered));
            });
        }

        [Fact]
        public void A_BoolFilter_uses_the_expression_as_the_predicate_and_can_invert_it()
        {
            Run(() =>
            {
                var store = new GLib.ListStore(PersonType);
                store.Append(new Person { Name = "Ada" }.Handle);
                store.Append(new Person { Name = "Grace", Home = new Place { City = "Rome" } }.Handle);

                // A closure over "does this person have a home", so the predicate
                // is arithmetic done here rather than a Gtk property.
                var predicate = new HasHomeCallback(HasHome);
                var filter = new BoolFilter(new CClosureExpression(GLib.GType.Boolean, predicate,
                    new PropertyExpression(PersonType, null, "home")));
                var filtered = new FilterListModel(store, filter);

                Assert.Equal(new[] { "Grace" }, NamesOf(filtered));

                filter.Invert = true;
                Assert.Equal(new[] { "Ada" }, NamesOf(filtered));

                GC.KeepAlive(predicate);
            });
        }

        // ------------------------------------------------- closures as expressions

        // GObject's generic (libffi) marshaller calls the callback with the C
        // signature the GValues describe: (this_object, param1 .. paramN,
        // user_data). Cdecl, because that is what a GCallback is.
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int NameLengthCallback(IntPtr this_, IntPtr name, IntPtr userData);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int SumCallback(IntPtr this_, int age, int bonus, IntPtr userData);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool HasHomeCallback(IntPtr this_, IntPtr home, IntPtr userData);

        private static int NameLength(IntPtr this_, IntPtr name, IntPtr userData)
        {
            return GLib.Marshaller.Utf8PtrToString(name).Length;
        }

        private static bool HasHome(IntPtr this_, IntPtr home, IntPtr userData)
        {
            return home != IntPtr.Zero;
        }

        [Fact]
        public void A_CClosureExpression_computes_its_value_from_the_parameters_it_is_given()
        {
            // gtk_cclosure_expression_new produced no constructor at all --
            // GClosureMarshal, GCallback and GClosureNotify are unmapped, so the
            // whole node was dropped and the class was left with nothing but a
            // GType. This is the type a list view uses to derive a display value
            // from an item, so it was the job that could not be done.
            //
            // "Ada Lovelace" is twelve characters, which is a fact about the
            // string in this file rather than about Gtk.
            Run(() =>
            {
                var callback = new NameLengthCallback(NameLength);
                using var expression = new CClosureExpression(GLib.GType.Int, callback, NameOf());

                Assert.Equal(GLib.GType.Int.Val, expression.ValueType.Val);

                var person = new Person { Name = "Ada Lovelace" };

                GLib.Value value;
                Assert.True(expression.Evaluate(person, out value));
                Assert.Equal(12, (int) value.Val);
                value.Dispose();

                person.Name = "Hedy";
                Assert.True(expression.Evaluate(person, out value));
                Assert.Equal(4, (int) value.Val);
                value.Dispose();

                GC.KeepAlive(callback);
            });
        }

        [Fact]
        public void A_closure_expression_over_two_parameters_gets_them_in_order()
        {
            // The array-plus-count defect this pins: with a single Expression
            // passed where GtkExpression** was expected, GTK read the
            // expression's own first machine word as element zero, so a closure
            // with two parameters could not be built at all. 41 + 1 is the sort
            // of sum whose answer is different if the arguments swap places --
            // so the second parameter is a constant, not another 41.
            Run(() =>
            {
                SumCallback callback = (this_, age, bonus, data) => age * 10 + bonus;
                using var expression = new ClosureExpression(GLib.GType.Int, callback,
                    new PropertyExpression(PersonType, null, "age"),
                    new ConstantExpression(new GLib.Value(7)));

                var person = new Person { Age = 41 };

                GLib.Value value;
                Assert.True(expression.Evaluate(person, out value));
                Assert.Equal(417, (int) value.Val);
                value.Dispose();

                GC.KeepAlive(callback);
            });
        }

        [Fact]
        public void A_closure_expression_fails_when_one_of_its_parameters_does()
        {
            // A closure is not consulted at all if a parameter cannot be
            // evaluated, so a caller cannot use one to supply a default -- which
            // is the job TryExpression has.
            Run(() =>
            {
                int calls = 0;
                NameLengthCallback callback = (this_, city, data) =>
                {
                    calls++;
                    return GLib.Marshaller.Utf8PtrToString(city).Length;
                };
                using var expression = new CClosureExpression(GLib.GType.Int, callback, HomeCityOf());

                var homeless = new Person { Name = "Ada" };

                GLib.Value value;
                Assert.False(expression.Evaluate(homeless, out value));
                Assert.Equal(0, calls);

                homeless.Home = new Place { City = "Rome" };
                Assert.True(expression.Evaluate(homeless, out value));
                Assert.Equal(4, (int) value.Val);
                Assert.Equal(1, calls);
                value.Dispose();

                GC.KeepAlive(callback);
            });
        }

        [Fact]
        public void A_closure_expression_re_runs_when_a_parameter_it_reads_changes()
        {
            Run(() =>
            {
                var callback = new NameLengthCallback(NameLength);
                using var expression = new CClosureExpression(GLib.GType.Int, callback, NameOf());

                var person = new Person { Name = "Ada" };
                var target = new Person { Age = 0 };

                var watch = new CClosureExpression(GLib.GType.Int, callback, NameOf())
                    .Bind(target, "age", person);

                Assert.Equal(3, target.Age);

                person.Name = "Ada Lovelace";
                Assert.Equal(12, target.Age);

                watch.Unwatch();
                GC.KeepAlive(callback);
            });
        }

        // -------------------------------------------------------- TryExpression

        [Fact]
        public void A_TryExpression_yields_the_first_branch_that_evaluates()
        {
            // gtk_try_expression_new arrived in Gtk 4.22, which is the version
            // the api.xml here is generated from.
            Run(() =>
            {
                using var expression = new TryExpression(
                    HomeCityOf(),
                    new ConstantExpression(new GLib.Value("nowhere")));

                var homeless = new Person { Name = "Ada" };

                GLib.Value fallback;
                Assert.True(expression.Evaluate(homeless, out fallback));
                Assert.Equal("nowhere", (string) fallback.Val);
                fallback.Dispose();

                homeless.Home = new Place { City = "Rome" };

                GLib.Value found;
                Assert.True(expression.Evaluate(homeless, out found));
                Assert.Equal("Rome", (string) found.Val);
                found.Dispose();
            });
        }

        [Fact]
        public void A_TryExpression_fails_when_every_branch_fails()
        {
            Run(() =>
            {
                using var expression = new TryExpression(HomeCityOf(), HomeCityOf());

                GLib.Value value;
                Assert.False(expression.Evaluate(new Person { Name = "Ada" }, out value));
            });
        }

        [Fact]
        public void A_TryExpression_gives_a_binding_the_fallback_it_otherwise_has_no_way_to_get()
        {
            // Bind leaves the target alone when evaluation fails; wrapping the
            // chain in a TryExpression is the documented answer, and this is the
            // difference between the two written out.
            Run(() =>
            {
                var person = new Person { Home = new Place { City = "London" } };
                var label = new Label("");

                var watch = new TryExpression(HomeCityOf(), new ConstantExpression(new GLib.Value("nowhere")))
                    .Bind(label, "label", person);

                Assert.Equal("London", label.Text);

                person.Home = null;

                Assert.Equal("nowhere", label.Text);

                watch.Unwatch();
            });
        }

        // --------------------------------------------------------- the hierarchy

        [Fact]
        public void Each_expression_object_is_natively_of_the_subtype_its_wrapper_claims()
        {
            // GtkExpression is a GTypeInstance fundamental, not a GObject, and
            // the metadata roots the whole family at GLib.Opaque to get it bound
            // at all -- so the managed hierarchy is asserted here rather than
            // enforced anywhere. GType.IsInstance reads the class pointer out of
            // the object Gtk actually returned, so it compares the two.
            Run(() =>
            {
                using var property = NameOf();
                using var constant = new ConstantExpression(new GLib.Value(1));
                using var objekt = new ObjectExpression(new Place());
                using var attempt = new TryExpression(NameOf());

                Assert.True(Expression.GType.IsInstance(property.Handle));
                Assert.True(Expression.GType.IsInstance(constant.Handle));
                Assert.True(Expression.GType.IsInstance(objekt.Handle));
                Assert.True(Expression.GType.IsInstance(attempt.Handle));

                Assert.True(PropertyExpression.GType.IsInstance(property.Handle));
                Assert.True(ConstantExpression.GType.IsInstance(constant.Handle));
                Assert.True(ObjectExpression.GType.IsInstance(objekt.Handle));
                Assert.True(TryExpression.GType.IsInstance(attempt.Handle));

                // ... and not the trivial answer: each subtype rejects the others.
                Assert.False(PropertyExpression.GType.IsInstance(constant.Handle));
                Assert.False(ConstantExpression.GType.IsInstance(property.Handle));
                Assert.False(ObjectExpression.GType.IsInstance(attempt.Handle));
            });
        }

        [Fact]
        public void The_two_closure_expression_kinds_are_different_native_types()
        {
            // gtk_cclosure_expression_new builds its GClosure and then hands off
            // to gtk_closure_expression_new, so it would be easy for the two
            // bindings to be one type wearing two names. They are not, and the
            // separate get_type symbols are what says so -- a get_type Gtk had
            // removed would be a null delegate here, not a link error.
            Run(() =>
            {
                var callback = new NameLengthCallback(NameLength);
                using var cclosure = new CClosureExpression(GLib.GType.Int, callback, NameOf());
                using var closure = new ClosureExpression(GLib.GType.Int, callback, NameOf());

                Assert.True(CClosureExpression.GType.IsInstance(cclosure.Handle));
                Assert.True(ClosureExpression.GType.IsInstance(closure.Handle));
                Assert.False(CClosureExpression.GType.IsInstance(closure.Handle));
                Assert.False(ClosureExpression.GType.IsInstance(cclosure.Handle));

                Assert.True(Expression.GType.IsInstance(cclosure.Handle));
                Assert.True(Expression.GType.IsInstance(closure.Handle));

                GC.KeepAlive(callback);
            });
        }
    }
}
