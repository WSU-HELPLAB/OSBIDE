using OSBIDE.Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OSBIDE.Web.Models.ViewModels
{
    public class DailyActivityViewModel
    {
        public List<OsbideUser> Students { get; set; }
        public DateTime SelectedDate { get; set; }
        public int SelectedStudentId { get; set; }

        public DailyActivityViewModel()
        {
            Students = new List<OsbideUser>();
        }
    }
}