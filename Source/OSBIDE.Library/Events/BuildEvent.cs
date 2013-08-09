using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OSBIDE.Library.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using EnvDTE;
using System.Text.RegularExpressions;

namespace OSBIDE.Library.Events
{
    [Serializable]
    public class BuildEvent : IOsbideEvent
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [Required]
        public int EventLogId { get; set; }

        [ForeignKey("EventLogId")]
        public virtual EventLog EventLog { get; set; }

        [Required]
        public DateTime EventDate { get; set; }

        [Required(AllowEmptyStrings = true)]
        public string SolutionName { get; set; }

        [Required]
        public virtual IList<BuildEventErrorListItem> ErrorItems { get; set; }

        [Required]
        public virtual IList<BuildEventBreakPoint> Breakpoints { get; set; }

        [Required]
        public string EventName { get { return BuildEvent.Name; } }

        [NotMapped]
        public static string Name { get { return "BuildEvent"; } }

        [NotMapped]
        public string PrettyName { get { return "Build"; } }

        public virtual List<BuildDocument> Documents { get; set; }

        IOsbideEvent IOsbideEvent.FromDict(Dictionary<string, object> values)
        {
            BuildEvent evt = new BuildEvent();
            if (values.ContainsKey("Id"))
            {
                evt.Id = (int)values["Id"];
            }
            if (values.ContainsKey("EventLogId"))
            {
                evt.EventLogId = (int)values["EventLogId"];
            }
            if (values.ContainsKey("EventLog"))
            {
                evt.EventLog = (EventLog)values["EventLog"];
            }
            if (values.ContainsKey("EventDate"))
            {
                evt.EventDate = (DateTime)values["EventDate"];
            }
            if (values.ContainsKey("SolutionName"))
            {
                evt.SolutionName = values["SolutionName"].ToString();
            }
            if (values.ContainsKey("ErrorItems"))
            {
                evt.ErrorItems = values["ErrorItems"] as List<BuildEventErrorListItem>;
            }
            if (values.ContainsKey("Breakpoints"))
            {
                evt.Breakpoints = values["Breakpoints"] as List<BuildEventBreakPoint>;
            }
            if (values.ContainsKey("Documents"))
            {
                evt.Documents = values["LineNumber"] as List<BuildDocument>;
            }
            return evt;
        }

        /// <summary>
        /// Returns the number of critical errors (those that start with "error") that the build contains.
        /// </summary>
        [NotMapped]
        public int CriticalErrorCount
        {
            get
            {
                int counter = 0;
                foreach (BuildEventErrorListItem errorItem in ErrorItems)
                {
                    //ignore non-errors
                    if (errorItem.ErrorListItem.Description.ToLower().StartsWith("error") == true)
                    {
                        counter++;
                    }
                }
                return counter;
            }
        }

        [NotMapped]
        public List<string> CriticalErrorNames
        {
            get
            {
                List<string> errors = new List<string>(ErrorItems.Count);
                string pattern = "error ([^:]+)";
                Dictionary<string, ErrorType> errorTypes = new Dictionary<string, ErrorType>();
                foreach (BuildEventErrorListItem item in ErrorItems)
                {
                    Match match = Regex.Match(item.ErrorListItem.Description, pattern);

                    //ignore bad matches
                    if (match.Groups.Count == 2)
                    {
                        string errorCode = match.Groups[1].Value.ToLower().Trim();
                        errors.Add(errorCode);
                    }
                }
                return errors;
            }
        }

        [NotMapped]
        public List<BuildEventErrorListItem> CriticalErrorItems
        {
            get
            {
                List<BuildEventErrorListItem> items = new List<BuildEventErrorListItem>();
                foreach (BuildEventErrorListItem errorItem in ErrorItems)
                {
                    //ignore non-errors
                    if (errorItem.ErrorListItem.Description.StartsWith("error") == true)
                    {
                        items.Add(errorItem);
                    }
                }
                return items;
            }
        } 

        public List<CodeDocument> GetSolutionFiles(Solution solution)
        {
            List<CodeDocument> files = new List<CodeDocument>();
            foreach (Project project in solution.Projects)
            {
                files = files.Union(GetProjectFiles(project)).ToList();
            }
            return files;
        }

        public List<CodeDocument> GetProjectFiles(Project project)
        {
            List<CodeDocument> files = GetProjectItemFiles(project.ProjectItems);
            return files;
        }

        //AC Note: since we're currently using C/C++, just keep those files
        private string[] allowedExtensions = { ".c", ".cpp", ".h" };
        private List<CodeDocument> GetProjectItemFiles(ProjectItems items)
        {
            List<CodeDocument> files = new List<CodeDocument>();
            foreach (ProjectItem item in items)
            {
                if (item.SubProject != null)
                {
                    files = files.Union(GetProjectItemFiles(item.ProjectItems)).ToList();
                }
                else if (item.ProjectItems != null && item.ProjectItems.Count > 0)
                {
                    files = files.Union(GetProjectItemFiles(item.ProjectItems)).ToList();
                }
                else
                {
                    string fileName = item.Name;
                    string extension = Path.GetExtension(fileName);
                    if (allowedExtensions.Contains(extension) == true)
                    {
                        if (item.Document != null)
                        {
                            files.Add((CodeDocument)DocumentFactory.FromDteDocument(item.Document));
                        }
                    }
                }
            }
            return files;
        }

        public BuildEvent()
        {
            ErrorItems = new List<BuildEventErrorListItem>();
            Breakpoints = new List<BuildEventBreakPoint>();
            EventDate = DateTime.UtcNow;
            Documents = new List<BuildDocument>();
        }

    }
}
