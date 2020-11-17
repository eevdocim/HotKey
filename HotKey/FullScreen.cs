using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotKey
{
    class FullSceen
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowRect(IntPtr hwnd, out RECT rc);
        protected Process[] procs;
        public bool isfull()
        {
            bool runningFullScreen = false;
            procs = Process.GetProcessesByName("explorer");
            int explorer;
            explorer = procs[0].Id;
            //Console.WriteLine("explorer: " + explorer);
            RECT appBounds;
            System.Drawing.Rectangle screenBounds;
            IntPtr hWnd;
            hWnd = GetForegroundWindow();
            int pid;
            GetWindowThreadProcessId(hWnd, out pid);
            //Console.WriteLine("process: " + pid);
            //Console.WriteLine("hwnd: "+hWnd);
            if (pid != explorer && hWnd != null && !hWnd.Equals(IntPtr.Zero))
            {
                GetWindowRect(hWnd, out appBounds);
                screenBounds = Screen.FromHandle(hWnd).Bounds;
                if ((appBounds.Bottom - appBounds.Top) == screenBounds.Height && (appBounds.Right - appBounds.Left) == screenBounds.Width)
                {
                    runningFullScreen = true;
                }
            }
            return (runningFullScreen);
        }
    }
}
