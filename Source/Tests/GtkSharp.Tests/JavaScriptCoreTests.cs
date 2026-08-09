using System;
using System.Linq;
using Xunit;

namespace GtkSharp.Tests
{
    /// <summary>
    /// <c>JavaScriptCoreSharp</c>: evaluating script, moving values across the
    /// boundary in both directions, and the exception channel.
    /// </summary>
    /// <remarks>
    /// Thirty bound types, of which the suite named three — and only to check
    /// that <c>21 * 2</c> came back as 42. JSC is also the assembly with the
    /// least excuse for that: unlike WebKit it needs no display, no session bus
    /// and no sandbox, so a <c>Context</c> can be built anywhere the library is
    /// installed.
    ///
    /// It is not installed on Windows — gvsbuild ships no jsc — so these are
    /// <c>[SkippableFact]</c> and run on Linux. That is also why
    /// <c>Docs/coverage.md</c> reports 0 of 590 generated lines for this
    /// assembly: the coverage run happens on Windows, where none of it can load.
    ///
    /// The oracle is arithmetic and text the test chose, computed by the engine
    /// and read back — or, for the round trips, a value the test put in and got
    /// out again.
    /// </remarks>
    public class JavaScriptCoreTests : GtkTestBase
    {
        public JavaScriptCoreTests(GtkFixture fixture) : base(fixture) { }

        static void RequireJsc()
            => Skip.IfNot(JavaScriptCore.Global.IsSupported,
                          "JavaScriptCore is not installed (gvsbuild ships no jsc).");

        // ------------------------------------------------- values out of script

        [SkippableFact]
        public void Every_javascript_type_comes_back_as_itself()
        {
            RequireJsc();

            Run(() =>
            {
                var context = new JavaScriptCore.Context();

                var number = context.Evaluate("6 * 7");
                Assert.True(number.IsNumber);
                Assert.Equal(42, number.ToInt32());
                Assert.Equal(42.0, number.ToDouble(), 6);

                var fraction = context.Evaluate("1 / 8");
                Assert.Equal(0.125, fraction.ToDouble(), 6);

                var text = context.Evaluate("'a' + 'b'");
                Assert.True(text.IsString);
                Assert.Equal("ab", text.ToString());

                var flag = context.Evaluate("3 > 2");
                Assert.True(flag.IsBoolean);
                Assert.True(flag.ToBoolean());

                var nothing = context.Evaluate("null");
                Assert.True(nothing.IsNull);
                Assert.False(nothing.IsUndefined);

                var missing = context.Evaluate("undefined");
                Assert.True(missing.IsUndefined);
                Assert.False(missing.IsNull);
            });
        }

        [SkippableFact]
        public void An_array_is_indexable_and_knows_its_length()
        {
            RequireJsc();

            Run(() =>
            {
                var context = new JavaScriptCore.Context();
                var array = context.Evaluate("['zero', 'one', 'two']");

                Assert.True(array.IsArray);
                Assert.True(array.IsObject, "an array is an object in JavaScript");

                Assert.Equal(3, array.ObjectGetProperty("length").ToInt32());
                Assert.Equal("zero", array.ObjectGetPropertyAtIndex(0).ToString());
                Assert.Equal("two", array.ObjectGetPropertyAtIndex(2).ToString());
            });
        }

        [SkippableFact]
        public void An_objects_properties_can_be_read_listed_and_removed()
        {
            RequireJsc();

            Run(() =>
            {
                var context = new JavaScriptCore.Context();
                var value = context.Evaluate("({ name: 'gtk', version: 4 })");

                Assert.True(value.IsObject);
                Assert.True(value.ObjectHasProperty("name"));
                Assert.False(value.ObjectHasProperty("absent"));

                Assert.Equal("gtk", value.ObjectGetProperty("name").ToString());
                Assert.Equal(4, value.ObjectGetProperty("version").ToInt32());

                var names = value.ObjectEnumerateProperties();
                Assert.Contains("name", names);
                Assert.Contains("version", names);

                Assert.True(value.ObjectDeleteProperty("version"));
                Assert.False(value.ObjectHasProperty("version"));
                Assert.True(value.ObjectHasProperty("name"), "deleting one must not take the other");
            });
        }

