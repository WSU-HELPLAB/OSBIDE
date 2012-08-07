using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ActivityReader.Library
{
    public class StringConstants
    {
        public static string SqlCeConnectionString
        {
            get
            {
                return "Data Source=reader.sdf;Max Database Size=4091";
            }
        }
    }
}
