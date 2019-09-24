namespace HideezMiddleware.Settings
{
    public interface IFileSerializer
    {
        bool Serialize<T>(string filePath, T serializedObject) where T : new();

        T Deserialize<T>(string filePath) where T : new();
    }
}
