﻿using OSBIDE.Web;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting.Web;
using OSBIDE.Library.Models;
using OSBIDE.Library.Events;
using System.Linq;
using OSBIDE.Library;
using System.Collections.Generic;
using System.Data.SqlServerCe;
using System.Data.Entity;
using System.Data.Objects;

namespace OSBIDE.UnitTests.Web
{
    
    
    /// <summary>
    ///This is a test class for OsbideWebServiceTest and is intended
    ///to contain all OsbideWebServiceTest Unit Tests
    ///</summary>
    [TestClass()]
    public class LocalOsbideWebServiceTest
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

        [TestMethod()]
        public void SaveUserTest()
        {
            OsbideWebService target = new OsbideWebService(); 
            OsbideUser testUser = new OsbideUser() { FirstName = "Test", LastName = "User", InstitutionId = "123" };
            OsbideUser result = target.SaveUser(testUser);
            Assert.AreNotEqual(0, result.Id);
        }

        [TestMethod()]
        public void LibraryVersionNumberTest()
        {
            OsbideWebService target = new OsbideWebService();
            Assert.AreEqual(OSBIDE.Library.StringConstants.LibraryVersion, target.LibraryVersionNumber());
        }

        [TestMethod]
        public void GetBinaryLogTest()
        {
            //setup values
            OsbideWebService target = new OsbideWebService();
            List<EventLog> logs = target.GetPastEvents(DateTime.MinValue, true);
            foreach (EventLog log in logs)
            {
                IOsbideEvent evt = EventFactory.FromZippedBinary(log.Data);
            }
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
            Enums.ServiceCode actual;
            OsbideUser testUser = new OsbideUser() { FirstName = "Test", LastName = "User", InstitutionId = "123" };

            //create a new BuildEvent to save
            BuildEvent build = new BuildEvent()
            {
                SolutionName = "foo"
            };
            LogType = build.EventName;

            //send it off to the service for saving
            EventLog logToSend = new EventLog(build, testUser);
            actual = (Enums.ServiceCode)target.SubmitLog(logToSend);

            //we should get an OK result back from the server
            Assert.AreEqual(Enums.ServiceCode.Ok, actual);

            //pull the last record from the DB
            OsbideContext db = target.Db;
            EventLog log = db.EventLogs.OrderByDescending(l => l.Id).FirstOrDefault();
            
            //make sure that the log matches what we originally inserted
            Assert.AreEqual(LogType, log.LogType);
 
            //assume byte arrays of the same length are the same.  Kind of
            //meh but it's probably okay
            Assert.AreEqual(logToSend.Data.Length, log.Data.Length);
        }

        [TestMethod()]
        public void GetPastEventsTest()
        {
            OsbideWebService target = new OsbideWebService();

            //AC note: This goes to the SQL Server Express database named "OsbideDebugContext" and does not use the connection string by that name.
            //This is how the OsbideWebService does things in testing (kind of odd).
            OsbideContext db = new OsbideContext("OsbideDebugContext");
            DateTime start = DateTime.Now.Subtract(new TimeSpan(360, 0, 0, 0, 0));
            List<EventLog> localQueryLogs = db.EventLogs.Where(log => log.DateReceived > start).ToList();
            List<EventLog> serviceQuery = target.GetPastEvents(start, false);

            Assert.AreEqual(serviceQuery.Count, localQueryLogs.Count);
        }
    }
}
