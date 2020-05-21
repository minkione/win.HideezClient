namespace HideezMiddleware.Workstation
{
    public interface IWorkstationIdProvider
    {
        void SaveWorkstationId(string id);
        string GetWorkstationId();
    }
}
