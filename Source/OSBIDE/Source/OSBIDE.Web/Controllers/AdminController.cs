﻿using OSBIDE.Library.CSV;
using OSBIDE.Library.Models;
using OSBIDE.Web.Models.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OSBIDE.Web.Controllers
{
    [AllowAccess(SystemRole.Instructor)]
    public class AdminController : ControllerBase
    {
        //
        // GET: /Admin/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult DailyActivity(int? userId, DateTime? day)
        {
            if (userId != null && day != null)
            {
            }
            List<OsbideUser> students = Db.Users.Where(u => u.Role == SystemRole.Student).ToList();
            
            return View();
        }

        [HttpPost]
        public ActionResult UploadRoster(HttpPostedFileBase file)
        {
            ViewBag.UploadResult = true;
            List<List<string>> csvList = new List<List<string>>();
            try
            {
                using (CsvReader reader = new CsvReader(file.InputStream))
                {
                    csvList = reader.Parse();
                }
            }
            catch (Exception)
            {
                ViewBag.UploadResult = false;
                return View("Index");
            }

            //Process of creating automatic user subscriptions:
            //1. Group all users by section
            //2. For each student in a given section, assign them to all other students in that section
            Dictionary<int, List<int>> sectionGroups = new Dictionary<int, List<int>>();

            //this loop takes care of process step #1
            foreach (List<string> row in csvList)
            {
                if (row.Count == 2)
                {
                    int studentId = 0;
                    int section = 0;
                    if (Int32.TryParse(row[0], out studentId) == true)
                    {
                        if (Int32.TryParse(row[1], out section) == true)
                        {
                            if (sectionGroups.ContainsKey(section) == false)
                            {
                                sectionGroups.Add(section, new List<int>());
                            }
                            if (sectionGroups[section].Contains(studentId) == false)
                            {
                                sectionGroups[section].Add(studentId);
                            }
                        }
                    }
                }
            }

            //this loop takes care of process step #2
            foreach (int section in sectionGroups.Keys)
            {
                List<int> sectionList = sectionGroups[section];
                List<UserSubscription> dbSubs = (from sub in Db.UserSubscriptions
                                                where 
                                                (
                                                    sectionList.Contains(sub.ObserverInstitutionId)
                                                    || sectionList.Contains(sub.SubjectInstitutionId)
                                                )
                                                && sub.ObserverSchoolId == CurrentUser.SchoolId
                                                select sub
                                                ).ToList();
                                                
                foreach (int observerId in sectionGroups[section])
                {
                    foreach (int subjectId in sectionGroups[section])
                    {
                        if (observerId.CompareTo(subjectId) != 0)
                        {
                            UserSubscription dbSub = dbSubs.Where(s => s.ObserverInstitutionId == observerId).Where(s => s.SubjectInstitutionId == subjectId).FirstOrDefault();
                            if (dbSub == null)
                            {
                                UserSubscription sub = new UserSubscription()
                                {
                                    ObserverInstitutionId = observerId,
                                    SubjectInstitutionId = subjectId,

                                    //assume that the subject and observer attend the same institution as the 
                                    //person uploading the CSV file
                                    ObserverSchoolId = CurrentUser.SchoolId,
                                    SubjectSchoolId = CurrentUser.SchoolId
                                };
                                Db.UserSubscriptions.Add(sub);
                            }
                        }
                    }
                }
            }

            //save any DB changes
            try
            {
                Db.SaveChanges();
            }
            catch (Exception)
            {
                ViewBag.UploadResult = false;
            }

            return View("Index");
        }
    }
}
