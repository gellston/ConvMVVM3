using ConvMVVM3.Core.Mvvm;
using ConvMVVM3.Core.Mvvm.Attributes;
using System.Threading.Tasks;

namespace ConvMVVM3.Tests.SampleViewModels
{
    public partial class SampleViewModel : ConvMVVM3.Core.Mvvm.ObservableObject
    {
        [ObservableProperty]
        private string _name = "Default Name";

        [ObservableProperty]
        private int _age = 25;



        
        [ObservableProperty]
        [NotifyCanExecuteChangedFor("LoadDataAsync")]
        private bool _isLoading = false;

        // Basic RelayCommand
        [RelayCommand]
        private void Save()
        {
            // Save logic
            Name = "Saved at " + System.DateTime.Now.ToString("HH:mm:ss");
        }

        // AsyncRelayCommand examples
        
        [AsyncRelayCommand]
        private async Task LoadDataAsync(CancellationToken token)
        {
            IsLoading = true;
            try
            {
                await Task.Delay(1000); // Simulate data loading
                Name = "Data Loaded";
                Age = 30;
            }
            finally
            {
                IsLoading = false;
            }
        }

        [AsyncRelayCommand]
        private async Task RefreshAsync()
        {
            IsLoading = true;
            try
            {
                await Task.Delay(500); // Simulate refresh
                Name = "Refreshed";
            }
            finally
            {
                IsLoading = false;
            }
        }

        [AsyncRelayCommand]
        private async Task ClearDataAsync()
        {
            await Task.Delay(200);
            Name = "Cleared";
            Age = 0;
        }
    }
}