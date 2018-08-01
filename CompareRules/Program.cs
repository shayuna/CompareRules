﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Data.SqlClient;

namespace CompareRules
{
    class Program
    {
        static void Main(string[] args)
        {
            //making the db contact. cross your fingers you mf.
            string sDataSrc = "192.168.200.4";
            string sConnStr = "Initial Catalog=LawData;User ID=sa;Password=;Data Source=" + sDataSrc;
            string sSql = "select c,hokc from hok_previousversions (nolock) order by hokc,c desc";
            int iCounter = 0;
            SqlConnection conn = new SqlConnection(sConnStr);
            try
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(sSql, conn);
                SqlDataReader dataReader = cmd.ExecuteReader();
                RecordDetails recA = null, recB = null;
                IList<IList<ComparableItem>> arAllRulesComparableItems = new List<IList<ComparableItem>>();
                IList<ComparableItem> arComparableItems = null,arComparableItemsA=null,arComparableItemsB=null;
                while (dataReader.Read())
                {
                    if (recA!=null && recA.HokC != Convert.ToInt32(dataReader.GetValue(1)))
                    {
                        iCounter++;
                        recA = null;
                        recB = null;
                        if (iCounter == 100) break;
                    }
                    if (recA == null)
                    {
                        recA = new RecordDetails(Convert.ToInt32(dataReader.GetValue(0)), Convert.ToInt32(dataReader.GetValue(1)));
                        if (arComparableItems != null) arAllRulesComparableItems.Add(arComparableItems);
                        arComparableItems = null;
                    }
                    else if (recB == null)
                    {
                        recB = new RecordDetails(Convert.ToInt32(dataReader.GetValue(0)), Convert.ToInt32(dataReader.GetValue(1)));
                    }
                    else
                    {
                        recA = recB;
                        recB = new RecordDetails(Convert.ToInt32(dataReader.GetValue(0)), Convert.ToInt32(dataReader.GetValue(1)));
                    }

                    if (recA!=null && recB!=null)
                    {
                        ICollection<HtmlNode> arNodesA = Helper.GetAllHtmlClausesInFileLoadedFromWeb("http://www.lawdata.co.il/lawdata_face_lift_test/gethok.asp?flnm=" + recA.HokC + "_" + recA.ID);
                        ICollection<HtmlNode> arNodesB = Helper.GetAllHtmlClausesInFileLoadedFromWeb("http://www.lawdata.co.il/lawdata_face_lift_test/gethok.asp?flnm=" + recB.HokC + "_" + recB.ID);
                        if (arNodesA.Count == 0)
                        {
                            recA = recB;
                            recB = null;
                        }
                        else if (arNodesB.Count == 0)
                        {
                            recB = null;
                        }
                        else
                        {
                            if (arComparableItems == null)
                            {
                                arComparableItems = Helper.FromHtmlNodesArrayToComparableItemsList(arNodesA, recA);
                                arComparableItemsA = arComparableItems;
                            }
                            else
                            {
                                arComparableItemsA = arComparableItemsB;
                            }
                            arComparableItemsB = Helper.FromHtmlNodesArrayToComparableItemsList(arNodesB, recB);
                            Helper.CompareComparableItemsStores(ref arComparableItemsA, ref arComparableItemsB);
                            Console.WriteLine("comparing rules");
                        }
                    }
                }
                dataReader.Close();
                cmd.Dispose();
                conn.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("can't open connection to db. error is " + ex.Message); 
            }
            Console.ReadKey();
        }
    }
}
