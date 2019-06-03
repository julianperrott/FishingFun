using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
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
        public static Process Get()
        {
            var processList = Process.GetProcesses();
            foreach (var p in processList)
            {
                if (p.ProcessName == "Wow" || p.ProcessName == "Wow-64")
                {
                    return p;
                }
            }
            return null;
        }

        [DllImport("user32.dll")]
        private static extern bool PostMessage(IntPtr hWnd, UInt32 Msg, int wParam, int lParam);

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

        public static void RightClickMouse()
        {
            PostMessage(WowProcess.Get().MainWindowHandle, Keys.WM_RBUTTONDOWN, Keys.VK_RMB, 0);
            Thread.Sleep(30 + random.Next(0, 47));
            PostMessage(WowProcess.Get().MainWindowHandle, Keys.WM_RBUTTONUP, Keys.VK_RMB, 0);
        }
    }
}