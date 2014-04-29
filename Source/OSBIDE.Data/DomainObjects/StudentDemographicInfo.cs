
namespace OSBIDE.Data.DomainObjects
{
    public class StudentDemographicInfo
    {
        public int Id { get; set; }
        public int InstitutionId { get; set; }
        public string Name { get; set; }
        public string Gender { get; set; }
        public int? Age { get; set; }
        public string Class { get; set; }
        public int Year { get; set; }
        public string Quarter { get; set; }
        public decimal Grade { get; set; }
        public decimal OverallGrade { get; set; }
        public string Ethnicity { get; set; }
    }
}
