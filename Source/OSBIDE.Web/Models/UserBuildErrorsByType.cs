using OSBIDE.Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OSBIDE.Web.Models
{
    public class UserBuildErrorsByType
    {
        public string ErrorName { get; set; }
        public List<int> UserIds { get; set; }

        public UserBuildErrorsByType()
        {
            UserIds = new List<int>();
        }
    }
}