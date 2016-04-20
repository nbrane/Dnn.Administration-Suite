
var nBraneAdminSuiteCacheViewModel = function () {
    var self = this;
	self.CacheDetails = ko.observable();
	self.PageOuputCacheVariations = ko.observable(0);
	self.TotalCacheItems = ko.observable(0);
	self.TotalCacheSizeLimit = ko.observable('');
	
	self.LoadInitialView = function() {
		self.ParentNode().ToggleLoadingScreen(true);

		self.ParentNode().ServerCallback('', 'LoadCacheDetails', 'PageId=' + controlPanelTabId, function (serverData) {
			if (serverData.Success){
				self.PageOuputCacheVariations(serverData.PageOuputCacheVariations);
				self.TotalCacheItems(serverData.TotalCacheItems);
				self.TotalCacheSizeLimit(serverData.TotalCacheSizeLimit);
				
				$('.nbr-right-dialog').fadeIn(400, function() {self.ParentNode().ToggleLoadingScreen(false);});
			}
			else{
				self.ParentNode().ToggleConfirmScreen('Sorry, We ran into a problem.', 'Please try again');
			}
		});
	};
	
	self.ClearOutputCacheThisPage = function() {
		self.ClearOutputCache(controlPanelTabId);
	};
	
	self.ClearOutputCacheAllPages = function() {
		self.ClearOutputCache(-1);
	};
	
	self.ClearOutputCacheAllSites = function() {
		self.ClearOutputCache(-2);
	};

	self.ClearOutputCache = function(pageId) {
		self.CloseDialog();
		self.ParentNode().ToggleLoadingScreen(true);
		
		self.ParentNode().ServerCallback('', 'ClearOutputCache', 'PageId=' + pageId, function (serverData) {
			if (serverData.Success){
				location.reload();
			}
			else{
				self.ParentNode().ToggleConfirmScreen('Sorry, We ran into a problem.', 'Please try again');
			}
		});
	};
	
	self.RecycleApp = function() {
		self.CloseDialog();
		self.ParentNode().ToggleLoadingScreen(true);
		
		self.ParentNode().ServerCallback('', 'RecycleApplication', null, function (serverData) {
			if (serverData.Success){
				location.reload();
			}
			else{
				self.ParentNode().ToggleConfirmScreen('Sorry, We ran into a problem.', 'Please try again');
			}
		});
	};
	
	self.InstallExtension = function() {
		self.CloseDialog();
		self.ParentNode().ToggleLoadingScreen(true);
		
		self.ParentNode().ServerCallback('', 'InstallExtensionUrl', 'PageId=' + controlPanelTabId, function (serverData) {
			if (serverData.Success){
				location.href = serverData.Message;
			}
			else{
				self.ParentNode().ToggleConfirmScreen('Sorry, We ran into a problem.', 'Please try again');
			}
		});
	};
	
	self.CloseDialog = function() {
		$('.nbr-upper-control-panel li.selected').removeClass('selected');
		$('.nbr-right-dialog').fadeOut(400);
	};

	self.ParentNode = function() {
		return ko.contextFor(nBraneAdminSuiteNode).$data;
	};

	self.LoadInitialView();
};

$(document).ready(function () {
    var cacheMgmtSubModuleNode = document.getElementById('nbr-admin-suite-cache');
    if (cacheMgmtSubModuleNode) {
        ko.cleanNode(cacheMgmtSubModuleNode);

        var myItem = new nBraneAdminSuiteCacheViewModel();
        ko.applyBindings(myItem, cacheMgmtSubModuleNode);
    }
});