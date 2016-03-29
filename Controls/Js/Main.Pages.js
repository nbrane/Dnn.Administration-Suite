
var nBraneAdminSuitePagesViewModel = function () {
    var self = this;
	self.Pages = ko.observableArray();
	self.SelectedPage = ko.observable();
	self.DefaultAction = ko.observable('edit');
	
	self.PageId = ko.observable(-1);
	self.PortalId = ko.observable(-1);
	self.PageName = ko.observable('');
	self.PageDescription = ko.observable('');
	self.PageVisible = ko.observable(true);
	self.PageDisabled = ko.observable(false);
	self.PagePosition = ko.observable('');
	self.PagePositionTarget = ko.observable('');
	self.PageVisibility = ko.observable('');
	self.PageTheme = ko.observable('');
	self.PageContainer = ko.observable('');
	self.PageUrls = ko.observableArray();
	self.PageList = ko.observableArray();
	self.ThemeList = ko.observableArray();
	self.ContainerList = ko.observableArray();
	self.ShowManagementLinks =  ko.observable(false);

	self.LoadInitialView = function() {
		self.ParentNode().ToggleLoadingScreen(true);

		if (self.ParentNode().CurrentSubaction() == 'all') {
			self.ShowManagementLinks(true);
		} else {
			self.DefaultAction('view');
		}
		
		self.ParentNode().ServerCallback('ListPages', 'parent=' + self.ParentNode().CurrentSubaction(), function(serverData) {
			if (serverData.Success){
				self.Pages(serverData.CustomObject);

				self.ParentNode().ToggleLoadingScreen(false);
			}
			else{
				self.ParentNode().ToggleConfirmScreen('Sorry, We ran into a problem.', 'Please try again');
			}
		});
	};
	
	self.ShowEditPageDialog = function(page) {
		self.SelectedPage(page);
		self.ParentNode().ToggleLoadingScreen(true);

		self.ParentNode().ServerCallback('LoadPageDetails', 'id=' + page.Value, function(serverData) {
			if (serverData.Success){
				self.PageId(serverData.CustomObject.Id);
				self.PageName(serverData.CustomObject.Name);
				self.PageDescription(serverData.CustomObject.Description);
				self.PageVisible(serverData.CustomObject.Visible);
				self.PageDisabled(serverData.CustomObject.Disabled);
				self.PageTheme(serverData.CustomObject.Theme);
				self.PageContainer(serverData.CustomObject.Container);
				
				self.PageUrls(serverData.CustomObject.Urls);
				self.PageList(serverData.CustomObject.AllPages);
				self.ThemeList(serverData.CustomObject.Themes);
				self.ContainerList(serverData.CustomObject.Containers);

				if (self.DefaultAction() == 'edit') {
					self.ParentNode().ToggleLoadingScreen(false);
					$('.nbr-dialog').fadeIn();
				}
				else if (self.DefaultAction() == 'view'){
					location.href = serverData.CustomObject.Urls[0].Value;
				}
			}
			else{
				self.ParentNode().ToggleConfirmScreen('Sorry, We ran into a problem.', 'Please try again');
			}
		});
	};
	
	self.ShowAddNewPageDialog = function() {
		self.SelectedPage(null);
		self.ParentNode().ToggleLoadingScreen(true);
		
		self.ParentNode().ServerCallback('LoadPageDetails', 'id=-1', function(serverData) {
			if (serverData.Success){
				self.PageId(serverData.CustomObject.Id);
				self.PageName('');
				self.PageDescription('');
				self.PageVisible(true);
				self.PageDisabled(false);
				self.PageTheme();
				self.PageContainer();
				
				self.PageUrls(null);
				self.PageList(serverData.CustomObject.AllPages);
				self.ThemeList(serverData.CustomObject.Themes);
				self.ContainerList(serverData.CustomObject.Containers);

				self.ParentNode().ToggleLoadingScreen(false);
				$('.nbr-dialog').fadeIn();
			}
			else{
				self.ParentNode().ToggleConfirmScreen('Sorry, We ran into a problem.', 'Please try again');
			}
		});

		$('.nbr-dialog').fadeIn();
	};
	
	self.AddPage = function(){
		self.ParentNode().ToggleLoadingScreen(true);
		
		var pageObject = {};
		pageObject.Id = self.PageId();
		pageObject.Name = self.PageName();
		pageObject.Description = self.PageDescription();
		pageObject.Visible = self.PageVisible();
		pageObject.Disabled = self.PageDisabled();
		pageObject.Position = self.PagePositionTarget();
		pageObject.PositionMode = self.PagePosition();
		
		self.ParentNode().ServerCallback('SavePage', JSON.stringify(pageObject), function(serverData) {
			if (serverData.Success){

				self.ParentNode().ToggleLoadingScreen(false);
				$('.nbr-dialog').fadeOut();
				
				self.LoadInitialView();
			}
			else{
				self.ParentNode().ToggleConfirmScreen('Sorry, We ran into a problem.', 'Please try again');
			}
		}, null,null, 'POST');
	};
	
	self.SetDefaultAction = function(){
		var listItem = event.target;
        if (listItem.nodeName != "li" && listItem.nodeName != "LI") {
            listItem = event.target.parentNode;
        }
		self.DefaultAction($(listItem).data('action'));
	};
	
	self.CloseDialog = function(module) {
		$('.nbr-dialog').fadeOut();
	};

    self.CloseSubMenu = function () {
        self.ParentNode().CloseSubMenu();
    };
	
	self.ParentNode = function() {
		return ko.contextFor(nBraneAdminSuiteNode).$data;
	};

	self.LoadInitialView();
};

$(document).ready(function () {
    var pagesSubModuleNode = document.getElementById('nbr-admin-suite-pages');
    if (pagesSubModuleNode) {
        ko.cleanNode(pagesSubModuleNode);

        var myItem = new nBraneAdminSuitePagesViewModel();
        ko.applyBindings(myItem, pagesSubModuleNode);
    }
});