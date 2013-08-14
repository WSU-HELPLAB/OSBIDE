using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OSBIDE.Library;
using OSBIDE.Library.Models;
using OSBIDE.Web.Models.Attributes;
using OSBIDE.Web.Models.ViewModels;
using OSBIDE.Web.Models.Queries;
using OSBIDE.Web.Models;
using OSBIDE.Library.Events;

namespace OSBIDE.Web.Controllers
{
    public class ProfileController : ControllerBase
    {
        //
        // GET: /Profile/
        [OsbideAuthorize]
        [RequiresVisualStudioConnectionForStudents]
        public ActionResult Index(int? id, int timestamp = -1)
        {
            ActivityFeedQuery query = new ActivityFeedQuery(Db);
            ActivityFeedQuery subscriptionsQuery = new ActivityFeedQuery(Db);
            ProfileViewModel vm = new ProfileViewModel();
            vm.User = CurrentUser;
            if (id != null)
            {
                OsbideUser user = Db.Users.Find(id);
                if (user != null)
                {
                    vm.User = user;
                }
            }

            if (timestamp > 0)
            {
                DateTime pullDate = new DateTime(timestamp);
                query.StartDate = pullDate;
            }
            else
            {
                query.StartDate = DateTime.UtcNow.AddHours(-48);

            }

            //add the event types that the user wants to see
            //TODO: make this dynamic based on user selection
            foreach (IOsbideEvent evt in ActivityFeedQuery.GetAllEvents())
            {
                query.AddEventType(evt);
                subscriptionsQuery.AddEventType(evt);
            }

            //add in the list of users that the current person cares about
            query.AddSubscriptionSubject(vm.User);
            subscriptionsQuery.AddSubscriptionSubject(vm.User);

            //build the feed view model
            vm.Feed = new FeedViewModel();
            vm.Feed.Feed = AggregateFeedItem.FromFeedItems(query.Execute().ToList());
            vm.Feed.LastLogId = -1;
            vm.Feed.SingleUserId = vm.User.Id;
            vm.Feed.LastPollDate = query.StartDate;
            vm.Score = Db.UserScores.Where(s => s.UserId == vm.User.Id).FirstOrDefault();
            vm.NumberOfPosts = (from e in Db.EventLogs
                                where (e.LogType == FeedCommentEvent.Name || e.LogType == AskForHelpEvent.Name)
                                && e.SenderId == vm.User.Id
                                select e
                                ).Count();
            vm.NumberOfComments = Db.LogComments.Where(c => c.AuthorId == vm.User.Id).Count();
            if (vm.Score == null)
            {
                vm.Score = new UserScore();
            }
            var maxQuery = Db.EventLogs.Where(e => e.SenderId == vm.User.Id).Select(e => e.Id);
            if (maxQuery.Count() > 0)
            {
                vm.Feed.LastLogId = maxQuery.Max();
            }

            // Build a catalog of recent commenting activity:
            // 1. Find all comments that the user has made
            // 2. Find all comments made by others on posts authored by the current user
            // 3. Find all comments made by others on posts on which the current user has written a comment

            DateTime maxLookback = base.DefaultErrorLookback;

            //1. find recent comments
            vm.RecentComments = (from comment in Db.LogComments
                                where comment.AuthorId == vm.User.Id
                                && comment.DatePosted >= maxLookback
                                orderby comment.DatePosted descending
                                select comment
                                ).ToList();

            //2. recent comments made by others on posts authored by the current user
            vm.CommentsMadeByOthers = (from comment in Db.LogComments
                                       where comment.Log.SenderId == vm.User.Id
                                       && comment.DatePosted >= maxLookback
                                       orderby comment.DatePosted descending
                                       select comment
                                       ).ToList();


            //TODO: implement this
            //3. comments made by others on posts in which the current user has written a comment
            

            //show subscriptions only if the user is accessing his own page
            if (vm.User.Id == CurrentUser.Id)
            {   
                List<int> eventLogIds = Db.EventLogSubscriptions.Where(s => s.UserId == vm.User.Id).Select(s => s.LogId).ToList();
                if (eventLogIds.Count > 0)
                {
                    foreach (int logId in eventLogIds)
                    {
                        subscriptionsQuery.AddEventId(logId);
                    }
                    vm.EventLogSubscriptions = AggregateFeedItem.FromFeedItems(subscriptionsQuery.Execute().ToList());
                }
                
            }

            return View(vm);
        }

        [OsbideAuthorize]
        public ActionResult Edit()
        {
            return View(BuildEditViewModel());
        }

