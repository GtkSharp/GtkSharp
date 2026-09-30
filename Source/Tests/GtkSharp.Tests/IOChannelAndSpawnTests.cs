using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Xunit;

namespace GtkSharp.Tests
{
    /// <summary>
    /// <c>GLib.IOChannel</c> (286 lines) and <c>GLib.Spawn</c> (168) were the two
    /// largest hand-written files in GLibSharp with nothing at all reaching them.
    /// Both talk to the operating system, so both have oracles that do not depend
    /// on the library: a file this test wrote, and a child process whose output
    /// this test chose.
    /// </summary>
    public class IOChannelAndSpawnTests : GtkTestBase
    {
        public IOChannelAndSpawnTests(GtkFixture fixture) : base(fixture) { }

        private static void WithTempFile(string contents, Action<string> body)
        {
            var path = Path.Combine(Path.GetTempPath(),
                                    "gtksharp-iochannel-" + Guid.NewGuid().ToString("N") + ".txt");
            try
            {
                if (contents != null)
                    File.WriteAllText(path, contents, new UTF8Encoding(false));
                body(path);
            }
            finally
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
        }

        // ---------------------------------------------------------- IOChannel

        [Fact]
        public void An_io_channel_reads_a_file_a_line_at_a_time()
        {
            Run(() => WithTempFile("first\nsecond\nthird\n", path =>
            {
                using var channel = new GLib.IOChannel(path, "r");

                Assert.Equal(GLib.IOStatus.Normal, channel.ReadLine(out var first));
                Assert.Equal(GLib.IOStatus.Normal, channel.ReadLine(out var second));

                // The line terminator is part of what ReadLine returns.
                Assert.Equal("first\n", first);
                Assert.Equal("second\n", second);
            }));
        }

        [Fact]
        public void Reading_past_the_last_line_reports_end_of_file()
        {
            Run(() => WithTempFile("only\n", path =>
            {
                using var channel = new GLib.IOChannel(path, "r");

                Assert.Equal(GLib.IOStatus.Normal, channel.ReadLine(out _));
                Assert.Equal(GLib.IOStatus.Eof, channel.ReadLine(out var past));
                Assert.Null(past);
            }));
        }

        [Fact]
        public void ReadLine_reports_where_the_terminator_was()
        {
            Run(() => WithTempFile("abcdef\n", path =>
            {
                using var channel = new GLib.IOChannel(path, "r");

                channel.ReadLine(out var line, out var terminator);

                Assert.Equal("abcdef\n", line);
                Assert.Equal(6ul, terminator);   // the index of the '\n'
            }));
        }

        [Fact]
        public void ReadToEnd_returns_the_whole_file()
        {
            Run(() => WithTempFile("all\nof\nit\n", path =>
            {
                using var channel = new GLib.IOChannel(path, "r");

                Assert.Equal(GLib.IOStatus.Normal, channel.ReadToEnd(out var text));
                Assert.Equal("all\nof\nit\n", text);
            }));
        }

        [Fact]
        public void ReadChars_fills_the_buffer_it_is_given()
        {
            Run(() => WithTempFile("0123456789", path =>
            {
                using var channel = new GLib.IOChannel(path, "r");

                var buffer = new byte[4];
                Assert.Equal(GLib.IOStatus.Normal, channel.ReadChars(buffer, out var read));

                Assert.Equal(4ul, read);
                Assert.Equal("0123", Encoding.UTF8.GetString(buffer));
            }));
        }

        [Fact]
        public void Seeking_rewinds_and_the_same_bytes_come_back()
        {
            Run(() => WithTempFile("repeatable\n", path =>
            {
                using var channel = new GLib.IOChannel(path, "r");

                channel.ReadLine(out var first);

                Assert.Equal(GLib.IOStatus.Normal, channel.SeekPosition(0, GLib.SeekType.Set));

                channel.ReadLine(out var second);

                Assert.Equal(first, second);
            }));
        }

        [Fact]
        public void Reading_a_unichar_decodes_a_multi_byte_character()
        {
            // The file holds 'é' as two UTF-8 bytes; ReadUnichar must hand back
            // one code point rather than a byte.
            Run(() => WithTempFile("é!", path =>
            {
                using var channel = new GLib.IOChannel(path, "r");

                Assert.Equal(GLib.IOStatus.Normal, channel.ReadUnichar(out var first));
                Assert.Equal((uint) 'é', first);

                Assert.Equal(GLib.IOStatus.Normal, channel.ReadUnichar(out var second));
                Assert.Equal((uint) '!', second);
            }));
        }