        // ------------------------------------------------- values into script

        [SkippableFact]
        public void A_value_built_in_csharp_reports_the_type_it_was_built_as()
        {
            RequireJsc();

            Run(() =>
            {
                var context = new JavaScriptCore.Context();

                var number = new JavaScriptCore.Value(context, 42.5);
                Assert.True(number.IsNumber);
                Assert.Equal(42.5, number.ToDouble(), 6);

                var text = JavaScriptCore.Value.NewString(context, "from C#");
                Assert.True(text.IsString);
                Assert.Equal("from C#", text.ToString());

                var flag = new JavaScriptCore.Value(context, true);
                Assert.True(flag.IsBoolean);
                Assert.True(flag.ToBoolean());

                var nothing = new JavaScriptCore.Value(context);
                Assert.True(nothing.IsNull);

                var missing = JavaScriptCore.Value.NewUndefined(context);
                Assert.True(missing.IsUndefined);
            });
        }

        [SkippableFact]
        public void A_value_set_from_csharp_is_visible_to_script()
        {
            // The round trip that matters: a value crosses into the engine, gets
            // used in an expression the test wrote, and the answer crosses back.
            RequireJsc();

            Run(() =>
            {
                var context = new JavaScriptCore.Context();

                context.SetValue("fromCSharp", new JavaScriptCore.Value(context, 21.0));

                Assert.Equal(42, context.Evaluate("fromCSharp * 2").ToInt32());

                context.SetValue("greeting", JavaScriptCore.Value.NewString(context, "hello"));

                Assert.Equal("hello world", context.Evaluate("greeting + ' world'").ToString());
            });
        }

        [SkippableFact]
        public void A_global_defined_by_script_can_be_fetched_by_name()
        {
            RequireJsc();

            Run(() =>
            {
                var context = new JavaScriptCore.Context();
                context.Evaluate("var answer = 6 * 7;");

                var value = context.GetValue("answer");

                Assert.True(value.IsNumber);
                Assert.Equal(42, value.ToInt32());

                // ...and the same value hangs off the global object.
                Assert.Equal(42, context.GlobalObject.ObjectGetProperty("answer").ToInt32());
            });
        }

        [SkippableFact]
        public void An_array_of_strings_crosses_the_boundary_intact()
        {
            RequireJsc();

            Run(() =>
            {
                var context = new JavaScriptCore.Context();

                var array = new JavaScriptCore.Value(context, new[] { "alpha", "bravo", "charlie" });

                Assert.True(array.IsArray);
                Assert.Equal(3, array.ObjectGetProperty("length").ToInt32());
                Assert.Equal("bravo", array.ObjectGetPropertyAtIndex(1).ToString());

                context.SetValue("names", array);
                Assert.Equal("alpha,bravo,charlie", context.Evaluate("names.join(',')").ToString());
            });
        }

        [SkippableFact]
        public void Properties_set_from_csharp_are_read_by_script()
        {
            RequireJsc();

            Run(() =>
            {
                var context = new JavaScriptCore.Context();
                var value = context.Evaluate("({})");

                value.ObjectSetProperty("n", new JavaScriptCore.Value(context, 7.0));
                value.ObjectSetPropertyAtIndex(0, JavaScriptCore.Value.NewString(context, "indexed"));

                Assert.Equal(7, value.ObjectGetProperty("n").ToInt32());
                Assert.Equal("indexed", value.ObjectGetPropertyAtIndex(0).ToString());

                context.SetValue("built", value);
                Assert.Equal(14, context.Evaluate("built.n * 2").ToInt32());
            });
        }

        // -------------------------------------------------------- calling back

