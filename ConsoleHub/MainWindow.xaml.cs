using WPFConsoleControl = ConsoleControl.WPF.ConsoleControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.IO;
using Path = System.IO.Path;
using System.Threading;
using System.Runtime.InteropServices;

namespace ConsoleHub
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainViewModel ViewModel = new MainViewModel();
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern int GetConsoleCP();
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern int GetConsoleOutputCP();
        public MainWindow()
        {
            this.DataContext = ViewModel;
            InitializeComponent();
            ConsoleHubTaskbarIcon.Icon = Properties.Resources.AppIcon;
            var autoRunFile = Path.Combine(Path.GetDirectoryName(typeof(MainWindow).Assembly.Location), "ConsoleHubAutoRun.txt");
            if (File.Exists(autoRunFile))
            {
                _ = LoadRunFile(autoRunFile);
            }
            var args = Environment.GetCommandLineArgs();
            foreach (var arg in args)
            {
                if (arg == "--tray" || arg == "-tray" || arg == "/tray")
                {
                    this.Hide();
                }
            }
        }

        private async void ExecuteButton_Click(object sender, RoutedEventArgs e)
        {
            await ExecuteCommand(ViewModel.FileNameToExecute, ViewModel.ArgumentsToExecute);
        }
        private async Task LoadRunFile(string fileName)
        {
            var commands = await File.ReadAllLinesAsync(fileName);
            foreach (var command in commands)
            {
                if (command.TrimStart().StartsWith("#"))
                {
                    continue;
                }
                await ExecuteCommand(command);
            }
        }
        private async Task ExecuteCommand(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                return;
            }
            command = command.Trim();
            string fileName, arguments;
            if (command.StartsWith('"'))
            {
                var end = command.IndexOf('"', 1);
                fileName = command.Substring(1, end - 1).TrimEnd();
                if (end + 1 >= command.Length)
                {
                    arguments = "";
                }
                else
                {
                    arguments = command.Substring(end + 1).TrimStart();
                }
            }
            else
            {
                var end = command.IndexOf(' ');
                if (end == -1)
                {
                    fileName = command;
                    arguments = "";
                }
                else
                {
                    fileName = command.Substring(0, end);
                    if (end + 1 >= command.Length)
                    {
                        arguments = "";
                    }
                    else
                    {
                        arguments = command.Substring(end + 1).TrimStart();
                    }
                }
            }
            await ExecuteCommand(fileName, arguments);
        }
        private async Task ExecuteCommand(string fileName, string arguments)
        {
            if (fileName.ToLowerInvariant() == "*delay")
            {
                await Task.Delay(Convert.ToInt32(arguments));
                return;
            }
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return;
            }
            var model = new ConsoleViewModel()
            {
                Content = new WPFConsoleControl()
            };
            model.Content.ProcessInterface.OnProcessExit +=
                (sender, eventArgs) =>
                    Dispatcher.Invoke(() => ViewModel.Consoles.Remove(model));
            model.Content.StartProcess(new ProcessStartInfo() {
                FileName = fileName,
                Arguments = arguments,
                StandardOutputEncoding = Encoding.GetEncoding(GetConsoleOutputCP()),
                StandardErrorEncoding = Encoding.GetEncoding(GetConsoleOutputCP()),
                StandardInputEncoding = Encoding.GetEncoding(GetConsoleCP())
            });
            try
            {
                GC.KeepAlive(model.Content.ProcessInterface.Process.Id);
            }
            catch (Exception)
            {
                return;
            }
            ViewModel.Consoles.Add(model);
            ViewModel.CurrectConsoleIndex = ViewModel.Consoles.Count - 1;
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var pathExt =
                string.Join(
                    ';',
                    (Environment.GetEnvironmentVariable("PATHEXT") ?? ".exe;.com")
                        .Split(";")
                        .Select(x => "*" + x));
            var dialog = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = $"Executable files|{pathExt}|All files|*"
            };
            if (dialog.ShowDialog().GetValueOrDefault())
            {
                ViewModel.FileNameToExecute = dialog.FileName;
            }
        }

        private void AboutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("A tool to help you manage your console applications.\n"
                + "You can create a file named ConsoleHubAutoRun.txt (Encoding: UTF-8) in the same directory as that of ConsoleHub.exe with commands line by line, then ConsoleHub will execute them automatically when starting.\n"
                + "Special Commands (case insensitive): \n"
                + "    *delay [milliseconds]", "ConsoleHub", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void LoadRunFileMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = $"Text files|*.txt|All files|*"
            };
            if (dialog.ShowDialog().GetValueOrDefault())
            {
                await LoadRunFile(dialog.FileName);
            }
        }

        private void ConsoleHubTaskbarIcon_TrayLeftMouseDown(object sender, RoutedEventArgs e)
        {
            this.Show();
            if (this.WindowState == WindowState.Minimized)
            {
                this.WindowState = WindowState.Normal;
            }
            this.Activate();
        }

        private void HideToTrayMenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }

        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void SendCtrlCMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var sendCtrlCFile = Path.Combine(Path.GetDirectoryName(typeof(MainWindow).Assembly.Location), "SendCtrlC.exe");
            if (!File.Exists(sendCtrlCFile))
            {
                MessageBox.Show("Can't find SendCtrlC.exe", "ControlHub", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            var pid = ViewModel?.CurrectConsole?.Content?.ProcessInterface?.Process?.Id;
            if (pid.HasValue)
            {
                Process.Start(sendCtrlCFile, $"{pid} 0");
            }
        }

        private void SendCtrlBreakMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var sendCtrlCFile = Path.Combine(Path.GetDirectoryName(typeof(MainWindow).Assembly.Location), "SendCtrlC.exe");
            if (!File.Exists(sendCtrlCFile))
            {
                MessageBox.Show("Can't find SendCtrlC.exe", "ControlHub", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            var pid = ViewModel?.CurrectConsole?.Content?.ProcessInterface?.Process?.Id;
            if (pid.HasValue)
            {
                Process.Start(sendCtrlCFile, $"{pid} 1");
            }
        }

        private void KillMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ViewModel?.CurrectConsole?.Content?.StopProcess();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            var sendCtrlCFile = Path.Combine(Path.GetDirectoryName(typeof(MainWindow).Assembly.Location), "SendCtrlC.exe");
            if (File.Exists(sendCtrlCFile))
            {
                foreach (var console in ViewModel.Consoles)
                {
                        try
                        {
                            var pid = console?.Content?.ProcessInterface?.Process?.Id;
                            if (pid.HasValue)
                            {
                                Process.Start(sendCtrlCFile, $"{pid} 1"); //Ctrl+Break Signal
                            }
                        }
                        catch (Exception)
                        {
                        }
                }
                Thread.Sleep(2000);
            }
            foreach (var console in ViewModel.Consoles)
            {
                try
                {
                    console?.Content?.ProcessInterface?.StopProcess();
                }
                catch (Exception)
                {
                }
            }
            ViewModel.Consoles.Clear();
        }
    }
}
