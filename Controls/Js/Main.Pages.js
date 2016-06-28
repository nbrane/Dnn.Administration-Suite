
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
	self.FocusOn = ko.observable(false);

	self.LoadInitialView = function() {
		self.ParentNode().ToggleLoadingScreen(true);

		if (self.ParentNode().CurrentSubaction() == 'all') {
		    self.ShowManagementLinks(true);
		}
		else if (self.ParentNode().CurrentSubaction() == 'settings') {
		    self.ShowManagementLinks(false);
		    self.DefaultAction('edit');
		} else {
			self.DefaultAction('view');
		}

		if (self.ParentNode().CurrentSubaction() == 'settings') {
		    self.ShowEditPageDialog({ Value: controlPanelTabId, Name: 'Current' });
		} else {
		    self.ParentNode().ServerCallback('Pages', 'ListPages', 'parent=' + self.ParentNode().CurrentSubaction(), function (serverData) {
		    	if (serverData.Success){
		    		self.Pages(serverData.CustomObject);

		    		self.ParentNode().ToggleLoadingScreen(false);
		    	}
		    	else{
		    		self.ParentNode().ToggleConfirmScreen('Sorry, We ran into a problem.', 'Please try again');
		    	}
		    });
		}
	};
	
	self.ShowAdvancedSettings = function() {
		self.ParentNode().ToggleLoadingScreen(true);
		location.href = self.PageUrls()[0].Value + "?ctl=tab&action=edit&returntabid=" + controlPanelTabId;
	};
	
	self.ShowEditPageDialog = function(page, event) {

	    if ($(event.target).hasClass("page-with-children")) {
	        self.ParentNode().ServerCallback('Pages', 'ListPages', 'parent=' + page.Value, function (serverData) {
	            if (serverData.Success) {
	                self.Pages(serverData.CustomObject);

	                self.ParentNode().ToggleLoadingScreen(false);
	            }
	            else {
	                self.ParentNode().ToggleConfirmScreen('Sorry, We ran into a problem.', 'Please try again');
	            }
	        });
	    }
	    else {
	        self.SelectedPage(page);

	        if (self.DefaultAction() == 'delete') {

	            self.ParentNode().ToggleConfirmScreen('Are you sure you want to delete the following page?', 'Id: ' + page.Value + ', Name: ' + page.Name, function () {
	                self.ParentNode().ToggleLoadingScreen(true);

	                var pageObject = {};
	                pageObject.Id = page.Value;

	                self.ParentNode().ServerCallback('Pages', 'DeletePage', JSON.stringify(pageObject), function (serverData) {
	                    if (serverData.Success) {

	                        self.ParentNode().ToggleLoadingScreen(false);
	                        if (serverData.CustomObject.Redirect) {
	                            location.href = serverData.CustomObject.Url;
	                        } else {
	                            self.LoadInitialView();
	                        }
	                    }
	                    else {
	                        self.ParentNode().ToggleConfirmScreen('Sorry, We ran into a problem.', 'Please try again');
	                    }
	                }, null, null, 'POST');
	            });
	        } else {
	            self.ParentNode().ToggleLoadingScreen(true);

	            self.ParentNode().ServerCallback('Pages', 'LoadPageDetails', 'id=' + page.Value, function (serverData) {
	                if (serverData.Success) {
	                    self.PageId(serverData.CustomObject.Id);
	                    self.PageName(serverData.CustomObject.Name);
	                    self.PageDescription(serverData.CustomObject.Description);
	                    self.PageVisible(serverData.CustomObject.Visible);
	                    self.PageDisabled(serverData.CustomObject.Disabled);
	                    self.PageUrls(serverData.CustomObject.Urls);
	                    self.PageList(serverData.CustomObject.AllPages);
	                    self.ThemeList(serverData.CustomObject.Themes);
	                    self.ContainerList(serverData.CustomObject.Containers);
	                    self.PageTheme(serverData.CustomObject.Theme);
	                    self.PageContainer(serverData.CustomObject.Container);

	                    if (self.DefaultAction() == 'edit') {
	                        self.ParentNode().ToggleLoadingScreen(false);
	                        $('.nbr-dialog').fadeIn();
	                        self.FocusOn(false);
	                    }
	                    else if (self.DefaultAction() == 'view') {
	                        location.href = serverData.CustomObject.Urls[0].Value;
	                    }
	                }
	                else {
	                    self.ParentNode().ToggleConfirmScreen('Sorry, We ran into a problem.', 'Please try again');
	                }
	            });
	        }
	    }
	};
	
	self.ShowAddNewPageDialog = function() {
		self.SelectedPage(null);
		self.ParentNode().ToggleLoadingScreen(true);
		
		self.ParentNode().ServerCallback('Pages', 'LoadPageDetails', 'id=-1', function (serverData) {
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
				self.FocusOn(true);
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
		pageObject.Theme = self.PageTheme();
		pageObject.Container = self.PageContainer();
		
		self.ParentNode().ServerCallback('Pages', 'SavePage', JSON.stringify(pageObject), function (serverData) {
			if (serverData.Success){

				self.ParentNode().ToggleLoadingScreen(false);
				self.CloseDialog();

				if (serverData.CustomObject.Redirect) {
					location.href = serverData.CustomObject.Url;
				} else {
				    if (self.ParentNode().CurrentSubaction() != 'settings') {
                        self.LoadInitialView();
				    }
				}
			}
			else{
				self.ParentNode().ToggleConfirmScreen('Sorry, We ran into a problem.', 'Please try again');
			}
		}, null,null, 'POST');
	};
	
	self.SetDefaultAction = function(data, event){
		var listItem = event.target;
        if (listItem.nodeName != "li" && listItem.nodeName != "LI") {
            listItem = event.target.parentNode;
        }
		self.DefaultAction($(listItem).data('action'));
	};
	
	self.CloseDialog = function(module) {
	    $('.nbr-dialog').fadeOut(400, function () { self.SelectedPage(null); });

	    if (self.ParentNode().CurrentSubaction() == 'settings') {
	        self.CloseSubMenu();
	    }
	};

    self.CloseSubMenu = function () {
        self.ParentNode().CloseSubMenu();
    };

    self.Localization = function (keyName) {
        return self.ParentNode().Language()[keyName];
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