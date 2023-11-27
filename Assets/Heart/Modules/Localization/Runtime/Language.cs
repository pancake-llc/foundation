using System;
using Pancake.Linq;
using UnityEngine;

namespace Pancake.Localization
{
    [Serializable]
    public class Language : IEquatable<Language>
    {
        public static Language[] BuiltInLanguages
        {
            get { return new[] {Chinese, English, French, German, Italian, Japanese, Korean, Portuguese, Russian, Spanish, Vietnamese, Unknown}; }
        }

        public static Language Chinese => new(SystemLanguage.Chinese.ToString(), "zh");
        public static Language English => new(SystemLanguage.English.ToString(), "en");
        public static Language French => new(SystemLanguage.French.ToString(), "fr");
        public static Language German => new(SystemLanguage.German.ToString(), "de");
        public static Language Italian => new(SystemLanguage.Italian.ToString(), "it");
        public static Language Japanese => new(SystemLanguage.Japanese.ToString(), "ja");
        public static Language Korean => new(SystemLanguage.Korean.ToString(), "ko");
        public static Language Portuguese => new(SystemLanguage.Portuguese.ToString(), "pt");
        public static Language Russian => new(SystemLanguage.Russian.ToString(), "ru");
        public static Language Spanish => new(SystemLanguage.Spanish.ToString(), "es");
        public static Language Vietnamese => new(SystemLanguage.Vietnamese.ToString(), "vi");
        public static Language Unknown => new(SystemLanguage.Unknown.ToString(), "");

        [SerializeField] private string name;
        [SerializeField] private string code;
        [SerializeField] private bool custom;

        public string Name => name;
        public string Code => code;
        public bool Custom => custom;

        public Language(string name, string code, bool custom = false)
        {
            this.name = name ?? "";
            this.code = code ?? "";
            this.custom = custom;
        }

        public bool Equals(Language other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Code == other.Code;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Language) obj);
        }

        public override int GetHashCode() { return Code.GetHashCode(); }

        public static bool operator ==(Language left, Language right) { return Equals(left, right); }

        public static bool operator !=(Language left, Language right) { return !Equals(left, right); }

        public override string ToString() { return Name; }

        public static implicit operator Language(SystemLanguage systemLanguage)
        {
            int index = Array.FindIndex(BuiltInLanguages, x => x.name == systemLanguage.ToString());
            return index >= 0 ? BuiltInLanguages[index] : Unknown;
        }

        public static explicit operator SystemLanguage(Language language)
        {
            if (language.custom) return SystemLanguage.Unknown;

            var systemLanguages = (SystemLanguage[]) Enum.GetValues(typeof(SystemLanguage));
            int index = Array.FindIndex(systemLanguages, x => x.ToString() == language.Name);
            return index >= 0 ? systemLanguages[index] : SystemLanguage.Unknown;
        }
    }
}