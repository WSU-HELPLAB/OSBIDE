using OSBIDE.Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OSBIDE.Web.Models.Queries
{
    /// <summary>
    /// Will retrieve a list of users to which the current user (observer) is subscribed.
    /// </summary>
    public class StudentSubscriptionsQuery
    {
        private OsbideContext _db;
        private OsbideUser _observer;

        public StudentSubscriptionsQuery(OsbideContext db, OsbideUser observer)
        {
            if (db == null || observer == null)
            {
                throw new Exception("Parameters cannot be null");
            }
            _db = db;
            _observer = observer;
        }

        public List<OsbideUser> Execute()
        {
            List<OsbideUser> subjects = new List<OsbideUser>();

            subjects = (from subscription in _db.UserSubscriptions
                        join user in _db.Users on
                                          new { InstitutionId = subscription.SubjectInstitutionId, SchoolId = subscription.SubjectSchoolId }
                                          equals new { InstitutionId = user.InstitutionId, SchoolId = user.SchoolId }
                        where subscription.ObserverSchoolId == _observer.SchoolId
                           && subscription.ObserverInstitutionId == _observer.InstitutionId
                        select user).ToList();
            return subjects;
        }
    }
}