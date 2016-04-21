using DotNetNuke.Common.Utilities;
using Microsoft.ApplicationBlocks.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nBrane.Modules.AdministrationSuite.Components
{
    class Data
    {
        private static IDataReader SearchForUsersDataReader(int portalId, string filterTerm, int pageNumber, int pageSize)
        {
            return SqlHelper.ExecuteReader(Config.GetConnectionString(), "nBrane_AdminSuite_SearchForUsers", portalId, filterTerm, pageNumber, pageSize);
        }

        private static IDataReader GetExtensionUsageDataReader(int portalId)
        {
            return SqlHelper.ExecuteReader(Config.GetConnectionString(), "nBrane_AdminSuite_GetExtensionUsage", portalId);
        }

        public static List<SearchUserInfo> SearchForUsers(int portalId, string filterTerm, int pageNumber, int pageSize)
        {
            return CBO.FillCollection<SearchUserInfo>(SearchForUsersDataReader(portalId, filterTerm, pageNumber, pageSize));
        }

        public static List<ExtensionUsageInfo> GetExtensionUsage(int portalId)
        {
            return CBO.FillCollection<ExtensionUsageInfo>(GetExtensionUsageDataReader(portalId));
        }
    }

    public class SearchUserInfo
    {
        public int UserId { get; set; }
        public string DisplayName { get; set; }
        public int TotalRecords { get; set; }
    }

    public class ExtensionUsageInfo
    {
        public int DesktopModuleID { get; set; }
        public int N { get; set; }
        public string FriendlyName { get; set; }
    }
}
