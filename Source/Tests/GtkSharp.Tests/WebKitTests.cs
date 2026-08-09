using System;
using System.Collections.Generic;
using Xunit;

namespace GtkSharp.Tests
{
    /// <summary>
    /// <c>WebKit.WebView</c> actually loading a document and running script in
    /// it, and <c>JavaScriptCore</c> reading the results back.
    /// </summary>
    /// <remarks>
    /// <c>WebkitGtkSharp</c> binds 178 types and the suite named three of them —
    /// and only to check that a WebView could be constructed. That is a low bar
    /// for the largest optional dependency in the tree.
    ///
    /// Everything here is <c>[SkippableFact]</c> on
    /// <see cref="TestEnvironment.WebKitUsable"/>, so it skips on Windows, where
    /// gvsbuild ships no WebKit, and in a container that cannot create the user
    /// namespace WebKit's sandbox needs. It runs on an ordinary Linux desktop.
    /// See <c>Docs/testing.md</c> for why the sandbox is left on.
    ///
    /// The loads are local: <c>LoadHtml</c> and <c>LoadPlainText</c> take content
    /// directly, so nothing here touches the network. A test that fetched a page
    /// would be testing the internet.
    ///
    /// The oracle is what the engine reports back about a document this file
    /// wrote — its title, its scripted result, its progress — never merely that a
    /// call returned.
    /// </remarks>
    public class WebKitTests : GtkTestBase
    {
        public WebKitTests(GtkFixture fixture) : base(fixture) { }

        static bool PumpUntil(Func<bool> condition, int timeoutMs = 15000)
        {
            var clock = System.Diagnostics.Stopwatch.StartNew();
            while (!condition() && clock.ElapsedMilliseconds < timeoutMs)
            {
                if (Gtk.Application.EventsPending())
                    Gtk.Application.RunIteration(false);
                else
                    System.Threading.Thread.Sleep(1);
            }

            return condition();
        }

        /// <summary>Loads <paramref name="html"/> and returns once the engine
        /// says it has finished, so later assertions read a settled document.</summary>
        static WebKit.WebView Loaded(string html)
        {
            var view = new WebKit.WebView();

            bool finished = false;
            view.LoadChanged += (o, args) =>
            {
                if (args.LoadEvent == WebKit.LoadEvent.Finished)
                    finished = true;
            };

            view.LoadHtml(html, null);

            Assert.True(PumpUntil(() => finished), "the document should finish loading");
            return view;
        }

        /// <summary>Evaluates <paramref name="script"/> in a loaded view and
        /// returns the JSC value it produced.</summary>
        static JavaScriptCore.Value Evaluate(WebKit.WebView view, string script)
        {
            JavaScriptCore.Value result = null;
            Exception failure = null;
            bool done = false;

            view.EvaluateJavascript(script, null, null, null, (o, res, data) =>
            {
                try
                {
                    result = view.EvaluateJavascriptFinish(res);
                }
                catch (Exception e)
                {
                    failure = e;
                }
                finally
                {
                    done = true;
                }
            });

            Assert.True(PumpUntil(() => done), "the script should finish evaluating");

            if (failure != null)
                throw failure;

            return result;
        }

        // ------------------------------------------------------------ loading

        [SkippableFact]
        public void A_loaded_document_reports_the_title_it_declared()
        {
            // The engine parsed the html and told the binding about it. Nothing
            // short of a real load produces a title.
            Skip.IfNot(TestEnvironment.WebKitUsable, TestEnvironment.WebKitSkipReason);

            Run(() =>
            {
                var view = Loaded("<!doctype html><title>a chosen title</title><p>body");

                Assert.True(PumpUntil(() => view.Title == "a chosen title"),
                            $"the title should arrive; it was '{view.Title}'");

                Assert.False(view.IsLoading, "and the view should no longer be loading");
                Assert.Equal(1.0, view.EstimatedLoadProgress, 3);
            });
        }

        [SkippableFact]
        public void The_load_signal_reports_its_stages_in_order_and_ends_at_finished()
        {
            // LoadChanged is how an application drives a progress bar, so the
            // ordering is the contract rather than an implementation detail.
            Skip.IfNot(TestEnvironment.WebKitUsable, TestEnvironment.WebKitSkipReason);

            Run(() =>
            {
                var view = new WebKit.WebView();
                var stages = new List<WebKit.LoadEvent>();

                view.LoadChanged += (o, args) => stages.Add(args.LoadEvent);

                view.LoadHtml("<!doctype html><title>stages</title>", null);

                Assert.True(PumpUntil(() => stages.Contains(WebKit.LoadEvent.Finished)),
                            "the load should reach Finished");

                Assert.Equal(WebKit.LoadEvent.Started, stages[0]);
                Assert.Equal(WebKit.LoadEvent.Finished, stages[stages.Count - 1]);
                Assert.Contains(WebKit.LoadEvent.Committed, stages);
            });
        }

