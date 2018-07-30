using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompareRules
{
    class ComparableItemID
    {
        private int iHokC, iPosition;
        public ComparableItemID (int iHokC,int iPosition)
        {
            this.iHokC = iHokC;
            this.iPosition = iPosition;
        }
        public int HokC
        {
            get
            {
                return iHokC;
            }
        }
        public int Position
        {
            get
            {
                return iPosition;
            }
        }
    }
}
