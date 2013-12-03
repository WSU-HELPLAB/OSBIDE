﻿using OSBIDE.Library.Models;
using OSBIDE.Web.Models.Attributes;
using OSBIDE.Web.Models.FileSystem;
using OSBIDE.Web.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OSBIDE.Web.Controllers
{
    [OsbideAuthorize]
    public class CourseController : ControllerBase
    {
        //
        // GET: /Courses/

        public ActionResult Index()
        {
            return RedirectToAction("MyCourses");
        }

        public ActionResult MyCourses()
        {
            return View();
        }

        [HttpPost]
        public ActionResult CreateAssignment(Assignment vm)
        {
            //make sure that assignment creation is allowed
            CourseUserRelationship relationship = Db.CourseUserRelationships
                                                    .Where(cr => cr.CourseId == vm.CourseId)
                                                    .Where(cr => cr.UserId == CurrentUser.Id)
                                                    .FirstOrDefault();
            if (relationship != null)
            {
                if (relationship.Role == CourseRole.Coordinator)
                {
                    vm.IsDeleted = false;

                    //AC note: I'm not using ModelState.IsValid because it's flagging the non-mapped ReleaseTime/DueTime as invalid. 
                    //As such, there's potential for the db insert to go bad.  Thus, the try/catch.
                    try
                    {
                        Db.Assignments.Add(vm);
                        Db.SaveChanges();
                    }
                    catch (Exception)
                    {

                    }
                }
            }
            return RedirectToAction("Details", new { id = vm.CourseId });
        }

        public ActionResult DeleteAssignment(int id)
        {
            Assignment some_assignment = Db.Assignments.Where(a => a.Id == id).FirstOrDefault();
            if (some_assignment != null)
            {
                //make sure the current user is a course coordinator
                CourseUserRelationship relationship = some_assignment.Course.CourseUserRelationships.Where(c => c.UserId == CurrentUser.Id).FirstOrDefault();
                if (relationship != null)
                {
                    if (relationship.Role == CourseRole.Coordinator)
                    {
                        some_assignment.IsDeleted = true;
                        Db.SaveChanges();
                        return RedirectToAction("Details", new { id = some_assignment.CourseId });
                    }
                }
            }
            return RedirectToAction("MyCourses");
        }

        public ActionResult Details(int id = -1)
        {
            Course currentCourse = (from course in Db.Courses
                                   .Include("Assignments")
                                   .Include("CourseUserRelationships")
                                   .Include("CourseUserRelationships.User")
                                    where course.Id == id
                                    select course).FirstOrDefault();

            //bad ID or invalid course, redirect
            if (id == -1 || currentCourse == null)
            {
                return RedirectToAction("Index", "Feed");
            }

            //build VM
            CourseDetailsViewModel vm = new CourseDetailsViewModel();
            vm.CurrentCourse = currentCourse;
            vm.Assignments = currentCourse.Assignments.Where(a => a.IsDeleted == false).ToList();
            vm.CurrentUser = CurrentUser;
            vm.Coordinators = currentCourse.CourseUserRelationships.Where(c => c.Role == CourseRole.Coordinator).Select(u => u.User).ToList();

            //figure out what files are attached to various assignments
            FileSystem fs = new FileSystem();
            foreach (Assignment assignment in currentCourse.Assignments)
            {
                vm.AssignmentFiles.Add(assignment.Id, new List<string>());
                FileCollection files = fs.Course(currentCourse).Assignment(assignment).AllFiles();
                foreach (string file in files)
                {
                    vm.AssignmentFiles[assignment.Id].Add(file);
                }
            }

            //find all course documents
            vm.CourseDocuments = fs.Course(currentCourse).CourseDocs().AllFiles().ToList();

            return View(vm);
        }

        public JsonResult GetAllCourses()
        {
            CoursesViewModel vm = BuildViewModel();
            var simpleCourse = vm.CoursesByPrefix.Select(c => new
            {
                Prefix = c.Key,
                Courses = c.Value.Select(
                    co => new
                    {
                        Prefix = c.Key,
                        CourseNumber = co.Value.CourseNumber,
                        Description = co.Value.Description
                    }
                )
            });
            return this.Json(simpleCourse, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Search()
        {
            CoursesViewModel vm = BuildViewModel();
            return View(vm);
        }

        [HttpPost]
        public ActionResult Search(CoursesViewModel vm)
        {
            vm = BuildViewModel(vm);
            if (vm.SelectedCourse > 0)
            {
                Course toJoin = Db.Courses.Where(c => c.Id == vm.SelectedCourse).FirstOrDefault();
                if (toJoin != null)
                {
                    CourseUserRelationship cur = new CourseUserRelationship()
                    {
                        CourseId = toJoin.Id,
                        UserId = CurrentUser.Id,
                        Role = CourseRole.Student,
                        IsApproved = !toJoin.RequiresApprovalBeforeAdmission,
                        IsActive = true
                    };
                    Db.CourseUserRelationships.Add(cur);
                    Db.SaveChanges();

                    if (toJoin.RequiresApprovalBeforeAdmission == true)
                    {
                        vm.ServerMessage = string.Format("A request to join {0} has been sent to the course instructor.  Until then, certain features related to the course may be unavailable.", toJoin.Name);
                    }
                    else
                    {
                        vm.ServerMessage = string.Format("You are now enrolled in {0}.", toJoin.Name);
                    }
                }
                else
                {
                    vm.ServerMessage = "There server experienced an error when trying to add you to the selected course.  Please try again.  If the problem persists, please contact support@osbide.com.";
                }
            }
            return View(vm);
        }

        private CoursesViewModel BuildViewModel(CoursesViewModel vm = null)
        {
            if (vm == null)
            {
                vm = new CoursesViewModel();
            }
            vm.AllCourses = Db.Courses
                .Where(c => c.SchoolId == CurrentUser.SchoolId)
                .OrderBy(c => c.Prefix)
                .ToList();
            vm.CurrentUser = CurrentUser;

            foreach (Course course in vm.AllCourses)
            {
                if (vm.CoursesByPrefix.ContainsKey(course.Prefix) == false)
                {
                    vm.CoursesByPrefix.Add(course.Prefix, new SortedDictionary<string, Course>());
                }
                vm.CoursesByPrefix[course.Prefix].Add(course.CourseNumber, course);
            }
            return vm;
        }

    }
}
