using OSBIDE.Library.Events;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace OSBIDE.Library.Models
{
    /// <summary>
    /// FeedOption.NULL is used for internal bookkeeping.  It cannot be set by the user.
    /// </summary>
    [Flags]
    public enum FeedSetting : int
    {
        NULL = 1,
        BuildEvent = 2,
        ExceptionEvent = 4,
        FeedPostEvent = 8,
        AskForHelpEvent = 16,
        SubmitEvent = 32,
        LogCommentEvent = 64
    };

    public class UserFeedSetting
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }
        public virtual OsbideUser User { get; set; }
        public DateTime SettingsDate { get; set; }

        public int Settings { get; set; }

        public UserFeedSetting()
        {
            SettingsDate = DateTime.UtcNow;
        }

        public UserFeedSetting(UserFeedSetting other) : this()
        {
            this.Id = other.Id;
            this.Settings = other.Settings;
            this.UserId = other.UserId;
            this.SettingsDate = other.SettingsDate;
        }

        public bool HasSetting(FeedSetting setting)
        {
            int result = Settings & (int)setting;
            return result == (int)setting;
        }

        public static IOsbideEvent FeedOptionToOsbideEvent(FeedSetting option)
        {
            IOsbideEvent evt = null;
            switch (option)
            {
                case FeedSetting.AskForHelpEvent:
                    evt = new AskForHelpEvent();
                    break;
                case FeedSetting.BuildEvent:
                    evt = new BuildEvent();
                    break;
                case FeedSetting.ExceptionEvent:
                    evt = new ExceptionEvent();
                    break;
                case FeedSetting.FeedPostEvent:
                    evt = new FeedPostEvent();
                    break;
                case FeedSetting.SubmitEvent:
                    evt = new SubmitEvent();
                    break;
                case FeedSetting.LogCommentEvent:
                    evt = new LogCommentEvent();
                    break;
            }
            return evt;
        }

        public List<FeedSetting> ActiveSettings
        {
            get
            {
                List<FeedSetting> allSettings = Enum.GetValues(typeof(FeedSetting)).Cast<FeedSetting>().ToList();
                List<FeedSetting> userSettings = new List<FeedSetting>();
                foreach (FeedSetting setting in allSettings)
                {
                    if (HasSetting(setting) == true)
                    {
                        userSettings.Add(setting);
                    }
                }
                return userSettings;
            }
        }

        public bool HasSetting(IOsbideEvent evt)
        {
            FeedSetting option = FeedSetting.NULL;
            if (evt.EventName == AskForHelpEvent.Name)
            {
                option = FeedSetting.AskForHelpEvent;
            }
            else if (evt.EventName == BuildEvent.Name)
            {
                option = FeedSetting.BuildEvent;
            }
            else if (evt.EventName == ExceptionEvent.Name)
            {
                option = FeedSetting.ExceptionEvent;
            }
            else if (evt.EventName == FeedPostEvent.Name)
            {
                option = FeedSetting.FeedPostEvent;
            }
            else if (evt.EventName == LogCommentEvent.Name)
            {
                option = FeedSetting.LogCommentEvent;
            }
            else if (evt.EventName == SubmitEvent.Name)
            {
                option = FeedSetting.SubmitEvent;
            }
            return HasSetting(option);
        }

        public void SetSetting(IOsbideEvent evt, bool value)
        {
            FeedSetting option = FeedSetting.NULL;
            if (evt.EventName == AskForHelpEvent.Name)
            {
                option = FeedSetting.AskForHelpEvent;
            }
            else if (evt.EventName == BuildEvent.Name)
            {
                option = FeedSetting.BuildEvent;
            }
            else if (evt.EventName == ExceptionEvent.Name)
            {
                option = FeedSetting.ExceptionEvent;
            }
            else if (evt.EventName == FeedPostEvent.Name)
            {
                option = FeedSetting.FeedPostEvent;
            }
            else if (evt.EventName == LogCommentEvent.Name)
            {
                option = FeedSetting.LogCommentEvent;
            }
            else if (evt.EventName == SubmitEvent.Name)
            {
                option = FeedSetting.SubmitEvent;
            }
            if (option != FeedSetting.NULL)
            {
                SetSetting(option, value);
            }
        }

        public void SetSetting(FeedSetting setting, bool value)
        {
            switch (value)
            {
                case true:
                    AddSetting(setting);
                    break;
                case false:
                    RemoveSetting(setting);
                    break;
            }
        }

        protected void AddSetting(FeedSetting setting)
        {
            Settings = (byte)(Settings | (byte)setting);
        }

        protected void RemoveSetting(FeedSetting setting)
        {
            //~ is a bitwise not in c#
            //Doing a bitwise AND on a NOTed level should result in the level being removed
            Settings = (byte)(Settings & (~(byte)setting));
        }

    }
}
