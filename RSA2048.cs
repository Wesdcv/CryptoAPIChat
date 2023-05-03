using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace RSA2048Sharp
{
    public class RSA2048
    {
        private static string file = String.Format(@"{0}\keys.txt", Environment.CurrentDirectory);
        public static void GenerateKeys()
        {
            using (RSACryptoServiceProvider RSA2048 = new RSACryptoServiceProvider(384))
            {
                RSA2048.PersistKeyInCsp = false;
                string PublicKey = RSA2048.ToXmlString(false);
                string PrivateKey = RSA2048.ToXmlString(true);

                string[] Keys = { PublicKey, PrivateKey };

                File.WriteAllText(file, string.Empty);
                using (StreamWriter writer = new StreamWriter(file))
                {
                    foreach (string key in Keys)
                    {
                        writer.WriteLine(key);
                    }
                }
            }
        }
        public static string[] GetKeys ()
        {
            string[] Keys = File.ReadAllLines(file);
            return Keys;
        }

        public static string Encrypt(string PublicKey, string plain)
        {
            using (RSACryptoServiceProvider RSA2048 = new RSACryptoServiceProvider(384))
            {
                RSA2048.PersistKeyInCsp = false;
                RSA2048.FromXmlString(PublicKey);
                return Convert.ToBase64String(RSA2048.Encrypt(Encoding.UTF8.GetBytes(plain), false));
                //return Encoding.UTF8.GetString(RSA2048.Encrypt(Encoding.UTF8.GetBytes(plain), false));
            }
        }

        public static string Decrypt(string PrivateKey, string cipher)
        {
            using (RSACryptoServiceProvider RSA2048 = new RSACryptoServiceProvider(384))
            {
                RSA2048.PersistKeyInCsp = false;
                RSA2048.FromXmlString(PrivateKey);
                return Encoding.UTF8.GetString(RSA2048.Decrypt(Convert.FromBase64String(cipher), false));
                //return Encoding.UTF8.GetString(RSA2048.Decrypt(Encoding.UTF8.GetBytes(cipher), false));
            }
        }
    }
     
}