        [OsbideAuthorize]
        [HttpPost]
        public ActionResult Edit(EditProfileViewModel vm)
        {
            if (ModelState.IsValid)
            {
                // We can determine which is desired by checking which button was pressed
                if (Request.Form["updateEmail"] != null)
                {
                    UpdateEmail(vm);   
                }
                else if (Request.Form["updatePassword"] != null)
                {
                    UpdatePassword(vm);
                }
                else if (Request.Form["updateSubscriptions"] != null)
                {
                    UpdateSubscriptions(vm);
                }
                else if (Request.Form["changeEmailNotifications"] != null)
                {
                    UpdateEmailNotificationSettings(vm);
                }
            }
            return View(BuildEditViewModel(vm));
        }

        #region Edit helper methods

        private EditProfileViewModel BuildEditViewModel(EditProfileViewModel oldVm = null)
        {
            EditProfileViewModel vm = new EditProfileViewModel();
            if (oldVm != null)
            {
                vm = oldVm;
            }
            vm.User = CurrentUser;

            vm.UsersInCourse = Db.Users.Where(u => u.SchoolId == CurrentUser.SchoolId).ToList();
            StudentSubscriptionsQuery subs = new StudentSubscriptionsQuery(Db, CurrentUser);
            List<OsbideUser> subscriptionsAsUsers = subs.Execute().ToList();
            List<UserSubscription> subscriptions = Db
                .UserSubscriptions.Where(s => s.ObserverSchoolId == CurrentUser.SchoolId)
                .Where(s => s.ObserverInstitutionId == CurrentUser.InstitutionId)
                .ToList();
            foreach (OsbideUser user in subscriptionsAsUsers)
            {
                UserSubscription us = subscriptions.Where(s => s.SubjectInstitutionId == user.InstitutionId).FirstOrDefault();
                if (us != null)
                {
                    vm.UserSubscriptions[user.Id] = us;
                }
            }
            return vm;
        }

        private void UpdateEmailNotificationSettings(EditProfileViewModel vm)
        {
            CurrentUser.ReceiveNotificationEmails = vm.ReceiveEmailNotifications;
            vm.UpdateEmailSettingsMessage = "Your email settings have been updated.";
        }

        private void UpdateSubscriptions(EditProfileViewModel vm)
        {
            //remove all current subscriptions that are not required
            List<UserSubscription> nonEssentialSubscriptions = Db.UserSubscriptions
                .Where(s => s.ObserverInstitutionId == CurrentUser.InstitutionId)
                .Where(s => s.ObserverSchoolId == CurrentUser.SchoolId)
                .Where(s => s.IsRequiredSubscription == false)
                .ToList();
            foreach (UserSubscription subscription in nonEssentialSubscriptions)
            {
                Db.UserSubscriptions.Remove(subscription);
            }
            Db.SaveChanges();

            //add in requested subscriptions
            foreach (string key in Request.Form.Keys)
            {
                if (key.StartsWith("subscription_") == true)
                {
                    int userId = -1;
                    string[] pieces = key.Split('_');
                    if(pieces.Length == 2)
                    {
                        if (Int32.TryParse(pieces[1], out userId) == true)
                        {
                            OsbideUser user = Db.Users.Where(u => u.Id == userId).FirstOrDefault();
                            if (user != null)
                            {
                                UserSubscription sub = new UserSubscription()
                                {
                                    IsRequiredSubscription = false,
                                    ObserverSchoolId = CurrentUser.SchoolId,
                                    ObserverInstitutionId = CurrentUser.InstitutionId,
                                    SubjectSchoolId = user.SchoolId,
                                    SubjectInstitutionId = user.InstitutionId
                                };
                                Db.UserSubscriptions.Add(sub);
                            }
                        }
                    }
                }
            }
            Db.SaveChanges();

        }

        private void UpdateEmail(EditProfileViewModel vm)
        {
            //Attempt to update email address.
            //Check to make sure email address isn't in use
            OsbideUser user = Db.Users.Where(u => u.Email.CompareTo(vm.NewEmail) == 0).FirstOrDefault();
            if (user == null)
            {
                //update email address
                CurrentUser.Email = vm.NewEmail;

                //the email address acts as the hash for the user's password so we've got to change that as well
                UserPassword up = Db.UserPasswords.Where(p => p.UserId == CurrentUser.Id).FirstOrDefault();
                up.Password = UserPassword.EncryptPassword(vm.OldPassword, CurrentUser);
                Db.SaveChanges();

                vm.UpdateEmailSuccessMessage = string.Format("Your email has been successfully updated to \"{0}.\"", CurrentUser.Email);
            }
            else
            {
                ModelState.AddModelError("", "The requested email is already in use.");
            }
        }

