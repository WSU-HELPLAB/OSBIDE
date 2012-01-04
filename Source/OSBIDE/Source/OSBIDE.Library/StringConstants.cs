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

        public static string UserDataPath
        {
            get
            {
                return Path.Combine(DataRoot, "UserData.dat");
            }
        }

        public static string LocalDatabasePath
        {
            get
            {
                return Path.Combine(DataRoot, "LocalDb.sdf");
            }
        }

        public static string LocalDataConnectionString
        {
            get
            {
                return string.Format("Data Source={0}", LocalDatabasePath);
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
                return "http://osble.org";
            }
        }
    }
}
