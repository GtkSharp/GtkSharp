// This is free and unencumbered software released into the public domain.
// Happy coding!!! - GtkSharp Team

using Gtk;

namespace Samples
{
    [Section(Name = "Radio Button", Category = Category.Widgets)]
    class RadioButtonSection : ListSection
    {
        public RadioButtonSection()
        {
            AddItem(CreateRadioButton());
        }

        public (string, Widget) CreateRadioButton()
        {
            // create radio buttons
            var radio1 = new RadioButton("First Option");
            // notice that constructor takes the master radio
            // so a group can be defined
            var radio2 = new RadioButton(radio1,"Second Option");
            var radio3 = new RadioButton(radio1, "Third Option");

            // separated group
            var radio4 = new RadioButton("Yet another Option");
            var radio5 = new RadioButton("One more Option");
            var radio6 = new RadioButton("Another Option");

            // group can be defined like
            radio5.JoinGroup(radio4);
            radio6.JoinGroup(radio4);

            // set active ones
            radio1.Active = true;
            radio4.Active = true;

            // event example
            radio1.Toggled += OnRadioToggled;
            radio2.Toggled += OnRadioToggled;
            radio3.Toggled += OnRadioToggled;
            radio4.Toggled += OnRadioToggled;
            radio5.Toggled += OnRadioToggled;
            radio6.Toggled += OnRadioToggled;

            // add buttons to a box
            // so we can display this example
            var box = new Box(Orientation.Vertical, 6);

            box.Add(radio1);
            box.Add(radio2);
            box.Add(radio3);
            box.Add(radio4);
            box.Add(radio5);
            box.Add(radio6);

            return ("Radio button:", box);
        }

        void OnRadioToggled(object sender, System.EventArgs e)
        {
            var s = sender as RadioButton;
            ApplicationOutput.WriteLine(sender, $"{s.Label} is now {s.Active}");
        }
    }
}
