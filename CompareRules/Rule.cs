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
    }
}
