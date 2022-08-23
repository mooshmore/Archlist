using PlaylistSaver.Windows.ViewModels;
using System;

namespace PlaylistSaver.ProgramData.Stores
{
    public class NavigationStore
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
