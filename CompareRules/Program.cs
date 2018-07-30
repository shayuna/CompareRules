using System;
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
            string sSql = "select c,hokc from hok_previousversions (nolock) order by hokc,c";
            int iCounter = 0;
            SqlConnection conn = new SqlConnection(sConnStr);
            try
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(sSql, conn);
                SqlDataReader dataReader = cmd.ExecuteReader();
                RecordDetails recA = null, recB = null;
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
                        string sUrlA = "http://www.lawdata.co.il/lawdata_face_lift_test/gethok.asp?flnm="+recA.HokC+"_"+recA.ID;
                        ICollection<HtmlNode> arNodesA = Helper.GetAllHtmlClausesInFileLoadedFromWeb(sUrlA);

                        string sUrlB = "http://www.lawdata.co.il/lawdata_face_lift_test/gethok.asp?flnm=" + recB.HokC + "_" + recB.ID;
                        ICollection<HtmlNode> arNodesB = Helper.GetAllHtmlClausesInFileLoadedFromWeb(sUrlB);
                        if (arNodesA.Count== 0)
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
                            ICollection<ComparableItem> arComparableItemsA = new List<ComparableItem>();
                            int iCnt = 0;
                            foreach(HtmlNode eNode in arNodesA)
                            {
                                iCnt++;
                                // #shay - stopped here. shou
                                arComparableItemsA.Add (new ComparableItem(recA.ID, iCnt, -1, RelationType.NONE, eNode));
                            }
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

/*
            foreach (HtmlNode oNode in arNodes)
            {
//                Console.WriteLine(oNode.InnerText);
            }
            */
            Console.ReadKey();
        }
    }
}
