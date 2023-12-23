using System;
using UnityEngine;

namespace Pancake
{
    public static partial class C
    {
        public static string Format(this string fmt, params object[] args) => string.Format(System.Globalization.CultureInfo.InvariantCulture.NumberFormat, fmt, args);

        /// <summary>
        /// Call action and remove reference
        /// </summary>
        /// <param name="action"></param>
        public static void CallActionClean(ref Action action)
        {
            if (action == null) return;
            var a = action;
            a();
            action = null;
        }

        /// <summary>
        /// copy <paramref name="data"/> parameter data in to clipboard
        /// </summary>
        /// <param name="data">string</param>
        public static void CopyToClipboard(this string data)
        {
            var textEditor = new TextEditor {text = data};
            textEditor.SelectAll();
            textEditor.Copy();
        }

        /// <summary>
        /// formatting Big Numbers: The “aa” Notation
        ///
        /// number                alphabet
        /// 1                        1
        /// 1000                     1K
        /// 1000000                  1M
        /// 1000000000               1B
        /// 1000000000000            1T
        /// 1000000000000000         1AA
        ///
        /// source  : https://gram.gs/gramlog/formatting-big-numbers-aa-notation/
        /// </summary>
        /// <param name="number">BigInteger</param>
        /// <returns></returns>
        public static string ToAlphabet(this System.Numerics.BigInteger number)
        {
            var str = number.ToString();
            int len = str.Length;
            if (number.Sign < 0 && len <= 4 || number.Sign > 0 && len <= 3) return str;
            var stringBuilder = new System.Text.StringBuilder();
            var index = 0;
            if (number.Sign < 0)
            {
                stringBuilder.Append('-');
                len--;
                index = 1;
            }

            //{0, ""}, {1, "K"}, {2, "M"}, {3, "B"}, {4, "T"}
            int intPart = len % 3;
            if (intPart == 0) intPart = 3;
            intPart += index;
            intPart += 2; // for floating point
            if (intPart > len) intPart = len;

            var tempString = stringBuilder.ToString();

            stringBuilder.Clear();
            for (int i = index; i < intPart; i++)
            {
                stringBuilder.Append(str[i]);
            }

            double floating = double.Parse(stringBuilder.ToString());
            floating /= 100;
            stringBuilder.Clear();
            stringBuilder.Append(tempString).Append(floating);

            if (len > 15)
            {
                var n = (len - 16) / 3;
                var firstChar = (char) (65 + n / 26);
                var secondChar = (char) (65 + n % 26);
                stringBuilder.Append(firstChar);
                stringBuilder.Append(secondChar);
            }
            else if (len > 12) stringBuilder.Append('T');
            else if (len > 9) stringBuilder.Append('B');
            else if (len > 6) stringBuilder.Append('M');
            else if (len > 3) stringBuilder.Append('K');

            return stringBuilder.ToString();
        }

        /// <summary>
        /// formatting Big Numbers: The “aa” Notation
        ///
        /// number                alphabet
        /// 1                        1
        /// 1000                     1K
        /// 1000000                  1M
        /// 1000000000               1B
        /// 1000000000000            1T
        /// 1000000000000000         1AA
        ///
        /// </summary>
        /// <param name="value">string number</param>
        /// <returns></returns>
        public static string ToAlphabet(this string value)
        {
            value = value.Split('.')[0];
            int len = value.Length;
            var stringBuilder = new System.Text.StringBuilder();
            var index = 0;
            var num = 3;
            if (value[0] == '-')
            {
                stringBuilder.Append('-');
                len--;
                index = 1;
                num = 4;
            }

            if (len <= num) return value; // return here if not converted to alphabet

            //{0, ""}, {1, "K"}, {2, "M"}, {3, "B"}, {4, "T"}
            int intPart = len % 3;
            if (intPart == 0) intPart = 3;
            intPart += index;
            intPart += 2; // for floating point
            if (intPart > len) intPart = len;

            var tempString = stringBuilder.ToString();

            stringBuilder.Clear();
            for (int i = index; i < intPart; i++)
            {
                stringBuilder.Append(value[i]);
            }

            double floating = double.Parse(stringBuilder.ToString());
            floating /= 100;
            stringBuilder.Clear();
            stringBuilder.Append(tempString).Append(floating);

            if (len > 15)
            {
                var n = (len - 16) / 3;
                var firstChar = (char) (65 + n / 26);
                var secondChar = (char) (65 + n % 26);
                stringBuilder.Append(firstChar);
                stringBuilder.Append(secondChar);
            }
            else if (len > 12) stringBuilder.Append('T');
            else if (len > 9) stringBuilder.Append('B');
            else if (len > 6) stringBuilder.Append('M');
            else stringBuilder.Append('K');

            return stringBuilder.ToString();
        }

