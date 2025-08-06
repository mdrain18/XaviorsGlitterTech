using System.Xml;
using Verse;

namespace GlitterTech
{
    public class PatchOperationAddModExtension : PatchOperation
    {
        public string xpath;
        public DefModExtension value;

        protected override bool ApplyWorker(XmlDocument xml)
        {
            var nodes = xml.SelectNodes(xpath);
            if (nodes == null || nodes.Count == 0) return false;

            bool applied = false;

            foreach (XmlNode node in nodes)
            {
                XmlNode modExtensionsNode = null;

                foreach (XmlNode child in node.ChildNodes)
                {
                    if (child.Name == "modExtensions")
                    {
                        modExtensionsNode = child;
                        break;
                    }
                }

                if (modExtensionsNode == null)
                {
                    modExtensionsNode = xml.CreateElement("modExtensions");
                    node.AppendChild(modExtensionsNode);
                }

                XmlElement ext = xml.CreateElement("li");
                ext.SetAttribute("Class", "GlitterTech.ModExtension_PowerEfficiencyTarget");
                modExtensionsNode.AppendChild(ext);
                applied = true;
            }

            return applied;
        }
    }
}