        [Fact]
        public void Writing_through_a_channel_puts_the_text_in_the_file()
        {
            Run(() => WithTempFile(null, path =>
            {
                using (var channel = new GLib.IOChannel(path, "w"))
                {
                    Assert.Equal(GLib.IOStatus.Normal, channel.WriteChars("written\n", out var remainder));
                    Assert.True(string.IsNullOrEmpty(remainder),
                                $"the whole string should have been written, {remainder} was left");
                    channel.Flush();
                }

                Assert.Equal("written\n", File.ReadAllText(path));
            }));
        }

        [Fact]
        public void Writing_bytes_reports_how_many_went_out()
        {
            Run(() => WithTempFile(null, path =>
            {
                var payload = Encoding.UTF8.GetBytes("bytes");

                using (var channel = new GLib.IOChannel(path, "w"))
                {
                    Assert.Equal(GLib.IOStatus.Normal, channel.WriteChars(payload, out var written));
                    Assert.Equal((ulong) payload.Length, written);
                    channel.Flush();
                }

                Assert.Equal("bytes", File.ReadAllText(path));
            }));
        }

        [Fact]
        public void A_channel_reports_back_the_buffering_settings_it_was_given()
        {
            Run(() => WithTempFile("x", path =>
            {
                using var channel = new GLib.IOChannel(path, "r");

                Assert.True(channel.Buffered);

                channel.BufferSize = 8192;
                Assert.Equal(8192ul, channel.BufferSize);

                // Buffering can only be turned off on a channel with no
                // encoding: with one set, GLib logs a critical and leaves the
                // channel buffered rather than failing the call.
                channel.Buffered = false;
                Assert.True(channel.Buffered);

                channel.Encoding = null;
                channel.Buffered = false;

                Assert.False(channel.Buffered);
            }));
        }

        [Fact]
        public void Setting_the_encoding_to_null_puts_the_channel_in_binary_mode()
        {
            Run(() => WithTempFile("text", path =>
            {
                using var channel = new GLib.IOChannel(path, "r");

                Assert.Equal("UTF-8", channel.Encoding);

                channel.Encoding = null;

                Assert.Null(channel.Encoding);
            }));
        }

        [Fact]
        public void The_line_terminator_can_be_changed_and_is_reported_back()
        {
            Run(() => WithTempFile("a|b|c", path =>
            {
                using var channel = new GLib.IOChannel(path, "r");

                channel.LineTerminator = new[] { '|' };

                Assert.Equal(new[] { '|' }, channel.LineTerminator);

                channel.ReadLine(out var first);
                Assert.Equal("a|", first);
            }));
        }

        [Fact]
        public void Shutting_a_channel_down_flushes_what_was_written()
        {
            Run(() => WithTempFile(null, path =>
            {
                var channel = new GLib.IOChannel(path, "w");
                channel.WriteChars("flushed on shutdown", out _);

                Assert.Equal(GLib.IOStatus.Normal, channel.Shutdown(true));

                Assert.Equal("flushed on shutdown", File.ReadAllText(path));
            }));
        }

        [Fact]
        public void Opening_a_file_that_is_not_there_raises_rather_than_returning_a_dead_channel()
        {
            Run(() =>
            {
                var missing = Path.Combine(Path.GetTempPath(),
                                           "gtksharp-absent-" + Guid.NewGuid().ToString("N"));

                Assert.ThrowsAny<GLib.GException>(() => new GLib.IOChannel(missing, "r"));
            });
        }

        // -------------------------------------------------------------- Spawn

        /// <summary>A child process that prints <paramref name="text"/> and exits
        /// 0, expressed for whichever shell this platform has.</summary>
        private static string[] EchoArgv(string text)
            => RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? new[] { "cmd.exe", "/c", "echo " + text }
                : new[] { "/bin/sh", "-c", "echo " + text };

        private static string[] FailingArgv(int code)
            => RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? new[] { "cmd.exe", "/c", "exit " + code }
                : new[] { "/bin/sh", "-c", "exit " + code };