        [SkippableFact]
        public void Plain_text_is_loaded_as_text_rather_than_parsed_as_markup()
        {
            // The distinction the two calls exist for: LoadPlainText must not
            // interpret the angle brackets, and the document body proves which
            // happened.
            Skip.IfNot(TestEnvironment.WebKitUsable, TestEnvironment.WebKitSkipReason);

            Run(() =>
            {
                var view = new WebKit.WebView();

                bool finished = false;
                view.LoadChanged += (o, args) =>
                {
                    if (args.LoadEvent == WebKit.LoadEvent.Finished)
                        finished = true;
                };

                view.LoadPlainText("<b>not bold</b>");

                Assert.True(PumpUntil(() => finished), "the text should finish loading");

                var text = Evaluate(view, "document.body.textContent");

                Assert.True(text.IsString);
                Assert.Contains("<b>not bold</b>", text.ToString());
            });
        }

        [SkippableFact]
        public void A_document_loaded_from_a_file_uri_is_read_off_disk()
        {
            // LoadUri against a file:// URI, so it is a real navigation without
            // being a network test.
            Skip.IfNot(TestEnvironment.WebKitUsable, TestEnvironment.WebKitSkipReason);

            Run(() =>
            {
                var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(),
                                                  "gtksharp-webkit-" + Guid.NewGuid().ToString("N") + ".html");
                try
                {
                    System.IO.File.WriteAllText(path,
                        "<!doctype html><meta charset=\"utf-8\"><title>from disk</title><p>hello");

                    var view = new WebKit.WebView();
                    bool finished = false;
                    view.LoadChanged += (o, args) =>
                    {
                        if (args.LoadEvent == WebKit.LoadEvent.Finished)
                            finished = true;
                    };

                    view.LoadUri(new Uri(path).AbsoluteUri);

                    Assert.True(PumpUntil(() => finished), "the file should finish loading");
                    Assert.True(PumpUntil(() => view.Title == "from disk"),
                                $"the title should come from the file; it was '{view.Title}'");
                    Assert.StartsWith("file://", view.Uri);
                }
                finally
                {
                    if (System.IO.File.Exists(path))
                        System.IO.File.Delete(path);
                }
            });
        }

        // --------------------------------------------------------- javascript

        [SkippableFact]
        public void A_script_result_comes_back_as_a_typed_JavaScriptCore_value()
        {
            // The whole reason JavaScriptCoreSharp is ordered before
            // WebkitGtkSharp in the build: the result of a script *is* a JSCValue,
            // and without that binding this call could only hand back a pointer.
            Skip.IfNot(TestEnvironment.WebKitUsable, TestEnvironment.WebKitSkipReason);

            Run(() =>
            {
                var view = Loaded("<!doctype html><title>js</title><p id='p'>text in the page");

                var number = Evaluate(view, "6 * 7");
                Assert.True(number.IsNumber);
                Assert.Equal(42, number.ToInt32());

                var text = Evaluate(view, "document.getElementById('p').textContent");
                Assert.True(text.IsString);
                Assert.Equal("text in the page", text.ToString());

                var flag = Evaluate(view, "1 < 2");
                Assert.True(flag.IsBoolean);
                Assert.True(flag.ToBoolean());
            });
        }

        [SkippableFact]
        public void Script_can_change_the_document_and_the_change_is_visible_afterwards()
        {
            // Two round trips: the first mutates the DOM, the second reads it
            // back. A call that silently did nothing would pass a single
            // evaluation and fail this.
            Skip.IfNot(TestEnvironment.WebKitUsable, TestEnvironment.WebKitSkipReason);

            Run(() =>
            {
                var view = Loaded("<!doctype html><title>mutable</title><p id='p'>before");

                Evaluate(view, "document.getElementById('p').textContent = 'after'");

                var text = Evaluate(view, "document.getElementById('p').textContent");

                Assert.Equal("after", text.ToString());
            });
        }

        [SkippableFact]
        public void A_script_that_throws_surfaces_as_an_exception_rather_than_a_null()
        {
            // The error path of EvaluateJavascriptFinish. Returning null instead
            // would make a broken script look like a script that returned
            // nothing.
            Skip.IfNot(TestEnvironment.WebKitUsable, TestEnvironment.WebKitSkipReason);

            Run(() =>
            {
                var view = Loaded("<!doctype html><title>throws</title>");

                Assert.ThrowsAny<GLib.GException>(
                    () => Evaluate(view, "throw new Error('deliberate');"));
            });
        }

