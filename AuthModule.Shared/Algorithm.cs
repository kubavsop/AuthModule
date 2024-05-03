using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace AuthModule.Shared;

public static class Algorithm
{
    public static string SHA256Hash(string input)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] inputBytes = Convert.FromBase64String(input);

            byte[] hashBytes = sha256.ComputeHash(inputBytes);

            return Convert.ToBase64String(hashBytes);
        }
    }

    public static string SHA256HashUTF8(string input)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);

            byte[] hashBytes = sha256.ComputeHash(inputBytes);

            return Convert.ToBase64String(hashBytes);
        }
    }

    public static string RSAEncrypt(string dataToEncryptString, string publicKeyString, bool DoOAEPPadding)
    {
        try
        {
            var dataToEncrypt = Convert.FromBase64String(dataToEncryptString);
            var publicKey = Convert.FromBase64String(publicKeyString);

            byte[] encryptedData;
            using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider())
            {
                RSA.ImportRSAPublicKey(publicKey, out int bytesRead);

                encryptedData = RSA.Encrypt(dataToEncrypt, DoOAEPPadding);
            }

            return Convert.ToBase64String(encryptedData);
        }

        catch (CryptographicException e)
        {
            Console.WriteLine(e.Message);

            return null;
        }
    }

    public static string RSADecrypt(string dataToEncryptString, string privateKeyString, bool DoOAEPPadding)
    {
        try
        {
            byte[] decryptedData;

            var dataToEncrypt = Convert.FromBase64String(dataToEncryptString);
            var privateKey = Convert.FromBase64String(privateKeyString);


            using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider())
            {
                RSA.ImportRSAPrivateKey(privateKey, out int bytesRead);

                decryptedData = RSA.Decrypt(dataToEncrypt, DoOAEPPadding);
            }

            return Convert.ToBase64String(decryptedData);
        }

        catch (CryptographicException e)
        {
            Console.WriteLine(e.ToString());

            return null;
        }
    }

    public static string AesEncrypt(string privateKey, string? passphrase)
    {
        if (passphrase is null)
        {
            return privateKey;
        }

        using (Aes aesAlg = Aes.Create())
        {
            byte[] salt = GenerateRandomSalt();

            Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(passphrase, salt, 10000, HashAlgorithmName.SHA256);
            aesAlg.Key = pdb.GetBytes(32);
            aesAlg.IV = pdb.GetBytes(16);

            using (MemoryStream msEncrypt = new MemoryStream())
            {
                msEncrypt.Write(salt, 0, salt.Length);

                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt,
                           aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV), CryptoStreamMode.Write))
                {
                    using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                    {
                        swEncrypt.Write(privateKey);
                    }
                }

                byte[] encrypted = msEncrypt.ToArray();

                string encryptedPrivateKey = Convert.ToBase64String(encrypted);

                return encryptedPrivateKey;
            }
        }
    }

    public static bool AesDecrypt(string encryptedPrivateKey, string? passphrase, out string decryptedPrivateKey)
    {
        if (passphrase is null)
        {
            decryptedPrivateKey = encryptedPrivateKey;
            return true;
        }

        try
        {
            byte[] encryptedPrivateKeyBytes = Convert.FromBase64String(encryptedPrivateKey);

            using (Aes aesAlg = Aes.Create())
            {
                byte[] salt = new byte[16];
                Array.Copy(encryptedPrivateKeyBytes, 0, salt, 0, 16);

                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(passphrase, salt, 10000, HashAlgorithmName.SHA256);
                aesAlg.Key = pdb.GetBytes(32);
                aesAlg.IV = pdb.GetBytes(16);

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                using (MemoryStream msDecrypt =
                       new MemoryStream(encryptedPrivateKeyBytes, 16, encryptedPrivateKeyBytes.Length - 16))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            decryptedPrivateKey = srDecrypt.ReadToEnd();

                            return true;
                        }
                    }
                }
            }
        }
        catch (CryptographicException)
        {
            decryptedPrivateKey = "";
            return false;
        }
    }

    private static byte[] GenerateRandomSalt()
    {
        byte[] salt = new byte[16];
        using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }

        return salt;
    }
}