        [Fact]
        public void Spawning_a_child_synchronously_captures_what_it_printed()
        {
            Run(() =>
            {
                Assert.True(GLib.Process.SpawnSync(null, EchoArgv("hello from the child"), null,
                                                   GLib.SpawnFlags.SearchPath, null,
                                                   out var stdout, out var stderr, out var status));

                Assert.Contains("hello from the child", stdout);
                Assert.Equal(0, status);
                Assert.True(string.IsNullOrWhiteSpace(stderr),
                            $"nothing should have gone to stderr, got {stderr}");
            });
        }

        [Fact]
        public void A_child_that_exits_non_zero_reports_a_non_zero_status()
        {
            // The spawn itself succeeds -- the return value says whether the
            // child could be started, not whether it liked its work. Conflating
            // the two is the usual mistake with this API.
            Run(() =>
            {
                Assert.True(GLib.Process.SpawnSync(null, FailingArgv(3), null,
                                                   GLib.SpawnFlags.SearchPath, null,
                                                   out _, out _, out var status));

                Assert.NotEqual(0, status);
            });
        }

        [Fact]
        public void Spawning_something_that_does_not_exist_raises()
        {
            Run(() =>
            {
                Assert.ThrowsAny<GLib.GException>(
                    () => GLib.Process.SpawnSync(null, new[] { "gtksharp-no-such-program" }, null,
                                                 GLib.SpawnFlags.SearchPath, null,
                                                 out _, out _, out _));
            });
        }

        [Fact]
        public void A_child_runs_in_the_working_directory_it_was_given()
        {
            Run(() =>
            {
                var directory = Path.Combine(Path.GetTempPath(),
                                             "gtksharp-cwd-" + Guid.NewGuid().ToString("N"));
                Directory.CreateDirectory(directory);
                try
                {
                    var argv = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                        ? new[] { "cmd.exe", "/c", "cd" }
                        : new[] { "/bin/sh", "-c", "pwd" };

                    Assert.True(GLib.Process.SpawnSync(directory, argv, null,
                                                       GLib.SpawnFlags.SearchPath, null,
                                                       out var stdout, out _, out _));

                    // macOS and some Linux setups report /tmp through a symlink,
                    // so compare the leaf rather than the whole path.
                    Assert.Contains(Path.GetFileName(directory), stdout);
                }
                finally
                {
                    Directory.Delete(directory, true);
                }
            });
        }

        [Fact]
        public void A_child_sees_the_environment_it_was_handed()
        {
            Run(() =>
            {
                var argv = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? new[] { "cmd.exe", "/c", "echo %GTKSHARP_TEST%" }
                    : new[] { "/bin/sh", "-c", "echo $GTKSHARP_TEST" };

                var envp = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? new[] { "GTKSHARP_TEST=carried", "SystemRoot=" + Environment.GetEnvironmentVariable("SystemRoot"), "PATH=" + Environment.GetEnvironmentVariable("PATH") }
                    : new[] { "GTKSHARP_TEST=carried", "PATH=/usr/bin:/bin" };

                Assert.True(GLib.Process.SpawnSync(null, argv, envp,
                                                   GLib.SpawnFlags.SearchPath, null,
                                                   out var stdout, out _, out _));

                Assert.Contains("carried", stdout);
            });
        }

        [Fact]
        public void A_command_line_is_split_and_run()
        {
            Run(() =>
            {
                var command = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? "cmd.exe /c echo command-line"
                    : "/bin/echo command-line";

                Assert.True(GLib.Process.SpawnCommandLineSync(command,
                                                              out var stdout, out _, out var status));

                Assert.Contains("command-line", stdout);
                Assert.Equal(0, status);
            });
        }

        [Fact]
        public void An_asynchronously_spawned_child_hands_back_a_process_to_wait_on()
        {
            Run(() =>
            {
                Assert.True(GLib.Process.SpawnAsync(null, EchoArgv("async"), null,
                                                    GLib.SpawnFlags.SearchPath
                                                    | GLib.SpawnFlags.DoNotReapChild,
                                                    null, out var child));

                try
                {
                    // SpawnAsync exists to hand back a handle on the child, so
                    // the process it names has to be identifiable.
                    Assert.True(child.Pid > 0, $"expected a real pid, got {child.Pid}");
                }
                finally
                {
                    child.Close();
                }
            });
        }
    }
}
