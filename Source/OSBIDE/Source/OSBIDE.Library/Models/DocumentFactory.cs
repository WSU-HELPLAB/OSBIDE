﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EnvDTE;
using System.IO;
using EnvDTE80;

namespace OSBIDE.Library.Models
{
    public class DocumentFactory
    {
        //kind of silly right now, but it does set us up nicely for future expansion
        public static IVSDocument FromDteDocument(Document document)
        {
            CodeDocument codeDocument = new CodeDocument();
            DTE2 dte = (DTE2)document.DTE;
            codeDocument.FileName = document.Name;
            codeDocument.Content = File.ReadAllText(document.FullName);

            //start at 1 when iterating through Error List
            for (int i = 1; i <= dte.ToolWindows.ErrorList.ErrorItems.Count; i++)
            {
                ErrorItem item = dte.ToolWindows.ErrorList.ErrorItems.Item(i);
                
                //only grab events that are related to the current file
                if (item.FileName.CompareTo(document.FullName) == 0)
                {
                    codeDocument.ErrorItems.Add(ErrorListItem.FromErrorItem(item));
                }
            }

            //add in breakpoint information
            for (int i = 1; i <= dte.Debugger.Breakpoints.Count; i++)
            {
                BreakPoint bp = new BreakPoint(dte.Debugger.Breakpoints.Item(i));

                //agan, only grab breakpoints set in the current document
                if (bp.File.CompareTo(document.FullName) == 0)
                {
                    codeDocument.BreakPoints.Add(bp);
                }
            }
            return codeDocument;
        }
    }
}
