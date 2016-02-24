namespace WebAPI.Models
{
    interface IFingerprintRepository
    
    {
        string GetFingerprintByHash(string hash);
    }
}
