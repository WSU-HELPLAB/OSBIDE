using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OSBIDE.Web.Models.Queries
{
    public interface IOsbideQuery<T>
    {
        IQueryable<T> Execute();
    }
}