using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Chat
{
    class Crypt
    {
       
       public  byte[] Key, IV;
        public Crypt(byte[] Key,byte[] IV) {
            
            this.Key = Key;
            this.IV = IV;

        }

        public string EncryptStringToBytesAes(Message mess)
        {
            string str = mess.GetString();

            byte[] Results;
           
          
           
            TripleDESCryptoServiceProvider TDESAlgorithm = new TripleDESCryptoServiceProvider();
            TDESAlgorithm.Key = Key;
            TDESAlgorithm.IV = IV;
           TDESAlgorithm.Mode = CipherMode.ECB;
            TDESAlgorithm.Padding = PaddingMode.PKCS7;
            byte[] DataToEncrypt = Encoding.Unicode.GetBytes(str);
            try
            {
                ICryptoTransform Encryptor = TDESAlgorithm.CreateEncryptor();
                Results = Encryptor.TransformFinalBlock(DataToEncrypt, 0, DataToEncrypt.Length);
            }
            finally
            {
                TDESAlgorithm.Clear();
                
            }
            return Convert.ToBase64String(Results);

        }
        public string DecryptStringFromBytesAes(Message mess)
        {
            string str = mess.GetString();
            byte[] Results;
            TripleDESCryptoServiceProvider TDESAlgorithm = new TripleDESCryptoServiceProvider();
            TDESAlgorithm.Key = Key;
            TDESAlgorithm.IV = IV;
            TDESAlgorithm.Mode = CipherMode.ECB;
            TDESAlgorithm.Padding = PaddingMode.PKCS7;
            byte[] DataToDecrypt = Convert.FromBase64String(str);
            try
            {
                ICryptoTransform Decryptor = TDESAlgorithm.CreateDecryptor();
                Results = Decryptor.TransformFinalBlock(DataToDecrypt, 0, DataToDecrypt.Length);
            }
            finally
            {
                TDESAlgorithm.Clear();
                
            }
            return Encoding.Unicode.GetString(Results);


        }
    }
}
