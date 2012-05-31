using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EnvDTE80;
using OSBIDE.Library.Models;
using Ionic.Zip;
using System.IO;
using System.ComponentModel.DataAnnotations;

namespace OSBIDE.Library.Events
{
    [Serializable]
    public class SubmitEvent : IOsbideEvent
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EventLogId { get; set; }

        [ForeignKey("EventLogId")]
        public virtual EventLog EventLog { get; set; }

        [Required]
        public DateTime EventDate { get; set; }

        [Required]
        public string SolutionName { get; set; }

        [Required]
        public string EventName { get { return SubmitEvent.Name; } }

        [NotMapped]
        public static string Name { get { return "SubmitEvent"; } }

        [Required]
        public string AssignmentName { get; set; }

        [Required]
        [Column(TypeName = "image")]
        public byte[] SolutionData { get; private set; }

        public SubmitEvent()
        {
            EventDate = DateTime.Now;
            SolutionData = new byte[0];
        }

        public SubmitEvent(string solutionPath)
            : this()
        {
            SolutionName = solutionPath;
            MemoryStream stream = new MemoryStream();
            using (ZipFile zip = new ZipFile())
            {
                string rootPath = Path.GetDirectoryName(solutionPath);
                zip.AddDirectory(rootPath);
                zip.Save(stream);
                stream.Position = 0;
            }
            SolutionData = stream.ToArray();
        }

        public SubmitEvent(DTE2 dte) : this(dte.Solution.FullName)
        {
        }
    }
}
