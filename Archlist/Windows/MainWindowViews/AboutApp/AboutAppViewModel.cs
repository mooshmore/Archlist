using Archlist.ProgramData.Bases;
using Archlist.ProgramData.Stores;
using Archlist.Windows.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToastMessageService;

namespace Archlist.Windows.MainWindowViews.AboutApp
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
