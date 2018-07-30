using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Text.RegularExpressions;

enum ComparisonResults
{
    DIFFERENT,IDENTICAL,SIMILAR
}

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
        public static void CompareComparableItemsStores(ref IList<ComparableItem> arComparableItemsA, ref IList<ComparableItem> arComparableItemsB)
        {
        }
        public static ComparisonResults CompareTwoHtmlElements(HtmlNode eNodeA,HtmlNode eNodeB)
        {
            string[] arWordsA = Helper.FromTxtToWords(eNodeA.InnerText);
            string[] arWordsB = Helper.FromTxtToWords(eNodeB.InnerText);
            ComparisonResults rslt = ComparisonResults.DIFFERENT;

            int iWordsFrom2In1 = 0, iWordsFrom1In2 = 0;
            for (int jj = 0; jj < arWordsA.Length; jj++)
            {
                for (int kk = 0; kk < arWordsB.Length; kk++)
                {
                    if (arWordsA[jj] == arWordsB[kk])
                    {
                        iWordsFrom2In1++;
                        break;
                    }
                }
            }
            for (int jj = 0; jj < arWordsB.Length; jj++)
            {
                for (int kk = 0; kk < arWordsA.Length; kk++)
                {
                    if (arWordsB[jj] == arWordsA[kk])
                    {
                        iWordsFrom1In2++;
                        break;
                    }
                }
            }
            if (Regex.Replace(eNodeA.InnerText, @"[\s\r\n\t.,;:]+", "") == Regex.Replace(eNodeB.InnerText, @"[\s\r\n\t.,;:]+", "")) rslt = ComparisonResults.IDENTICAL;
            else if (((iWordsFrom2In1 / arWordsA.Length >= 0.6) && arWordsA.Length >= 10 && arWordsA.Length * 2.5 > arWordsB.Length) ||
                    ((iWordsFrom1In2 / arWordsB.Length >= 0.6) && arWordsB.Length >= 10 && arWordsB.Length * 2.5 > arWordsA.Length) ||
                    (eNodeA.QuerySelector(".hkoteretseifin") != null && eNodeB.QuerySelector(".hkoteretseifin") != null && iWordsFrom1In2 / arWordsB.Length >= 0.5 && iWordsFrom2In1 / arWordsA.Length >= 0.5))
                rslt = ComparisonResults.SIMILAR;

            return rslt;
        }
        public static string[] FromTxtToWords(string sTxt)
        {
            string sPattern = @"[\-, +[\](){ }.!';:"" ?\s]";
            string[] arTxt = Regex.Split(sTxt, sPattern);
            return arTxt;
        }

    }
}

