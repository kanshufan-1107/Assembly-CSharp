using System;
using System.Security.Cryptography;
using System.Text;

public static class Crypto
{
	public static class SHA1
	{
		public static string Calc(byte[] bytes, int start, int count)
		{
			byte[] array = System.Security.Cryptography.SHA1.Create().ComputeHash(bytes, start, count);
			StringBuilder sb = new StringBuilder();
			byte[] array2 = array;
			foreach (byte b in array2)
			{
				sb.Append(b.ToString("x2"));
			}
			return sb.ToString();
		}

		public static string Calc(byte[] bytes)
		{
			return Calc(bytes, 0, bytes.Length);
		}

		public static string Calc(string message)
		{
			byte[] bytes = new byte[message.Length * 2];
			Buffer.BlockCopy(message.ToCharArray(), 0, bytes, 0, bytes.Length);
			return Calc(bytes);
		}
	}
}
