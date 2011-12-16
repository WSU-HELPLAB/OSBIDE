using OSBIDE.VSPackage;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using OSBIDE.Library.Models;
using OSBIDE.Library.Events;

namespace OSBIDE.VSPackage.UnitTests
{
    
    
    /// <summary>
    ///This is a test class for OSBIDEPackageTest and is intended
    ///to contain all OSBIDEPackageTest Unit Tests
    ///</summary>
    [TestClass()]
    public class OSBIDEPackageTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion

        /// <summary>
        ///A test for GetSavedUserData
        ///</summary>
        [TestMethod()]
        [DeploymentItem("OSBIDE.VSPackage.dll")]
        public void UserDataFileTest()
        {
            OSBIDEPackage_Accessor package = new OSBIDEPackage_Accessor();
            OsbideUser actualUser = new OsbideUser();
            OsbideUser expectedUser = new OsbideUser()
            {
                FirstName = "Joe",
                LastName = "Smith",
                InstitutionId = "123",
                Id = 1
            };
            package.SaveUserData(expectedUser);
            actualUser = package.GetSavedUserData();
            Assert.AreEqual(expectedUser.FirstName, actualUser.FirstName);
            Assert.AreEqual(expectedUser.LastName, actualUser.LastName);
            Assert.AreEqual(expectedUser.Id, actualUser.Id);
            Assert.AreEqual(expectedUser.InstitutionId, actualUser.InstitutionId);
        }
    }
}
