using System;
using System.Text;

namespace Puzzled
{
    public static class StringExtensions
    {
        private static StringBuilder _nicifyBuilder = new StringBuilder();

        public static string NicifyName(this string name)
        {
            _nicifyBuilder.Clear();
            _nicifyBuilder.Capacity = name.Length * 2;
            for (int i = 0; i < name.Length; i++)
            {
                if (char.IsUpper(name[i]))
                {
                    _nicifyBuilder.Append(' ');
                    _nicifyBuilder.Append(name[i]);
                } else
                {
                    if (i == 0)
                        _nicifyBuilder.Append(char.ToUpper(name[i]));
                    else
                        _nicifyBuilder.Append(name[i]);
                }
            }

            return _nicifyBuilder.ToString();
        }

        public static string GetNextName (this string name)
        {
            var lastSpace = name.LastIndexOf(' ');
            if (lastSpace == -1)
                return name + " 1";

            if (int.TryParse(name.Substring(lastSpace + 1), out var value))
                return name.Substring(0, lastSpace) + " " + (value + 1).ToString();

            return name + " 1";
        }
    }
}
