using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace CRMIntegration
{
    public class CRMCheckFormat
    {
        private readonly static int KEY_LENGTH_BITS;

        private readonly static string SAGE_CRM_KEY;

        static CRMCheckFormat()
        {
            CRMCheckFormat.KEY_LENGTH_BITS = 192;
            CRMCheckFormat.SAGE_CRM_KEY = "SVSVEPBCPBRROLRJDHBPUURPDBCCQPDLDORBOPJBPBJCBOJORDJLNRNPQJPLQQBODHRHERRRBRUBLHHPQHRPQRUJURQCHLRULRORXJQLBDCCPRCBBPNRPPRBHUXOR";
        }

        public CRMCheckFormat()
        {
        }

        public static string CheckFormat(string input)
        {
            bool flag;
            string str;
            byte[] bytes = null;
            byte[] numArray = null;
            if (input.IndexOf('&') != 0)
            {
                flag = true;
                bytes = Encoding.ASCII.GetBytes(input);
            }
            else
            {
                flag = false;
                numArray = CRMCheckFormat.CRMDecodeStr(input.Substring(1, input.Length - 1));
            }
            TripleDESCryptoServiceProvider tripleDESCryptoServiceProvider = new TripleDESCryptoServiceProvider()
            {
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7
            };
            PasswordDeriveBytes passwordDeriveByte = new PasswordDeriveBytes(Encoding.ASCII.GetBytes(CRMCheckFormat.SAGE_CRM_KEY), null);
            tripleDESCryptoServiceProvider.Key = passwordDeriveByte.CryptDeriveKey("TripleDES", "SHA1", CRMCheckFormat.KEY_LENGTH_BITS, new byte[8]);
            tripleDESCryptoServiceProvider.IV = new byte[8];
            if (!flag)
            {
                bytes = tripleDESCryptoServiceProvider.CreateDecryptor().TransformFinalBlock(numArray, 0, (int)numArray.Length);
                str = Encoding.UTF8.GetString(bytes);
            }
            else
            {
                numArray = tripleDESCryptoServiceProvider.CreateEncryptor().TransformFinalBlock(bytes, 0, (int)bytes.Length);
                str = string.Concat("&", CRMCheckFormat.CRMEncodeBytes(numArray));
            }
            tripleDESCryptoServiceProvider.Clear();
            return str;
        }

        private static byte[] CRMDecodeStr(string input)
        {
            List<byte> nums = new List<byte>();
            for (int i = 0; i < input.Length; i += 2)
            {
                if (input[i] >= 'A' && input[i] <= 'P' && input[i + 1] >= 'A' && input[i + 1] <= 'P')
                {
                    int num = input[i] - 65 << '\u0004';
                    num = num | input[i + 1] - 65;
                    nums.Add(Convert.ToByte(num));
                }
            }
            return nums.ToArray();
        }

        private static string CRMEncodeBytes(byte[] input)
        {
            string str = null;
            byte[] numArray = input;
            for (int i = 0; i < (int)numArray.Length; i++)
            {
                byte num = numArray[i];
                char chr = (char)((num >> 4) + 65);
                string str1 = chr.ToString();
                chr = (char)((num & 15) + 65);
                str = string.Concat(str, str1, chr.ToString());
            }
            return str;
        }
    }
}