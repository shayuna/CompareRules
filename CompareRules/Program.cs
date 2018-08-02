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
            string sDataSrc = "192.168.200.4";
            string sConnStr = "Initial Catalog=LawData;User ID=sa;Password=;Data Source=" + sDataSrc;
            string sSql = "select hp.c,hp.hokc from hok_previousversions hp (nolock) " +
                        "inner join( " +
                        "select top 20 hokc from hok_previousversions hp (nolock) " +
                        "left join Hok_DocsIncludingVersionsDeltas (nolock) hd on hp.hokc= hd.c " +
                        "where isnull(hd.c,0)= 0 " +
                        "group by hokc having count(*) > 1 order by hokc" +
                        ")q1 on hp.hokc = q1.hokc order by hokc,c desc ";

            int iCounter = 0;
            SqlConnection connRead = new SqlConnection(sConnStr);
            try
            {
                connRead.Open();
                SqlCommand cmdRead = new SqlCommand(sSql, connRead);
                SqlDataReader dataReader = cmdRead.ExecuteReader();
                RecordDetails recA = null, recB = null;
                IList<Rule> arRules = new List<Rule>();
                IList<ComparableItem> arComparableItemsA=null,arComparableItemsB=null;
                Rule oRule = null;
                while (dataReader.Read())
                {
                    if (recA!=null && recA.HokC != Convert.ToInt32(dataReader.GetValue(1)))
                    {
                        iCounter++;
                        recA = null;
                        recB = null;
                        arComparableItemsA = null;
                        arComparableItemsB = null;
                        if (iCounter == 100) break;
                    }
                    if (recA == null)
                    {
                        recA = new RecordDetails(Convert.ToInt32(dataReader.GetValue(0)), Convert.ToInt32(dataReader.GetValue(1)));
                        if (oRule != null)
                        {
                            oRule.Serialize();
                            arRules.Add(oRule);
                            Helper.WriteToDB(oRule.Version.HokC);
                        }
                        oRule = new Rule(recA);
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
                        if (arComparableItemsA == null)
                        {
                            arComparableItemsA = oRule.ComparableItems;
                        }
                        else
                        {
                            arComparableItemsA = arComparableItemsB;
                        }
                        ICollection<HtmlNode> arNodesB = Helper.GetAllHtmlClausesInFileLoadedFromWeb("http://www.lawdata.co.il/lawdata_face_lift_test/gethok.asp?flnm=" + recB.HokC + "_" + recB.ID);
                        arComparableItemsB = Helper.FromHtmlNodesArrayToComparableItemsList(arNodesB, recB);
                        Helper.CompareComparableItemsStores(ref arComparableItemsA, ref arComparableItemsB);
                        Console.WriteLine("comparing rules");
                    }
                }
                if (oRule != null)
                {
                    oRule.Serialize();
                    arRules.Add(oRule);
                    Helper.WriteToDB(oRule.Version.HokC);
                }
                dataReader.Close();
                cmdRead.Dispose();
                connRead.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("can't open connection to db. error is " + ex.Message); 
            }
  //          Console.ReadKey();
        }
    }
}
