using PlaylistSaver.ProgramData.Bases;
using PlaylistSaver.ProgramData.Stores;
using PlaylistSaver.Windows.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToastMessageService;

namespace PlaylistSaver.Windows.MainWindowViews.AboutApp
{
    public class AboutAppViewModel : ViewModelBase
    {
        public string AppVersion => GlobalItems.AppVersion;
        public RelayCommand CopyTextCommand { get; }

        public AboutAppViewModel()
        {
            CopyTextCommand = new RelayCommand(CopyText);
        }


        public void CopyText(object text)
        {
            System.Windows.Clipboard.SetText((string)text);
            ToastMessage.Display("Text copied!");
        }

    }
}
