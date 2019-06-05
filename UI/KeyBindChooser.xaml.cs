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

namespace FishingFun
{
    /// <summary>
    /// Interaction logic for KeyBindChooser.xaml
    /// </summary>
    public partial class KeyBindChooser : UserControl
    {
        public ConsoleKey CastKey { get; set; } = ConsoleKey.D4;

        public UIElement FocusTarget { get; set; }

        public KeyBindChooser()
        {
            InitializeComponent();
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
                    this.CastKey = ck;
                    KeyBind.Text = GetCastKeyText(this.CastKey);

                    FocusTarget?.Focus();

                    return;
                }
            }
            KeyBind.Text = "";
        }

        private void KeyBind_LostFocus(object sender, RoutedEventArgs e)
        {
            ProcessKeybindText(KeyBind.Text);
            if (string.IsNullOrEmpty(KeyBind.Text))
            {
                KeyBind.Text = GetCastKeyText(this.CastKey);
            }
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
