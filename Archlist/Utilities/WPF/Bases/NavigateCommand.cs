using Archlist.ProgramData.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.WPF.Bases
{
    public class NavigateCommand : CommandBase
    {
        private readonly NavigationSystem _navigationStore;
        private readonly Func<ViewModelBase> _createViewModel;

        public NavigateCommand(NavigationSystem navigationStore, Func<ViewModelBase> createViewModel)
        {
            _navigationStore = navigationStore;
            _createViewModel = createViewModel;
        }

        public override void Execute(object parameter)
        {
            if (_createViewModel != null)
                _navigationStore.CurrentViewModel = _createViewModel();
            else
                _navigationStore.CurrentViewModel = null;
        }
    }
}
