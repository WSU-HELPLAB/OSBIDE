using System.Linq;
using System.Collections.Generic;

using OSBIDE.Data.DomainObjects;
using OSBIDE.Data.SQLDatabase.Edmx;

namespace OSBIDE.Data.SQLDatabase
{
    public class CriteriaLookupsProc
    {
        public static List<AgeLookup> GetAges()
        {
            using (var context = new OsbideProcs())
            {
                var ages = (from a in context.GetAgeLookup()
                            select new AgeLookup { Age = a.Age.Value, DisplayText = a.Age.Value.ToString() }).ToList();

                ages.Insert(0, new AgeLookup { Age = -1, DisplayText = "Any" });

                return ages;
            }
        }

        public static List<CourseLookup> GetCourses()
        {
            using (var context = new OsbideProcs())
            {
                var courses = (from c in context.GetCourseLookup()
                               select new CourseLookup { CourseId = c.CourseId, DisplayName = c.CourseName }).ToList();

                courses.Insert(0, new CourseLookup { CourseId = -1, DisplayName = "Any" });

                return courses;
            }
        }
        public static List<string> GetDeliverables()
        {
            using (var context = new OsbideProcs())
            {
                var dls = (from d in context.GetDeleverableLookup()
                        select d.Deliverable).ToList();
                
                dls.Insert(0, "Any");
                
                return dls;
            }
        }
        public static List<GenderLookup> GetGenders()
        {
            using (var context = new OsbideProcs())
            {
                return new List<GenderLookup>
                {
                    new GenderLookup{GenderId=(int)OSBIDE.Library.Models.Gender.Unknown, DisplayName="Any"},
                    new GenderLookup{GenderId=(int)OSBIDE.Library.Models.Gender.Female, DisplayName="Female"},
                    new GenderLookup{GenderId=(int)OSBIDE.Library.Models.Gender.Male, DisplayName="Male"},
                };
            }
        }
    }
}
