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
        private ComparableItem[] oDescendants;
        RelationType eAncestorRelationType;
        HtmlNode eNode;
        public ComparableItem(int iHokVersionID, int iPosition, ComparableItem[] oDescendants, RelationType eAncestorRelationType, HtmlNode eNode)
        {
            this.iHokVersionID = iHokVersionID;
            this.iPosition = iPosition;
            this.oDescendants= oDescendants;
            this.eAncestorRelationType = eAncestorRelationType;
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

        public ComparableItem[] Descendants
        {
            get
            {
                return oDescendants;
            }
        }

        public RelationType AncestorRelationType
        {
            get
            {
                return eAncestorRelationType;
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
