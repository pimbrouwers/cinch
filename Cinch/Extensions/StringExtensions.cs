using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace CinchORM
{
    public static class StringExtensions
    {

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


    }
}