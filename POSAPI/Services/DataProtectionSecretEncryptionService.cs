using BackOfficeBlazor.Admin.Services.Interfaces;
using Microsoft.AspNetCore.DataProtection;

namespace POSAPI.Services
{
    public class DataProtectionSecretEncryptionService : ISecretEncryptionService
    {
        private readonly IDataProtector _protector;

        public DataProtectionSecretEncryptionService(IDataProtectionProvider provider)
        {
            _protector = provider.CreateProtector("PrinterConfig.SharePassword.v1");
        }

        public string Encrypt(string plainText)
        {
            if (string.IsNullOrWhiteSpace(plainText))
                return "";

            return _protector.Protect(plainText);
        }

        public string Decrypt(string cipherText)
        {
            if (string.IsNullOrWhiteSpace(cipherText))
                return "";

            return _protector.Unprotect(cipherText);
        }
    }
}
