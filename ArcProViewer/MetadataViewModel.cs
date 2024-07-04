using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;

namespace ArcProViewer
{
    internal class MetadataViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<KeyValuePair<string, string>> Items { get; }

        public ICommand ValueDoubleClickCommand { get; }

        public MetadataViewModel()
        {
            Items = new ObservableCollection<KeyValuePair<string, string>>();
            ValueDoubleClickCommand = new RelayCommand(ExecuteValueDoubleClickCommand);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private void ExecuteValueDoubleClickCommand(object parameter)
        {
            if (parameter is KeyValuePair<string, string> item)
            {
                string url = ((KeyValuePair<string, string>) parameter).Value;
                if (url.ToLower().Trim().StartsWith("https://"))
                {
                    Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                }
            }
        }
    }
}
