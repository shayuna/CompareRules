using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Data.SqlClient;
using System.Threading;
using System.Runtime.InteropServices;
using System.IO;

namespace CompareRules
{
    class Program
    {
        private static Mutex mutex;

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const int SW_HIDE = 0;
        private const int SW_SHOW = 5;
        private const int SW_MINIMIZE = 6;

        static void Main(string[] args)
        {
            bool bCreateNew;
            const string sAppName = "CompareRules";
            mutex = new Mutex(true, sAppName, out bCreateNew);
            if (!bCreateNew) Environment.Exit(0);

            var handle = GetConsoleWindow();
            ShowWindow(handle, SW_MINIMIZE);

            string sDataSrc = "192.168.200.4";
            string sConnStr = "Initial Catalog=LawData;User ID=sa;Password=;Data Source=" + sDataSrc;
            string sSql = "";
            bool bTest = true;

            if (bTest)
            {
                sSql = "select hp.c,hp.hokc from hok_previousversions hp (nolock) " +
                            "left join( " +
                            "select top 20 hokc from hok_previousversions hp (nolock) " +
                            "left join Hok_DocsIncludingVersionsDeltas (nolock) hd on hp.hokc= hd.c " +
//                            "where isnull(hd.c,0)= 0 " +
                            "group by hokc having count(*) > 1 order by hokc" +
                            ")q1 on hp.hokc = q1.hokc where hp.hokc=28851 order by hokc,c desc ";
            }
            else
            {
                sSql = "select hp.c,hp.hokc from hok_previousversions hp (nolock) " +
                            "inner join( " +
                            "select top 100 hokc from hok_previousversions hp (nolock) " +
                            "left join Hok_DocsIncludingVersionsDeltas (nolock) hd on hp.hokc= hd.c " +
                            "where isnull(hd.c,0)= 0 " +
                            "group by hokc having count(*) > 1 order by hokc" +
                            ")q1 on hp.hokc = q1.hokc order by hokc,c desc ";
            }

            int iCounter = 0;
            SqlConnection connRead = new SqlConnection(sConnStr);
            try
            {
                connRead.Open();
                SqlCommand cmdRead = new SqlCommand(sSql, connRead);
                SqlDataReader dataReader = cmdRead.ExecuteReader();
                RecordDetails recA = null, recB = null;
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

                        oRule.Serialize();
                        Helper.WriteToDB(oRule.Version.HokC);

                        if (iCounter == 100) break;
                    }
                    if (recA == null)
                    {
                        recA = new RecordDetails(Convert.ToInt32(dataReader.GetValue(0)), Convert.ToInt32(dataReader.GetValue(1)));
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
                        ICollection<HtmlNode> arNodesB=null;
                        if (bTest)
                        {
                            arNodesB = Helper.GetAllHtmlClausesInHtmlDocument(Helper.GetHtmlDocFromUrl("http://www.lawdata.co.il/lawdata_face_lift_test/gethok.asp?flnm=" + recB.HokC + "_" + recB.ID));
                        }
                        else
                        {
                            string sPath = "d://inetpub//wwwroot//upload//hok//" + recB.HokC + "_" + recB.ID + ".htm";
                            if (!File.Exists(sPath)) sPath = "d://inetpub//wwwroot//upload//hok//" + recB.HokC + "_" + recB.ID + ".html";
                            if (File.Exists(sPath))arNodesB = Helper.GetAllHtmlClausesInHtmlDocument(Helper.GetHtmlDocFromDisk(sPath));
                        }
                        arComparableItemsB = Helper.FromHtmlNodesArrayToComparableItemsList(arNodesB, recB);
                        if (arComparableItemsB.Count>0)Helper.CompareComparableItemsStores(arComparableItemsA, arComparableItemsB);
                        Console.WriteLine("comparing rules");
                    }
                }
                if (oRule != null)
                {
                    oRule.Serialize();
                    Helper.WriteToDB(oRule.Version.HokC);
                }
                dataReader.Close();
                cmdRead.Dispose();
                connRead.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("there was an error. it could be anywhere. the error message is " + ex.Message); 
            }
//            Console.ReadKey();
        }
    }
}
