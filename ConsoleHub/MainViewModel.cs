using WPFConsoleControl = ConsoleControl.WPF.ConsoleControl;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.ComponentModel;

namespace ConsoleHub
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public ObservableCollection<ConsoleViewModel> Consoles { get; set; } = new ObservableCollection<ConsoleViewModel>();
        public string FileNameToExecute { get; set; }
        public string ArgumentsToExecute { get; set; }
        public int CurrectConsoleIndex { get; set; }
        public ConsoleViewModel CurrectConsole { get; set; }
    }
}
