using Interceptor;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace InteractWithInputSource
{
    class Program
    {
        public const UInt32 WM_KEYDOWN = 0x0100;
        public const UInt32 WM_KEYUP = 0x0101;
        public const UInt32 WM_RBUTTONDOWN = 0x204;
        public const UInt32 WM_RBUTTONUP = 0x205;
        const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        const int MOUSEEVENTF_RIGHTUP = 0x10;
        public const int VK_RMB = 0x02;

        public enum keyState
        {
            KEYDOWN = 0,
            EXTENDEDKEY = 1,
            KEYUP = 2
        };

        [DllImport("user32.dll")]
        public static extern bool PostMessage(IntPtr hWnd, UInt32 Msg, int wParam, int lParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);

        [DllImport("user32.dll")]
        private static extern bool keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        static async Task Main(string[] args)
        {
            var process = Get();

            while(true)
            {
                var input = new Input();
                input.Load();

                await Task.Delay(4000);


                //input.MoveMouseTo(5, 5);  // Please note this doesn't use the driver to move the mouse; it uses System.Windows.Forms.Cursor.Position
                //input.MoveMouseBy(25, 25); //  Same as above ^
                //input.SendLeftClick();


                //Console.WriteLine("Press X using keybd_event");
                //keybd_event((byte)ConsoleKey.X.GetHashCode(), 0, (uint)keyState.KEYDOWN, (UIntPtr)0);
                //await Task.Delay(1000);
                //keybd_event((byte)ConsoleKey.X.GetHashCode(), 0, (uint)keyState.KEYUP, (UIntPtr)0);
                //await Task.Delay(4000);

                Console.WriteLine("Sedning input");
                input.KeyPressDelay = 1; // See below for explanation; not necessary in non-game apps
                input.SendKeys(Keys.O);  // Presses the ENTER key down and then up (this constitutes a key press)
                input.Unload();

                // Or you can do the same thing above using these two lines of code
                input.SendKey(Keys.Enter, KeyState.Down);
                Thread.Sleep(1); // For use in games, be sure to sleep the thread so the game can capture all events. A lagging game cannot process input quickly, and you so you may have to adjust this to as much as 40 millisecond delay. Outside of a game, a delay of even 0 milliseconds can work (instant key presses).
                input.SendKey(Keys.Enter, KeyState.Up);

                input.SendText("hello, I am typing!");

                /* All these following characters / numbers / symbols work */
                input.SendText("abcdefghijklmnopqrstuvwxyz");
                input.SendText("1234567890");
                input.SendText("!@#$%^&*()");
                input.SendText("[]\\;',./");
                input.SendText("{}|:\"<>?");


                // And finally
                input.Unload();
                Console.WriteLine("Sent input");
                await Task.Delay(40000);



                Console.WriteLine("Position cursor at 200,200 using System.Windows.Forms.Cursor.Position");
                System.Windows.Forms.Cursor.Position = new System.Drawing.Point(200, 200);
                await Task.Delay(4000);

                Console.WriteLine("Position cursor at 300,300 using SetCursorPos");
                SetCursorPos(300, 300);
                await Task.Delay(4000);

                Console.WriteLine("right click using PostMessage");
                PostMessage(process.MainWindowHandle, WM_RBUTTONDOWN, VK_RMB, 0);
                await Task.Delay(1000);
                PostMessage(process.MainWindowHandle, WM_RBUTTONUP, VK_RMB, 0);
                await Task.Delay(4000);

                Console.WriteLine("right click using mouse_event");
                mouse_event(MOUSEEVENTF_RIGHTDOWN | MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);
                await Task.Delay(4000);

                Console.WriteLine("Press X using keybd_event");
                keybd_event((byte)ConsoleKey.X.GetHashCode(), 0, (uint)keyState.KEYDOWN, (UIntPtr)0);
                await Task.Delay(1000);
                keybd_event((byte)ConsoleKey.X.GetHashCode(), 0, (uint)keyState.KEYUP, (UIntPtr)0);
                await Task.Delay(4000);

                Console.WriteLine("Press X using PostMessage");
                PostMessage(process.MainWindowHandle, WM_KEYDOWN, (int)ConsoleKey.X, 0);
                await Task.Delay(1000);
                PostMessage(process.MainWindowHandle, WM_KEYUP, (int)ConsoleKey.X, 0);
                await Task.Delay(4000);
            }
        }

        public static Process Get(string name = "")
        {
            var names = string.IsNullOrEmpty(name) ? new List<string> { "InputSource" } : new List<string> { name };
            var processList = Process.GetProcesses();
            foreach (var p in processList)
            {
                if (names.Contains(p.ProcessName)) { return p; }
            }
            return null;
        }
    }
}
