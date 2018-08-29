using System.Windows;

namespace FireFly.Models
{
    public class ReplayFile : DependencyObject
    {
        public static readonly DependencyProperty FullPathProperty =
            DependencyProperty.Register("FullPath", typeof(string), typeof(ReplayFile), new PropertyMetadata(""));

        public static readonly DependencyProperty IsPausedProperty =
            DependencyProperty.Register("IsPaused", typeof(bool), typeof(ReplayFile), new PropertyMetadata(false));

        public static readonly DependencyProperty IsPlayingProperty =
                    DependencyProperty.Register("IsPlaying", typeof(bool), typeof(ReplayFile), new PropertyMetadata(false));

        public static readonly DependencyProperty IsRemoteProperty =
            DependencyProperty.Register("IsRemote", typeof(bool), typeof(ReplayFile), new PropertyMetadata(false));

        public static readonly DependencyProperty NameProperty =
                    DependencyProperty.Register("Name", typeof(string), typeof(ReplayFile), new PropertyMetadata(""));

        public string FullPath
        {
            get { return (string)GetValue(FullPathProperty); }
            set { SetValue(FullPathProperty, value); }
        }

        public bool IsPaused
        {
            get { return (bool)GetValue(IsPausedProperty); }
            set { SetValue(IsPausedProperty, value); }
        }

        public bool IsPlaying
        {
            get { return (bool)GetValue(IsPlayingProperty); }
            set { SetValue(IsPlayingProperty, value); }
        }

        public bool IsRemote
        {
            get { return (bool)GetValue(IsRemoteProperty); }
            set { SetValue(IsRemoteProperty, value); }
        }

        public string Name
        {
            get { return (string)GetValue(NameProperty); }
            set { SetValue(NameProperty, value); }
        }
    }
}