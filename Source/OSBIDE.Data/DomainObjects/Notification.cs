
namespace OSBIDE.Data.DomainObjects
{
    public class Notification
    {
        public int UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int EventLogId { get; set; }
        public bool Viewed { get; set; }
    }
}
