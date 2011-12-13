using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OSBIDE.Library.Events
{
    public interface IOsbideEvent
    {
        DateTime EventDate { get; set; }
        string SolutionName { get; set; }
        string EventName { get; }
    }
}
