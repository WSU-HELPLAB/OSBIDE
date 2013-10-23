using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OSBIDE.Library.Models
{
    public class OsbideContextSeeder
    {
        public static void Seed(OsbideContext context)
        {
            //add in some sample schools
            School wsu = new School() { Name = "Washington State University" };
            context.Schools.Add(wsu);
            context.Schools.Add(new School() { Name = "Other Institution" });
            context.SaveChanges();

            //add in default chat rooms
            context.ChatRooms.Add(new ChatRoom() { Name = "Class Chat", SchoolId = wsu.Id, IsDefaultRoom = true });
            context.ChatRooms.Add(new ChatRoom() { Name = "Section 1 Chat", SchoolId = wsu.Id });

            //add in some default subscriptions
            context.UserSubscriptions.Add(new UserSubscription() { ObserverInstitutionId = 123, ObserverSchoolId = 1, SubjectSchoolId = 1, SubjectInstitutionId = 456 });
            context.UserSubscriptions.Add(new UserSubscription() { ObserverInstitutionId = 123, ObserverSchoolId = 1, SubjectSchoolId = 1, SubjectInstitutionId = 789, IsRequiredSubscription = true });
            context.UserSubscriptions.Add(new UserSubscription() { ObserverInstitutionId = 456, ObserverSchoolId = 1, SubjectSchoolId = 1, SubjectInstitutionId = 789, IsRequiredSubscription = true });

            //add some test users
            IdenticonRenderer renderer = new IdenticonRenderer();
            OsbideUser joe = new OsbideUser()
                {
                    FirstName = "Joe",
                    LastName = "User",
                    Email = "joe@user.com",
                    InstitutionId = 123,
                    SchoolId = wsu.Id,
                    Role = SystemRole.Student
                };
            joe.SetProfileImage(renderer.Render(joe.Email.GetHashCode(), 128));
            context.Users.Add(joe);

            OsbideUser betty = new OsbideUser()
            {
                FirstName = "Betty",
                LastName = "Rogers",
                Email = "betty@rogers.com",
                InstitutionId = 456,
                SchoolId = wsu.Id,
                Role = SystemRole.Student
            };
            betty.SetProfileImage(renderer.Render(betty.Email.GetHashCode(), 128));
            context.Users.Add(betty);
            context.SaveChanges();

            OsbideUser adam = new OsbideUser()
            {
                FirstName = "Adam",
                LastName = "Carter",
                Email = "cartera@wsu.edu",
                InstitutionId = 789,
                SchoolId = wsu.Id,
                Role = SystemRole.Instructor
            };
            adam.SetProfileImage(renderer.Render(adam.Email.GetHashCode(), 128));
            context.Users.Add(adam);
            context.SaveChanges();

            //...and set their passwords
            UserPassword up = new UserPassword();
            up.UserId = joe.Id;
            up.Password = UserPassword.EncryptPassword("123123", joe);
            context.UserPasswords.Add(up);

            up = new UserPassword();
            up.UserId = betty.Id;
            up.Password = UserPassword.EncryptPassword("123123", betty);
            context.UserPasswords.Add(up);

            up = new UserPassword();
            up.UserId = adam.Id;
            up.Password = UserPassword.EncryptPassword("123123", adam);
            context.UserPasswords.Add(up);
            context.SaveChanges();

            //also set up some courses
            context.Courses.Add(new Course()
            {
                Name = "CptS 121"
            }
                );

            context.Courses.Add(new Course()
            {
                Name = "CptS 122"
            }
            );

            context.Courses.Add(new Course()
            {
                Name = "CptS 223"
            }
            );
            context.SaveChanges();

            //add students to the courses
            context.Courses.Find(1).Coordinators.Add(new CourseCoordinator() { CoordinatorId = 3, CourseId = 1});
            context.Courses.Find(1).Students.Add(new CourseStudent() { StudentId = 1, CourseId = 1 });
            context.Courses.Find(1).Students.Add(new CourseStudent() { StudentId = 2, CourseId = 1 });
            context.SaveChanges();
        }
    }
}
