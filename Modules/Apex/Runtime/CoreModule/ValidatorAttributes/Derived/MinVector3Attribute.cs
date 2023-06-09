using System;

namespace Pancake.Apex
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class MinVector3Attribute : ValidatorAttribute
    {
        public readonly float x;
        public readonly float y;
        public readonly float z;

        public readonly string xProperty;
        public readonly string yProperty;
        public readonly string zProperty;

        public MinVector3Attribute(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public MinVector3Attribute(string xProperty, string yProperty, string zProperty)
        {
            this.xProperty = xProperty;
            this.yProperty = yProperty;
            this.zProperty = zProperty;
        }

        public MinVector3Attribute(string xProperty, float y, float z)
        {
            this.xProperty = xProperty;
            this.y = y;
            this.z = z;

            yProperty = string.Empty;
            zProperty = string.Empty;
        }

        public MinVector3Attribute(float x, string yProperty, float z)
        {
            this.x = x;
            this.yProperty = yProperty;
            this.z = z;

            xProperty = string.Empty;
            zProperty = string.Empty;
        }

        public MinVector3Attribute(float x, float y, string zProperty)
        {
            this.x = x;
            this.y = y;
            this.zProperty = zProperty;

            xProperty = string.Empty;
            yProperty = string.Empty;
        }

        public MinVector3Attribute(string xProperty, string yProperty, float z)
        {
            this.xProperty = xProperty;
            this.yProperty = yProperty;
            this.z = z;

            zProperty = string.Empty;
        }

        public MinVector3Attribute(float x, string yProperty, string zProperty)
        {
            this.x = x;
            this.yProperty = yProperty;
            this.zProperty = zProperty;

            xProperty = string.Empty;
        }

        public MinVector3Attribute(string xProperty, float y, string zProperty)
        {
            this.xProperty = xProperty;
            this.y = y;
            this.zProperty = zProperty;

            yProperty = string.Empty;
        }
    }
}