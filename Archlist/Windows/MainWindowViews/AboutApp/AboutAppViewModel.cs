using Utilities.WPF.Bases;
using Archlist.ProgramData.Stores;
using MsServices.ToastMessageService;

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
