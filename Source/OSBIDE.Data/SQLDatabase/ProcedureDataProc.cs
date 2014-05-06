using System;
using System.Collections.Generic;
using System.Linq;

using OSBIDE.Data.DomainObjects;
using OSBIDE.Data.SQLDatabase.Edmx;

namespace OSBIDE.Data.SQLDatabase
{
    public class ProcedureDataProc
    {
        public static List<StudentDemographicInfo> Get(Criteria criteria)
        {
            using (var context = new OsbideProcs())
            {
                var minDate = new DateTime(2000, 1,1);
                return (from p in context.GetProcedureData((!criteria.DateFrom.HasValue || criteria.DateFrom < minDate) ? minDate : criteria.DateFrom,
                            (!criteria.DateTo.HasValue || criteria.DateTo < minDate) ? DateTime.Today : criteria.DateTo,
                            criteria.NameToken == null ? string.Empty : criteria.NameToken,
                            criteria.GenderId,
                            criteria.AgeFrom,
                            criteria.AgeTo,
                            criteria.CourseId,
                            criteria.Deliverable,
                            criteria.GradeFrom.HasValue ? criteria.GradeFrom.Value : 0,
                            criteria.GradeTo.HasValue ? criteria.GradeTo.Value : 1000)
                        select new StudentDemographicInfo
                        {
                            Name = p.FirstName + " " + p.LastName,
                            Age = p.Age,
                            Class = p.Prefix + " " + p.CourseNumber + ", " + p.Season + " " + p.Year,
                            Gender = p.Gender,
                            Deliverable = p.Deliverable,
                            Ethnicity = p.Ethnicity,
                            Id = p.UserId,
                            InstitutionId = p.InstitutionId,
                            Grade = p.Grade,
                        }).ToList();
            }
        }
    }
}
