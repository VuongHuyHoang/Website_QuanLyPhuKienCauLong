using System.Web;
using System.Web.Mvc;

namespace WebsiteBanPhuKienCauLong_65131148
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
