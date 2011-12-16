using OSBIDE.Library.Events;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using OSBIDE.Library.Models;
using OSBIDE.Controls;

namespace OSBIDE.UnitTests.Controls
{
    /// <summary>
    ///This is a test class for OSBLE.Controls.AccountWindow and is intended
    ///to contain all AccountWindow Unit Tests
    ///</summary>
    [TestClass()]
    public class AccountWindowTest
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

        [TestMethod()]
        [DeploymentItem("OSBIDE.Controls.dll")]
        public void DataContextTest()
        {
            OsbideUser testUser = new OsbideUser()
            {
                FirstName = "Joe",
                LastName = "Smith",
                InstitutionId = "123"
            };
            AccountWindow window = new AccountWindow();
            window.ActiveUser = testUser;
            OsbideUser boundUser = window.DataContext as OsbideUser;
            Assert.IsNotNull(boundUser);
            Assert.AreEqual(testUser.FirstName, boundUser.FirstName);
        }
    }
}
