Main Program
------------

CodeGenerator: Static class. Contains Main. It uses the Parser to load the
               IGeneratable objects, validates them, and then calls the
               IGeneratable.Generate() method on each of them.

GenerationInfo: Stores info passed in on the command line, such as the
                assembly name and glue library name. Passed to
                IGeneratable.Generate().

Parser: Reads the foo-api.xml file and creates IGeneratable objects

Statistics: Static class. Used by other classes to keep statistics on
            generated classes

SymbolTable: Static class. Keeps track of the type hierarchy and the
             mappings between C types and IGeneratable classes


IGeneratables
-------------
The IGeneratable interface is implemented by all classes that
represent types.

GenBase: Abstract base class for any api.xml element that will have its own
         generated .cs file

    CallbackGen: Handles <callback> elements by creating a public delegate type
                 for the public API (in NAME.cs), and an internal type that
                 wraps that delegate, to be passed as the actual unmanaged
                 callback (in NAMESPACESharp.NAMENative.cs)

    ClassBase: Abstract base class for types that will be converted
               to C# classes, structs, or interfaces

        ClassGen: Handles <class> elements (static classes)

        HandleBase: base class for wrapped IntPtr reference types.

            OpaqueGen: Handles <boxed> and <struct> elements with the
                       "opaque" flag (by creating C# classes)

            ObjectBase: base class for GObject/GInterface types

                InterfaceGen: Handles <interface> elements

                ObjectGen: Handles <object> elements

        StructBase: Abstract base class for types that will be translated
                    to C# structs.

            BoxedGen: Handles non-opaque <boxed> elements

            StructGen: Handles non-opaque <struct> elements

    EnumGen: Handles <enum> elements.

SimpleBase: Abstract base class for types which aren't generated from
            xml like simple types or manually wrapped/implemented types.

    ByRefGen: Handles struct types that must be passed into C code by
              reference (at the moment, only GValue/GLib.Value)
    
    ManualGen: Handles types that must be manually marshalled between
               managed and unmanaged code (by handwritten classes such
               as GLib.List)
    
    MarshalGen: Handles types that must be manually marshalled between
                managed and unmanaged code via special CallByName/FromNative
                syntax (eg time_t<->DateTime, gunichar<->char)
    
    OwnableGen: Handles ownable types.

    SimpleGen: Handles types that can be simply converted from an
               unmanaged type to a managed type (int, byte, short, etc...)
	
    LPGen : marshals system specific long and "size" types.
	
    LPUGen : marshals system specific unsigned long and "size" types.
    
    ConstStringGen: Handles conversion between "const char *" and
                    System.String
    
        StringGen: Handles conversion between non-const "char *" and
                   System.String

    AliasGen: Handles <alias> elements. "Generates" type aliases by
          	  ignoring them (eg, outputting "Gdk.Rectangle" wherever the
          	  API calls for a GtkAllocation).


Other code-generating classes used by IGeneratables
---------------------------------------------------

ImportSignature: Represents a signature for an unmanaged method

MethodBase: Abstract base class for constructors, methods and virtual methods
    Ctor: Handles <constructor> elements
    Method: Handles <method> elements
    VirtualMethod: Abstract class for virtual methods
        GObjectVM: Handles virtual methods for objects
        InterfaceVM: Handles virtual methods for interfaces

MethodBody: Used by Method and its subclasses

PropertyBase: Abstract base class for property-like elements
    FieldBase: Abstract base class for field-like elements
        ObjectField: Handles <field> elements in objects
        StructField: Handles <field> elements in structs
            ClassField: Handles <field> elements in classes
    Property: Handles <property> elements
        ChildProperty: Handles <childprop> elements

Signal: Handles <signal> elements

ManagedCallString: Represents a call to a managed method from a method that
                   has unmanaged data

Parameter: Represents a single parameter to a method
    ErrorParameter: Represents an "out" parameter used to indicate an error
    StructParameter: Represents a parameter that is a struct
    ArrayParameter: Represents an parameter that is an array. Can be
                    null-terminated or not.
        ArrayCountPair: Represents an array parameter for which the number of
                        elements is given by another parameter.

Parameters: Represents the list of parameters to a method

ReturnValue: Represents the return value of a method, virtual method or signal

SignalHandler: Used by Signal. Like CallbackGen, this creates an internal type
               to wrap a signal-handler delegate.

Signature: Represents the signature of a method

VMSignature: Used by Signal. Represents the signature of a virtual method.
