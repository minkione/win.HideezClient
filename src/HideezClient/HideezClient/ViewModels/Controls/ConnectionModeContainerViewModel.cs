using HideezClient.Mvvm;
using HideezMiddleware.ConnectionModeProvider;

namespace HideezClient.ViewModels.Controls
{
    internal sealed class ConnectionModeContainerViewModel: LocalizedObject
    {
        readonly bool _isCsrMode;
        readonly bool _isWinBleMode;

        public bool IsCsrMode => _isCsrMode; 

        public bool IsWinBleMode => _isWinBleMode;

        public ConnectionModeContainerViewModel(IConnectionModeProvider connectionModeProvider)
        {
            _isCsrMode = connectionModeProvider.IsCsrMode;
            _isWinBleMode = connectionModeProvider.IsWinBleMode;
        }
    }
}
