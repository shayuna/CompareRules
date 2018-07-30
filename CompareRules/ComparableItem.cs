using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;

enum RelationType
{
    NONE,IDENTICAL,SIMILAR,ABSENT,PRESENT
}

namespace CompareRules
{
    class ComparableItem
    {
        private int iHokVersionID, iPosition, iRelatedTo;
        RelationType eRelationType;
        HtmlNode eNode;
        public ComparableItem(int iHokVersionID, int iPosition, int iRelatedTo, RelationType eRelationType, HtmlNode eNode)
        {
            this.iHokVersionID = iHokVersionID;
            this.iPosition = iPosition;
            this.iRelatedTo = iRelatedTo;
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

        public int RelatedTo
        {
            get
            {
                return iRelatedTo;
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
