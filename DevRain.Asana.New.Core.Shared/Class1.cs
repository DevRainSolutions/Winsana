using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DevRain.Asana.New.Core.Shared
{
    public class TextHelper
    {
        public static string SanitizeHtml(string html, string acceptable)
        {
            //string acceptable = "script|link|title";
            string stringPattern = @"</?(?(?=" + acceptable +
                                   @")notag|[a-zA-Z0-9]+)(?:\s[a-zA-Z0-9\-]+=?(?:(["",']?).*?\1?)?)*\s*/?>";
            return Regex.Replace(html, stringPattern, "");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="s1">альбом/час/день</param>
        /// <param name="s2">альбома/часа/дня</param>
        /// <param name="s3">альбомов/часов/дней</param>
        /// <param name="nullable"></param>
        /// <param name="includeValue"></param>
        /// <param name="formatProvider"></param>
        /// <returns></returns>
        public static string GetText(decimal value, string s1, string s2, string s3, string nullable = "",
            bool includeValue = true, IFormatProvider formatProvider = null)
        {
            if (value == 0) return nullable;


            var s = "";
            if (value != 11 && value.ToString().ToCharArray().Last() == '1')
            {
                s = s1;
            }
            else if (!value.ToString().EndsWith("12") && !value.ToString().EndsWith("13") &&
                     !value.ToString().EndsWith("14") &&
                     (value.ToString().ToCharArray().Last() == '2' || value.ToString().ToCharArray().Last() == '3' ||
                      value.ToString().ToCharArray().Last() == '4'))
            {
                s = s2;
            }
            else
            {
                s = s3;
            }

            if (!includeValue)
            {
                return s;
            }

            return string.Format("{0} {1}", formatProvider != null ? value.ToString(formatProvider) : value.ToString(),
                s);
        }

        public static string SafeTruncate(string s, int value)
        {
            if (string.IsNullOrEmpty(s)) return "";

            if (s.Length <= value) return s;

            return s.Substring(0, value);
        }

        public static string SafeClearText(string s, int? value)
        {
            if (string.IsNullOrEmpty(s)) return "";

            var r = s.Replace("\n", " ").Replace("\r", " ");

            if (value.HasValue)
                return SafeTruncate(r, value.Value);
            return r;
        }


        public static string TimeDateAgo(DateTime dateTime)
        {
            DateTime now = DateTime.Now;

            if (dateTime.DayOfYear == now.DayOfYear)
            {
                var hours = now.Subtract(dateTime).Hours;

                if (hours <= 0)
                {
                    var minutes = now.Subtract(dateTime).Minutes;
                    if (minutes <= 0)
                    {
                        return "recently";
                    }
                    return string.Format("{0} ago", GetText(minutes, "min", "mins", "mins"));
                }

                return string.Format("{0} ago", GetText(hours, "hr", "hrs", "hrs"));
            }
            var days = now.Subtract(dateTime).Days + 1;
            return string.Format("{0} ago", GetText(days, "day", "days", "days"));
        }


        public static string PrepareDateTimeForDisplay(DateTime dateTime)
        {
            if (dateTime.Date == DateTime.Today)
            {
                return "Сегодня, {0}".FormatWith(dateTime.ToString("t"));
            }
            else if (dateTime.Date == DateTime.Today.AddDays(-1))
            {
                return "Вчера, {0}".FormatWith(dateTime.ToString("t"));
            }
            return dateTime.ToString();
        }


        public static string PrepareWebText(string text)
        {
            if (text == null) return null;

            var r = Replace(text);

            return Replace(r);
        }

        private static string Replace(string text)
        {
            return
                text.Replace("&quot;", "'")
                    .Replace("&amp;", "&")
                    .Replace("&#13;", "")
                    .Replace("\n ", "\n")
                    .Replace("&rdquo;", "'")
                    .Replace("&raquo;", "'")
                    .Replace("&laquo;", "'")
                    .Replace("&rsquo;", "'")
                    .Replace("&lsquo;", "'")
                    .Replace("&ldquo;", "'");
        }

        public static string ClearHtml(string text)
        {
            return Regex.Replace(text, @"<[^>]+>|&nbsp;", "").Trim();
        }
    }

    public static class StringEx
    {
        /// <summary>
        /// аналог String.Format()
        /// </summary>
        /// <param name="format"></param>
        /// <param name="formatParams"></param>
        /// <returns></returns>
        public static string FormatWith(this string format, params object[] formatParams)
        {
            return String.Format(format, formatParams);
        }


        /// <summary>
        /// обрезает строку до указанной длины, заменяя последние буквы троеточием
        /// </summary>
        /// <param name="str"></param>
        /// <param name="maxLen"></param>
        /// <returns></returns>
        public static string TrimByLength(this string str, int maxLen)
        {
            if (string.IsNullOrEmpty(str)) return "";

            if (str.Length >= maxLen & maxLen > 3)
            {
                return str.Substring(0, maxLen - 3) + "...";
            }
            return str;
        }

        public static string Capitalize(this string s)
        {
            if (String.IsNullOrEmpty(s)) return s;

            return char.ToUpper(s[0]) + s.Substring(1);
        }

        public static string Summary(this string text, int length = 50)
        {
            if (text.Length <= length) return text;

            return text.Substring(0, length) + "...";
        }

        public static object To<T>(this object value, T defaultValue)
        {
            Debug.WriteLine("public static object To<T>(this object value, T defaultValue)");
            if (value == null) return defaultValue;

            var v = value.ToString();

            if (string.IsNullOrEmpty(v)) return defaultValue;

            if (typeof(T) == typeof(int))
            {
                return int.Parse(v);
            }

            if (typeof(T) == typeof(long))
            {
                return long.Parse(v);
            }

            if (typeof(T) == typeof(bool))
            {
                return bool.Parse(v);
            }

            if (typeof(T) == typeof(string)) //BDF
            {
                return v;
            }

            if (typeof(T) == typeof(double))
            {
                return double.Parse(v);
            }

            throw new NotImplementedException("");
        }

        public static bool IsEmpty(this string s)
        {
            return string.IsNullOrEmpty(s);
        }
    }
}
