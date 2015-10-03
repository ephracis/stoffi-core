using System;
using System.Linq;

namespace Stoffi.Core.Common
{
    /// <summary>
    /// A collection of helper methods for string related operations.
    /// </summary>
    public class StringHelper
    {
        /// <summary>
        /// Capitalize a string.
        /// </summary>
        /// <param name="str">The string to capitalize</param>
        public static string Capitalize(string str)
        {
            if (String.IsNullOrWhiteSpace(str))
                return "";
            str = str.ToLower();
            char[] a = str.ToCharArray();
            a[0] = Char.ToUpper(a[0]);
            return new String(a);
        }

        /// <summary>
        /// Capitalizes the words of a string.
        /// </summary>
        /// <remarks>
        /// Words with less than five letter are turned to lower case.
        /// </remarks>
        /// <param name="str">The string to titleize</param>
        /// <returns>A titleized string</returns>
        public static string Titleize(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
                return "";
            string[] words = str.Split(' ');
            for (int i = 0; i < words.Count(); i++)
            {
                if (words[i].Length < 5)
                    words[i] = words[i].ToLower();
                else
                    words[i] = Capitalize(words[i]);
            }
            return string.Join(" ", words);
        }

        /// <summary>
        /// Turn a string into CamelCase.
        /// </summary>
        /// <param name="str">The string to classify</param>
        /// <returns>A CamelCase string</returns>
        public static string Classify(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
                return "";
            return string.Join("",
                from word in str.Split(' ') select Capitalize(word));
        }
    }
}