        private void UpdatePassword(EditProfileViewModel vm)
        {
            //update the user's password
            UserPassword up = Db.UserPasswords.Where(p => p.UserId == CurrentUser.Id).FirstOrDefault();
            if (up != null)
            {
                up.Password = UserPassword.EncryptPassword(vm.NewPassword, CurrentUser);
                Db.SaveChanges();
                vm.UpdatePasswordSuccessMessage = "Your password has been successfully updated.";
            }
            else
            {
                ModelState.AddModelError("", "An error occurred while updating your password.  Please try again.  If the problem persists, please contact support at \"support@osble.org\".");
            }
        }
        #endregion

        /// <summary>
        /// Returns the profile picture for the supplied user id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public FileStreamResult Picture(int id, int size = 128)
        {
            ProfileImage image = Db.ProfileImages.Where(p => p.UserID == id).FirstOrDefault();
            IdenticonRenderer renderer = new IdenticonRenderer();
            System.Drawing.Bitmap userBitmap;
            if (image != null)
            {
                try
                {
                    userBitmap = image.GetProfileImage();
                }
                catch (Exception)
                {
                    
                    userBitmap = renderer.Render(image.User.Email.GetHashCode(), 128);
                    image.SetProfileImage(userBitmap);
                    Db.SaveChanges();
                }
            }
            else
            {
                OsbideUser user = Db.Users.Where(u => u.Id == id).FirstOrDefault();
                if (user != null && user.ProfileImage == null)
                {
                    try
                    {
                        user.SetProfileImage(renderer.Render(user.Email.GetHashCode(), 128));
                        userBitmap = user.ProfileImage.GetProfileImage();
                        Db.SaveChanges();
                    }
                    catch (Exception)
                    {
                        userBitmap = renderer.Render(1, 128);
                    }
                }
                else
                {
                    userBitmap = renderer.Render(1, 128);
                }
            }

            if (size != 128)
            {
                Bitmap bmp = new Bitmap(userBitmap, size, size);
                Graphics graph = Graphics.FromImage(userBitmap);
                graph.InterpolationMode = InterpolationMode.High;
                graph.CompositingQuality = CompositingQuality.HighQuality;
                graph.SmoothingMode = SmoothingMode.AntiAlias;
                graph.DrawImage(bmp, new Rectangle(0, 0, size, size));
                userBitmap = bmp;
            }

            MemoryStream stream = new MemoryStream();
            userBitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
            stream.Position = 0;
            return new FileStreamResult(stream, "image/png");
        }

        [HttpPost]
        [OsbideAuthorize]
        public ActionResult Picture(HttpPostedFileBase file)
        {
            //two options: user uploaded a profile picture 
            //             OR user requested a default profile picture
            if (Request.Params["upload"] != null)
            {
                //if the file is null, check the Request.Files construct before giving up
                if (file == null)
                {
                    try
                    {
                        file = Request.Files["file"];
                    }
                    catch (Exception)
                    {
                    }
                }
                if (file != null) // Upload Picture
                {
                    Bitmap image;
                    try
                    {
                        image = new Bitmap(file.InputStream);
                    }
                    catch
                    {   // If image format is invalid, discard it.
                        image = null;
                    }

                    if (image != null)
                    {
                        int thumbSize = 128;

                        // Crop image to a square.
                        int square = Math.Min(image.Width, image.Height);
                        using (Bitmap cropImage = new Bitmap(square, square))
                        {
                            using (Bitmap finalImage = new Bitmap(thumbSize, thumbSize))
                            {
                                Graphics cropGraphics = Graphics.FromImage(cropImage);
                                Graphics finalGraphics = Graphics.FromImage(finalImage);

                                // Center cropped image horizontally, leave at the top vertically. (better focus on subject)
                                cropGraphics.DrawImage(image, -(image.Width - cropImage.Width) / 2, 0);

                                // Convert to thumbnail.
                                finalGraphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

                                finalGraphics.DrawImage(cropImage,
                                                    new Rectangle(0, 0, thumbSize, thumbSize),
                                                    new Rectangle(0, 0, square, square),
                                                    GraphicsUnit.Pixel);

                                // Write image to user's profile
                                CurrentUser.SetProfileImage(finalImage);
                            }
                        }
                    }
                }
            }
            else
            {
                //reset to default profile picture
                IdenticonRenderer renderer = new IdenticonRenderer();
                CurrentUser.SetProfileImage(renderer.Render(CurrentUser.Email.GetHashCode(), 128));
            }
            return RedirectToAction("Edit");
        }

    }
}
