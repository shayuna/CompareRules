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

                    oNodeToWorkOn.SetAttributeValue("class", oNodeToWorkOn.GetAttributeValue("class","") + " " + "changedInTime");
                    oNodeToWorkOn.SetAttributeValue("data-version", Convert.ToString(oDescendant.HokVersionID));
                    if (sRelationTypeToAncestor != "") oNodeToWorkOn.SetAttributeValue("data-relationTypeToAncestor", sRelationTypeToAncestor);

                    if (oDescendant.IsNew) SetNodeAsNew(oNodeToWorkOn);

                    HtmlNode oNodeToInsertAfter = Item.Node;
                    while (oNodeToInsertAfter.ParentNode.ChildNodes.Count == 1) oNodeToInsertAfter = oNodeToInsertAfter.ParentNode;
                    oNodeToInsertAfter.ParentNode.InsertAfter(oNode, oNodeToInsertAfter);
                }
                else if (oDescendant.IsNew)
                {
                    Item.Node.SetAttributeValue("class", Item.Node.GetAttributeValue("class", "") + " " + "changedInTime");
                    Item.Node.SetAttributeValue("data-version", Convert.ToString(oDescendant.HokVersionID));
                    SetNodeAsNew(Item.Node);
                }
                IterateOnItemDescendants(Item, oDescendant.Descendants);
            }
        }
        private void IterateOnItemDescendants_old(ComparableItem Item, IList<ComparableItem> Descendants)
        {
            foreach (ComparableItem oDescendant in Descendants)
            {
                if (oDescendant.RelationTypeToAncestor != RelationType.IDENTICAL)
                {
                    HtmlNode oNode = oDescendant.Node.Clone();

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

                    oNode.SetAttributeValue("class", "changedInTime");
                    oNode.SetAttributeValue("data-version", Convert.ToString(oDescendant.HokVersionID));
                    if (sRelationTypeToAncestor != "") oNode.SetAttributeValue("data-relationTypeToAncestor", sRelationTypeToAncestor);

                    if (oDescendant.Descendants.Count == 0) SetNodeAsNew(oNode);
                    /*

                                        // debugging part - start
                                        //here is the debugging part. for debugging purposes only, i want to mark the different ancestor-descendant relations with different border colors;

                    //                    oNode.InnerHtml = "<span>"+oDescendant.RelationTypeToAncestor + " *** " + oDescendant.HokVersionID + " *** " + "</span>"+oNode.InnerHtml;

                                        string sBorderColor = "",sFontColor="black";
                                        switch (oDescendant.RelationTypeToAncestor)
                                        {
                                            case RelationType.ABSENT:
                                                sBorderColor = "red";
                                                break;
                                            case RelationType.SIMILAR:
                                                sBorderColor = "green";
                                                break;
                                            default:
                                                sBorderColor = "black";
                                                break;
                                        }
                                        if (sIsNew == "1")
                                        {
                                            sFontColor = "orange";
                                        }
                                        oNode.SetAttributeValue("style", "border:2px solid " + sBorderColor + ";color:" + sFontColor);
                                        // debugging part - end
                      */
                    Item.Node.AppendChild(oNode);

                }
                IterateOnItemDescendants(Item, oDescendant.Descendants);
            }
        }
        public bool Serialize()
        {
            foreach (ComparableItem Item in arComparableItems)
            {
                if (Item.Descendants.Count > 0)
                {
                    IterateOnItemDescendants(Item, Item.Descendants);
                }
                else
                {
                    SetNodeAsNew(Item.Node);
                }
            }
            //            oDoc.Save(@"d:\\inetpub\wwwroot\upload\hok_docsincludingversionsdeltas\"+oVersion.HokC+".htm");
            oDoc.Save(@"c:\\" + oVersion.HokC + ".htm");
            return true;
        }
        private bool SetNodeAsNew(HtmlNode oNode)
        {
            bool bRslt = true;
            try
            {
                oNode.SetAttributeValue("data-isnew", "1");
            }
            catch (Exception ex)
            {
                bRslt = false;
                Console.WriteLine("error in SetNodeAsNew. exception is - " + ex.Message);
            }
            return bRslt;

        }
    }
}
