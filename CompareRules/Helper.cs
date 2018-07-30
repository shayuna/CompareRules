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
    }
}
