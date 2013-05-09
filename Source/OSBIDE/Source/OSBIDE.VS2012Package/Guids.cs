// Guids.cs
// MUST match guids.h
using System;

namespace OSBIDE_VS2012Package
{
    static class GuidList
    {
        public const string guidOSBIDE_VS2012PackagePkgString = "04d2100c-6adc-4ff3-a2d2-45b81fed4f27";
        public const string guidOSBIDE_VS2012PackageCmdSetString = "fb3151bc-d1a0-4e9b-8d61-70c69fc4ad46";
        
        public const string guidToolWindowPersistanceString = "eee1c7ba-00ea-4b22-88d7-6cb17837c3d3";
        public const string guidOsbideActivityFeedTool = "eee1c7ba-00ea-4b22-88d7-6cb17837c3d4";
        public const string guidOsbideActivityFeedDetailsTool = "eee1c7ba-00ea-4b22-88d7-6cb17837c3d5";
        public const string guidOsbideChatTool = "eee1c7ba-00ea-4b22-88d7-6cb17837c3d6";
        public const string guidOsbideUserProfileTool = "eee1c7ba-00ea-4b22-88d7-6cb17837c3d7";
        public const string guidOSBIDE_ContextMenuCmdSetString = "eee1c7ba-00ea-4b22-88d7-6cb17837c3d8";
        public const string guidOsbideCreateAccountTool = "eee1c7ba-00ea-4b22-88d7-6cb17837c3d9";
        public const string guidOsbideAskTheProfessorTool = "eee1c7ba-00ea-4b22-88d7-6cb17837c3da";

        public static readonly Guid guidOSBIDE_VS2012PackageCmdSet = new Guid(guidOSBIDE_VS2012PackageCmdSetString);
        public static readonly Guid guidOSBIDE_ContextMenuCmdSet = new Guid(guidOSBIDE_ContextMenuCmdSetString);
    };
}