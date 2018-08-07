using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using System.Net;
using System.Data.SqlClient;

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
            return oDoc.QuerySelectorAll(".hsubclausewrapper,.hkoteretseif,.hearot");
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
                    (iIndexInA+iIncrementA+1>=arComparableItemsA.Count && 
                    iIndexInB+iIncrementB+1>=arComparableItemsB.Count)
                    )
                {
                    if (eRslt==RelationType.DIFFERENT)
                    {
                        arComparableItemsB[iIndexInB].RelationTypeToAncestor = RelationType.ABSENT;
                        arComparableItemsA[iIndexInA].addDescendant(arComparableItemsB[iIndexInB]);
                        arComparableItemsA[iIndexInA].IsNew = true;
                        iIncrementA = 0;
                        iIncrementB = 0;
                    }
                    else if (eTurn == Turn.A)
                    {
                        arComparableItemsB[iIndexInB + iIncrementB].RelationTypeToAncestor = eRslt;
                        arComparableItemsA[iIndexInA].addDescendant(arComparableItemsB[iIndexInB + iIncrementB]);
                        for (int ii = 0;ii< iIncrementB; ii++)
                        {
                            arComparableItemsB[iIndexInB + ii].RelationTypeToAncestor = RelationType.ABSENT;
                            arComparableItemsA[iIndexInA].addDescendant(arComparableItemsB[iIndexInB + ii]);
                        }
                    }
                    else
                    {
                        arComparableItemsB[iIndexInB].RelationTypeToAncestor = eRslt;
                        arComparableItemsA[iIndexInA + iIncrementA].addDescendant(arComparableItemsB[iIndexInB]);
                        for (int ii = 0; ii < iIncrementA; ii++)
                        {
                            arComparableItemsA[iIndexInA + ii].IsNew = true;
                        }
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
                    if (eTurn == Turn.A && iIndexInA+iIncrementA+1 < arComparableItemsA.Count)
                    {
                        iIncrementA++;
                        eTurn = Turn.B;
                    }
                    else if (eTurn == Turn.B && iIndexInB + iIncrementB+1 < arComparableItemsB.Count)
                    {
                        iIncrementB++;
                        eTurn = Turn.A;
                    }
                    else if (iIndexInA + iIncrementA + 1 < arComparableItemsA.Count)
                    {
                        iIncrementA++;
                    }
                    else if (iIndexInB + iIncrementB + 1 < arComparableItemsB.Count)
                    {
                        iIncrementB++;
                    }
                    else
                    {
                        bContinue = false;
                    }
                }
            }
        }
        public static RelationType CompareTwoHtmlElements(HtmlNode eNodeA,HtmlNode eNodeB)
        {
            string sTxtA = WebUtility.HtmlDecode(eNodeA.InnerText);
            string sTxtB = WebUtility.HtmlDecode(eNodeB.InnerText);
            string[] arWordsA = Helper.FromTxtToWords(sTxtA);
            string[] arWordsB = Helper.FromTxtToWords(sTxtB);
            RelationType rslt = RelationType.DIFFERENT;

            string sClassA = String.Join(",",eNodeA.GetClassList());
            string sClassB = String.Join(",", eNodeB.GetClassList());


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
            if (Regex.Replace(sTxtA, @"[\s\r\n\t.,;:]+", "") == Regex.Replace(sTxtB, @"[\s\r\n\t.,;:]+", "")) rslt = RelationType.IDENTICAL;
            else if ((((double)iWordsFrom2In1 / arWordsA.Length >= 0.6) && arWordsA.Length >= 10 && arWordsA.Length * 2.5 > arWordsB.Length) ||
                    (((double)iWordsFrom1In2 / arWordsB.Length >= 0.6) && arWordsB.Length >= 10 && arWordsB.Length * 2.5 > arWordsA.Length) ||
                    (((sClassA.Contains("hkoteretseif") && sClassB.Contains("hkoteretseif")) || (sClassA.Contains("hearot") && sClassB.Contains("hearot"))) && (double)iWordsFrom1In2 / arWordsB.Length >= 0.5 && (double)iWordsFrom2In1 / arWordsA.Length >= 0.5))
                rslt = RelationType.SIMILAR;
            return rslt;
        }
        public static string[] FromTxtToWords(string sTxt)
        {
            string sPattern = @"[\-, +[\](){ }.!';:"" ?\s]+";
            string[] arTxt = Regex.Split(sTxt.Trim(), sPattern);
            return arTxt;
        }
        public static bool WriteToDB(int C)
        {
            bool bRslt = true;
            try
            {
                string sDataSrc = "192.168.200.4";
                string sConnStr = "Initial Catalog=LawData;User ID=sa;Password=;Data Source=" + sDataSrc;
                SqlConnection connWrite = new SqlConnection(sConnStr);
                connWrite.Open();
                SqlCommand cmdWrite = new SqlCommand();
                cmdWrite.Connection = connWrite;
                cmdWrite.CommandType = System.Data.CommandType.Text;
                cmdWrite.CommandText = "insert into Hok_DocsIncludingVersionsDeltas (c) values (@c)";
                cmdWrite.Parameters.AddWithValue("@c", C);
                cmdWrite.ExecuteNonQuery();
                cmdWrite.Dispose();
                connWrite.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("oops. something happened when trying to write data to db. hokc=" + C + " exception is - " + ex.Message);
                bRslt = false;
            }
            return bRslt;
        }
        public static bool assignNodeFixedAttributes(HtmlNode oNode, string sHokVersionID,bool bSetNew)
        {
            bool bRslt = true;
            try
            {
                oNode.SetAttributeValue("class", oNode.GetAttributeValue("class", "") + " " + "changedInTime");
                oNode.SetAttributeValue("data-version", sHokVersionID);
                if (bSetNew) Helper.SetNodeAsNew(oNode);
            }
            catch (Exception ex)
            {
                bRslt = false;
                Console.WriteLine("error in assignNodeFixedAttributes. error is - " + ex.Message);
            }
            return bRslt;
        }
        private static bool SetNodeAsNew(HtmlNode oNode)
        {
            bool bRslt = true;
            try
            {
                oNode.SetAttributeValue("data-isnew", "1");
            }
            catch (Exception ex)
            {
                bRslt = false;
                Console.WriteLine("error in SetNodeAsNew. exception is - " + ex.Message);
            }
            return bRslt;

        }
    }
}

