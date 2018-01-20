using System;

namespace Samples
{
    class SectionAttribute : Attribute
    {
        public string Name { get; set; }

        public Category Category { get; set; }
    }
}