        [SkippableFact]
        public void A_javascript_function_can_be_called_from_csharp_with_no_arguments()
        {
            RequireJsc();

            Run(() =>
            {
                var context = new JavaScriptCore.Context();
                var function = context.Evaluate("(function () { return 'called'; })");

                Assert.True(function.IsFunction);

                var result = function.FunctionCall();

                Assert.Equal("called", result.ToString());
            });
        }

        [SkippableFact]
        public void A_javascript_function_can_be_called_from_csharp_with_arguments()
        {
            // jsc_value_function_callv takes (n_parameters, JSCValue **params).
            // The generated wrapper took a single Value and passed its handle
            // where the array was expected, so JSC read the Value's own GObject
            // header as params[0]. There was no way to call a function with
            // arguments at all.
            RequireJsc();

            Run(() =>
            {
                var context = new JavaScriptCore.Context();
                var add = context.Evaluate("(function (a, b) { return a + b; })");

                var result = add.FunctionCall(
                    new JavaScriptCore.Value(context, 20.0),
                    new JavaScriptCore.Value(context, 22.0));

                Assert.True(result.IsNumber);
                Assert.Equal(42, result.ToInt32());
            });
        }

        [SkippableFact]
        public void The_arguments_arrive_in_the_order_they_were_given()
        {
            // Addition would pass even if the array were reversed, so the oracle
            // has to be an operation that is not commutative.
            RequireJsc();

            Run(() =>
            {
                var context = new JavaScriptCore.Context();
                var join = context.Evaluate("(function (a, b, c) { return a + '-' + b + '-' + c; })");

                var result = join.FunctionCall(
                    JavaScriptCore.Value.NewString(context, "first"),
                    JavaScriptCore.Value.NewString(context, "second"),
                    JavaScriptCore.Value.NewString(context, "third"));

                Assert.Equal("first-second-third", result.ToString());
            });
        }

        [SkippableFact]
        public void A_method_on_an_object_can_be_invoked_with_arguments()
        {
            RequireJsc();

            Run(() =>
            {
                var context = new JavaScriptCore.Context();
                var value = context.Evaluate("({ base: 10, plus: function (n) { return this.base + n; } })");

                var result = value.ObjectInvokeMethod("plus", new JavaScriptCore.Value(context, 32.0));

                Assert.Equal(42, result.ToInt32());
            });
        }

        [SkippableFact]
        public void A_constructor_can_be_called_with_arguments()
        {
            RequireJsc();

            Run(() =>
            {
                var context = new JavaScriptCore.Context();
                var ctor = context.Evaluate("(function Point (x, y) { this.x = x; this.y = y; })");

                Assert.True(ctor.IsConstructor);

                var point = ctor.ConstructorCall(
                    new JavaScriptCore.Value(context, 3.0),
                    new JavaScriptCore.Value(context, 4.0));

                Assert.Equal(3, point.ObjectGetProperty("x").ToInt32());
                Assert.Equal(4, point.ObjectGetProperty("y").ToInt32());
            });
        }

        // ---------------------------------------------------------- exceptions

        [SkippableFact]
        public void A_script_that_throws_leaves_the_exception_on_the_context()
        {
            RequireJsc();

            Run(() =>
            {
                var context = new JavaScriptCore.Context();

                context.Evaluate("throw new Error('deliberate failure');");

                var exception = context.Exception;

                Assert.NotNull(exception);
                Assert.Equal("deliberate failure", exception.Message);
                Assert.Equal("Error", exception.Name);

                context.ClearException();
                Assert.Null(context.Exception);
            });
        }

        [SkippableFact]
        public void An_exception_thrown_from_csharp_reaches_script()
        {
            RequireJsc();

            Run(() =>
            {
                var context = new JavaScriptCore.Context();

                context.ThrowWithName("CustomError", "thrown from C#");

                var exception = context.Exception;

                Assert.NotNull(exception);
                Assert.Equal("CustomError", exception.Name);
                Assert.Equal("thrown from C#", exception.Message);
            });
        }

