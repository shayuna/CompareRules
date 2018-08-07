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
            for (int ii= Descendants.Count-1; ii>=0;ii--)
            {
                ComparableItem oDescendant = Descendants[ii];
                if (oDescendant.RelationTypeToAncestor != RelationType.IDENTICAL)
                {
                    HtmlNode oNodeToClone = oDescendant.Node;
                    while (oNodeToClone.ParentNode.ChildNodes.Count == 1) oNodeToClone = oNodeToClone.ParentNode;
                    HtmlNode oNode = oNodeToClone.Clone();
                    HtmlNode oNodeToWorkOn = oNode.QuerySelector(".hsubclausewrapper,.hkoteretseif,.hearot");
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

                    Helper.assignNodeFixedAttributes(oNodeToWorkOn,Convert.ToString(oDescendant.HokVersionID),oDescendant.IsNew);
                    if (sRelationTypeToAncestor != "") oNodeToWorkOn.SetAttributeValue("data-relationTypeToAncestor", sRelationTypeToAncestor);

                    HtmlNode oNodeToInsertAfter = Item.Node;
                    while (oNodeToInsertAfter.ParentNode.ChildNodes.Count == 1) oNodeToInsertAfter = oNodeToInsertAfter.ParentNode;
                    oNodeToInsertAfter.ParentNode.InsertAfter(oNode, oNodeToInsertAfter);
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
            oDoc.Save(@"c:\\" + oVersion.HokC + ".htm");
            return true;
        }
    }
}
