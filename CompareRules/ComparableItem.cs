using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace CompareRules
{
    enum RelationType
    {
        NONE, IDENTICAL, SIMILAR, COMES_AFTER, ABSENT
    }
    class ComparableItem
    {
        private int iHokVersionID, iPosition;
        private IList<ComparableItem> lDescendants = new List<ComparableItem>();
        RelationType eAncestorRelationType=RelationType.NONE;
        HtmlNode eNode;
        public ComparableItem(int iHokVersionID, int iPosition, HtmlNode eNode)
        {
            this.iHokVersionID = iHokVersionID;
            this.iPosition = iPosition;
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

        public bool addDescendant(ComparableItem oDescendant)
        {
            lDescendants.Add(oDescendant);
            return true;
        }

        public IList<ComparableItem> Descendants
        {
            get
            {
                return lDescendants;
            }
        }

        public RelationType AncestorRelationType
        {
            get
            {
                return eAncestorRelationType;
            }
            set
            {
                eAncestorRelationType=value;
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
