using Archlist.Windows.ViewModels;
using System;

namespace Archlist.ProgramData.Stores
{
    public class NavigationSystem
    {
        public event Action CurrentVievModelChanged;

        private ViewModelBase _currentViewModel;

        public ViewModelBase CurrentViewModel
        {
            get => _currentViewModel;
            set
            {
                _currentViewModel = value;
                OnCurrentViewModelChanged();
            }
        }

        private void OnCurrentViewModelChanged()
        {
            CurrentVievModelChanged?.Invoke();
        }
    }
}
