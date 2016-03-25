
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
	
	self.LoadInitialView = function() {
		self.ParentNode().ToggleLoadingScreen(true);
		
		self.ParentNode().ServerCallback('ListPages', 'parent=all', function(serverData) {
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

				self.ParentNode().ToggleLoadingScreen(false);
				$('.nbr-dialog').fadeIn();
			}
			else{
				self.ParentNode().ToggleConfirmScreen('Sorry, We ran into a problem.', 'Please try again');
			}
		});
	};
	
	self.ShowAddNewPageDialog = function() {
		self.SelectedPage(null);
		
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
		
		
		self.ParentNode().ServerCallback('SavePage', JSON.stringify(pageObject), function(serverData) {
			if (serverData.Success){

				self.ParentNode().ToggleLoadingScreen(false);
				$('.nbr-dialog').fadeIn();
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