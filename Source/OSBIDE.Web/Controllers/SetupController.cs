using OSBIDE.Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OSBIDE.Web.Controllers
{
    public class SetupController : ControllerBase
    {
        //
        // GET: /Setup/
        public ActionResult Index()
        {
            //check for default schools and chat rooms.  If they exist, redirect to the home page
            if (Db.ChatRooms.Count() != 0 && Db.Schools.Count() != 0)
            {
                return RedirectToAction("Index", "Home");
            }
            return View(OsbideUser.GenericUser());
        }

        [HttpPost]
        public ActionResult Index(OsbideUser user)
        {
            if (Db.Schools.Count() == 0)
            {
                School wsu = new School()
                {
                    Name = "Washington State University"
                };
                School other = new School()
                {
                    Name = "Other Instituion"
                };
                Db.Schools.Add(wsu);
                Db.Schools.Add(other);
                Db.SaveChanges();
            }
            if (Db.ChatRooms.Count() == 0)
            {
                Db.ChatRooms.Add(new Library.Models.ChatRoom()
                {
                    IsDefaultRoom = true,
                    Name = "Class Chat",
                    SchoolId = 1
                }
                );

                Db.ChatRooms.Add(new Library.Models.ChatRoom()
                {
                    IsDefaultRoom = false,
                    Name = "Chat Room #1",
                    SchoolId = 1
                }
                );

                Db.ChatRooms.Add(new Library.Models.ChatRoom()
                {
                    IsDefaultRoom = false,
                    Name = "Chat Room #2",
                    SchoolId = 1
                }
                );

                Db.ChatRooms.Add(new Library.Models.ChatRoom()
                {
                    IsDefaultRoom = true,
                    Name = "Class Chat",
                    SchoolId = 2
                }
                );
                Db.SaveChanges();
            }

            if (Db.Courses.Count() == 0)
            {
                Db.Courses.Add(new Course()
                {
                    Name = "CptS 121",
                    SchoolId = 1
                }
                );

                Db.Courses.Add(new Course()
                {
                    Name = "CptS 122",
                    SchoolId = 1
                }
                );

                Db.Courses.Add(new Course()
                {
                    Name = "CptS 223",
                    SchoolId = 1
                }
                );
                Db.SaveChanges();
            }

            return RedirectToAction("Index", "Home");
        }
    }
}
