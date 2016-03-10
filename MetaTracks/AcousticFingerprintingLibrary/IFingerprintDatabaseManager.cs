namespace AcousticFingerprintingLibrary
{
    public interface IFingerprintDatabaseManager
    {
        void SendFullFileToDatabase();

        void CompareHashToDatabase();

        void CreateTestTable();

        void WaitUntilTableReady(string tableName);

        void RemoveTestTable();
    }
}