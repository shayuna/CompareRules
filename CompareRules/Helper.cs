using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Text.RegularExpressions;

namespace CompareRules
{
    enum Turn
    {
        A, B
    }

    static class Helper
    {
        public static ICollection<HtmlNode> GetAllHtmlClausesInFileLoadedFromWeb(string sUrl)
        {
            return Helper.GetAllHtmlClausesInHtmlDocument(Helper.GetHtmlDocFromUrl(sUrl));
        }
        public static ICollection<HtmlNode> GetAllHtmlClausesInHtmlDocument(HtmlDocument oDoc)
        {
            return oDoc.QuerySelectorAll(".hsubclausewrapper,.hkoteretseifin,.hearot");
        }
        public static HtmlDocument GetHtmlDocFromUrl(string sUrl)
        {
            HtmlWeb oHtmlWeb = new HtmlWeb();
            oHtmlWeb.OverrideEncoding = Encoding.GetEncoding(1255);
            return oHtmlWeb.Load(sUrl);
        }
        public static IList<ComparableItem> FromHtmlNodesArrayToComparableItemsList(ICollection<HtmlNode> arNodes, RecordDetails rec)
        {

            IList<ComparableItem> arComparableItems = new List<ComparableItem>();
            int iPosition = 0;
            foreach (HtmlNode eNode in arNodes)
            {
                iPosition++;
                arComparableItems.Add(new ComparableItem(rec.ID, iPosition, eNode));
            }
            return arComparableItems;
        }
        public static void CompareComparableItemsStores(ref IList<ComparableItem> arComparableItemsA, ref IList<ComparableItem> arComparableItemsB)
        {
            int iIndexInA = 0, iIndexInB = 0, iIncrementA = 0, iIncrementB = 0;
            Turn eTurn=Turn.A;

            bool bContinue = true;

            while (bContinue && iIndexInA<arComparableItemsA.Count && iIndexInB<arComparableItemsB.Count)
            {
                RelationType eRslt;
                if (eTurn==Turn.A) eRslt = Helper.CompareTwoHtmlElements(arComparableItemsA[iIndexInA].Node, arComparableItemsB[iIndexInB + iIncrementB].Node);
                else eRslt = Helper.CompareTwoHtmlElements(arComparableItemsA[iIndexInA + iIncrementA].Node, arComparableItemsB[iIndexInB].Node);
                if (
                    eRslt == RelationType.IDENTICAL || 
                    eRslt == RelationType.SIMILAR || 
                    iIncrementA>100 || 
                    iIncrementB>100 || 
                    iIndexInA+iIncrementA+1>=arComparableItemsA.Count || 
                    iIndexInB+iIncrementB+1>=arComparableItemsB.Count
                    )
                {
                    if (eRslt==RelationType.DIFFERENT)
                    {
                        if (
                            iIncrementA > 100 ||
                            iIncrementB > 100 ||
                            iIndexInA + iIncrementA+1>= arComparableItemsA.Count
                        )
                        {
                            arComparableItemsB[iIndexInB].AncestorRelationType = RelationType.COMES_AFTER;
                            arComparableItemsA[iIndexInA].addDescendant(arComparableItemsB[iIndexInB]);
                        }
                        iIncrementA = 0;
                        iIncrementB = 0;
                    }
                    else if (eTurn == Turn.A)
                    {
                        arComparableItemsB[iIndexInB + iIncrementB].AncestorRelationType = eRslt;
                        for (int ii = 0;ii< iIncrementB; ii++)
                        {
                            arComparableItemsB[iIndexInB + ii].AncestorRelationType = RelationType.ABSENT;
                            arComparableItemsA[iIndexInA].addDescendant(arComparableItemsB[iIndexInB + ii]);
                        }
                        arComparableItemsA[iIndexInA].addDescendant(arComparableItemsB[iIndexInB + iIncrementB]);
                    }
                    else
                    {
                        arComparableItemsB[iIndexInB].AncestorRelationType = eRslt;
/* we don't need to relate to to elements in b. if the descendants array is empty, it means that it isn't found in the previous version. plus, another problem: we can't tag one ComparabaleItem object with two AncestorRelationType tags. (here we should use both COME_AFTER and SIMILAR/IDENTICAL*/
/*
                        for (int ii = 0; ii < iIncrementA; ii++)
                        {
                            arComparableItemsA[iIndexInA + ii].addDescendant(arComparableItemsB[iIndexInB]);
                        }
                        arComparableItemsB[iIndexInB].AncestorRelationType = RelationType.COMES_AFTER;
*/
                        arComparableItemsA[iIndexInA + iIncrementA].addDescendant(arComparableItemsB[iIndexInB]);
                    }
                    if (iIncrementA == 0 && iIncrementB == 0)
                    {
                        iIndexInA++;
                        iIndexInB++;
                    }
                    else if (eTurn == Turn.A)
                    {
                        iIndexInA++;
                        iIndexInB += iIncrementB + 1;
                    }
                    else
                    {
                        iIndexInA+= iIncrementA + 1;
                        iIndexInB++;
                    }
                    iIncrementA = 0;
                    iIncrementB = 0;
                }
                else
                {
                    if (eTurn == Turn.A)
                    {
                        iIncrementA++;
                        eTurn = Turn.B;
                    }
                    else
                    {
                        iIncrementB++;
                        eTurn = Turn.A;
                    }
                }
            }
        }
        public static RelationType CompareTwoHtmlElements(HtmlNode eNodeA,HtmlNode eNodeB)
        {
            string[] arWordsA = Helper.FromTxtToWords(eNodeA.InnerText);
            string[] arWordsB = Helper.FromTxtToWords(eNodeB.InnerText);
            RelationType rslt = RelationType.DIFFERENT;

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
            if (Regex.Replace(eNodeA.InnerText, @"[\s\r\n\t.,;:]+", "") == Regex.Replace(eNodeB.InnerText, @"[\s\r\n\t.,;:]+", "")) rslt = RelationType.IDENTICAL;
            else if ((((double)iWordsFrom2In1 / arWordsA.Length >= 0.6) && arWordsA.Length >= 10 && arWordsA.Length * 2.5 > arWordsB.Length) ||
                    (((double)iWordsFrom1In2 / arWordsB.Length >= 0.6) && arWordsB.Length >= 10 && arWordsB.Length * 2.5 > arWordsA.Length) ||
                    (eNodeA.QuerySelector(".hkoteretseifin") != null && eNodeB.QuerySelector(".hkoteretseifin") != null && (double)iWordsFrom1In2 / arWordsB.Length >= 0.5 && (double)iWordsFrom2In1 / arWordsA.Length >= 0.5))
                rslt = RelationType.SIMILAR;

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

