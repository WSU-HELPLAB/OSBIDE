using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace OSBIDE.Library
{
    public class LocalErrorLogger : ILogger
    {
        string filePath = "";

        public LocalErrorLogger()
        {
            filePath = StringConstants.LocalErrorLogPath;
        }

        public LocalErrorLogger(string filePath)
        {
            this.filePath = filePath;
        }

        public void WriteToLog(string content, bool newLine = true)
        {
            lock (this)
            {
                try
                {
                    using (StreamWriter writer = File.AppendText(filePath))
                    {
                        string text = string.Format("{0}:\t{1}", DateTime.Now.ToString("HH:mm:ss"), content);
                        if (newLine)
                        {
                            writer.WriteLine(text);
                        }
                        else
                        {
                            writer.Write(text);
                        }
                    }
                }
                catch (Exception ex)
                {
                    //ignore exception.
                }
            }
        }
    }
}
