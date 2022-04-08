
using System;
using System.ComponentModel;
using System.Reflection;
using UnityEngine;

namespace GoPlaySDK
{
    // string class extension classes for URL 
    static public class UrlExtension
    {
        public static string ToURL(this string s, bool useLiveServer = false)
        {
            return string.Concat(useLiveServer ? URLs.BASE_LIVE_SERVER : URLs.BASE_DEV_SERVER, s);
        }
    }


    // WWWForm class extension classes for input checking
    static public class WWWFormExtension
    {
        public static void AddFieldIfValid(this WWWForm form, string field, string value, bool lowerCase=false)
        {
            if (string.IsNullOrEmpty(value) == false)
                form.AddField(field, lowerCase ? value.ToLower() : value);
        }

        public static void AddFieldLowerCase(this WWWForm form, string field, string value)
        {
           form.AddField(field, value.ToLower());
        }
    }

}