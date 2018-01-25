// This is free and unencumbered software released into the public domain.
// Happy coding!!! - GtkSharp Team

using System;

namespace Samples
{
    class SectionAttribute : Attribute
    {
        public string Name { get; set; }

        public Category Category { get; set; }
    }
}