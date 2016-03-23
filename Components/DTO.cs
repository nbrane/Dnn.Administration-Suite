using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nBrane.Modules.AdministrationSuite.Components.DTO
{
    public class ApiTemplateResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string HTML { get; set; }
        public string JS { get; set; }
    }

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T CustomObject { get; set; }
    }


    public class GenericListItem
    {
        public string Value { get; set; }
        public string Name { get; set; }
    }

    public class PageDetails
    {
        public PageDetails()
        {

        }

        public PageDetails(DotNetNuke.Entities.Tabs.TabInfo dnnTab)
        {
            this.PortalId = dnnTab.PortalID;
            this.Id = dnnTab.TabID;
            this.Name = dnnTab.TabName;
            this.Title = dnnTab.Title;
            this.Description = dnnTab.Description;
            this.Theme = dnnTab.SkinSrc;
            this.Container = dnnTab.ContainerSrc;
            this.Visible = dnnTab.IsVisible;
            this.Disabled = dnnTab.DisableLink;

            this.Urls = new List<GenericListItem>();
            foreach (var tabUrl in dnnTab.TabUrls)
            {
                var newGenericItem = new GenericListItem();
                newGenericItem.Name = tabUrl.SeqNum.ToString();
                newGenericItem.Value = tabUrl.Url;
                this.Urls.Add(newGenericItem);
            }

            this.AllPages = new List<GenericListItem>();
        }
        private int PortalId { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Theme { get; set; }
        public string Container { get; set; }
        public bool Visible { get; set; }
        public bool Disabled { get; set; }

        public List<GenericListItem> AllPages { get; set; }
        public List<GenericListItem> Urls { get; set; }

        public void LoadAllPages()
        {
            foreach (var tab in DotNetNuke.Entities.Tabs.TabController.GetPortalTabs(this.PortalId, -1, true, true, false, false))
            {
                var newGenericItem = new GenericListItem();
                newGenericItem.Name = tab.IndentedTabName;
                newGenericItem.Value = tab.TabID.ToString();
                this.AllPages.Add(newGenericItem);
            }
        }
    }
}
