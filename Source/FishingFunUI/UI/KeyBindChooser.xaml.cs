using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace FishingFun
{
    public class Macro1KeyBindChooser : KeyBindChooser {
        public Macro1KeyBindChooser() : base()
        {
            StorageIndex = 1;
            ReadConfiguration();
        }
    }

    public class Macro2KeyBindChooser : KeyBindChooser
    {
        public Macro2KeyBindChooser() : base()
        {
            StorageIndex = 2;
            ReadConfiguration();
        }
    }

    public partial class KeyBindChooser : UserControl
    {
        private string[] KeybindTexts = new string[] { "4", "5", "6" };
        protected int StorageIndex { get; set; } = 0;

        public ConsoleKey CastKey { get; set; } = ConsoleKey.D4;

        private static string Filename = "keybind.txt";

        public EventHandler CastKeyChanged;

        public KeyBindChooser()
        {
            CastKeyChanged += (s, e) => { };

            InitializeComponent();
            ReadConfiguration();
        }

        protected void ReadConfiguration()
        {
            try
            {
                if (File.Exists(Filename))
                {
                    var fileContents = File.ReadAllText(Filename);
                    var keybindChunks = fileContents.Split(';');
                    if (keybindChunks.Length == 3)
                    {
                        KeybindTexts = keybindChunks;
                    }
                    CastKey = GetConsoleKey();
                    KeyBind.Text = GetCastKeyText(this.CastKey);
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                CastKey = ConsoleKey.D4;
                KeyBind.Text = GetCastKeyText(this.CastKey);
            }
        }

        private void WriteConfiguration()
        {
            KeybindTexts[StorageIndex] = ((int)CastKey).ToString();

            string output = "";
            for (int i = 0; i < KeybindTexts.Length; i++)
            {
                output += KeybindTexts[i];
                if (i < KeybindTexts.Length - 1)
                {
                    output += ";";
                }
            }
            File.WriteAllText(Filename, output);
        }

        private void CastKey_Focus(object sender, RoutedEventArgs e)
        {
            KeyBind.Text = "";
        }

        private void KeyBind_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            var key = e.Key.ToString();
            ProcessKeybindText(key);
        }

        private void ProcessKeybindText(string key)
        {
            if (!string.IsNullOrEmpty(key))
            {
                ConsoleKey ck;
                if (Enum.TryParse<ConsoleKey>(key, out ck))
                {
                    // Read first so we pick up changes from all 3 classes that share this file. Maybe a shared storage manager
                    // would be best, but this is quick and dirty to allow multiple keybinds.
                    ReadConfiguration();
                    this.CastKey = ck;
                    KeyBind.Text = GetCastKeyText(this.CastKey);
                    WriteConfiguration();
                    CastKeyChanged?.Invoke(this, null);
                    return;
                }
            }
            KeyBind.Text = "";
        }

        private ConsoleKey GetConsoleKey()
        {
            if (this.StorageIndex < 0 || this.StorageIndex >= this.KeybindTexts.Length)
            {
                return ConsoleKey.D4;
            }

            return (ConsoleKey)int.Parse(this.KeybindTexts[this.StorageIndex]);
        }

        private string GetCastKeyText(ConsoleKey ck)
        {
            string keyText = ck.ToString();
            if (keyText.Length == 1) { return keyText; }
            if (keyText.StartsWith("D") && keyText.Length == 2)
            {
                return keyText.Substring(1, 1);
            }
            return "?";
        }
    }
}