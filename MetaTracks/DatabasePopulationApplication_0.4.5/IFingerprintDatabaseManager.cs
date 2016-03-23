namespace DatabasePopulationApplication_0._4._5
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