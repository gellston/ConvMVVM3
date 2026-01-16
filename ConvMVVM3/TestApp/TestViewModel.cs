using ConvMVVM3.Core.Mvvm;
using ConvMVVM3.Core.Mvvm.Attributes;
using System.Threading.Tasks;

namespace TestApp
{
    public partial class TestViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _name;

        [ObservableProperty]
        private int _age;

        [ObservableProperty]
        private bool _isBusy;

        // 안전한 이름들 - Execute 충돌 회피
        [RelayCommand]
        private void HandleAction()
        {
            // 액션 처리
        }
        
        [RelayCommand]
        private void ProcessData()
        {
            // 데이터 처리
        }
        
        [RelayCommand(CanExecute = nameof(CanExecuteAction))]
        private void ConditionalAction()
        {
            // 조건부 액션
        }
        
        private bool CanExecuteAction() => !_isBusy;

        // AsyncRelayCommand 테스트
        [AsyncRelayCommand]
        private async Task LoadDataAsync()
        {
            // 비동기 데이터 로딩
        }
        
        [AsyncRelayCommand]
        private async Task SaveDataAsync()
        {
            // 비동기 데이터 저장
        }
    }
}