        /// <summary>
        /// swap value of <paramref name="keyA"/> and <paramref name="keyB"/>
        /// </summary>
        /// <param name="keyA"></param>
        /// <param name="keyB"></param>
        public static void SwapPlayerPrefs<T>(string keyA, string keyB)
        {
            switch (typeof(T))
            {
                // ReSharper disable once ConvertTypeCheckPatternToNullCheck
                case Type intType when intType == typeof(int):
                    int tempInt = PlayerPrefs.GetInt(keyA);
                    PlayerPrefs.SetInt(keyA, PlayerPrefs.GetInt(keyB));
                    PlayerPrefs.SetInt(keyB, tempInt);
                    break;
                // ReSharper disable once ConvertTypeCheckPatternToNullCheck
                case Type stringType when stringType == typeof(string):
                    string tempString = PlayerPrefs.GetString(keyA);
                    PlayerPrefs.SetString(keyA, PlayerPrefs.GetString(keyB));
                    PlayerPrefs.SetString(keyB, tempString);
                    break;
                // ReSharper disable once ConvertTypeCheckPatternToNullCheck
                case Type floatType when floatType == typeof(float):
                    float tempFloat = PlayerPrefs.GetFloat(keyA);
                    PlayerPrefs.SetFloat(keyA, PlayerPrefs.GetFloat(keyB));
                    PlayerPrefs.SetFloat(keyB, tempFloat);
                    break;
                default: throw new NotSupportedException($"{nameof(T)} not supported!");
            }
        }

        /// <summary>
        /// Attach a DelayHandle on to the behaviour. If the behaviour is destroyed before the DelayHandle is completed,
        /// e.g. through a scene change, the DelayHandle callback will not execute.
        /// </summary>
        /// <param name="target">The behaviour to attach this DelayHandle to.</param>
        /// <param name="duration">The duration to wait before the DelayHandle fires.</param>
        /// <param name="onComplete">The action to run when the DelayHandle elapses.</param>
        /// <param name="onUpdate">A function to call each tick of the DelayHandle. Takes the number of seconds elapsed since
        /// the start of the current cycle.</param>
        /// <param name="isLooped">Whether the DelayHandle should restart after executing.</param>
        /// <param name="useRealTime">Whether the DelayHandle uses real-time(not affected by slow-mo or pausing) or
        /// game-time(affected by time scale changes).</param>
        public static DelayHandle Delay(
            this MonoBehaviour target,
            float duration,
            Action onComplete,
            Action<float> onUpdate = null,
            bool isLooped = false,
            bool useRealTime = false)
        {
            return App.Delay(
                target,
                duration,
                onComplete,
                onUpdate,
                isLooped,
                useRealTime);
        }

        /// <summary>
        /// 
        /// </summary>
        public static void GotoStore()
        {
            string url = Application.platform switch
            {
                RuntimePlatform.IPhonePlayer => $"https://apps.apple.com/us/app/{HeartSettings.AppstoreAppId}",
                _ => "market://details?id=" + Application.identifier
            };

            Application.OpenURL(url);
        }
    }
}