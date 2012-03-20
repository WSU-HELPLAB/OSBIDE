using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OSBIDE.VSPackage;
using OSBIDE.Library.Events;
using OSBIDE.Library;
using OSBIDE.Library.Models;
using System.Threading;

namespace OSBIDE.UnitTests.VSPackage
{
    [TestClass()]
    public class ServiceClientTest
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
        ///A test for OsbideEventCreated.  This is a load test.
        ///</summary>
        [TestMethod()]
        [DeploymentItem("OSBIDE.VSPackage.dll")]
        public void OsbideEventCreatedTest()
        {
            ILogger logger = new LocalErrorLogger();
            OsbideUser dummyUser = new OsbideUser() { FirstName = "foo", LastName = "bar", InstitutionId = "000" };
            ServiceClient_Accessor client = new ServiceClient_Accessor(null, dummyUser, logger);

            try
            {
                //fire off 10 events
                for (int i = 0; i < 10; i++)
                {
                    BuildEvent build = new BuildEvent();
                    build.SolutionName = i.ToString();
                    client.OsbideEventCreated(this, new EventCreatedArgs(build));
                }

                //if we got here, then we must be okay
                Assert.AreEqual(true, true);
            }
            catch (Exception ex)
            {
                //oops, something went wrong!
                Assert.AreEqual(true, false);
            }
        }
    }
}
