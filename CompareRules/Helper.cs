﻿using System;
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
        public static ICollection<HtmlNode> GetAllHtmlClausesInHtmlDocument(HtmlDocument oDoc)
        {
            ICollection<HtmlNode> arNodesInDoc = oDoc.QuerySelectorAll(".hsubclausewrapper,.hkoteretseif,.hearot");
            foreach (HtmlNode oNode in arNodesInDoc)
            {
                oNode.SetAttributeValue("class", oNode.GetAttributeValue("class", "") + " " + "comparableItm");
            }
            ICollection < HtmlNode > arNodesToReturn = oDoc.QuerySelectorAll(".comparableItm");
            foreach (HtmlNode oNode in arNodesToReturn)
            {
                oNode.SetAttributeValue("class", oNode.GetAttributeValue("class", "").Replace(" comparableItm",""));
            }
            return arNodesToReturn;
        }
        public static HtmlDocument GetHtmlDocFromUrl(string sUrl)
        {
            HtmlWeb oHtmlWeb = new HtmlWeb();
            oHtmlWeb.OverrideEncoding = Encoding.GetEncoding(1255);
            return oHtmlWeb.Load(sUrl);
        }
        public static HtmlDocument GetHtmlDocFromDisk(string sPath)
        {
            HtmlDocument oDoc = new HtmlDocument();
            oDoc.Load(sPath);
            return oDoc;
        }
        public static IList<ComparableItem> FromHtmlNodesArrayToComparableItemsList(ICollection<HtmlNode> arNodes, RecordDetails rec)
        {

            IList<ComparableItem> arComparableItems = new List<ComparableItem>();
            if (arNodes != null)
            {
                int iPosition = 0;
                foreach (HtmlNode eNode in arNodes)
                {
                    iPosition++;
                    arComparableItems.Add(new ComparableItem(rec.ID, iPosition, eNode));
                }
            }
            return arComparableItems;
        }
        private static double GetAlternativeMatchScore(Turn eTurn, IList<ComparableItem> arComparableItemsA, IList<ComparableItem> arComparableItemsB,int iIndexInA,int iIncrementA,int iIndexInB,int iIncrementB)
        /* suppose we find a similar element. are we sure this is the right one ? maybe the similarity is a chance similarity and doesn't originate from a common ancestor ?*/
        /* so here  is the part where we try to match the forced matched element with its best match in its vicinity*/
        {
            double dScore = 0;
            int ii = 0;
            if (eTurn == Turn.A)
            {
                for (ii = 0; ii < iIncrementB+100 && ii+iIndexInA<arComparableItemsA.Count; ii++)
                {
                    dScore = Math.Max(dScore, GetMatchScore(arComparableItemsA[ii].Node, arComparableItemsB[iIndexInB + iIncrementB].Node));
                }
            }
            else if (eTurn == Turn.B)
            {
                for (ii = 0; ii < iIncrementA + 100 && ii + iIndexInB < arComparableItemsB.Count; ii++)
                {
                    dScore = Math.Max(dScore, GetMatchScore(arComparableItemsA[iIndexInA+iIncrementA].Node, arComparableItemsB[ii].Node));
                }
            }
            return dScore;
        }

        public static void CompareComparableItemsStores(IList<ComparableItem> arComparableItemsA, IList<ComparableItem> arComparableItemsB)
        {
            try
            {
                int iIndexInA = 0, iIndexInB = 0, iIncrementA = 0, iIncrementB = 0;
                Turn eTurn = Turn.A;

                bool bContinue = true;
                bool bReachedItemsAArrayEnd = false, bReachedItemsBArrayEnd = false;

                while (bContinue /*&& iIndexInA<arComparableItemsA.Count && iIndexInB<arComparableItemsB.Count*/)
                {
                    RelationType eRslt;
                    double dMatchScore;
                    if (eTurn == Turn.A) eRslt = Helper.CompareTwoHtmlElements(arComparableItemsA[iIndexInA].Node, arComparableItemsB[iIndexInB + iIncrementB].Node);
                    else eRslt = Helper.CompareTwoHtmlElements(arComparableItemsA[iIndexInA + iIncrementA].Node, arComparableItemsB[iIndexInB].Node);
                    if (
                        eRslt == RelationType.IDENTICAL ||
                        eRslt == RelationType.SIMILAR ||
                        iIncrementA > 100 ||
                        iIncrementB > 100 ||
                        (iIndexInA + iIncrementA + 1 >= arComparableItemsA.Count &&
                        iIndexInB + iIncrementB + 1 >= arComparableItemsB.Count)
                        )
                    {
                        if (eRslt == RelationType.SIMILAR)
                        {
                            if (eTurn == Turn.A) dMatchScore = Helper.GetMatchScore(arComparableItemsA[iIndexInA].Node, arComparableItemsB[iIndexInB + iIncrementB].Node);
                            else dMatchScore = Helper.GetMatchScore(arComparableItemsA[iIndexInA + iIncrementA].Node, arComparableItemsB[iIndexInB].Node);

                            if (dMatchScore < GetAlternativeMatchScore(eTurn, arComparableItemsA, arComparableItemsB, iIndexInA, iIncrementA, iIndexInB, iIncrementB)) eRslt = RelationType.DIFFERENT;
                        }
                        if (eRslt == RelationType.DIFFERENT)
                        {
                            if (!bReachedItemsBArrayEnd)/* we already did something with the last item in arComparableItemsB. so we don't want to reproduce the element in this branch */
                            {
                                arComparableItemsB[iIndexInB].RelationTypeToAncestor = RelationType.ABSENT;
                                arComparableItemsA[iIndexInA].addDescendant(arComparableItemsB[iIndexInB]);
                            }
                            if (!bReachedItemsAArrayEnd)/* we already did something with the last item in arComparableItemsA. so we don't want to reproduce the element in this branch */
                            {
                                arComparableItemsA[iIndexInA].IsNew = true;
                            }
                            iIncrementA = 0;
                            iIncrementB = 0;
                        }
                        else if (eTurn == Turn.A)
                        {
                            arComparableItemsB[iIndexInB + iIncrementB].RelationTypeToAncestor = eRslt;
                            arComparableItemsA[iIndexInA].addDescendant(arComparableItemsB[iIndexInB + iIncrementB]);
                            for (int ii = 0; ii < iIncrementB; ii++)
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
                            iIndexInA += iIncrementA + 1;
                            iIndexInB++;
                        }

                        if (iIndexInA >= arComparableItemsA.Count && iIndexInB >= arComparableItemsB.Count) bContinue = false;
                        if (iIndexInA >= arComparableItemsA.Count)
                        {
                            bReachedItemsAArrayEnd = true;
                            iIndexInA = arComparableItemsA.Count - 1;
                        }
                        if (iIndexInB >= arComparableItemsB.Count)
                        {
                            bReachedItemsBArrayEnd = true;
                            iIndexInB = arComparableItemsB.Count - 1;
                        }

                        iIncrementA = 0;
                        iIncrementB = 0;

                    }
                    else
                    {
                        if (eTurn == Turn.A && iIndexInA + iIncrementA + 1 < arComparableItemsA.Count)
                        {
                            iIncrementA++;
                            eTurn = Turn.B;
                        }
                        else if (eTurn == Turn.B && iIndexInB + iIncrementB + 1 < arComparableItemsB.Count)
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
            catch (Exception ex)
            {
                Console.WriteLine("something in the compare process (in Helper.CompareComparableItemsStores function) went sour. the error message is - " + ex.Message);
            }
        }
        public static double GetMatchScore(HtmlNode eNodeA, HtmlNode eNodeB)
        {
            string sTxtA = WebUtility.HtmlDecode(eNodeA.InnerText);
            string sTxtB = WebUtility.HtmlDecode(eNodeB.InnerText);
            string[] arWordsA = Helper.FromTxtToWords(sTxtA);
            string[] arWordsB = Helper.FromTxtToWords(sTxtB);
            double iScore=0;

            string sClassA = String.Join(",", eNodeA.GetClassList());
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
            if (Regex.Replace(sTxtA, @"[\s\r\n\t.,;:]+", "") == Regex.Replace(sTxtB, @"[\s\r\n\t.,;:]+", "")) iScore = 1;
            if (((double)iWordsFrom2In1 / arWordsA.Length >= 0.6) && arWordsA.Length >= 10 && arWordsA.Length * 2.5 > arWordsB.Length && iScore < (double)iWordsFrom2In1 / arWordsA.Length) iScore = (double)iWordsFrom2In1 / arWordsA.Length;
            if (((double)iWordsFrom1In2 / arWordsB.Length >= 0.6) && arWordsB.Length >= 10 && arWordsB.Length * 2.5 > arWordsA.Length && iScore < (double)iWordsFrom1In2 / arWordsB.Length) iScore = (double)iWordsFrom1In2 / arWordsB.Length;
            if (iScore==0 && (((sClassA.Contains("hkoteretseif") && sClassB.Contains("hkoteretseif")) || (sClassA.Contains("hearot") && sClassB.Contains("hearot"))) && (double)iWordsFrom1In2 / arWordsB.Length >= 0.5 && (double)iWordsFrom2In1 / arWordsA.Length >= 0.5)){
                iScore = Math.Max((double)iWordsFrom1In2 / arWordsB.Length, (double)iWordsFrom2In1 / arWordsA.Length)+0.1; /*we add 0.1 because we must normalize the value. on account that the scale of what is regarded as RelationType.SIMILAR is different (0.5 on this consition set and 0.6 on the first two conditions) */
            }
            return iScore;
        }
        public static RelationType CompareTwoHtmlElements(HtmlNode eNodeA, HtmlNode eNodeB)
        {
            RelationType relation = RelationType.DIFFERENT;
            double iScore = GetMatchScore(eNodeA, eNodeB);
            if (iScore == 1) relation = RelationType.IDENTICAL;
            else if (iScore >= 0.6) relation = RelationType.SIMILAR;
            return relation;
        }
        public static RelationType CompareTwoHtmlElements_old(HtmlNode eNodeA,HtmlNode eNodeB)
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
        public static IList<HtmlNode> getChildElements(HtmlNode elm)
        {
            List<HtmlNode> arElm = new List<HtmlNode>();
            foreach (HtmlNode eChild in elm.ChildNodes)
            {
                if (eChild.Name != "#text") arElm.Add(eChild);
            }
            return arElm;
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

