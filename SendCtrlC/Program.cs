using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace SendCtrlC
{
    class Program
    {
        [DllImport("kernel32.dll")]
        internal static extern bool GenerateConsoleCtrlEvent(uint dwCtrlEvent, uint dwProcessGroupId);
        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool AttachConsole(uint dwProcessId);
        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        internal static extern bool FreeConsole();
        [DllImport("kernel32.dll")]
        static extern bool SetConsoleCtrlHandler(ConsoleCtrlDelegate HandlerRoutine, bool Add);
        delegate bool ConsoleCtrlDelegate(uint CtrlType);
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                throw new ArgumentException(nameof(args));
            }
            uint pid = Convert.ToUInt32(args[0]);
            if (AttachConsole(pid))
            {
                SetConsoleCtrlHandler(null, true);
                try
                {
                    GenerateConsoleCtrlEvent(args.Length >= 2 ? Convert.ToUInt32(args[1]) : 0, 0);
                }
                finally
                {
                    FreeConsole();
                }
            }
        }
    }
}
