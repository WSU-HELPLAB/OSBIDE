using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace ActivityReader.Controls.ViewModels
{
    public class ActivityFeedViewModel
    {
        public ObservableCollection<FeedMessageViewModel> Feed { get; set; }
        public ActivityFeedViewModel()
        {
            Feed = new ObservableCollection<FeedMessageViewModel>();
        }
    }
}
