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

        public RecordDetails Version
        {
            get
            {
                return oVersion;
            }
        }

        private void IterateOnItemDescendants(ComparableItem Item, IList<ComparableItem> Descendants,HtmlNode oNodeInDom)
        {
            bool bIsFirstAbsentInIteration = true;
            for (int ii = 0; ii < Descendants.Count; ii++)
            {
                ComparableItem oDescendant = Descendants[ii];
                HtmlNode oNodeToWorkOn = null;
                if (oDescendant.RelationTypeToAncestor != RelationType.IDENTICAL)
                {
                    HtmlNode oNodeToClone = oDescendant.Node;
                    while (Helper.getChildElements(oNodeToClone.ParentNode).Count == 1) oNodeToClone = oNodeToClone.ParentNode;
                    HtmlNode oNode = oNodeToClone.Clone();
                    oNodeToWorkOn = oNode.QuerySelector(".hsubclausewrapper,.hkoteretseif,.hearot");
                    if (oNodeToWorkOn == null) oNodeToWorkOn = oNode;
                    string sRelationTypeToAncestor = "";
                    switch (oDescendant.RelationTypeToAncestor)
                    {
                        case RelationType.ABSENT:
                            sRelationTypeToAncestor = "ABSENT";
                            break;
                        case RelationType.SIMILAR:
                            sRelationTypeToAncestor = "SIMILAR";
                            break;
                    }

                    Helper.assignNodeFixedAttributes(oNodeToWorkOn, Convert.ToString(oDescendant.HokVersionID), oDescendant.IsNew);
                    if (sRelationTypeToAncestor != "") oNodeToWorkOn.SetAttributeValue("data-relationtypetoancestor", sRelationTypeToAncestor);

                    while (Helper.getChildElements(oNodeInDom.ParentNode).Count == 1) oNodeInDom = oNodeInDom.ParentNode;
                    if (bIsFirstAbsentInIteration && oDescendant.RelationTypeToAncestor == RelationType.ABSENT)
                    {
                        while (oNodeInDom.QuerySelector(".hsubclausewrapper,.hkoteretseif,.hearot").GetAttributeValue("data-relationtypetoancestor", "") == "SIMILAR") oNodeInDom = oNodeInDom.PreviousSiblingElement();
                        oNodeInDom.ParentNode.InsertBefore(oNode, oNodeInDom);
                        bIsFirstAbsentInIteration = false;
                    }
                    else
                    {
                        while (oNodeInDom.NextSiblingElement() != null &&
                            oNodeInDom.NextSiblingElement().QuerySelector(".hsubclausewrapper,.hkoteretseif,.hearot") != null && 
                            oNodeInDom.NextSiblingElement().QuerySelector(".hsubclausewrapper,.hkoteretseif,.hearot").GetAttributeValue("data-relationtypetoancestor", "") == "SIMILAR") oNodeInDom = oNodeInDom.NextSiblingElement();
                        oNodeInDom.ParentNode.InsertAfter(oNode, oNodeInDom);
                    }
                    oNodeInDom = oNodeToWorkOn;
                }
                else if (oDescendant.IsNew)
                {
                    Helper.assignNodeFixedAttributes(Item.Node, Convert.ToString(oDescendant.HokVersionID), true);
                }
                IterateOnItemDescendants(Item, oDescendant.Descendants, oNodeInDom);
            }
        }
        public bool Serialize()
        {
            foreach (ComparableItem Item in arComparableItems)
            {
                if (Item.IsNew)
                {
                    Helper.assignNodeFixedAttributes(Item.Node,Convert.ToString(Item.HokVersionID),true);
                }
                else
                {
                    IterateOnItemDescendants(Item, Item.Descendants,Item.Node);
                }
            }
            oDoc.Save(@"d:\\inetpub\wwwroot\upload\hok_docsincludingversionsdeltas\"+oVersion.HokC+".htm");
//            oDoc.Save(@"c:\\allversionsincludedinrule\" + oVersion.HokC + ".htm");
            return true;
        }
    }
}
