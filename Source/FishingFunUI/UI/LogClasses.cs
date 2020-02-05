using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;

#nullable enable
namespace FishingFun
{
    public class LogEntry : PropertyChangedBase
    {
        public DateTime DateTime { get; set; }

        public string Time
        {
            get { return DateTime.ToLongTimeString(); }
        }

        public int Index { get; set; }

        public string Message { get; set; } = string.Empty;
    }

    public class CollapsibleLogEntry : LogEntry
    {
        public List<LogEntry> Contents { get; set; } = new List<LogEntry>();
    }

    public class PropertyChangedBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                if (PropertyChanged != null)
                {
                    PropertyChangedEventHandler handler = PropertyChanged;
                    if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
                }
            }));
        }
    }
}