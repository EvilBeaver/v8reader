using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml;

namespace V8Reader.Utils
{
    static class XMLMerge
    {
        public static void Perform(XElement dest, XElement source, IList<NSPair> nsMap)
        {
            InternalPerform(dest, source, nsMap);
        }

        private static void InternalPerform(XElement dest, XElement source, IList<NSPair> nsMap)
        {

            // Объявить все пространства в исходном документе,
            // заменяя префиксы у уже существующих.
            var nsDeclarations = dest.Attributes().Where<XAttribute>((attr, res) => { return attr.IsNamespaceDeclaration; }).ToList<XAttribute>();

            Dictionary<string, string> workMap = new Dictionary<string, string>();

            foreach (var mapItem in nsMap)
            {
                bool nsFound = false;
                foreach (var nsDecl in nsDeclarations)
                {
                    if (nsDecl.Value == mapItem.Uri)
                    {
                        nsFound = true;
                        workMap.Add(nsDecl.Value, mapItem.Prefix);
                    }
                }

                if (!nsFound)
                {
                    workMap.Add(mapItem.Uri, mapItem.Prefix);
                    dest.Add(new XAttribute(XNamespace.Xmlns + mapItem.Prefix, mapItem.Uri));
                }

            }

            XNamespace xsiNS = "http://www.w3.org/2001/XMLSchema-instance";
            char[] nsSep = new char[1];
            nsSep[0] = ':';

            foreach (XElement el in source.DescendantsAndSelf())
            {
                XNamespace defaultNS = el.GetDefaultNamespace();

                List<XAttribute> atList = el.Attributes(xsiNS.GetName("type")).ToList<XAttribute>();
                foreach (var attr in atList)
                {
                    string[] pair = attr.Value.Split(nsSep);
                    if (pair.Length == 2)
                    {
                        string workPrefix;
                        if (workMap.TryGetValue(defaultNS.NamespaceName, out workPrefix))
                        {
                            attr.Value = workPrefix + ":" + pair[1];
                        }
                    }
                    else
                    {
                        string workPrefix;
                        if (workMap.TryGetValue(defaultNS.NamespaceName, out workPrefix))
                        {
                            attr.Value = workPrefix + ":" + attr.Value;
                        }
                    }
                }
            }

            var nsSrcNS = source.Attributes().Where<XAttribute>((attr, res) => { return attr.IsNamespaceDeclaration; }).ToList<XAttribute>();
            foreach (var attr in nsSrcNS)
            {
                if (workMap.ContainsKey(attr.Value))
                {
                    attr.Remove();
                }
            }

            dest.Add(source);

        }

        public struct NSPair
        {
            public string Prefix;
            public string Uri;

            public static NSPair New(string prefix, string uri)
            {
                return new NSPair() { Prefix = prefix, Uri = uri };
            }
        }

        public class NamespaceMap : List<NSPair>
        {
            public void Add(string prefix, string uri)
            {
                Add(NSPair.New(prefix, uri));
            }
        }

    }

}
