using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OSBIDE.UnitTests.WebServices;
using System.Data.Entity;
using System.Data.SqlServerCe;
using OSBIDE.Library.Models;
using OSBIDE.Library;

namespace OSBIDE.UnitTests.Library.Model
{
    [TestClass()]
    public class OsbideContextTests
    {

        [TestMethod()]
        public void InsertUserWithIdTest()
        {
            //make sure that we start with a fresh DB
            Database.SetInitializer<OsbideContext>(new DropCreateDatabaseAlways<OsbideContext>());

            //then init the db
            SqlCeConnection conn = new SqlCeConnection(StringConstants.LocalDataConnectionString);
            OsbideContext localDb = new OsbideContext(conn, true);
            
            //calling any function will rebuild the DB
            List<EventLog> dummyList = localDb.EventLogs.ToList();
            OsbideUser testUser = new OsbideUser()
            {
                FirstName = "Joe",
                LastName = "User",
                Id = 10,
                OsbleId = 0,
                InstitutionId = "0"
            };
            if (localDb.Users.Where(u => u.Id == testUser.Id).Count() > 0)
            {
                localDb.Users.Remove(localDb.Users.Where(u => u.Id == testUser.Id).FirstOrDefault());
                localDb.SaveChanges();
            }
            bool result = localDb.InsertUserWithId(testUser);
            OsbideUser user = localDb.Users.FirstOrDefault();
            Assert.AreEqual(true, result);
            Assert.AreEqual(testUser.Id, user.Id);
        }

        [TestMethod()]
        public void InsertEventLogWithIdTest()
        {
            //make sure that we start with a fresh DB
            Database.SetInitializer<OsbideContext>(new DropCreateDatabaseAlways<OsbideContext>());

            //then init the db
            SqlCeConnection conn = new SqlCeConnection(StringConstants.LocalDataConnectionString);
            OsbideContext localDb = new OsbideContext(conn, true);

            //calling any function will rebuild the DB
            List<EventLog> dummyList = localDb.EventLogs.ToList();

            //create dummy user first
            InsertUserWithIdTest();

            //then create a dummy event log
            EventLog testLog = new EventLog()
            {
                Data = new byte[1],
                DateReceived = DateTime.Now,
                Id = 5,
                LogType = "test",
                SenderId = 10 //ID of the test user generated in InsertUserWithIdTest()
            };

            bool result = localDb.InsertEventLogWithId(testLog);
            EventLog log = localDb.EventLogs.FirstOrDefault();
            Assert.AreEqual(true, result);
            Assert.AreEqual(testLog.Id, log.Id);
            localDb.EventLogs.Remove(log);
            localDb.SaveChanges();
        }
    }
}
