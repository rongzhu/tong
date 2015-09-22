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

        public static StreamReader ToReader(this string str)
        {
            return new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(str)));
        }
    }
}