        [SkippableFact]
        public void A_pushed_handler_is_told_about_the_exception()
        {
            // The callback goes the other way across the boundary: JSC invokes a
            // managed delegate. Reading the exception inside it proves both
            // directions of that marshalling.
            RequireJsc();

            Run(() =>
            {
                var context = new JavaScriptCore.Context();
                string seen = null;

                context.PushExceptionHandler((ctx, exception) => seen = exception.Message);

                context.Evaluate("throw new Error('handled');");

                Assert.Equal("handled", seen);

                context.PopExceptionHandler();
            });
        }

        [SkippableFact]
        public void Working_script_leaves_no_exception_behind()
        {
            // The control: without it, "Exception is not null" could be reporting
            // a context that always has one.
            RequireJsc();

            Run(() =>
            {
                var context = new JavaScriptCore.Context();

                context.Evaluate("1 + 1");

                Assert.Null(context.Exception);
            });
        }

        // ------------------------------------------------- syntax and JSON

        [SkippableFact]
        public void Syntax_is_checked_without_running_anything()
        {
            RequireJsc();

            Run(() =>
            {
                var context = new JavaScriptCore.Context();

                var good = context.CheckSyntax("var x = 1;", JavaScriptCore.CheckSyntaxMode.Script,
                                               "test", 1, out var noError);
                Assert.Equal(JavaScriptCore.CheckSyntaxResult.Success, good);
                Assert.Null(noError);

                var bad = context.CheckSyntax("var = ;", JavaScriptCore.CheckSyntaxMode.Script,
                                              "test", 1, out var error);
                Assert.NotEqual(JavaScriptCore.CheckSyntaxResult.Success, bad);
                Assert.NotNull(error);

                // Checking is not running: the side effect must not have happened.
                context.CheckSyntax("globalThis.ranAnyway = true;",
                                    JavaScriptCore.CheckSyntaxMode.Script, "test", 1, out _);
                Assert.True(context.Evaluate("typeof globalThis.ranAnyway === 'undefined'").ToBoolean());
            });
        }

        [SkippableFact]
        public void A_value_round_trips_through_json()
        {
            // The test writes the JSON, the engine parses and re-serialises it,
            // and the test compares against what it wrote.
            RequireJsc();

            Run(() =>
            {
                var context = new JavaScriptCore.Context();

                var value = new JavaScriptCore.Value(context, "{\"n\":1,\"s\":\"two\"}");

                Assert.True(value.IsObject);
                Assert.Equal(1, value.ObjectGetProperty("n").ToInt32());
                Assert.Equal("two", value.ObjectGetProperty("s").ToString());

                Assert.Equal("{\"n\":1,\"s\":\"two\"}", value.ToJson(0));
            });
        }

        // ----------------------------------------------------- virtual machines

        [SkippableFact]
        public void Contexts_in_different_virtual_machines_do_not_share_globals()
        {
            // A VirtualMachine is the isolation boundary, and the only way to see
            // it is that a global set in one is invisible in the other.
            RequireJsc();

            Run(() =>
            {
                var first = new JavaScriptCore.Context(new JavaScriptCore.VirtualMachine());
                var second = new JavaScriptCore.Context(new JavaScriptCore.VirtualMachine());

                first.Evaluate("var shared = 'in the first';");

                Assert.Equal("in the first", first.Evaluate("shared").ToString());
                Assert.True(second.Evaluate("typeof shared === 'undefined'").ToBoolean());
            });
        }

        [SkippableFact]
        public void A_context_reports_the_virtual_machine_it_belongs_to()
        {
            RequireJsc();

            Run(() =>
            {
                var vm = new JavaScriptCore.VirtualMachine();
                var context = new JavaScriptCore.Context(vm);

                Assert.Same(vm, context.VirtualMachine);
            });
        }
    }
}
