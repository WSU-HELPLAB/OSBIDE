using OSBIDE.Library.Events;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace OSBIDE.UnitTests.Library.Events
{
    
    
    /// <summary>
    ///This is a test class for EventFactoryTest and is intended
    ///to contain all EventFactoryTest Unit Tests
    ///</summary>
    [TestClass()]
    public class EventFactoryTest
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
        ///Serializes and then deserializes and IOsbideEvent object in order to make sure that data is being
        ///preserved through the serialization process
        ///</summary>
        [TestMethod()]
        [DeploymentItem("OSBIDE.Library.dll")]
        public void BinaryConversionTest()
        {
            BuildEvent build = new BuildEvent();

            //AC: needs to be rewritten
            Assert.AreEqual(true, false);
            /*
            build.ErrorItems.Add(new OSBIDE.Library.Models.ErrorListItem() { Description = "test" });
            build.EventDate = DateTime.Now;
            build.SolutionName = "test";

            //convert to binary and then reconvert
            byte[] binaryForm = EventFactory.ToZippedBinary(build);
            BuildEvent convertedEvent = (BuildEvent)EventFactory.FromZippedBinary(binaryForm);

            //after going through the conversion, ensure that the build items remain the same
            Assert.AreEqual(build.SolutionName, convertedEvent.SolutionName);
            Assert.AreEqual(build.ErrorItems.Count, convertedEvent.ErrorItems.Count);
             * */
        }
    }
}
