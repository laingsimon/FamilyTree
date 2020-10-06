using System;

namespace FamilyTree.Models
{
    public static class StringExtensions
    {
        public static string FromBase64(this string base64)
        {
            return System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(base64));
        }

        public static string ToBase64(this string value)
        {
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(value));
        }
    }
}