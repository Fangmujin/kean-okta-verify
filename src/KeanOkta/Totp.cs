using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace KeanOkta;

public static class Totp
{
	private const string B32 = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

	public static byte[] B32Decode(string input)
	{
		string text = (input ?? "").Trim().TrimEnd('=').ToUpperInvariant();
		List<byte> list = new List<byte>();
		int num = 0;
		int num2 = 0;
		string text2 = text;
		foreach (char value in text2)
		{
			int num3 = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567".IndexOf(value);
			if (num3 >= 0)
			{
				num2 = (num2 << 5) | num3;
				num += 5;
				if (num >= 8)
				{
					num -= 8;
					list.Add((byte)((uint)(num2 >> num) & 0xFFu));
					num2 &= (1 << num) - 1;
				}
			}
		}
		return list.ToArray();
	}

	public static string AtCounter(byte[] key, long counter, int digits = 6)
	{
		using HMACSHA1 hMACSHA = new HMACSHA1(key);
		byte[] array = new byte[8];
		long num = counter;
		for (int num2 = 7; num2 >= 0; num2--)
		{
			array[num2] = (byte)(num & 0xFF);
			num >>= 8;
		}
		byte[] array2 = hMACSHA.ComputeHash(array);
		int num3 = array2[19] & 0xF;
		int num4 = ((array2[num3] & 0x7F) << 24) | (array2[num3 + 1] << 16) | (array2[num3 + 2] << 8) | array2[num3 + 3];
		int num5 = ((digits != 8) ? 1000000 : 100000000);
		return (num4 % num5).ToString((digits != 8) ? "D6" : "D8");
	}

	public static long CounterAt(DateTimeOffset when)
	{
		return when.ToUnixTimeSeconds() / 30;
	}

	public static void Triple(string secretB32, DateTimeOffset when, out string prev, out string cur, out string next, out long counter, out byte[] key, out byte[] hash)
	{
		key = B32Decode(secretB32);
		counter = CounterAt(when);
		prev = AtCounter(key, counter - 1);
		cur = AtCounter(key, counter);
		next = AtCounter(key, counter + 1);
		using HMACSHA1 hMACSHA = new HMACSHA1(key);
		byte[] array = new byte[8];
		long num = counter;
		for (int num2 = 7; num2 >= 0; num2--)
		{
			array[num2] = (byte)(num & 0xFF);
			num >>= 8;
		}
		hash = hMACSHA.ComputeHash(array);
	}
}
