using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OSBIDE.UnitTests.WebServices;
using OSBIDE.Library;
using OSBIDE.Library.Models;
using OSBIDE.Library.Events;
using OSBIDE.Library.ServiceClient;

namespace OSBIDE.UnitTests.Web
{
    [TestClass()]
    public class RemoteOsbideWebServiceTest
    {
        private OsbideWebServiceClient serviceClient;

        public RemoteOsbideWebServiceTest()
        {
            //AC: We could just read the info in from the app.config file, but that isn't how we do it in the VS Package.
            serviceClient = new OsbideWebServiceClient(ServiceBindings.OsbideServiceBinding, ServiceBindings.RemoteOsbideServiceEndpoint);
        }

        [TestMethod()]
        public void EchoTest()
        {
            string test = "asdf";
            string result = serviceClient.Echo("asdf");
            Assert.AreEqual(test, result);
        }

        [TestMethod()]
        public void SaveUserTest()
        {
            OsbideUser test = new OsbideUser() { FirstName = "test", LastName = "test", InstitutionId = "000" };
            OsbideUser result = serviceClient.SaveUser(test);
            Assert.AreNotEqual(test.Id, result.Id);
            Assert.AreEqual(test.FirstName, result.FirstName);
            Assert.AreEqual(test.LastName, result.LastName);
            Assert.AreEqual(test.InstitutionId, result.InstitutionId);
        }

        [TestMethod()]
        public void SubmitLogTest()
        {
            OsbideUser sender = new OsbideUser() { FirstName = "test", LastName = "test", InstitutionId = "000" };
            BuildEvent buildEvent = new BuildEvent() { SolutionName = "foo" };
            EventLog log = new EventLog(buildEvent, sender);
            Enums.ServiceCode result = (Enums.ServiceCode)serviceClient.SubmitLog(log);
            Assert.AreEqual(Enums.ServiceCode.Ok, result);
        }
    }
}
