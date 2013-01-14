// Guids.cs
// MUST match guids.h
using System;

namespace WashingtonStateUniversity.OSBIDE_VS2012Package
{
    static class GuidList
    {
        public const string guidOSBIDE_VS2012PackagePkgString = "04d2100c-6adc-4ff3-a2d2-45b81fed4f27";
        public const string guidOSBIDE_VS2012PackageCmdSetString = "fb3151bc-d1a0-4e9b-8d61-70c69fc4ad46";
        public const string guidToolWindowPersistanceString = "eee1c7ba-00ea-4b22-88d7-6cb17837c3d3";
        public static readonly Guid guidOSBIDE_VS2012PackageCmdSet = new Guid(guidOSBIDE_VS2012PackageCmdSetString);
    };
}