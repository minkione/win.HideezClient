namespace HideezSafe.Modules.FileSerializer
{
    // Todo: Add check for Serializable classes
    interface IFileSerializer
    {
        bool Serialize<T>(string filePath, T serializedObject);

        T Deserialize<T>(string filePath);
    }
}
