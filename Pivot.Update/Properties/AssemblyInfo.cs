using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Pivot.Update")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Redpoint Software")]
[assembly: AssemblyProduct("Pivot.Update")]
[assembly: AssemblyCopyright("Copyright © Redpoint Software 2012")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("6d8d73e0-096b-4da0-b9ae-d708b4ef3105")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]

// Permit other Pivot.Update components to access internal functionality.
[assembly: InternalsVisibleTo("Pivot.Update.Server")]
[assembly: InternalsVisibleTo("Pivot.Update.Service")]
[assembly: InternalsVisibleTo("Pivot.Update.Tests")]
[assembly: InternalsVisibleTo("pvclnt")]
[assembly: InternalsVisibleTo("pvctrl")]
