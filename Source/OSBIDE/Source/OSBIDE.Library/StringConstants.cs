﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

namespace OSBIDE.Library
{

    public static class StringConstants
    {
        public static string DataRoot
        {
            get
            {
                string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "OSBIDE");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                return path;
            }
        }

        public static string UserNameCacheKey
        {
            get
            {
                return "UserName";
            }
        }

        public static string PasswordCacheKey
        {
            get
            {
                return "Password";
            }
        }

        public static string AuthenticationCacheKey
        {
            get
            {
                return "AuthKey";
            }
        }

        public static string LocalCacheDirectory
        {
            get
            {
                string path = Path.Combine(DataRoot, "cache");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                return path;
            }
        }

        public static string ActivityFeedUrl
        {
            get
            {
#if DEBUG
                return "http://localhost:24867/Feed";
#else
                return "http://osbide.com/Feed";
#endif
            }
        }

        public static string CreateAccountUrl
        {
            get
            {
#if DEBUG
                return "http://localhost:24867/Account/Create";
#else
                return "http://osbide.com/Account/Create";
#endif
            }
        }

        public static string AskTheProfessorUrl
        {
            get
            {
#if DEBUG
                return "http://localhost:24867/PrivateQuestion";
#else
                return "http://osbide.com/PrivateQuestion";
#endif
            }
        }

        public static string ChatUrl
        {
            get
            {
#if DEBUG
                return "http://localhost:24867/Chat";
#else
                return "http://osbide.com/Chat";
#endif
            }
        }

        public static string ProfileUrl
        {
            get
            {
#if DEBUG
                return "http://localhost:24867/Profile";
#else
                return "http://osbide.com/Profile";
#endif
            }
        }

        public static string LocalDatabasePath
        {
            get
            {
#if DEBUG
                return Path.Combine(DataRoot, "LocalDb_debug.sdf");
#else
                return Path.Combine(DataRoot, "LocalDb.sdf");
#endif
            }
        }

        public static string LocalDataConnectionString
        {
            get
            {
                return string.Format("Data Source={0};Max Database Size=4091", LocalDatabasePath);
            }
        }

        public static string LocalErrorLogExtension
        {
            get
            {
                return ".log";
            }
        }

        public static string LocalErrorLogFileName
        {
            get
            {
                return DateTime.Today.ToString("yyyy-MM-dd");
            }
        }

        public static string LocalErrorLogPath
        {
            get
            {
                return Path.Combine(DataRoot, LocalErrorLogFileName + LocalErrorLogExtension);
            }
        }

        public static string LibraryVersion
        {
            get
            {
                string versionNumber = "";
                Assembly asm = Assembly.GetAssembly(typeof(StringConstants));
                if (asm.FullName != null)
                {
                    AssemblyName assemblyName = new AssemblyName(asm.FullName);
                    versionNumber = assemblyName.Version.ToString();
                }
                return versionNumber;

            }
        }

        public static string OsbidePackageUrl
        {
            get
            {
                return "http://osbide.codeplex.com/releases";
            }
        }
    }
}
