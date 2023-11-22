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

        private static Language chinese;

        public static Language Chinese
        {
            get
            {
                if (chinese == null) chinese = new Language(SystemLanguage.Chinese.ToString(), "zh");
                return chinese;
            }
        }

        private static Language english;

        public static Language English
        {
            get
            {
                if (english == null) english = new Language(SystemLanguage.English.ToString(), "en");
                return english;
            }
        }

        private static Language french;

        public static Language French
        {
            get
            {
                if (french == null) french = new Language(SystemLanguage.French.ToString(), "fr");
                return french;
            }
        }

        private static Language german;

        public static Language German
        {
            get
            {
                if (german == null) german = new Language(SystemLanguage.German.ToString(), "de");
                return german;
            }
        }

        private static Language italian;

        public static Language Italian
        {
            get
            {
                if (italian == null) italian = new Language(SystemLanguage.Italian.ToString(), "it");
                return italian;
            }
        }

        private static Language japanese;

        public static Language Japanese
        {
            get
            {
                if (japanese == null) japanese = new Language(SystemLanguage.Japanese.ToString(), "ja");
                return japanese;
            }
        }

        private static Language korean;

        public static Language Korean
        {
            get
            {
                if (korean == null) korean = new Language(SystemLanguage.Korean.ToString(), "ko");
                return korean;
            }
        }

        private static Language portuguese;

        public static Language Portuguese
        {
            get
            {
                if (portuguese == null) portuguese = new Language(SystemLanguage.Portuguese.ToString(), "pt");
                return portuguese;
            }
        }

        private static Language russian;

        public static Language Russian
        {
            get
            {
                if (russian == null) russian = new Language(SystemLanguage.Russian.ToString(), "ru");
                return russian;
            }
        }

        private static Language spanish;

        public static Language Spanish
        {
            get
            {
                if (spanish == null) spanish = new Language(SystemLanguage.Spanish.ToString(), "es");
                return spanish;
            }
        }

        private static Language vietnamese;

        public static Language Vietnamese
        {
            get
            {
                if (vietnamese == null) vietnamese = new Language(SystemLanguage.Vietnamese.ToString(), "vi");
                return vietnamese;
            }
        }

        private static Language unknown;

        public static Language Unknown
        {
            get
            {
                if (unknown == null) unknown = new Language(SystemLanguage.Unknown.ToString(), "");
                return unknown;
            }
        }

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