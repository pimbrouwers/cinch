using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Cinch
{
    public static class StringExtensions
    {

        ///// <summary>
        ///// Encodes a string to Base64
        ///// </summary>
        ///// <param name="str"></param>
        ///// <returns></returns>
        //public static string ToBase64(this string str)
        //{
        //    byte[] toEncodeAsBytes = ASCIIEncoding.ASCII.GetBytes(str);
        //    string encoded = Convert.ToBase64String(toEncodeAsBytes);

        //    return encoded;
        //}

        ///// <summary>
        ///// Decodes Base 64 string
        ///// </summary>
        ///// <param name="str"></param>
        ///// <param name="encodedString"></param>
        ///// <returns></returns>
        //public static string FromBase64(this string str)
        //{
        //    byte[] decodedBytes = Convert.FromBase64String(str);
        //    string s = Encoding.ASCII.GetString(decodedBytes);

        //    return s;
        //}

        ///// <summary>
        ///// Converts any string into a URL friendly version
        ///// </summary>
        ///// <param name="str"></param>
        ///// <returns></returns>
        public static string ToFriendlyString(this string str)
        {
            if (string.IsNullOrEmpty(str)) return "";

            str = str.ToLower();

            str = Regex.Replace(str, @"&\w+;", "");
            str = Regex.Replace(str, @"[^a-z0-9\-\s]", "", RegexOptions.IgnoreCase);
            str = str.Replace(" ", "-");
            str = Regex.Replace(str, @"-{2,}", "-");

            return str;
        }

        ///// <summary>
        ///// Used to strip tags (html/xml) from any text source
        ///// </summary>
        ///// <param name="source"></param>
        ///// <returns></returns>
        //public static string StripTags(this string source)
        //{
        //    char[] array = new char[source.Length];
        //    int arrayIndex = 0;
        //    bool inside = false;

        //    for (int i = 0; i < source.Length; i++)
        //    {
        //        char let = source[i];
        //        if (let == '<')
        //        {
        //            inside = true;
        //            continue;
        //        }
        //        if (let == '>')
        //        {
        //            inside = false;
        //            continue;
        //        }
        //        if (!inside)
        //        {
        //            array[arrayIndex] = let;
        //            arrayIndex++;
        //        }
        //    }
        //    return new string(array, 0, arrayIndex);
        //}

        ///// <summary>
        ///// Used to strip Slashes from any text source
        ///// </summary>
        ///// <param name="source"></param>
        ///// <returns></returns>
        //public static string StripSlashes(this string source)
        //{
        //    string newStr = source.Replace("\\", "");
        //    newStr = newStr.Replace("/", "");

        //    return newStr;
        //}

        ///// <summary>
        ///// Checks if string value is parseable as a number (any kind)
        ///// </summary>
        ///// <param name="s"></param>
        ///// <returns></returns>
        public static bool IsNumeric(this string s)
        {
            double result;
            return double.TryParse(s, out result);
        }

        ///// <summary>
        ///// Simple function to convert any string to title case
        ///// </summary>
        ///// <param name="text"></param>
        ///// <returns></returns>
        //public static string ConvertToProperCase(this string text)
        //{
        //    TextInfo myTI = new CultureInfo("en-US", false).TextInfo;
        //    return myTI.ToTitleCase(text.ToLower());
        //}

        ///// <summary>
        ///// Encodes ascii characters and returns string
        ///// </summary>
        ///// <param name="value"></param>
        ///// <returns></returns>
        //public static string EncodeNonAsciiCharacters(this string value)
        //{
        //    StringBuilder sb = new StringBuilder();
        //    foreach (char c in value)
        //    {
        //        if (c > 127)
        //        {
        //            // This character is too big for ASCII
        //            string encodedValue = "\\u" + ((int)c).ToString("x4");
        //            sb.Append(encodedValue);
        //        }
        //        else
        //        {
        //            sb.Append(c);
        //        }
        //    }
        //    return sb.ToString();
        //}

        ///// <summary>
        ///// Decodes ascii characters and returns decoded string
        ///// </summary>
        ///// <param name="value"></param>
        ///// <returns></returns>
        //public static string DecodeEncodedNonAsciiCharacters(this string value)
        //{
        //    return Regex.Replace(
        //        value,
        //        @"\\u(?<Value>[a-zA-Z0-9]{4})",
        //        m =>
        //        {
        //            return ((char)int.Parse(m.Groups["Value"].Value, NumberStyles.HexNumber)).ToString();
        //        });
        //}

        ///// <summary>
        ///// Cleans string of magic quotes etc.
        ///// </summary>
        ///// <param name="value"></param>
        ///// <returns></returns>
        //public static string CleanString(this string value)
        //{
        //    string clean = value;
        //    Dictionary<string, string> chars = new Dictionary<string, string>() 
        //    {
        //        {"’", "'"},
        //        {"“", "\""},
        //        {"”", "\""},
        //        {"–", "&ndash;"},
        //        {System.Environment.NewLine, " "},
        //        {"\n", " "},
        //        {"\r", " "},
        //    };

        //    foreach (KeyValuePair<string, string> ch in chars)
        //        clean = clean.Replace(ch.Key, ch.Value);

        //    return clean;
        //}

        /// <summary>
        /// Explodes a string based on splitChar and returns List<string>()
        /// </summary>
        /// <param name="stringToSplit"></param>
        /// <param name="splitChar"></param>
        /// <returns></returns>
        public static List<string> StringToStringList(this string stringToSplit, char splitChar = ',')
        {
            List<string> stringList = new List<string>();

            if (!String.IsNullOrWhiteSpace(stringToSplit))
            {
                string[] splitString = stringToSplit.Split(splitChar);


                foreach (string str in splitString)
                {
                    if (String.IsNullOrWhiteSpace(str))
                        continue;

                    stringList.Add(str.Trim());
                }
            }

            return stringList;
        }


        ///// <summary>
        ///// Parses string and returns a collection of sentences
        ///// </summary>
        ///// <param name="value"></param>
        ///// <returns></returns>
        //public static List<string> GetSentences(this string value)
        //{
        //    Regex rx = new Regex(@"(\S.+?[.!?])(?=\s+|$)");
        //    MatchCollection matches = rx.Matches(value);
        //    List<string> sentences = new List<string>();

        //    if (matches.Count > 0)
        //    {
        //        for (int i = 0; i < matches.Count; i++)
        //        {
        //            sentences.Add(matches[i].Value);
        //        }
        //    }

        //    return sentences;
        //}
    }
}