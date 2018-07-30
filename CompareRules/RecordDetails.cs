using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompareRules
{
    class RecordDetails
    {
        private int iID, iHokC;

        public RecordDetails (int iID,int iHokC)
        {
            this.iID = iID;
            this.iHokC = iHokC;
        }

        public int ID
        {
            get
            {
                return iID;
            }
        }
        public int HokC
        {
            get
            {
                return iHokC;
            }
        }
    }
}
