using WPFConsoleControl = ConsoleControl.WPF.ConsoleControl;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace ConsoleHub
{
    public class ConsoleViewModel : INotifyPropertyChanged, IEquatable<ConsoleViewModel>
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public WPFConsoleControl Content { get; set; }
        public string Title => $"{Content.ProcessInterface.Process.ProcessName} (PID: {Content.ProcessInterface.Process.Id})";

        public override bool Equals(object obj)
        {
            return Equals(obj as ConsoleViewModel);
        }

        public bool Equals(ConsoleViewModel other)
        {
            return other != null &&
                   EqualityComparer<WPFConsoleControl>.Default.Equals(Content, other.Content) &&
                   Title == other.Title;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Content, Title);
        }

        public static bool operator ==(ConsoleViewModel left, ConsoleViewModel right)
        {
            return EqualityComparer<ConsoleViewModel>.Default.Equals(left, right);
        }

        public static bool operator !=(ConsoleViewModel left, ConsoleViewModel right)
        {
            return !(left == right);
        }
    }
}
