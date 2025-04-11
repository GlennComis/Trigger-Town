public interface ISaveable
{
    string SaveKey { get; }
    object CaptureData();        // Return data to save
    void RestoreData(object data); // Apply loaded data
}