namespace BackOfficeBlazor.Admin.Services.Interfaces
{
    public interface ISecretEncryptionService
    {
        string Encrypt(string plainText);
        string Decrypt(string cipherText);
    }
}
