
var nBraneAdminSuiteModulesViewModel = function () {
    var self = this;
    self.Modules = ko.observableArray();
	self.PageModules = ko.observableArray();
	self.Pages = ko.observableArray();
	self.Containers = ko.observableArray();
	self.Panes = ko.observableArray();
	self.Categories = ko.observableArray();

	self.ModuleTitle = ko.observable('');
	self.ModuleVisibility = ko.observable('');
	self.ModuleLocation = ko.observable('');
	self.ModulePosition = ko.observable('');
	self.ModuleContainer = ko.observable('');
	self.CopyModulePage = ko.observable(-1);
	self.CopyModuleId = ko.observable(-1);

	self.SelectedCategory = ko.observable(null);
	self.SelectedModule = ko.observable();
	self.DialogVisible = ko.observable(false);
	self.FocusOnTitle = ko.observable(false);
	
	self.CopyModulePage.subscribe(function(newValue) {
		if (newValue && newValue != -1) {
			self.ParentNode().ToggleLoadingScreen(true);
			self.ParentNode().ServerCallback('ListModulesOnPage', 'portalid=' + controlPanelPortalId + '&tabid=' + newValue, function(serverData) {
				if (serverData.Success){
					self.PageModules(serverData.CustomObject);
					self.ParentNode().ToggleLoadingScreen(false);
				}
				else{
					self.ParentNode().ToggleConfirmScreen('Sorry, We ran into a problem.', 'Please try again');
				}
			});
		}
    });
	
	self.LoadInitialView = function() {
		self.ParentNode().ToggleLoadingScreen(true);
		self.ParentNode().ServerCallback('ListModuleCategories', 'category=all', function(serverData) {
			if (serverData.Success){
				self.Categories(serverData.CustomObject);

				self.ParentNode().ToggleLoadingScreen(false);
			}
			else{
				self.ParentNode().ToggleConfirmScreen('Sorry, We ran into a problem.', 'Please try again');
			}
		});
	};

	self.LoadCategory = function (value) {
	    self.ParentNode().ToggleLoadingScreen(true);
	    self.ParentNode().ServerCallback('ListModules', 'category=' + value, function (serverData) {
	        if (serverData.Success) {
	            self.Modules(serverData.Modules);
	            self.Panes(controlPanelPanes);
	            self.Containers(serverData.Containers);
	            self.ModuleLocation(serverData.DefaultModuleLocation);

	            self.ParentNode().ToggleLoadingScreen(false);
	        }
	        else {
	            self.ParentNode().ToggleConfirmScreen('Sorry, We ran into a problem.', 'Please try again');
	        }
	    });
	};

	self.AddModule = function(module) {
		var moduleObject = {};
		moduleObject.Module = self.SelectedModule().Value;
		moduleObject.Title = self.ModuleTitle();
		moduleObject.Container = self.ModuleContainer();
		moduleObject.Visibility = self.ModuleVisibility();
		moduleObject.Location = self.ModuleLocation();
		moduleObject.Position = self.ModulePosition();
		
		self.SaveModule(moduleObject);
	};
	
	self.CopyModule = function(module) {
		if(self.CopyModulePage() != -1) {
			var moduleObject = {};
			moduleObject.PageId = self.CopyModulePage();
			moduleObject.ModuleId = self.CopyModuleId();
			moduleObject.Container = self.ModuleContainer();
			moduleObject.Visibility = self.ModuleVisibility();
			moduleObject.Location = self.ModuleLocation();
			moduleObject.Position = self.ModulePosition();
			moduleObject.CreateAs = 'copy';
			
			self.SaveModule(moduleObject);
		}
	};
	
	self.ShareModule = function(module) {
		if(self.CopyModulePage() != -1) {
			var moduleObject = {};
			moduleObject.PageId = self.CopyModulePage();
			moduleObject.ModuleId = self.CopyModuleId();
			moduleObject.Container = self.ModuleContainer();
			moduleObject.Visibility = self.ModuleVisibility();
			moduleObject.Location = self.ModuleLocation();
			moduleObject.Position = self.ModulePosition();
			moduleObject.CreateAs = 'link';
			
			self.SaveModule(moduleObject);
		}
	};
	
	self.SaveModule = function(moduleObject){
		self.ParentNode().ToggleLoadingScreen(true);
		
		self.ParentNode().ServerCallback('SaveModule', JSON.stringify(moduleObject), function(serverData) {
			if (serverData.Success){
				self.ParentNode().ToggleConfirmScreen('Success!', 'moduleId: ' + serverData.CustomObject + '. refresh the page to show the new module?', function(){
							location.reload();
						}
					);
				
				self.CloseDialog();
				self.CloseSubMenu();
			}
			else{
				self.ParentNode().ToggleConfirmScreen('Sorry, We ran into a problem.', 'Please try again');
			}
		}, null,null, 'POST');
	};
	
	self.CloseDialog = function(module) {
		$('.nbr-dialog').fadeOut(400, function() {self.SelectedModule(null);});
	};
	
	self.SelectCategory = function (category) {
	    if (self.SelectedCategory() == category) {
	        self.SelectedCategory(null);
	        self.Modules(null);
	    } else {
	        self.SelectedCategory(category);
	        self.LoadCategory(category.Name);
	    }
	};

	self.ShowAddNewModuleDialog = function(module) {
		self.SelectedModule(module);
		self.DialogVisible(true);
		$('.nbr-dialog').fadeIn();
		
		self.FocusOnTitle(true);
	};
	
	self.ShowCopyModulePanel = function() {
		self.SelectedModule(null);
		
		self.ParentNode().ToggleLoadingScreen(true);
		
		self.ParentNode().ServerCallback('ListAllPages', 'portalId=' + controlPanelPortalId, function(serverData) {
			if (serverData.Success){
				self.Pages(serverData.CustomObject);
				self.ParentNode().ToggleLoadingScreen(false);
			}
			else{
				self.ParentNode().ToggleConfirmScreen('Sorry, We ran into a problem.', 'Please try again');
			}
		});
		
		self.DialogVisible(true);
		$('.nbr-dialog').fadeIn();
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
    var modulesSubModuleNode = document.getElementById('nbr-admin-suite-modules');
    if (modulesSubModuleNode) {
        ko.cleanNode(modulesSubModuleNode);

        var myItem = new nBraneAdminSuiteModulesViewModel();
        ko.applyBindings(myItem, modulesSubModuleNode);
    }
});