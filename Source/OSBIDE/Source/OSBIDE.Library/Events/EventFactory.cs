using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Ionic.Zip;
using EnvDTE;
using EnvDTE80;

namespace OSBIDE.Library.Events
{
    public enum DebugActions { Start, StepOver, StepInto, StepOut, StopDebugging };
    public class EventFactory
    {
        private static List<string> debugCommands = (new string[] { "Debug.Start", "Debug.StepOver", "Debug.StepInto", "Debug.StepOut", "Debug.StopDebugging" }).ToList();

        public static IOsbideEvent FromCommand(string commandName, DTE2 dte)
        {
            IOsbideEvent oEvent = null;

            //debugging events
            if (debugCommands.Contains(commandName))
            {
                DebugActions action = (DebugActions)debugCommands.IndexOf(commandName);
                DebugEvent debug = new DebugEvent();
                debug.SolutionName = dte.Solution.FullName;
                debug.EventDate = DateTime.Now;

                //we don't really use this, so set to -1 so that it stands out
                debug.EventReason = -1;

                //kind of reappropriating this for our current use.  Consider refactoring.
                debug.ExecutionAction = (int)action;
                debug.DocumentName = dte.ActiveDocument.FullName;

                //throw the content of the output window into the event if we just stopped debugging
                if (action == DebugActions.StopDebugging)
                {
                    OutputWindowPane debugWindow = dte.ToolWindows.OutputWindow.OutputWindowPanes.Item("Debug");
                    if (debugWindow != null)
                    {
                        TextDocument text = debugWindow.TextDocument;
                        TextSelection selection = text.Selection;
                        selection.StartOfDocument();
                        selection.EndOfDocument(true);
                        debug.DebugOutput = selection.Text;
                        selection.EndOfDocument();
                    }
                }

                oEvent = debug;
            }

            return oEvent;
        }

        /// <summary>
        /// Converts a zipped, binary format of IOsbideEvent back into object form
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static IOsbideEvent FromZippedBinary(byte[] data)
        {
            MemoryStream zippedStream = new MemoryStream(data);
            MemoryStream rawStream = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();
            string FileName = "";

            //unzip the memory stream
            using (ZipFile zip = ZipFile.Read(zippedStream))
            {
                if (zip.Entries.Count == 1)
                {
                    ZipEntry entry = zip.Entries.ElementAt(0);
                    FileName = entry.FileName;
                    entry.Extract(rawStream);
                    rawStream.Position = 0;
                }
                else
                {
                    throw new Exception("Expecting a zip file with exactly one item.");
                }
            }

            //figure out what needs to be deserialized
            IOsbideEvent evt = (IOsbideEvent)formatter.Deserialize(rawStream);            
            return evt;
        }

        /// <summary>
        /// Converts the supplied IOsbideEvent into a zipped, binary format
        /// </summary>
        /// <param name="evt"></param>
        /// <returns></returns>
        public static byte[] ToZippedBinary(IOsbideEvent evt)
        {
            MemoryStream memStream = new MemoryStream();
            MemoryStream zipStream = new MemoryStream();
            BinaryFormatter serializer = new BinaryFormatter();
            serializer.Serialize(memStream, evt);

            //go back to position zero so that the zip file can read the memory stream
            memStream.Position = 0;

            //zip up to save space
            using (ZipFile zip = new ZipFile())
            {
                ZipEntry entry = zip.AddEntry(evt.EventName, memStream);
                zip.Save(zipStream);
                zipStream.Position = 0;
            }
            return zipStream.ToArray();
        }
    }
}
