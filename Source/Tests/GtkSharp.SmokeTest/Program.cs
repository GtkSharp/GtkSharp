using Gtk;

Gtk.Application.Init();

var window = new Window("GtkSharp ARM64 smoke test");
var button = Button.NewWithLabel("Click me");
var clicked = false;

button.Clicked += (_, _) => clicked = true;
window.Add(button);
button.Show();
window.Show();

// Emite la señal "clicked" de GTK de forma síncrona.
button.Click();

if (!clicked)
{
	Console.Error.WriteLine("FAIL: Clicked event did not fire");
	return 1;
}

Console.WriteLine(
	$"OK: GtkSharp click event fired on {System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture}"
);
return 0;
