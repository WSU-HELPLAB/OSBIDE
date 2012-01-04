using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;

namespace OSBIDE.UnitTests.Library
{
    [TestClass()]
    public class StringConstantTests
    {
        [TestMethod()]
        public void VersionNumberTest()
        {
            string expected = "";
            string actual = OSBIDE.Library.StringConstants.LibraryVersion;

            Assembly asm = Assembly.GetAssembly(typeof(OSBIDE.Library.StringConstants));
            if (asm.FullName != null)
            {
                AssemblyName assemblyName = new AssemblyName(asm.FullName);
                expected = assemblyName.Version.ToString();
            }
            Assert.AreEqual(expected, actual);
        }
    }
}
