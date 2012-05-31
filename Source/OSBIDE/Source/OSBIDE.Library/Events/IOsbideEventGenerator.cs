using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OSBIDE.Library.Events
{
    public interface IOsbideEventGenerator
    {
        event EventHandler<SubmitEventArgs> SolutionSubmitRequest;
        event EventHandler<SolutionDownloadedEventArgs> SolutionDownloaded;
    }
}
