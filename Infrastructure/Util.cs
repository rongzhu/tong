using System;
using System.IO;
using System.Text;

namespace tongbro
{
    public static class Util
    {
        public static bool HasContent(this string str)
        {
            return !string.IsNullOrEmpty(str);
        }

		///<summary>Trim long string to given length. add "..." if string is cut. the given length cannot be less than 3.</summary>
		///<returns>returns trimmed string. its length will not exceed the given len.</returns>
		public static string LimitLen(string longString, int length)
		{
			if (longString != null && longString.Length > length)
				return longString.Substring(0, length - 3) + "...";
			else
				return longString;
		}

		public static StreamReader ToReader(this string str)
        {
            return new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(str)));
        }
    }
}
