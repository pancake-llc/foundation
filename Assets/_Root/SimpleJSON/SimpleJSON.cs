/* * * * *
 * A simple JSON Parser / builder
 * ------------------------------
 *
 * It mainly has been written as a simple JSON parser. It can build a JSON string
 * from the node-tree, or generate a node tree from any valid JSON string.
 *
 * Written by Bunny83
 * 2012-06-09
 *
 * Changelog now external. See CHANGELOG.md
 *
 * The MIT License (MIT)
 *
 * Copyright (c) 2012-2022 Markus Göbel (Bunny83)
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 *
 * * * * */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Pancake
{
    public enum JSONNodeType
    {
        Array = 1,
        Object = 2,
        String = 3,
        Number = 4,
        NullValue = 5,
        Boolean = 6,
        None = 7,
        Custom = 0xFF,
    }

    public enum JSONTextMode
    {
        Compact,
        Indent
    }

    public abstract partial class JSONNode
    {
        protected const string TOKEN_NULL = "null";
        protected const string TOKEN_TRUE = "true";
        protected const string TOKEN_FALSE = "false";

        #region common interface

        public static bool forceASCII = false; // Use Unicode by default
        public static bool allowLineComments = true; // allow "//"-style comments at the end of a line

        public abstract JSONNodeType Tag { get; }

        public virtual JSONNode this[int aIndex] { get { throw new NotImplementedException(); } set { throw new NotImplementedException(); } }
        public virtual JSONNode this[string aKey] { get { throw new NotImplementedException(); } set { throw new NotImplementedException(); } }

        public virtual string Value { get { return null; } protected set { } }

        public virtual int Count { get { return 0; } }

        public virtual bool IsNumber { get { return false; } }
        public virtual bool IsString { get { return false; } }
        public virtual bool IsBoolean { get { return false; } }
        public virtual bool IsNull { get { return false; } }
        public virtual bool IsArray { get { return false; } }
        public virtual bool IsObject { get { return false; } }

        public virtual bool Inline { get { return false; } set { } }

        public virtual void Add(string aKey, JSONNode aItem)
        {
            throw new NotImplementedException();
        }

        public virtual void Add(JSONNode aItem)
        {
            Add(null, aItem);
        }

        public virtual JSONNode Remove(string aKey)
        {
            throw new NotImplementedException();
        }

        public virtual JSONNode Remove(int aIndex)
        {
            throw new NotImplementedException();
        }

        public virtual JSONNode Remove(JSONNode aNode)
        {
            throw new NotImplementedException();
        }

        public virtual void Clear()
        {
            throw new NotImplementedException();
        }

        public virtual JSONNode Clone()
        {
            throw new NotImplementedException();
        }

        public virtual IEnumerable<JSONNode> Children
        {
            get
            {
                yield break;
            }
        }

        public virtual bool HasKey(string aKey)
        {
            return false;
        }

        public virtual JSONNode GetValueOrDefault(string aKey, JSONNode aDefault)
        {
            return aDefault;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            WriteToStringBuilder(sb, 0, 0, JSONTextMode.Compact);
            return sb.ToString();
        }

        public virtual string ToString(int aIndent)
        {
            StringBuilder sb = new StringBuilder();
            WriteToStringBuilder(sb, 0, aIndent, aIndent > 0 ? JSONTextMode.Indent : JSONTextMode.Compact);
            return sb.ToString();
        }

        internal abstract void WriteToStringBuilder(StringBuilder aSB, int aIndent, int aIndentInc, JSONTextMode aMode);

        #endregion common interface

        #region typecasting properties

        public virtual double AsDouble
        {
            get
            {
                if (double.TryParse(Value, NumberStyles.Float, CultureInfo.InvariantCulture, out double v))
                {
                    return v;
                }
                return 0.0;
            }
            set
            {
                Value = value.ToString(CultureInfo.InvariantCulture);
            }
        }

        public virtual int AsInt
        {
            get { return (int)AsDouble; }
            set { AsDouble = value; }
        }

        public virtual float AsFloat
        {
            get { return (float)AsDouble; }
            set { AsDouble = value; }
        }

        public virtual bool AsBool
        {
            get
            {
                if (bool.TryParse(Value, out bool v))
                {
                    return v;
                }
                return !string.IsNullOrEmpty(Value);
            }
            set
            {
                Value = (value) ? TOKEN_TRUE : TOKEN_FALSE;
            }
        }

        public virtual long AsLong
        {
            get
            {
                if (long.TryParse(Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out long val))
                {
                    return val;
                }
                return 0L;
            }
            set
            {
                Value = value.ToString(CultureInfo.InvariantCulture);
            }
        }

        public virtual ulong AsULong
        {
            get
            {
                if (ulong.TryParse(Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out ulong val))
                {
                    return val;
                }
                return 0;
            }
            set
            {
                Value = value.ToString(CultureInfo.InvariantCulture);
            }
        }

        public virtual JSONArray AsArray
        {
            get
            {
                return this as JSONArray;
            }
        }

        public virtual JSONObject AsObject
        {
            get
            {
                return this as JSONObject;
            }
        }

        #endregion typecasting properties

        #region operators

        public static implicit operator JSONNode(string s)
        {
            return (s == null) ? (JSONNode) JSONNull.CreateOrGet() : new JSONString(s);
        }

        public static implicit operator string(JSONNode d)
        {
            return (d == null) ? null : d.Value;
        }

        public static implicit operator JSONNode(double n)
        {
            return new JSONNumber(n);
        }

        public static implicit operator double(JSONNode d)
        {
            return (d == null) ? 0 : d.AsDouble;
        }

        public static implicit operator JSONNode(float n)
        {
            return new JSONNumber(n);
        }

        public static implicit operator float(JSONNode d)
        {
            return (d == null) ? 0 : d.AsFloat;
        }

        public static implicit operator JSONNode(int n)
        {
            return new JSONNumber(n);
        }

        public static implicit operator int(JSONNode d)
        {
            return (d == null) ? 0 : d.AsInt;
        }

        public static implicit operator JSONNode(long n)
        {
            return new JSONString(n.ToString(CultureInfo.InvariantCulture));
        }

        public static implicit operator long(JSONNode d)
        {
            return (d == null) ? 0L : d.AsLong;
        }

        public static implicit operator JSONNode(ulong n)
        {
            return new JSONString(n.ToString(CultureInfo.InvariantCulture));
        }

        public static implicit operator ulong(JSONNode d)
        {
            return (d == null) ? 0 : d.AsULong;
        }

        public static implicit operator JSONNode(bool b)
        {
            return new JSONBool(b);
        }

        public static implicit operator bool(JSONNode d)
        {
            return (d == null) ? false : d.AsBool;
        }

        public static implicit operator JSONNode(KeyValuePair<string, JSONNode> aKeyValue)
        {
            return aKeyValue.Value;
        }

        public static bool operator ==(JSONNode a, object b)
        {
            if (ReferenceEquals(a, b))
            {
                return true;
            }
            bool aIsNull = a is JSONNull || ReferenceEquals(a, null) || a is JSONLazyCreator;
            bool bIsNull = b is JSONNull || ReferenceEquals(b, null) || b is JSONLazyCreator;
            if (aIsNull && bIsNull)
            {
                return true;
            }
            return !aIsNull && a.Equals(b);
        }

        public static bool operator !=(JSONNode a, object b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        #endregion operators

        [ThreadStatic]
        private static StringBuilder m_EscapeBuilder;

        internal static StringBuilder EscapeBuilder
        {
            get
            {
                if (m_EscapeBuilder == null)
                {
                    m_EscapeBuilder = new StringBuilder();
                }
                return m_EscapeBuilder;
            }
        }

        internal static string Escape(string aText)
        {
            var sb = EscapeBuilder;
            sb.Length = 0;
            if (sb.Capacity < aText.Length + aText.Length / 10)
            {
                sb.Capacity = aText.Length + aText.Length / 10;
            }
            foreach (char c in aText)
            {
                switch (c)
                {
                    case '\\':
                        sb.Append("\\\\");
                        break;
                    case '\"':
                        sb.Append("\\\"");
                        break;
                    case '\n':
                        sb.Append("\\n");
                        break;
                    case '\r':
                        sb.Append("\\r");
                        break;
                    case '\t':
                        sb.Append("\\t");
                        break;
                    case '\b':
                        sb.Append("\\b");
                        break;
                    case '\f':
                        sb.Append("\\f");
                        break;
                    default:
                        if (c < ' ' || (forceASCII && c > 127))
                        {
                            ushort val = c;
                            sb.Append("\\u").Append(val.ToString("X4"));
                        }
                        else
                        {
                            sb.Append(c);
                        }
                        break;
                }
            }
            string result = sb.ToString();
            sb.Length = 0;
            return result;
        }

        private static JSONNode ParseElement(string token, bool quoted)
        {
            if (quoted)
            {
                return token;
            }

            if (token.Length <= 5)
            {
                if (token.Equals(TOKEN_FALSE, StringComparison.InvariantCultureIgnoreCase))
                {
                    return false;
                }
                if (token.Equals(TOKEN_TRUE, StringComparison.InvariantCultureIgnoreCase))
                {
                    return true;
                }
                if (token.Equals(TOKEN_NULL, StringComparison.InvariantCultureIgnoreCase))
                {
                    return JSONNull.CreateOrGet();
                }
            }

            if (double.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out double val))
            {
                return val;
            }
            else
            {
                return token;
            }
        }

        public static JSONNode Parse(string aJSON)
        {
            Stack<JSONNode> stack = new Stack<JSONNode>();
            JSONNode ctx = null;
            int i = 0;
            StringBuilder Token = new StringBuilder();
            string TokenName = null;
            bool QuoteMode = false;
            bool TokenIsQuoted = false;
            while (i < aJSON.Length)
            {
                switch (aJSON[i])
                {
                    case '{':
                        if (QuoteMode)
                        {
                            Token.Append(aJSON[i]);
                            break;
                        }
                        stack.Push(new JSONObject());
                        if (ctx != null)
                        {
                            ctx.Add(TokenName, stack.Peek());
                        }
                        TokenName = null;
                        Token.Length = 0;
                        ctx = stack.Peek();
                        break;

                    case '[':
                        if (QuoteMode)
                        {
                            Token.Append(aJSON[i]);
                            break;
                        }

                        stack.Push(new JSONArray());
                        if (ctx != null)
                        {
                            ctx.Add(TokenName, stack.Peek());
                        }
                        TokenName = null;
                        Token.Length = 0;
                        ctx = stack.Peek();
                        break;

                    case '}':
                    case ']':
                        if (QuoteMode)
                        {

                            Token.Append(aJSON[i]);
                            break;
                        }
                        if (stack.Count == 0)
                            throw new Exception("JSON Parse: Too many closing brackets");

                        stack.Pop();
                        if (Token.Length > 0 || TokenIsQuoted)
                            ctx.Add(TokenName, ParseElement(Token.ToString(), TokenIsQuoted));
                        TokenIsQuoted = false;
                        TokenName = null;
                        Token.Length = 0;
                        if (stack.Count > 0)
                            ctx = stack.Peek();
                        break;

                    case ':':
                        if (QuoteMode)
                        {
                            Token.Append(aJSON[i]);
                            break;
                        }
                        TokenName = Token.ToString();
                        Token.Length = 0;
                        TokenIsQuoted = false;
                        break;

                    case '"':
                        QuoteMode ^= true;
                        TokenIsQuoted |= QuoteMode;
                        break;

                    case ',':
                        if (QuoteMode)
                        {
                            Token.Append(aJSON[i]);
                            break;
                        }
                        if (Token.Length > 0 || TokenIsQuoted)
                            ctx.Add(TokenName, ParseElement(Token.ToString(), TokenIsQuoted));
                        TokenIsQuoted = false;
                        TokenName = null;
                        Token.Length = 0;
                        TokenIsQuoted = false;
                        break;

                    case '\r':
                    case '\n':
                        break;

                    case ' ':
                    case '\t':
                        if (QuoteMode)
                        {
                            Token.Append(aJSON[i]);
                        }
                        break;

                    case '\\':
                        ++i;
                        if (QuoteMode)
                        {
                            char C = aJSON[i];
                            switch (C)
                            {
                                case 't':
                                    Token.Append('\t');
                                    break;
                                case 'r':
                                    Token.Append('\r');
                                    break;
                                case 'n':
                                    Token.Append('\n');
                                    break;
                                case 'b':
                                    Token.Append('\b');
                                    break;
                                case 'f':
                                    Token.Append('\f');
                                    break;
                                case 'u':
                                    string s = aJSON.Substring(i + 1, 4);
                                    Token.Append((char)int.Parse(
                                        s,
                                        System.Globalization.NumberStyles.AllowHexSpecifier));
                                    i += 4;
                                    break;
                                default:
                                    Token.Append(C);
                                    break;
                            }
                        }
                        break;
                    case '/':
                        if (allowLineComments && !QuoteMode && i + 1 < aJSON.Length && aJSON[i + 1] == '/')
                        {
                            while (++i < aJSON.Length && aJSON[i] != '\n' && aJSON[i] != '\r') ;
                            break;
                        }
                        Token.Append(aJSON[i]);
                        break;
                    case '\uFEFF': // remove / ignore BOM (Byte Order Mark)
                        break;

                    default:
                        Token.Append(aJSON[i]);
                        break;
                }
                ++i;
            }
            if (QuoteMode)
            {
                throw new Exception("JSON Parse: Quotation marks seems to be messed up.");
            }
            if (ctx == null)
            {
                return ParseElement(Token.ToString(), TokenIsQuoted);
            }
            return ctx;
        }
    }
    // End of JSONNode

    public partial class JSONArray : JSONNode, IList<JSONNode>
    {
        private List<JSONNode> m_List = new List<JSONNode>();
        private bool inline = false;
        public override bool Inline
        {
            get { return inline; }
            set { inline = value; }
        }

        public override JSONNodeType Tag { get { return JSONNodeType.Array; } }
        public override bool IsArray { get { return true; } }
        public bool IsReadOnly => false;

        public override JSONNode this[int aIndex]
        {
            get
            {
                if (aIndex < 0 || aIndex >= m_List.Count)
                {
                    throw new IndexOutOfRangeException();
                }

                return m_List[aIndex];
            }
            set
            {
                if (aIndex < 0 || aIndex >= m_List.Count)
                {
                    throw new IndexOutOfRangeException();
                }

                if (value == null)
                {
                    value = JSONNull.CreateOrGet();
                }

                m_List[aIndex] = value;
            }
        }

        public override JSONArray AsArray
        {
            get => this;
        }

        public int Capacity
        {
            get { return m_List.Capacity; }
            set { m_List.Capacity = value; }
        }

        public override int Count
        {
            get { return m_List.Count; }
        }

        public override void Add(string aKey, JSONNode aItem)
        {
            if (aKey != null)
            {
                throw new NotImplementedException();
            }

            if (aItem == null)
            {
                aItem = JSONNull.CreateOrGet();
            }

            m_List.Add(aItem);
        }

        public override JSONNode Remove(string aKey)
        {
            throw new NotImplementedException();
        }

        public override JSONNode Remove(int aIndex)
        {
            if (aIndex < 0 || aIndex >= m_List.Count)
            {
                throw new IndexOutOfRangeException();
            }

            JSONNode tmp = m_List[aIndex];
            m_List.RemoveAt(aIndex);
            return tmp;
        }

        public override JSONNode Remove(JSONNode aNode)
        {
            m_List.Remove(aNode);
            return aNode;
        }

        public override JSONNode Clone()
        {
            var node = new JSONArray();
            node.m_List.Capacity = m_List.Capacity;
            foreach (var n in m_List)
            {
                if (n != null)
                {
                    node.Add(n.Clone());
                }
                else
                {
                    node.Add(null);
                }
            }
            return node;
        }

        public override IEnumerable<JSONNode> Children
        {
            get
            {
                foreach (var node in m_List)
                {
                    yield return node;
                }
            }
        }

        internal override void WriteToStringBuilder(StringBuilder aSB, int aIndent, int aIndentInc, JSONTextMode aMode)
        {
            aSB.Append('[');
            int count = m_List.Count;
            if (inline)
            {
                aMode = JSONTextMode.Compact;
            }
            for (int i = 0; i < count; i++)
            {
                if (i > 0)
                {
                    aSB.Append(',');
                }
                if (aMode == JSONTextMode.Indent)
                {
                    aSB.AppendLine();
                }

                if (aMode == JSONTextMode.Indent)
                {
                    aSB.Append(' ', aIndent + aIndentInc);
                }
                m_List[i].WriteToStringBuilder(aSB, aIndent + aIndentInc, aIndentInc, aMode);
            }
            if (aMode == JSONTextMode.Indent)
            {
                aSB.AppendLine().Append(' ', aIndent);
            }
            aSB.Append(']');
        }

        public int IndexOf(JSONNode item)
        {
            return m_List.IndexOf(item);
        }

        public void Insert(int index, JSONNode item)
        {
            m_List.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            m_List.RemoveAt(index);
        }

        public override void Clear()
        {
            m_List.Clear();
        }

        public bool Contains(JSONNode item)
        {
            return m_List.Contains(item);
        }

        public void CopyTo(JSONNode[] array, int arrayIndex)
        {
            m_List.CopyTo(array, arrayIndex);
        }

        bool ICollection<JSONNode>.Remove(JSONNode item)
        {
            return m_List.Remove(item);
        }

        IEnumerator<JSONNode> IEnumerable<JSONNode>.GetEnumerator()
        {
            return m_List.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_List.GetEnumerator();
        }
    }
    // End of JSONArray

    public partial class JSONObject : JSONNode, IDictionary<string, JSONNode>
    {
        private Dictionary<string, JSONNode> m_Dict = new Dictionary<string, JSONNode>();

        private bool inline = false;

        public override bool Inline
        {
            get { return inline; }
            set { inline = value; }
        }

        public override JSONNodeType Tag { get { return JSONNodeType.Object; } }
        public override bool IsObject { get { return true; } }

        ICollection<string> IDictionary<string, JSONNode>.Keys
        {
            get
            {
                return m_Dict.Keys;
            }
        }

        public ICollection<string> Keys
        {
            get
            {
                return m_Dict.Keys;
            }
        }

        ICollection<JSONNode> IDictionary<string, JSONNode>.Values
        {
            get
            {
                return m_Dict.Values;
            }
        }

        public ICollection<JSONNode> Values
        {
            get
            {
                return m_Dict.Values;
            }
        }

        public bool IsReadOnly => false;

        public override JSONNode this[string aKey]
        {
            get
            {
                if (m_Dict.ContainsKey(aKey))
                {
                    return m_Dict[aKey];
                }
                else
                {
                    return new JSONLazyCreator(this, aKey);
                }
            }
            set
            {
                if (value == null)
                {
                    value = JSONNull.CreateOrGet();
                }
                if (m_Dict.ContainsKey(aKey))
                {
                    m_Dict[aKey] = value;
                }
                else
                {
                    m_Dict.Add(aKey, value);
                }
            }
        }

        public override JSONObject AsObject
        {
            get => this;
        }

        public override int Count
        {
            get { return m_Dict.Count; }
        }

        public override void Add(string aKey, JSONNode aItem)
        {
            if (aKey == null)
            {
                throw new NotImplementedException();
            }

            if (aItem == null)
            {
                aItem = JSONNull.CreateOrGet();
            }

            m_Dict[aKey] = aItem;
        }

        public override JSONNode Remove(string aKey)
        {
            if (!m_Dict.ContainsKey(aKey))
            {
                return null;
            }
            JSONNode tmp = m_Dict[aKey];
            m_Dict.Remove(aKey);
            return tmp;
        }

        public override JSONNode Remove(int aIndex)
        {
            throw new NotImplementedException();
        }

        public override JSONNode Remove(JSONNode aNode)
        {
            foreach (var kvp in m_Dict)
            {
                if (kvp.Value == aNode)
                {
                    m_Dict.Remove(kvp.Key);
                    return kvp.Value;
                }
            }

            return null;
        }

        public override JSONNode Clone()
        {
            var node = new JSONObject();
            foreach (var n in m_Dict)
            {
                node.Add(n.Key, n.Value.Clone());
            }
            return node;
        }

        public override bool HasKey(string aKey)
        {
            return m_Dict.ContainsKey(aKey);
        }

        public override JSONNode GetValueOrDefault(string aKey, JSONNode aDefault)
        {
            if (m_Dict.TryGetValue(aKey, out JSONNode res))
            {
                return res;
            }
            return aDefault;
        }

        public override IEnumerable<JSONNode> Children
        {
            get
            {
                foreach (var node in m_Dict.Values)
                {
                    yield return node;
                }
            }
        }

        internal override void WriteToStringBuilder(StringBuilder aSB, int aIndent, int aIndentInc, JSONTextMode aMode)
        {
            aSB.Append('{');
            bool first = true;
            if (inline)
            {
                aMode = JSONTextMode.Compact;
            }
            foreach (var k in m_Dict)
            {
                if (!first)
                    aSB.Append(',');
                first = false;
                if (aMode == JSONTextMode.Indent)
                    aSB.AppendLine();
                if (aMode == JSONTextMode.Indent)
                    aSB.Append(' ', aIndent + aIndentInc);
                aSB.Append('\"').Append(Escape(k.Key)).Append('\"');
                if (aMode == JSONTextMode.Compact)
                    aSB.Append(':');
                else
                    aSB.Append(" : ");
                k.Value.WriteToStringBuilder(aSB, aIndent + aIndentInc, aIndentInc, aMode);
            }
            if (aMode == JSONTextMode.Indent)
            {
                aSB.AppendLine().Append(' ', aIndent);
            }
            aSB.Append('}');
        }

        public bool ContainsKey(string key)
        {
            return m_Dict.ContainsKey(key);
        }

        bool IDictionary<string, JSONNode>.Remove(string key)
        {
            return m_Dict.Remove(key);
        }

        public bool TryGetValue(string key, out JSONNode value)
        {
            return m_Dict.TryGetValue(key, out value);
        }

        public void Add(KeyValuePair<string, JSONNode> item)
        {
            ((IDictionary<string, JSONNode>)m_Dict).Add(item);
        }

        public override void Clear()
        {
            m_Dict.Clear();
        }

        public bool Contains(KeyValuePair<string, JSONNode> item)
        {
            return m_Dict.ContainsKey(item.Key);
        }

        public void CopyTo(KeyValuePair<string, JSONNode>[] array, int arrayIndex)
        {
            ((IDictionary<string, JSONNode>)m_Dict).CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<string, JSONNode> item)
        {
            return ((IDictionary<string, JSONNode>)m_Dict).Remove(item);
        }

        IEnumerator<KeyValuePair<string, JSONNode>> IEnumerable<KeyValuePair<string, JSONNode>>.GetEnumerator()
        {
            return m_Dict.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_Dict.GetEnumerator();
        }
    }
    // End of JSONObject

    public partial class JSONString : JSONNode
    {
        private string m_Data;

        public override JSONNodeType Tag { get { return JSONNodeType.String; } }
        public override bool IsString { get { return true; } }
        public override bool IsNull { get { return m_Data == null; } }

        public override string Value
        {
            get { return m_Data; }
            protected set
            {
                m_Data = value;
            }
        }

        public JSONString() : this(null)
        {

        }

        public JSONString(string aData)
        {
            m_Data = aData;
        }

        public override void Clear()
        {
            Value = null;
        }

        public override JSONNode Clone()
        {
            return new JSONString(m_Data);
        }

        internal override void WriteToStringBuilder(StringBuilder aSB, int aIndent, int aIndentInc, JSONTextMode aMode)
        {
            if (m_Data == null)
            {
                aSB.Append(TOKEN_NULL);
                return;
            }

            aSB.Append('\"').Append(Escape(m_Data)).Append('\"');
        }

        public override bool Equals(object obj)
        {
            switch (obj)
            {
                case JSONNull nullObj:
                    return m_Data == null;
                case string stringObj:
                    return m_Data.Equals(stringObj);
                case JSONString jsonStringObj:
                    return m_Data.Equals(jsonStringObj.m_Data);
                default:
                    return base.Equals(obj);
            }
        }

        public override int GetHashCode()
        {
            return m_Data.GetHashCode();
        }
    }
    // End of JSONString

    public partial class JSONNumber : JSONNode
    {
        private const long MAX_SAFE_INTEGER = (long)1 << 53;
        private const long MIN_SAFE_INTEGER = -(long)1 << 53;

        private double m_Data;

        public override JSONNodeType Tag { get { return JSONNodeType.Number; } }
        public override bool IsNumber { get { return true; } }

        public override string Value
        {
            get { return m_Data.ToString("R", CultureInfo.InvariantCulture); }
            protected set
            {
                if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double v))
                {
                    m_Data = v;
                }
            }
        }

        public override double AsDouble
        {
            get { return m_Data; }
            set { m_Data = value; }
        }

        public override long AsLong
        {
            get { return (long)m_Data; }
            set
            {
                if (value > MAX_SAFE_INTEGER || value < MIN_SAFE_INTEGER)
                {
                    throw new ArgumentException("JSONNumber cannot store an INT64 this large without losing precision");
                }

                m_Data = value;
            }
        }

        public override ulong AsULong
        {
            get { return (ulong)m_Data; }
            set
            {
                if (value > MAX_SAFE_INTEGER)
                {
                    throw new ArgumentException("JSONNumber cannot store an INT64 this large without losing precision");
                }

                m_Data = value;
            }
        }

        public JSONNumber(double aData)
        {
            m_Data = aData;
        }

        public JSONNumber(string aData)
        {
            Value = aData;
        }

        public override JSONNode Clone()
        {
            return new JSONNumber(m_Data);
        }

        internal override void WriteToStringBuilder(StringBuilder aSB, int aIndent, int aIndentInc, JSONTextMode aMode)
        {
            aSB.Append(Value);
        }

        public static bool IsNumeric(object value)
        {
            switch (value)
            {
                case byte _:
                case decimal _:
                case double _:
                case float _:
                case int _:
                case long _:
                case sbyte _:
                case short _:
                case uint _:
                case ulong _:
                case ushort _:
                    return true;
                default:
                    return false;
            }
        }

        public override bool Equals(object obj)
        {
            switch (obj)
            {
                case null:
                    return false;
                case JSONNumber jsonNumber:
                    return m_Data == jsonNumber.m_Data;
                default:
                    if (IsNumeric(obj))
                    {
                        return Convert.ToDouble(obj) == m_Data;
                    }
                    return base.Equals(obj);
            }
        }

        public override int GetHashCode()
        {
            return m_Data.GetHashCode();
        }
    }
    // End of JSONNumber

    public partial class JSONBool : JSONNode
    {
        private bool m_Data;

        public override JSONNodeType Tag { get { return JSONNodeType.Boolean; } }
        public override bool IsBoolean { get { return true; } }

        public override string Value
        {
            get { return m_Data.ToString(); }
            protected set
            {
                if (bool.TryParse(value, out bool v))
                {
                    m_Data = v;
                }
            }
        }

        public override bool AsBool
        {
            get { return m_Data; }
            set { m_Data = value; }
        }

        public JSONBool(bool aData)
        {
            m_Data = aData;
        }

        public JSONBool(string aData)
        {
            Value = aData;
        }

        public override JSONNode Clone()
        {
            return new JSONBool(m_Data);
        }

        internal override void WriteToStringBuilder(StringBuilder aSB, int aIndent, int aIndentInc, JSONTextMode aMode)
        {
            aSB.Append((m_Data) ? TOKEN_TRUE : TOKEN_FALSE);
        }

        public override bool Equals(object obj)
        {
            switch (obj)
            {
                case null:
                    return false;
                case JSONBool jsonBool:
                    return m_Data == jsonBool.m_Data;
                case bool boolObj:
                    return m_Data == boolObj;
                default:
                    return false;
            }
        }

        public override int GetHashCode()
        {
            return m_Data.GetHashCode();
        }
    }
    // End of JSONBool

    public partial class JSONNull : JSONNode
    {
        private static readonly JSONNull m_StaticInstance = new JSONNull();
        public static bool reuseSameInstance = true;
        public static JSONNull CreateOrGet()
        {
            if (reuseSameInstance)
            {
                return m_StaticInstance;
            }
            return new JSONNull();
        }

        private JSONNull() { }

        public override JSONNodeType Tag { get { return JSONNodeType.NullValue; } }
        public override bool IsNull { get { return true; } }

        public override string Value
        {
            get { return null; }
            protected set { }
        }

        public override bool AsBool
        {
            get { return false; }
            set { }
        }

        public override JSONNode Clone()
        {
            return CreateOrGet();
        }

        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(this, obj))
            {
                return true;
            }
            return (obj is JSONNull);
        }

        public override int GetHashCode()
        {
            return 0;
        }

        internal override void WriteToStringBuilder(StringBuilder aSB, int aIndent, int aIndentInc, JSONTextMode aMode)
        {
            aSB.Append(TOKEN_NULL);
        }
    }
    // End of JSONNull

    internal partial class JSONLazyCreator : JSONNode
    {
        private JSONNode m_Node = null;
        private string m_Key = null;
        public override JSONNodeType Tag { get { return JSONNodeType.None; } }
        public override bool IsNull { get { return true; } }

        public JSONLazyCreator(JSONNode aNode)
        {
            m_Node = aNode;
            m_Key = null;
        }

        public JSONLazyCreator(JSONNode aNode, string aKey)
        {
            m_Node = aNode;
            m_Key = aKey;
        }

        private T Set<T>(T aVal) where T : JSONNode
        {
            if (m_Key == null)
            {
                m_Node.Add(aVal);
            }
            else
            {
                m_Node.Add(m_Key, aVal);
            }
            m_Node = null; // Be GC friendly.
            return aVal;
        }

        public override JSONNode this[int aIndex]
        {
            get { return new JSONLazyCreator(this); }
            set { Set(new JSONArray()).Add(value); }
        }

        public override JSONNode this[string aKey]
        {
            get { return new JSONLazyCreator(this, aKey); }
            set { Set(new JSONObject()).Add(aKey, value); }
        }

        public override void Add(JSONNode aItem)
        {
            Set(new JSONArray()).Add(aItem);
        }

        public override void Add(string aKey, JSONNode aItem)
        {
            Set(new JSONObject()).Add(aKey, aItem);
        }

        public static bool operator ==(JSONLazyCreator a, object b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(JSONLazyCreator a, object b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return true;
            }
            if (obj is JSONNull)
            {
                return true;
            }

            return System.Object.ReferenceEquals(this, obj);
        }

        public override int GetHashCode()
        {
            return 0;
        }

        public override int AsInt
        {
            get { Set(new JSONNumber(0)); return 0; }
            set { Set(new JSONNumber(value)); }
        }

        public override float AsFloat
        {
            get { Set(new JSONNumber(0.0f)); return 0.0f; }
            set { Set(new JSONNumber(value)); }
        }

        public override double AsDouble
        {
            get { Set(new JSONNumber(0.0)); return 0.0; }
            set { Set(new JSONNumber(value)); }
        }

        public override long AsLong
        {
            get
            {
                Set(new JSONString("0"));
                return 0L;
            }
            set
            {
                Set(new JSONString(value.ToString(CultureInfo.InvariantCulture)));
            }
        }

        public override ulong AsULong
        {
            get
            {
                Set(new JSONString("0"));
                return 0L;
            }
            set
            {
                Set(new JSONString(value.ToString(CultureInfo.InvariantCulture)));
            }
        }

        public override bool AsBool
        {
            get { Set(new JSONBool(false)); return false; }
            set { Set(new JSONBool(value)); }
        }

        public override JSONArray AsArray
        {
            get { return Set(new JSONArray()); }
        }

        public override JSONObject AsObject
        {
            get { return Set(new JSONObject()); }
        }

        internal override void WriteToStringBuilder(StringBuilder aSB, int aIndent, int aIndentInc, JSONTextMode aMode)
        {
            aSB.Append(TOKEN_NULL);
        }
    }
    // End of JSONLazyCreator

    public static partial class JSON
    {
        public static JSONNode Parse(string aJSON)
        {
            return JSONNode.Parse(aJSON);
        }

        public static bool TryParse(string aJSON, out JSONNode aResult)
        {
            try
            {
                aResult = JSON.Parse(aJSON);
                return true;
            }
            catch (Exception e)
            {
#if UNITY_5_3_OR_NEWER
                UnityEngine.Debug.LogException(e);
#endif
                aResult = null;
                return false;
            }
        }
    }
}
