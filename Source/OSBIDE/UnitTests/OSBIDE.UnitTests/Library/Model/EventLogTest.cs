using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OSBIDE.Library.Models;
using OSBIDE.Library.Events;

namespace OSBIDE.UnitTests.Library.Model
{
    [TestClass()]
    public class EventLogTest
    {
        [TestMethod()]
        public void EventLogConstructorTest()
        {
            IOsbideEvent evt = new BuildEvent();
            OsbideUser user = new OsbideUser() { Id = 5, FirstName = "bob", LastName = "smith", InstitutionId = "0" };
            EventLog log = new EventLog(evt, user);
            IOsbideEvent convertedEvent = EventFactory.FromZippedBinary(log.Data);
            Assert.AreEqual(evt.EventName, evt.EventName);
            Assert.AreEqual(user.Id, log.SenderId);
        }
    }
}
