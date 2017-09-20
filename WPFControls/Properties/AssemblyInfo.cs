















#region Using directives

using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows;
using System.Windows.Markup;

#endregion

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Inflectra WPF Controls")]
[assembly: AssemblyDescription("WPF Controls for VS")]

[assembly: AssemblyCompany("Inflectra")]
[assembly: AssemblyProduct("Inflectra SpiraTeam Explorer for Visual Studio 2012")]
[assembly: AssemblyCopyright("© 2013 Inflectra")]
[assembly: AssemblyCulture("")]


// Needed to enable xbap scenarios
[assembly: AllowPartiallyTrustedCallers]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]
[assembly: CLSCompliant(true)]

//In order to begin building localizable applications, set 
//<UICulture>CultureYouAreCodingWith</UICulture> in your .csproj file
//inside a <PropertyGroup>.  For example, if you are using US english
//in your source files, set the <UICulture> to en-US.  Then uncomment
//the NeutralResourceLanguage attribute below.  Update the "en-US" in
//the line below to match the UICulture setting in the project file.

//[assembly: NeutralResourcesLanguage("en-US", UltimateResourceFallbackLocation.Satellite)]


[assembly: ThemeInfo(
	ResourceDictionaryLocation.SourceAssembly, //where theme specific resource dictionaries are located
	//(used if a resource is not found in the page, 
	// or application resource dictionaries)
	ResourceDictionaryLocation.SourceAssembly //where the generic resource dictionary is located
	//(used if a resource is not found in the page, 
	// app, or any theme specific resource dictionaries)
)]

//[assembly: XmlnsPrefix("http://schemas.xceed.com/wpf/xaml/toolkit", "xctk")]
//[assembly: XmlnsDefinition("http://schemas.xceed.com/wpf/xaml/toolkit", "Xceed.Wpf.Toolkit")]
//[assembly: XmlnsDefinition("http://schemas.xceed.com/wpf/xaml/toolkit", "Xceed.Wpf.Toolkit.Core.Converters")]
//[assembly: XmlnsDefinition("http://schemas.xceed.com/wpf/xaml/toolkit", "Xceed.Wpf.Toolkit.Core.Input")]
//[assembly: XmlnsDefinition("http://schemas.xceed.com/wpf/xaml/toolkit", "Xceed.Wpf.Toolkit.Core.Utilities")]
//[assembly: XmlnsDefinition("http://schemas.xceed.com/wpf/xaml/toolkit", "Xceed.Wpf.Toolkit.Chromes")]
//[assembly: XmlnsDefinition("http://schemas.xceed.com/wpf/xaml/toolkit", "Xceed.Wpf.Toolkit.Primitives")]
//[assembly: XmlnsDefinition("http://schemas.xceed.com/wpf/xaml/toolkit", "Xceed.Wpf.Toolkit.PropertyGrid")]
//[assembly: XmlnsDefinition("http://schemas.xceed.com/wpf/xaml/toolkit", "Xceed.Wpf.Toolkit.PropertyGrid.Attributes")]
//[assembly: XmlnsDefinition("http://schemas.xceed.com/wpf/xaml/toolkit", "Xceed.Wpf.Toolkit.PropertyGrid.Commands")]
//[assembly: XmlnsDefinition("http://schemas.xceed.com/wpf/xaml/toolkit", "Xceed.Wpf.Toolkit.PropertyGrid.Converters")]
//[assembly: XmlnsDefinition("http://schemas.xceed.com/wpf/xaml/toolkit", "Xceed.Wpf.Toolkit.PropertyGrid.Editors")]
//[assembly: XmlnsDefinition("http://schemas.xceed.com/wpf/xaml/toolkit", "Xceed.Wpf.Toolkit.Zoombox")]
//[assembly: XmlnsDefinition("http://schemas.xceed.com/wpf/xaml/toolkit", "Xceed.Wpf.Toolkit.Panels")]


//#pragma warning disable 1699
//[assembly: AssemblyDelaySign(false)]
//[assembly: AssemblyKeyFile(@"sn.snk")]
//[assembly: AssemblyKeyName("")]
//#pragma warning restore 1699


