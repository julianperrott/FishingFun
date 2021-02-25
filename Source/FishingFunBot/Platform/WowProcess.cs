using log4net;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

#nullable enable

namespace FishingFun
{
    public static class WowProcess
    {
        public static ILog logger = LogManager.GetLogger(typeof(FishingBot));

        private const UInt32 WM_KEYDOWN = 0x0100;
        private const UInt32 WM_KEYUP = 0x0101;
        private static ConsoleKey lastKey;
        private static Random random = new Random();
        public static int LootDelay=2000;


        public static bool IsWowClassic()
        {
            var wowProcess = Get();
            return wowProcess != null ? wowProcess.ProcessName.ToLower().Contains("classic") : false; ;
        }

        //Get the wow-process, if success returns the process else null
        public static Process? Get(string name = "")
        {
            var names = string.IsNullOrEmpty(name) ? new List<string> { "Wow", "WowClassic", "Wow-64" } : new List<string> { name };

            var processList = Process.GetProcesses();
            foreach (var p in processList)
            {
                if (names.Select(s => s.ToLower()).Contains(p.ProcessName.ToLower()))
                {
                    return p;
                }
            }

            logger.Error($"Failed to find the wow process, tried: {string.Join(", ", names)}");

            return null;
        }

        [DllImport("user32.dll")]
        public static extern bool PostMessage(IntPtr hWnd, UInt32 Msg, int wParam, int lParam);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, out uint ProcessId);

        private static Process GetActiveProcess()
        {
            IntPtr hwnd = GetForegroundWindow();
            uint pid;
            GetWindowThreadProcessId(hwnd, out pid);
            return Process.GetProcessById((int)pid);
        }

        private static void KeyDown(ConsoleKey key)
        {
            lastKey = key;
            var wowProcess = Get();
            if (wowProcess != null)
            {
                PostMessage(wowProcess.MainWindowHandle, WM_KEYDOWN, (int)key, 0);
            }
        }

        private static void KeyUp()
        {
            KeyUp(lastKey);
        }

        public static async Task PressKey(ConsoleKey key)
        {
            KeyDown(key);
            await Task.Delay(50 + random.Next(0, 75));
            KeyUp(key);
        }

        public static void KeyUp(ConsoleKey key)
        {
            var wowProcess = Get();
            if (wowProcess != null)
            {
                PostMessage(wowProcess.MainWindowHandle, WM_KEYUP, (int)key, 0);
            }
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetCursorPos(int x, int y);

        public static async Task RightClickMouse(ILog logger, System.Drawing.Point position)
        {
            //RightClickMouse_Original(logger, position);
            await RightClickMouse_LiamCooper(logger, position);
        }

        public static async Task RightClickMouse_Original(ILog logger, System.Drawing.Point position)
        {
            var activeProcess = GetActiveProcess();
            var wowProcess = WowProcess.Get();
            if (wowProcess != null)
            {
                var oldPosition = System.Windows.Forms.Cursor.Position;

                for (int i = 20; i > 0; i--)
                {
                    SetCursorPos(position.X + i, position.Y + i);
                    await Task.Delay(1);
                }
                await Task.Delay(1000);

                PostMessage(wowProcess.MainWindowHandle, Keys.WM_RBUTTONDOWN, Keys.VK_RMB, 0);
                await Task.Delay(30 + random.Next(0, 47));
                PostMessage(wowProcess.MainWindowHandle, Keys.WM_RBUTTONUP, Keys.VK_RMB, 0);

                await RefocusOnOldScreen(logger, activeProcess, wowProcess, oldPosition);
            }
        }

        public static async Task RightClickMouse()
        {
            var activeProcess = GetActiveProcess();
            var wowProcess = WowProcess.Get();
            if (wowProcess != null)
            {
                var oldPosition = System.Windows.Forms.Cursor.Position;
                PostMessage(wowProcess.MainWindowHandle, Keys.WM_RBUTTONDOWN, Keys.VK_RMB, 0);

                await Task.Delay(30 + random.Next(0, 47));
                PostMessage(wowProcess.MainWindowHandle, Keys.WM_RBUTTONUP, Keys.VK_RMB, 0);
            }
        }

        public static async Task LeftClickMouse()
        {
            var activeProcess = GetActiveProcess();
            var wowProcess = WowProcess.Get();
            if (wowProcess != null)
            {
                var oldPosition = System.Windows.Forms.Cursor.Position;
                PostMessage(wowProcess.MainWindowHandle, Keys.WM_LBUTTONDOWN, Keys.VK_RMB, 0);
                await Task.Delay(30 + random.Next(0, 47));
                PostMessage(wowProcess.MainWindowHandle, Keys.WM_LBUTTONUP, Keys.VK_RMB, 0);
            }
        }

        public static async Task RightClickMouse_LiamCooper(ILog logger, System.Drawing.Point position)
        {
            var activeProcess = GetActiveProcess();
            var wowProcess = WowProcess.Get();
            if (wowProcess != null)
            {
                mouse_event((int)MouseEventFlags.RightUp, position.X, position.Y, 0, 0);
                var oldPosition = System.Windows.Forms.Cursor.Position;

                await Task.Delay(200);
                System.Windows.Forms.Cursor.Position = position;
                await Task.Delay(LootDelay);
                mouse_event((int)MouseEventFlags.RightDown, position.X, position.Y, 0, 0);
                await Task.Delay(30 + random.Next(0, 47));
                mouse_event((int)MouseEventFlags.RightUp, position.X, position.Y, 0, 0);
                await RefocusOnOldScreen(logger, activeProcess, wowProcess, oldPosition);
                await Task.Delay(LootDelay / 2);
            }
        }

        private static async Task RefocusOnOldScreen(ILog logger, Process activeProcess, Process wowProcess, System.Drawing.Point oldPosition)
        {
            try
            {
                if (activeProcess.MainWindowTitle != wowProcess.MainWindowTitle)
                {
                    // get focus back on this screen
                    PostMessage(activeProcess.MainWindowHandle, Keys.WM_RBUTTONDOWN, Keys.VK_RMB, 0);
                    await Task.Delay(30);
                    PostMessage(activeProcess.MainWindowHandle, Keys.WM_RBUTTONUP, Keys.VK_RMB, 0);

                    KeyDown(ConsoleKey.Escape);
                    await Task.Delay(30);
                    KeyUp(ConsoleKey.Escape);

                    System.Windows.Forms.Cursor.Position = oldPosition;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }
        }

        [DllImport("user32.dll")]
        private static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

        [Flags]
        public enum MouseEventFlags
        {
            LeftDown = 0x00000002,
            LeftUp = 0x00000004,
            MiddleDown = 0x00000020,
            MiddleUp = 0x00000040,
            Move = 0x00000001,
            Absolute = 0x00008000,
            RightDown = 0x00000008,
            RightUp = 0x00000010
        }
    }
}