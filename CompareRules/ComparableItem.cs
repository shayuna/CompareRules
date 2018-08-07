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
        EMPTY, IDENTICAL, SIMILAR, DIFFERENT, ABSENT
    }
    class ComparableItem
    {
        private int iHokVersionID, iPosition;
        private IList<ComparableItem> lDescendants = new List<ComparableItem>();
        RelationType eRelationTypeToAncestor=RelationType.EMPTY;
        bool bIsNew = false;
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

        public RelationType RelationTypeToAncestor
        {
            get
            {
                return eRelationTypeToAncestor;
            }
            set
            {
                eRelationTypeToAncestor=value;
            }
        }
        public bool IsNew
        {
            get
            {
                return bIsNew;
            }
            set
            {
                bIsNew = value;
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
