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
            foreach (var mapElement in mapsElement.Elements(RewriteTags.RewriteMap))
            {
                var map = new IISRewriteMap(mapElement.Attribute(RewriteTags.Name)?.Value);
                foreach (var addElement in mapElement.Elements(RewriteTags.Add))
                {
                    map.AddOrUpdateEntry(addElement.Attribute(RewriteTags.Key).Value.ToLowerInvariant(), addElement.Attribute(RewriteTags.Value).Value);
                }
                rewriteMaps.Add(map.Name, map);
            }

            return rewriteMaps;
        }
    }
}