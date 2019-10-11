using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

public class CRMCrypto
{
    private readonly static int SaltTextLengthThreshold;

    static CRMCrypto()
    {
        CRMCrypto.SaltTextLengthThreshold = 4;
    }

    public CRMCrypto()
    {
    }

    public static string Decrypt(string cipherText, string encryptionKey, string saltText)
    {
        byte[] numArray;
        byte[] numArray1 = Convert.FromBase64String(cipherText);
        numArray = (saltText.Length >= CRMCrypto.SaltTextLengthThreshold ? Encoding.Unicode.GetBytes(saltText) : Encoding.Unicode.GetBytes(string.Concat(Enumerable.Repeat<string>(saltText, (4 + saltText.Length - 1) / saltText.Length))));
        using (Aes bytes = Aes.Create())
        {
            Rfc2898DeriveBytes rfc2898DeriveByte = new Rfc2898DeriveBytes(encryptionKey, numArray);
            bytes.Key = rfc2898DeriveByte.GetBytes(32);
            bytes.IV = rfc2898DeriveByte.GetBytes(16);
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (CryptoStream cryptoStream = new CryptoStream(memoryStream, bytes.CreateDecryptor(), CryptoStreamMode.Write))
                {
                    cryptoStream.Write(numArray1, 0, (int)numArray1.Length);
                    cryptoStream.Close();
                }
                cipherText = Encoding.Unicode.GetString(memoryStream.ToArray());
            }
        }
        return cipherText;
    }

    public static string Encrypt(string clearText, string encryptionKey, string saltText)
    {
        byte[] numArray;
        byte[] bytes = Encoding.Unicode.GetBytes(clearText);
        numArray = (saltText.Length >= CRMCrypto.SaltTextLengthThreshold ? Encoding.Unicode.GetBytes(saltText) : Encoding.Unicode.GetBytes(string.Concat(Enumerable.Repeat<string>(saltText, (4 + saltText.Length - 1) / saltText.Length))));
        using (Aes ae = Aes.Create())
        {
            Rfc2898DeriveBytes rfc2898DeriveByte = new Rfc2898DeriveBytes(encryptionKey, numArray);
            ae.Key = rfc2898DeriveByte.GetBytes(32);
            ae.IV = rfc2898DeriveByte.GetBytes(16);
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (CryptoStream cryptoStream = new CryptoStream(memoryStream, ae.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cryptoStream.Write(bytes, 0, (int)bytes.Length);
                    cryptoStream.Close();
                }
                clearText = Convert.ToBase64String(memoryStream.ToArray());
            }
        }
        return clearText;
    }
}