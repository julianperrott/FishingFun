using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace FishingFun
{
    public static class WowProcess
    {
        private const UInt32 WM_KEYDOWN = 0x0100;
        private const UInt32 WM_KEYUP = 0x0101;
        private static ConsoleKey lastKey;
        private static Random random = new Random();

        //Get the wow-process, if success returns the process else null
        public static Process Get(string name = "")
        {
            var names = string.IsNullOrEmpty(name) ? new List<string> { "Wow", "Wow-64" } : new List<string> { name };

            var processList = Process.GetProcesses();
            foreach (var p in processList)
            {
                if (names.Contains(p.ProcessName))
                {
                    return p;
                }
            }

            return null;
        }

        [DllImport("user32.dll")]
        private static extern bool PostMessage(IntPtr hWnd, UInt32 Msg, int wParam, int lParam);

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, out uint ProcessId);

        static Process GetActiveProcess()
        {
            IntPtr hwnd = GetForegroundWindow();
            uint pid;
            GetWindowThreadProcessId(hwnd, out pid);
            return Process.GetProcessById((int)pid);
        }

        private static void KeyDown(ConsoleKey key)
        {
            lastKey = key;
            PostMessage(Get().MainWindowHandle, WM_KEYDOWN, (int)key, 0);
        }

        private static void KeyUp()
        {
            KeyUp(lastKey);
        }

        public static void PressKey(ConsoleKey key)
        {
            KeyDown(key);
            Thread.Sleep(50 + random.Next(0, 75));
            KeyUp(key);
        }

        public static void KeyUp(ConsoleKey key)
        {
            PostMessage(Get().MainWindowHandle, WM_KEYUP, (int)key, 0);
        }

        public static void RightClickMouse(ILog logger, System.Drawing.Point position)
        {
            var activeProcess = GetActiveProcess();
            var wowProcess = WowProcess.Get();

            var oldPosition = System.Windows.Forms.Cursor.Position;

            System.Windows.Forms.Cursor.Position = position;
            PostMessage(wowProcess.MainWindowHandle, Keys.WM_RBUTTONDOWN, Keys.VK_RMB, 0);
            Thread.Sleep(30 + random.Next(0, 47));
            PostMessage(wowProcess.MainWindowHandle, Keys.WM_RBUTTONUP, Keys.VK_RMB, 0);

            RefocusOnOldScreen(logger, activeProcess, wowProcess, oldPosition);

        }

        private static void RefocusOnOldScreen(ILog logger, Process activeProcess, Process wowProcess, System.Drawing.Point oldPosition)
        {
            try
            {
                if (activeProcess.MainWindowTitle != wowProcess.MainWindowTitle)
                {
                    // get focus back on this screen
                    PostMessage(activeProcess.MainWindowHandle, Keys.WM_RBUTTONDOWN, Keys.VK_RMB, 0);
                    Thread.Sleep(30);
                    PostMessage(activeProcess.MainWindowHandle, Keys.WM_RBUTTONUP, Keys.VK_RMB, 0);

                    KeyDown(ConsoleKey.Escape);
                    Thread.Sleep(30);
                    KeyUp(ConsoleKey.Escape);

                    System.Windows.Forms.Cursor.Position = oldPosition;
                }
            }
            catch(Exception ex)
            {
                logger.Error(ex.Message);
            }
        }
    }
}