using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MsServices.DialogService
{
    public interface IDialogService
    {
        public void ShowDialog(string name, Action<string> callback);

        public void ShowDialog<ViewModel>(Action<string> callback);
    }

    public class DialogService : IDialogService
    {
        public void ShowDialog(string name, Action<string> callback)
        {
            var dialog = new DialogWindow();

            EventHandler closeEventHandler = null;
            closeEventHandler = (s, e) =>
            {
                callback(dialog.DialogResult.ToString());
                dialog.Closed -= closeEventHandler;
            };

            dialog.Closed += closeEventHandler;

            var type = Type.GetType($"MsServices.DialogService.Views.Base.BaseView");
            dialog.Content = Activator.CreateInstance(type);

            dialog.ShowDialog();
        }

        public void ShowDialog<ViewModel>(Action<string> callback)
        {
            throw new NotImplementedException();
        }
    }
}