        [SkippableFact]
        public void Undefined_and_null_are_distinguishable_from_each_other()
        {
            // JSCValue keeps the JavaScript distinction that C# does not have,
            // and collapsing the two would be a silent loss.
            Skip.IfNot(TestEnvironment.WebKitUsable, TestEnvironment.WebKitSkipReason);

            Run(() =>
            {
                var view = Loaded("<!doctype html><title>nullish</title>");

                var nothing = Evaluate(view, "null");
                var missing = Evaluate(view, "undefined");

                Assert.True(nothing.IsNull);
                Assert.False(nothing.IsUndefined);

                Assert.True(missing.IsUndefined);
                Assert.False(missing.IsNull);
            });
        }

        [SkippableFact]
        public void An_array_result_reports_itself_as_one_and_can_be_indexed()
        {
            Skip.IfNot(TestEnvironment.WebKitUsable, TestEnvironment.WebKitSkipReason);

            Run(() =>
            {
                var view = Loaded("<!doctype html><title>arrays</title>");

                var array = Evaluate(view, "['a', 'b', 'c']");

                Assert.True(array.IsArray);
                Assert.True(array.IsObject, "an array is an object in JavaScript");

                Assert.Equal("b", array.ObjectGetPropertyAtIndex(1).ToString());
                Assert.Equal(3, array.ObjectGetProperty("length").ToInt32());
            });
        }

        [SkippableFact]
        public void A_json_round_trip_survives_the_engine()
        {
            // The strongest available oracle for a value crossing the boundary:
            // the test writes the JSON, JavaScript parses and re-serialises it,
            // and the test compares against what it wrote.
            Skip.IfNot(TestEnvironment.WebKitUsable, TestEnvironment.WebKitSkipReason);

            Run(() =>
            {
                var view = Loaded("<!doctype html><title>json</title>");

                var value = Evaluate(view, "JSON.stringify(JSON.parse('{\"n\":1,\"s\":\"two\"}'))");

                Assert.True(value.IsString);
                Assert.Equal("{\"n\":1,\"s\":\"two\"}", value.ToString());
            });
        }

        // ----------------------------------------------------------- settings

        [SkippableFact]
        public void A_views_settings_keep_what_they_are_set_to()
        {
            Skip.IfNot(TestEnvironment.WebKitUsable, TestEnvironment.WebKitSkipReason);

            Run(() =>
            {
                var view = new WebKit.WebView();
                var settings = view.Settings;

                Assert.NotNull(settings);

                settings.UserAgent = "GtkSharp-test/1.0";
                settings.DefaultFontSize = 20;
                settings.AutoLoadImages = false;

                Assert.Equal("GtkSharp-test/1.0", settings.UserAgent);
                Assert.Equal(20u, settings.DefaultFontSize);
                Assert.False(settings.AutoLoadImages);
            });
        }

        [SkippableFact]
        public void The_user_agent_a_view_is_given_is_the_one_the_page_sees()
        {
            // The setting is only worth anything if it reaches the engine, and
            // the page reading navigator.userAgent is the far side of that.
            Skip.IfNot(TestEnvironment.WebKitUsable, TestEnvironment.WebKitSkipReason);

            Run(() =>
            {
                var view = new WebKit.WebView();
                view.Settings.UserAgent = "GtkSharp-test/2.0";

                bool finished = false;
                view.LoadChanged += (o, args) =>
                {
                    if (args.LoadEvent == WebKit.LoadEvent.Finished)
                        finished = true;
                };
                view.LoadHtml("<!doctype html><title>ua</title>", null);
                Assert.True(PumpUntil(() => finished), "the document should load");

                var agent = Evaluate(view, "navigator.userAgent");

                Assert.Equal("GtkSharp-test/2.0", agent.ToString());
            });
        }

        [SkippableFact]
        public void Turning_javascript_off_stops_a_script_running_in_the_page()
        {
            // The control that gives the settings tests meaning: a setting that
            // is stored but ignored would pass every round-trip assertion above.
            Skip.IfNot(TestEnvironment.WebKitUsable, TestEnvironment.WebKitSkipReason);

            Run(() =>
            {
                var view = new WebKit.WebView();
                view.Settings.EnableJavascript = false;

                bool finished = false;
                view.LoadChanged += (o, args) =>
                {
                    if (args.LoadEvent == WebKit.LoadEvent.Finished)
                        finished = true;
                };

                // The inline script would set the title if it ran.
                view.LoadHtml("<!doctype html><title>unchanged</title>" +
                              "<script>document.title = 'changed';</script>", null);

                Assert.True(PumpUntil(() => finished), "the document should load");

                Assert.True(PumpUntil(() => view.Title == "unchanged"),
                            $"the inline script should not have run; the title was '{view.Title}'");
            });
        }
    }
}
