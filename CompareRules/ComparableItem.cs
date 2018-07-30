using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;

enum RelationType
{
    NONE,IDENTICAL,SIMILAR,COMES_AFTER,ABSENT
}

namespace CompareRules
{
    class ComparableItem
    {
        private int iHokVersionID, iPosition;
        private ComparableItemID oRelatedTo;
        RelationType eRelationType;
        HtmlNode eNode;
        public ComparableItem(int iHokVersionID, int iPosition, ComparableItemID oRelatedTo, RelationType eRelationType, HtmlNode eNode)
        {
            this.iHokVersionID = iHokVersionID;
            this.iPosition = iPosition;
            this.oRelatedTo = oRelatedTo;
            this.eRelationType = eRelationType;
            this.eNode = eNode;
        }

        public int HokVersionID
        {
            get
            {
                return iHokVersionID;
            }
        }

        public int Position
        {
            get
            {
                return iPosition;
            }
        }

        public ComparableItemID RelatedTo
        {
            get
            {
                return oRelatedTo;
            }
        }

        public RelationType RelationType
        {
            get
            {
                return eRelationType;
            }
        }

        public HtmlNode Node
        {
            get
            {
                return eNode;
            }
        }
    }
}
