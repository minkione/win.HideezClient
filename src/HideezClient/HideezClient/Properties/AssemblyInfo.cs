﻿using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("HideezClient")]
[assembly: AssemblyDescription("Hideez Client")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Hideez Group Inc.")]
[assembly: AssemblyProduct("Hideez Client")]
[assembly: AssemblyCopyright("© 2017-2019 Hideez Group Inc. All rights reserved.")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Required to enable automated testings
[assembly: InternalsVisibleTo("HideezClient.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

//In order to begin building localizable applications, set
//<UICulture>CultureYouAreCodingWith</UICulture> in your .csproj file
//inside a <PropertyGroup>.  For example, if you are using US english
//in your source files, set the <UICulture> to en-US.  Then uncomment
//the NeutralResourceLanguage attribute below.  Update the "en-US" in
//the line below to match the UICulture setting in the project file.

//[assembly: NeutralResourcesLanguage("en-US", UltimateResourceFallbackLocation.Satellite)]


[assembly: ThemeInfo(
    ResourceDictionaryLocation.None, //where theme specific resource dictionaries are located
                                     //(used if a resource is not found in the page,
                                     // or application resource dictionaries)
    ResourceDictionaryLocation.SourceAssembly //where the generic resource dictionary is located
                                              //(used if a resource is not found in the page,
                                              // app, or any theme specific resource dictionaries)
)]

// Obfuscation rules
[assembly: Obfuscation(Exclude = false, Feature = "namespace('HideezClient.Views'):-rename")]
[assembly: Obfuscation(Exclude = false, Feature = "namespace('HideezClient.ViewModels'):-rename")]

[assembly: Obfuscation(Exclude = false, Feature = "namespace('HideezClient.PagesView'):-rename")]
[assembly: Obfuscation(Exclude = false, Feature = "namespace('HideezClient.PageViewModels'):-rename")]

[assembly: Obfuscation(Exclude = false, Feature = "namespace('HideezClient.Models'):-rename")]
[assembly: Obfuscation(Exclude = false, Feature = "namespace('HideezClient.Messages'):-rename")]

[assembly: Obfuscation(Exclude = false, Feature = "namespace('HideezClient.Controls'):-rename")]
[assembly: Obfuscation(Exclude = false, Feature = "namespace('HideezClient.Resources'):-rename")]
