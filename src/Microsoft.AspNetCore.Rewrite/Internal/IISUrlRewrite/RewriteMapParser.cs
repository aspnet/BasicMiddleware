using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Microsoft.AspNetCore.Rewrite.Internal.IISUrlRewrite
{
    public class RewriteMapParser
    {
        public IDictionary<string, IISRewriteMap> Parse(XElement xmlRoot)
        {
            var mapsElement = xmlRoot.Descendants(RewriteTags.RewriteMaps).SingleOrDefault();
            if (mapsElement == null)
            {
                return null;
            }

            var rewriteMaps = new Dictionary<string, IISRewriteMap>();
            foreach (XElement mapElement in mapsElement.Elements(RewriteTags.RewriteMap))
            {
                var map = new IISRewriteMap(mapElement.Attribute(RewriteTags.Name)?.Value);
                foreach (XElement addElement in mapElement.Elements(RewriteTags.Add))
                {
                    map.AddOrUpdateEntry(addElement.Attribute(RewriteTags.Key)?.Value, addElement.Attribute(RewriteTags.Value)?.Value);
                }
                rewriteMaps.Add(map.Name, map);
            }

            return rewriteMaps;
        }
    }
}