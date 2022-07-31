using PlaylistSaver.ProgramData.Stores;
using PlaylistSaver.Windows.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlaylistSaver.ProgramData.Bases
{
    class ParameterNavigationService<Parameter, VievModel> where VievModel : ViewModelBase
    {
        private readonly NavigationStore _navigationStore;
        private readonly Func<Parameter, VievModel>_createViewModel;

        public void Navigate(Parameter parameter)
        {
            _navigationStore.CurrentViewModel = _createViewModel(parameter);
        }
    }
}
