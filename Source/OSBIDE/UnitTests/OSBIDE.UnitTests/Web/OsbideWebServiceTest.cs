﻿using OSBIDE.Web;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting.Web;
using OSBIDE.Library.Models;
using OSBIDE.Library.Events;
using System.Linq;

namespace OSBIDE.UnitTests.Web
{
    
    
    /// <summary>
    ///This is a test class for OsbideWebServiceTest and is intended
    ///to contain all OsbideWebServiceTest Unit Tests
    ///</summary>
    [TestClass()]
    public class OsbideWebServiceTest
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

        [TestMethod()]
        public void EchoTest()
        {
            string send = "asdf";
            OsbideWebService target = new OsbideWebService();
            Assert.AreEqual(send, target.Echo(send));
        }

        /// <summary>
        ///A test for SubmitLog
        ///</summary>
        [TestMethod()]
        public void SubmitLogTest()
        {
            //setup values
            OsbideWebService target = new OsbideWebService(); 
            string LogType = string.Empty; 
            byte[] data = null; 
            Enums.ServiceCode expected = new Enums.ServiceCode();
            Enums.ServiceCode actual;

            //create a new BuildEvent to save
            BuildEvent build = new BuildEvent()
            {
                SolutionName = "foo"
            };
            data = EventFactory.ToZippedBinary(build);
            LogType = build.EventName;

            //send it off to the service for saving
            actual = (Enums.ServiceCode)target.SubmitLog(LogType, data);

            //we should get an OK result back from the server
            Assert.AreEqual(expected, actual);

            //pull the last record from the DB
            OsbideContext db = new OsbideContext();
            EventLog log = db.EventLogs.OrderByDescending(l => l.Id).FirstOrDefault();
            
            //make sure that the log matches what we originally inserted
            Assert.AreEqual(LogType, log.LogType);
 
            //assume byte arrays of the same length are the same.  Kind of
            //meh but it's probably okay
            Assert.AreEqual(data.Length, log.Data.Length);
        }
    }
}
