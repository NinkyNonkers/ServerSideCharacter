using System;
using System.Security.Cryptography;
using System.Text;

namespace ServerSideCharacter.Extensions
{
	public static class Md5Crypto
	{
		public static string ComputeMd5(string str)
		{
			MD5 md5 = new MD5CryptoServiceProvider();
			var buffer = md5.ComputeHash(Encoding.Default.GetBytes(str));
			return BitConverter.ToString(buffer).Replace("-", "");
		}
	}
}
