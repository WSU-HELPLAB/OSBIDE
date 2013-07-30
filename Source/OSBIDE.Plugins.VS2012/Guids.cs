// Guids.cs
// MUST match guids.h
using System;

namespace OSBIDE.Plugins.VS2012
{
    static class GuidList
    {
        public const string guidOSBIDE_Plugins_VS2012PkgString = "d36f74b0-6e55-4872-8696-0ca1d5a2203e";
        public const string guidOSBIDE_Plugins_VS2012CmdSetString = "8917860c-7c5d-4f90-a9dc-db2fb42ddf67";
        public const string guidToolWindowPersistanceString = "e592919c-94f6-427b-bd12-6cb46440bc3a";

        public static readonly Guid guidOSBIDE_Plugins_VS2012CmdSet = new Guid(guidOSBIDE_Plugins_VS2012CmdSetString);
    };
}