namespace HideezMiddleware.ConnectionModeProvider
{
    public interface IConnectionModeProvider
    {
        bool IsWinBleMode { get; }
        bool IsCsrMode { get; }

        GlobalConnectionMode GetConnectionMode();
    }
}
