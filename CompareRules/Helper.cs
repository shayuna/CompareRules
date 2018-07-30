using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace CompareRules
{
    static class Helper
    {
        public static ICollection<HtmlNode> GetAllHtmlClausesInFileLoadedFromWeb(string sUrl)
        {
            HtmlWeb oHtmlWeb = new HtmlWeb();
            oHtmlWeb.OverrideEncoding = Encoding.GetEncoding(1255);
            var doc = oHtmlWeb.Load(sUrl);
            return doc.QuerySelectorAll(".hsubclausewrapper,.hkoteretseifin,.hearot");
        }
        public static IList<ComparableItem> FromHtmlNodesArrayToComparableItemsList(ICollection<HtmlNode> arNodes, RecordDetails rec)
        {

            IList<ComparableItem> arComparableItems = new List<ComparableItem>();
            int iPosition = 0;
            foreach (HtmlNode eNode in arNodes)
            {
                iPosition++;
                arComparableItems.Add(new ComparableItem(rec.ID, iPosition, null, RelationType.NONE, eNode));
            }
            return arComparableItems;
        }
        public static void CompareComparableItemsArrays(ref IList<ComparableItem> arComparableItemsA, ref IList<ComparableItem> arComparableItemsB)
        {
        }
    }
}
