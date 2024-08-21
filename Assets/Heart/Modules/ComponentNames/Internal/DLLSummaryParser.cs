//#define DEBUG_XML_LOAD_EXCEPTIONS
//#define DEBUG_XML_LOAD_SUCCESS
//#define DEBUG_XML_LOAD_FAILED
//#define DEBUG_XML_LOAD_STEPS
//#define DEBUG_PARSE_COMMENT

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace Sisus
{
    /// <summary>
    /// Class that handles fetching summary tooltips for components
    /// from the XML documentation files of assembly DLL files.
    /// </summary>
    internal static class DLLSummaryParser
    {
        private static readonly Dictionary<Assembly, XmlDocument> cachedXMLDocuments = new();
        private static readonly StringBuilder stringBuilder = new(128);

        /// <summary>
        /// Attempts to find XML documentation file for dll that defines the given class type and parse XML documentation comments
        /// for the class.
        /// </summary>
        /// <param name="classType"> Type of the class whose members' tooltips we want. This cannot be null. </param>
        /// <param name="summary"> [out] The summary for the class. If no XML documentation is found, this will be set to an empty string. </param>
        /// <returns> True if tooltip was found. </returns>
        public static bool TryParseSummary([NotNull] Type classType, [CanBeNull] out string summary)
        {
            var assembly = classType.Assembly;
            XmlDocument xmlDocumentation;
            if (!cachedXMLDocuments.TryGetValue(assembly, out xmlDocumentation))
            {
                xmlDocumentation = GetXMLDocument(assembly);
                cachedXMLDocuments[assembly] = xmlDocumentation;
            }

            if (xmlDocumentation is null)
            {
                summary = "";
                return false;
            }

            var doc = xmlDocumentation["doc"];
            if (doc is null)
            {
#if DEV_MODE
				Debug.LogWarning("XML Documentation for assembly "+classType.Assembly.GetName().Name+" had no \"doc\" section");
#endif
                summary = "";
                return false;
            }

            var members = doc["members"];
            if (members is null)
            {
#if DEV_MODE
				Debug.LogWarning("XML Documentation for assembly "+classType.Assembly.GetName().Name+" had no \"members\" section under \"doc\"");
#endif
                summary = "";
                return false;
            }

            // For example:
            // T:UnityEngine.BoxCollider (class type)
            // T:Sisus.OdinSerializer.QueueFormatter`2 (class with two generic types)
            // F:UnityEngine.Camera.onPreCull (field)
            // P:UnityEngine.BoxCollider.size (property)
            // M:UnityEngine.AI.NavMeshAgent.CompleteOffMeshLink (method with no parameters)
            // M:UnityEngine.Collider.ClosestPoint(UnityEngine.Vector3) (method with parameters)
            // M:Namespace.ClassName`1.MethodName (method inside generic class)
            // M:Namespace.ClassName`1.MethodName(`0) (method inside generic class with parameter of class generic type)
            // M:Namespace.ClassName.#ctor (constructor with no parameters)
            // M:Namespace.ClassName.MethodName``1(``0[]) (method with generic parameter)
            // M:Namespace.ClassName.MethodName``2(System.Collections.Generic.Dictionary{``0,``1}) (method with two generic parameters)
            string match = "T:" + classType.FullName;

            foreach (object member in members)
            {
                // Skip non-XmlElement members to avoid exceptions
                if (!(member is XmlElement xmlElement))
                {
                    continue;
                }

                // skip members without attributes to avoid exceptions
                if (!xmlElement.HasAttributes)
                {
                    continue;
                }

                XmlAttributeCollection attributes = xmlElement.Attributes;
                XmlAttribute nameAttribute = attributes["name"];
                if (nameAttribute is null)
                {
                    continue;
                }

                string typePrefixAndFullName = nameAttribute.InnerText;
                if (string.IsNullOrEmpty(typePrefixAndFullName))
                {
                    continue;
                }

                if (typePrefixAndFullName.IndexOf(match, StringComparison.Ordinal) == -1)
                {
                    continue;
                }

                ParseXmlComment(xmlElement, stringBuilder);
                summary = stringBuilder.ToString();
                stringBuilder.Clear();
                return summary.Length > 0;
            }

            summary = "";
            return false;
        }

        /// <summary> Tries to find XML Documentation file for given assembly. </summary>
        /// <param name="assembly"> The assembly whose documentation file we want. This cannot be null. </param>
        /// <returns> XmlDocument containing the documentation for the assembly. Null if no documentation was found. </returns>
        [CanBeNull]
        private static XmlDocument GetXMLDocument([NotNull] Assembly assembly)
        {
            string xmlFilePath = GetXMLDocumentationFilepath(assembly);

            if (xmlFilePath.Length == 0)
            {
                return null;
            }

            using (var streamReader = new StreamReader(xmlFilePath))
            {
                var xmlDocument = new XmlDocument();
                try
                {
                    xmlDocument.Load(streamReader);
                    return xmlDocument;
                }
#if DEV_MODE && DEBUG_XML_LOAD_FAILED
				catch(Exception e)
				{
					Debug.LogWarning(e);
#else
                catch (Exception)
                {
#endif
                    return null;
                }
            }
        }

        [NotNull]
        private static string GetXMLDocumentationFilepath(Assembly assembly)
        {
            string dllPathWithFilePrefix = assembly.CodeBase;
            // convert from explicit to implicit filepath
            string dllPath = new Uri(dllPathWithFilePrefix).LocalPath;
            string directory = Path.GetDirectoryName(dllPath);
            string xmlFileName = Path.GetFileNameWithoutExtension(dllPath) + ".xml";
            string xmlFilePath = Path.Combine(directory, xmlFileName);

            if (File.Exists(xmlFilePath))
            {
#if DEV_MODE && DEBUG_XML_LOAD_SUCCESS
				Debug.Log("XML Found: "+ xmlFilePath);
#endif
                return xmlFilePath;
            }

#if DEV_MODE && DEBUG_XML_LOAD_FAILED
			Debug.LogWarning("XML documentation not found @ "+ xmlFilePath, null);
#endif
            return "";
        }

        public static void ParseXmlComment(string xmlComment, StringBuilder sb)
        {
            xmlComment = xmlComment.Trim();

            int charCount = xmlComment.Length;

            if (charCount == 0)
            {
                return;
            }

            if (xmlComment[0] != '<')
            {
#if DEV_MODE && DEBUG_PARSE_COMMENT
				Debug.Log("Returning whole xmlComment because first letter was not '<'\n\ninput:\n" + xmlComment);
#endif
                sb.Append(xmlComment);
                return;
            }

            XmlDocument doc;
            if (TryLoadXmlComment(xmlComment, out doc))
            {
#if DEV_MODE && DEBUG_PARSE_COMMENT
				Debug.Log("TryLoadXmlComment success:\n" + xmlComment);
#endif

                ParseXmlComment(doc.DocumentElement, sb);
                return;
            }

#if DEV_MODE && DEBUG_PARSE_COMMENT
			Debug.Log("TryLoadXmlComment failed:\n" + xmlComment);
#endif

            int openStart = 0;
            do
            {
                int openEnd = xmlComment.IndexOf('>', openStart + 1);
                if (openEnd == -1)
                {
#if DEV_MODE && DEBUG_PARSE_COMMENT
					Debug.Log("Returning whole xmlComment because could not find '>'\n\ninput:\n" + xmlComment);
#endif
                    sb.Append(xmlComment);
                    return;
                }

                int tagEnd = xmlComment.IndexOf(' ', openStart + 1);
                int from = openStart + 1;
                int fullNameLength = openEnd - from;
                int tagNameLength;
                if (tagEnd != -1 && tagEnd < openEnd)
                {
                    tagNameLength = tagEnd - from;
                }
                else
                {
                    tagNameLength = fullNameLength;
                }

                // tag name is part between last "<" and the following " " (if found before the next ">"
                string tagName = xmlComment.Substring(from, tagNameLength);

                string name;
                string body;

                // handle element without body like e.g. <inheritdoc cref="IDrawer.Label" />
                if (xmlComment[openEnd - 1] == '/')
                {
#if DEV_MODE && DEBUG_PARSE_COMMENT
					Debug.Log("Skipping to next '<' because element \""+xmlComment.Substring(openStart, openEnd - openStart + 1) + "\" had no body\n\ninput:\n" + xmlComment);
#endif

                    body = "";
                    fullNameLength--;
                    name = xmlComment.Substring(from, fullNameLength);

                    openStart = xmlComment.IndexOf('<', openEnd + 1);
                }
                else
                {
                    name = xmlComment.Substring(from, fullNameLength);

                    string closeTag = "</" + tagName + ">";
                    int closeStart = xmlComment.IndexOf(closeTag, openEnd + 1, StringComparison.Ordinal);
                    if (closeStart == -1)
                    {
#if DEV_MODE
						Debug.LogWarning("ParseXmlComment: failed to find closing tag for "+tagName+" so returning whole xmlComment");
#endif
                        sb.Append(xmlComment);
                        return;
                    }

                    from = openEnd + 1;
                    body = TrimAllLines(xmlComment.Substring(from, closeStart - from));

                    openStart = xmlComment.IndexOf('<', closeStart + closeTag.Length);
                }

                switch (tagName)
                {
                    case "summary":
                        name = "";
                        break;
                    case "param":
                        if (name.StartsWith("param name=\"", StringComparison.OrdinalIgnoreCase))
                        {
                            name = name.Substring(12, name.Length - 13);
                        }

                        break;
                    case "typeparam":
                        if (name.StartsWith("typeparam name=\"", StringComparison.OrdinalIgnoreCase))
                        {
                            name = name.Substring(16, name.Length - 17);
                        }

                        break;
                    case "exception":
                        if (name.StartsWith("exception cref=\"", StringComparison.OrdinalIgnoreCase))
                        {
                            name = name.Substring(16, name.Length - 17);
                        }

                        break;
                }

                AddTooltipLine(name, body, sb);
            } while (openStart != -1);
        }

        public static bool TryLoadXmlComment(string xmlComment, out XmlDocument doc)
        {
            doc = new XmlDocument();
            try
            {
                doc.LoadXml(xmlComment);
                return true;
            }
#if DEV_MODE && DEBUG_XML_LOAD_FAILED
			catch(XmlException e)
			{
				Debug.LogWarning(e);
				return false;
			}
#else
            catch (XmlException)
            {
                return false;
            }
#endif
        }

        public static void ParseXmlComment(XmlElement xmlElement, StringBuilder sb)
        {
            if (!xmlElement.HasAttributes)
            {
#if DEV_MODE && DEBUG_PARSE_COMMENT
				Debug.Log("C \"" + xmlElement.Name + "\" (Loc=" + xmlElement.LocalName + ", Pre=" + xmlElement.Prefix + ", Val=" + xmlElement.Value + ")\nInnerXml:\n" + xmlElement.InnerXml.Replace("><", ">\r\n<") + "\n\nInnerText:\n"+ xmlElement.InnerText+"\n\nOuterXml:"+xmlElement.OuterXml.Replace("><", ">\r\n<"));
#endif
                AddTooltipLine(xmlElement.InnerText, sb);
                return;
            }

#if DEV_MODE && DEBUG_PARSE_COMMENT
			Debug.Log("P("+ xmlElement.Attributes.Count + ") \"" + xmlElement.Name + "\" (Loc=" + xmlElement.LocalName+ ", Pre="+ xmlElement.Prefix+ ", Val="+ xmlElement.Value+")\nInnerXml:\n" + xmlElement.InnerXml.Replace("><",">\r\n<")+ "\n\nInnerText:\n"+ xmlElement.InnerText + "\n\nOuterXml:" + xmlElement.OuterXml.Replace("><", ">\r\n<")+ "\n\nxmlElement[\"name\"]=" + (xmlElement["name"] == null ? "null" : xmlElement["name"].Name));
#endif

            switch (xmlElement.Name)
            {
                case "param":
                case "typeparam":
                case "exception":
                    var name = xmlElement["name"];
                    if (name != null)
                    {
#if DEV_MODE
						Debug.Log(xmlElement.Name+"[\"name\"]: \""+name.InnerText+"\"");
#endif
                        AddTooltipLine(name.InnerText, xmlElement.InnerText, sb);
                        return;
                    }

                    var outerXml = xmlElement.OuterXml;

                    string beforeName = "<" + xmlElement.Name + " name=\"";
                    if (outerXml.StartsWith(beforeName, StringComparison.OrdinalIgnoreCase))
                    {
                        int nameStart = beforeName.Length;
                        int nameEnd = outerXml.IndexOf("\">", nameStart, StringComparison.Ordinal);
                        if (nameEnd != -1)
                        {
                            string parsedName = ObjectNames.NicifyVariableName(outerXml.Substring(nameStart, nameEnd - nameStart));
#if DEV_MODE && DEBUG_PARSE_COMMENT
							Debug.Log(xmlElement.Name + " name parsed: \""+parsedName + "\"\nouterXml:\n\n" + outerXml);
#endif
                            AddTooltipLine(parsedName, xmlElement.InnerText, sb);
                            return;
                        }
                    }

#if DEV_MODE
					Debug.LogWarning(xmlElement.Name+": failed to get name...\nouterXml:\n\n" + outerXml);
#endif

                    AddTooltipLine(xmlElement.InnerText, sb);
                    return;
            }

            foreach (var item in xmlElement)
            {
                var element = item as XmlElement;
                if (element == null)
                {
#if DEV_MODE
					Debug.LogWarning("item "+item.GetType()+" of NodeType "+((XmlNode)item).NodeType+" was not of type XmlElement");
#endif
                    continue;
                }

                ParseXmlComment(element, sb);
            }
        }

        private static void AddTooltipLine(string line, StringBuilder sb)
        {
            if (sb.Length > 0)
            {
                sb.Append('\n');
            }

            sb.Append(line.Trim());
        }

        private static void AddTooltipLine(string name, string body, StringBuilder sb)
        {
            if (sb.Length > 0)
            {
                sb.Append('\n');
                sb.Append('\n');
            }

            if (name.Length > 0)
            {
                sb.Append(ObjectNames.NicifyVariableName(name));
                if (body.Length > 0)
                {
                    sb.Append(" : ");
                    sb.Append(body);
                }
            }
            else
            {
                sb.Append(body);
            }
        }

        private static string TrimAllLines(string input)
        {
            input = input.Trim();

            for (int i = input.IndexOf('\n'); i != -1; i = input.IndexOf('\n', i + 1))
            {
                input = input.Substring(0, i + 1) + input.Substring(i + 1).TrimStart();
            }

            return input;
        }
    }
}
#endif