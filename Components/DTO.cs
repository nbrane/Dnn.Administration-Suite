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
        public string LANG { get; set; }
    }

    public class ApiModuleResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }

        public List<GenericListItem> Modules { get; set; }
        public List<GenericListItem> Panes { get; set; }
        public List<GenericSelectableListItem> Containers { get; internal set; }
        public string DefaultModuleLocation { get; internal set; }
    }

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T CustomObject { get; set; }
    }

    public enum PagePositionMode : int
    {
        After = 1,
        Before = 2,
        ChildOf = 3
    }

    public class GenericListItem
    {
        public string Value { get; set; }
        public string Name { get; set; }
    }


    public class GenericListImageItem : GenericListItem
    {
        public string Image { get; set; }
    }

    public class GenericPageListItem : GenericListImageItem
    {
        public bool HasChildren { get; set; }
    }

    public class GenericSelectableListItem : GenericListItem
    {
        public GenericSelectableListItem() { }
        public GenericSelectableListItem(string name, string value, bool selected)
        {
            this.Name = name;
            this.Value = value;
            this.Selected = selected;
        }

        public bool Selected { get; set; }
    }

    public class UserDetails
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DisplayName { get; set; }
        public string EmailAddress { get; set; }
        public string Password { get; set; }

        public DateTime Created { get; set; }
        public DateTime LastLogin { get; set; }
        public bool Locked { get; set; }
        public bool Authorized { get; set; }
    }

    public class ModuleDetails
    {
        public string Container { get; set; }
        public string Location { get; set; }
        public string Position { get; set; }
        public string Title { get; set; }
        public int Module { get; set; }
        public int Visibility { get; set; }
        public string ModuleInstance { get; internal set; }

        public int PageId { get; set; }
        public int ModuleId { get; set; }
        public string CreateAs { get; set; }
    }

    public class CacheResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }

        public int PageOuputCacheVariations { get; set; }
        public int SiteOuputCacheVariations { get; set; }

        public int TotalCacheItems { get; set; }
        public string TotalCacheSizeLimit { get; set; }
    }

    public class SavePageResponse
    {
        public bool Redirect { get; set; }
        public string Url { get; set; }
    }

    public class PageDetails
    {
        public PageDetails()
        {

        }

        public PageDetails(DotNetNuke.Entities.Tabs.TabInfo dnnTab)
        {
            if (dnnTab == null)
            {
                this.Id = -1;
            } else
            {
                this.PortalId = dnnTab.PortalID;
                this.Id = dnnTab.TabID;
                this.Name = dnnTab.TabName;
                this.Title = dnnTab.Title;
                this.Description = dnnTab.Description;
                this.Theme = dnnTab.SkinSrc;
                this.Container = dnnTab.ContainerSrc;
                this.Visible = dnnTab.IsVisible;

                this.Urls = new List<GenericListItem>();
                this.Urls.Add(new GenericListItem() { Name = dnnTab.FullUrl, Value = dnnTab.FullUrl });

                foreach (var tabUrl in dnnTab.TabUrls)
                {
                    var newGenericItem = new GenericListItem();
                    newGenericItem.Name = tabUrl.SeqNum.ToString();
                    newGenericItem.Value = tabUrl.Url;
                    this.Urls.Add(newGenericItem);
                }

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
        public List<GenericSelectableListItem> Themes { get; set; }
        public List<GenericSelectableListItem> Containers { get; set; }

        public string Position { get; set; }
        public string PositionMode { get; set; }

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

        public void LoadThemesAndContainers()
        {
            // this.Themes = new ControlPanelController().ListContainers("all", "skin");
            // this.Containers = new ControlPanelController().ListContainers("all", "container");
            if (string.IsNullOrWhiteSpace(DotNetNuke.Entities.Portals.PortalSettings.Current.ActiveTab.SkinSrc))
            {
                this.Theme = "-1"; //DotNetNuke.Entities.Portals.PortalSettings.Current.DefaultPortalSkin;
            } else
            {
                this.Theme = DotNetNuke.Entities.Portals.PortalSettings.Current.ActiveTab.SkinSrc;
            }

            this.Themes = Common.ListContainers("host", "skin");

            if (string.IsNullOrWhiteSpace(DotNetNuke.Entities.Portals.PortalSettings.Current.ActiveTab.ContainerSrc))
            {
                this.Container = "-1"; //DotNetNuke.Entities.Portals.PortalSettings.Current.DefaultPortalContainer;
            }
            else
            {
                this.Container = DotNetNuke.Entities.Portals.PortalSettings.Current.ActiveTab.ContainerSrc;
            }

            this.Containers = Common.ListContainers("host", "containers");

        }
    }
}
