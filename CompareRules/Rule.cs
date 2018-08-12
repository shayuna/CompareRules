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


        private void IterateOnItemDescendants(ComparableItem Item,IList<ComparableItem> Descendants)
        {
            HtmlNode oLastInsertedNode = Item.Node;
            for (int ii=0;ii<Descendants.Count;ii++)
            {
                ComparableItem oDescendant = Descendants[ii];
                if (oDescendant.RelationTypeToAncestor != RelationType.IDENTICAL)
                {
                    HtmlNode oNodeToClone = oDescendant.Node;
                    while (Helper.getChildElements(oNodeToClone.ParentNode).Count == 1) oNodeToClone = oNodeToClone.ParentNode;
                    HtmlNode oNode = oNodeToClone.Clone();
                    HtmlNode oNodeToWorkOn = oNode.QuerySelector(".hsubclausewrapper,.hkoteretseif,.hearot");
                    if (oNodeToWorkOn == null) oNodeToWorkOn = oNode;
                    HtmlNode oNodeToRelateTo = oLastInsertedNode;

                    string sRelationTypeToAncestor = "";
                    switch (oDescendant.RelationTypeToAncestor)
                    {
                        case RelationType.ABSENT:
                            sRelationTypeToAncestor = "ABSENT";
                            oNodeToRelateTo = Item.Node;
                            break;
                        case RelationType.SIMILAR:
                            sRelationTypeToAncestor = "SIMILAR";
                            break;
                    }

                    Helper.assignNodeFixedAttributes(oNodeToWorkOn,Convert.ToString(oDescendant.HokVersionID),oDescendant.IsNew);
                    if (sRelationTypeToAncestor != "") oNodeToWorkOn.SetAttributeValue("data-relationtypetoancestor", sRelationTypeToAncestor);

                    while (Helper.getChildElements(oNodeToRelateTo.ParentNode).Count == 1) oNodeToRelateTo = oNodeToRelateTo.ParentNode;
                    if (oDescendant.RelationTypeToAncestor == RelationType.ABSENT)
                    {
                        oNodeToRelateTo.ParentNode.InsertBefore(oNode, oNodeToRelateTo);
                    }
                    else
                    {
                        oNodeToRelateTo.ParentNode.InsertAfter(oNode, oNodeToRelateTo);
                    }
                    oLastInsertedNode = oNode;
                }
                else if (oDescendant.IsNew)
                {
                    Helper.assignNodeFixedAttributes(Item.Node, Convert.ToString(oDescendant.HokVersionID),true);
                }
                IterateOnItemDescendants(Item, oDescendant.Descendants);
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
                    IterateOnItemDescendants(Item, Item.Descendants);
                }
            }
            //            oDoc.Save(@"d:\\inetpub\wwwroot\upload\hok_docsincludingversionsdeltas\"+oVersion.HokC+".htm");
            oDoc.Save(@"c:\\allversionsincludedinrule\" + oVersion.HokC + ".htm");
            return true;
        }
    }
}
