using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SimpleJSON
{
    public interface ISimpleJSONSerializable
    {
        JSONNode ToJSONNode();
    }

    public static partial class JSON
    {
        public static string Serialize(object value, int aIndent = 0)
        {
            return ToJSONNode(value).ToString(aIndent);
        }

        public static JSONNode ToJSONNode(object value)
        {
            switch (value)
            {
                case null:
                    return JSONNull.CreateOrGet();
                case JSONNode jsonValue:
                    return jsonValue;
                case string strValue:
                    return new JSONString(strValue);
                case char charValue:
                    return new JSONString(new string(charValue, 1));
                case bool boolValue:
                    return new JSONBool(boolValue);
                case IList listValue:
                    return ToJSONNode(listValue);
                case IDictionary dictValue:
                    return ToJSONNode(dictValue);
                case ISimpleJSONSerializable serializableValue:
                    return serializableValue.ToJSONNode();
#if UNITY_5_3_OR_NEWER
                case Vector2 v2Value:
                    return v2Value;
                case Vector3 v3Value:
                    return v3Value;
                case Vector4 v4Value:
                    return v4Value;
                case Quaternion quatValue:
                    return quatValue;
                case Rect rectValue:
                    return rectValue;
                case RectOffset rectOffsetValue:
                    return rectOffsetValue;
                case Matrix4x4 matrixValue:
                    return matrixValue;
                case Color colorValue:
                    return colorValue;
                case Color32 color32Value:
                    return color32Value;
#endif

                case long _:
                case ulong _:
                case decimal _:
                    return new JSONString(value.ToString());
                default:
                    if (JSONNumber.IsNumeric(value))
                    {
                        return new JSONNumber(System.Convert.ToDouble(value));
                    }

                    return new JSONString(value.ToString());
            }
        }

        private static JSONArray ToJSONNode(IList list)
        {
            var jsonArray = new JSONArray();

            for (int i = 0; i < list.Count; i++)
            {
                jsonArray.Add(ToJSONNode(list[i]));
            }

            return jsonArray;
        }

        private static JSONObject ToJSONNode(IDictionary dict)
        {
            var jsonObject = new JSONObject();

            foreach (object key in dict.Keys)
            {
                jsonObject.Add(key.ToString(), ToJSONNode(dict[key]));
            }

            return jsonObject;
        }

        #region Extension methods
#if UNITY_5_3_OR_NEWER
        public static JSONNode ToJSONNode(this Vector2 vector2, bool asArray = false)
        {
            if (asArray)
            {
                return new JSONArray().WriteVector2(vector2);
            }
            else
            {
                return new JSONObject().WriteVector2(vector2);
            }
        }

        public static JSONNode ToJSONNode(this Vector3 vector3, bool asArray = false)
        {
            if (asArray)
            {
                return new JSONArray().WriteVector3(vector3);
            }
            else
            {
                return new JSONObject().WriteVector3(vector3);
            }
        }

        public static JSONNode ToJSONNode(this Vector4 vector4, bool asArray = false)
        {
            if (asArray)
            {
                return new JSONArray().WriteVector4(vector4);
            }
            else
            {
                return new JSONObject().WriteVector4(vector4);
            }
        }

        public static JSONNode ToJSONNode(this Quaternion quaternion, bool asArray = false)
        {
            if (asArray)
            {
                return new JSONArray().WriteQuaternion(quaternion);
            }
            else
            {
                return new JSONObject().WriteQuaternion(quaternion);
            }
        }

        public static JSONNode ToJSONNode(this Rect rect, bool asArray = false)
        {
            if (asArray)
            {
                return new JSONArray().WriteRect(rect);
            }
            else
            {
                return new JSONObject().WriteRect(rect);
            }
        }

        public static JSONNode ToJSONNode(this RectOffset rectOffset, bool asArray = false)
        {
            if (asArray)
            {
                return new JSONArray().WriteRectOffset(rectOffset);
            }
            else
            {
                return new JSONObject().WriteRectOffset(rectOffset);
            }
        }

        public static JSONNode ToJSONNode(this Matrix4x4 matrix)
        {
            return new JSONArray().WriteMatrix(matrix);
        }

        public static JSONNode ToJSONNode(this Color color, bool asArray = false)
        {
            if (asArray)
            {
                return new JSONArray().WriteColor(color);
            }
            else
            {
                return new JSONObject().WriteColor(color);
            }
        }

        public static JSONNode ToJSONNode(this Color32 color32, bool asArray = false)
        {
            if (asArray)
            {
                return new JSONArray().WriteColor32(color32);
            }
            else
            {
                return new JSONObject().WriteColor32(color32);
            }
        }
#endif

        public static JSONNode ToJSONNode<T>(this List<T> list)
        {
            return ToJSONNode((IList)list);
        }

        public static JSONNode ToJSONNode<T>(this Dictionary<string, T> dict)
        {
            return ToJSONNode((IDictionary)dict);
        }

        #endregion
    }
}
