using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace CompareRules
{
    class Rule
    {
        private RecordDetails oVersion;
        private HtmlDocument oDoc;
        private ICollection<HtmlNode> arNodes;
        private IList<ComparableItem> arComparableItems;

        public Rule(RecordDetails oVersion)
        {
            this.oVersion = oVersion;
            oDoc = Helper.GetHtmlDocFromUrl("http://www.lawdata.co.il/lawdata_face_lift_test/gethok.asp?flnm=" + oVersion.HokC + "_" + oVersion.ID);
            arNodes = Helper.GetAllHtmlClausesInHtmlDocument(oDoc);
            arComparableItems = Helper.FromHtmlNodesArrayToComparableItemsList(arNodes, oVersion);
        }

        public IList<ComparableItem> ComparableItems
        {
            get
            {
                return arComparableItems;
            }
        }

        private void IterateOnItemDescendants(ComparableItem Item,IList<ComparableItem> Descendants)
        {
            foreach (ComparableItem oDescendant in Descendants)
            {
                if (oDescendant.AncestorRelationType != RelationType.IDENTICAL)
                {
                    HtmlNode oNode = oDescendant.Node.Clone();
//                    oNode.InnerHtml = "<span>"+oDescendant.AncestorRelationType + " *** " + oDescendant.HokVersionID + " *** " + "</span>"+oNode.InnerHtml;
                    oNode.SetAttributeValue("class", "appended");
                    oNode.SetAttributeValue("data-version", Convert.ToString(oDescendant.HokVersionID));
                    oNode.SetAttributeValue("style", "border:10px solid red");
                    Item.Node.AppendChild(oNode);
                }
                IterateOnItemDescendants(Item, oDescendant.Descendants);
            }
        }

        public bool Serialize()
        {
            foreach (ComparableItem Item in arComparableItems)
            {
                IterateOnItemDescendants(Item, Item.Descendants);
            }
            oDoc.Save(@"c:\\testHok_"+oVersion.HokC+".htm");
            return true;
        }
    }
}
