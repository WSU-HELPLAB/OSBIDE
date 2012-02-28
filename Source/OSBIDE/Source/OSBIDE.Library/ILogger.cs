using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OSBIDE.Library
{
    public interface ILogger
    {
        /// <summary>
        /// Writes the supplied text to OSBIDE's log file.
        /// </summary>
        /// <param name="content"></param>
        void WriteToLog(string content, bool newLine = true);
    }
}
