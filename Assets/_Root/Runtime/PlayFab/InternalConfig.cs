#if PANCAKE_PLAYFAB
using System;

namespace Pancake.GameService
{
    [Serializable]
    public class InternalConfig
    {
        public string countryCode;
        public string field1;
        public string field2;
        public string field3;
        public string field4;
        public string field5;

        public InternalConfig(string countryCode, string field1, string field2, string field3, string field4, string field5)
        {
            this.countryCode = countryCode;
            this.field1 = field1;
            this.field2 = field2;
            this.field3 = field3;
            this.field4 = field4;
            this.field5 = field5;
        }

        public InternalConfig()
        {
            countryCode = "";
            field1 = "";
            field2 = "";
            field3 = "";
            field4 = "";
            field5 = "";
        }
    }
}
#endif