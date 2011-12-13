// Guids.cs
// MUST match guids.h
using System;

namespace OSBIDE.VSPackage
{
    public static class GuidList
    {
        public const string guidOSBIDE_VSPackagePkgString = "34968e68-73e9-4448-830f-5a445a3ff22d";
        public const string guidOSBIDE_VSPackageCmdSetString = "7d8e3fcc-b13c-4efa-9adf-5c9bd9401169";
        public const string guidToolWindowPersistanceString = "eee1c7ba-00ea-4b22-88d7-6cb17837c3d3";

        public static readonly Guid guidOSBIDE_VSPackageCmdSet = new Guid(guidOSBIDE_VSPackageCmdSetString);
    };
}