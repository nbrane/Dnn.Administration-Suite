
var nBraneAdminSuiteModulesViewModel = function () {
    var self = this;
	self.Modules = ko.observableArray();
	self.Containers = ko.observableArray();
	self.Panes = ko.observableArray();
	self.ModuleTitle = ko.observable('');
	self.ModuleVisibility = ko.observable('');
	self.ModuleLocation = ko.observable('');
	self.ModulePosition = ko.observable('');
	self.ModuleContainer = ko.observable('');

	self.SelectedModule = ko.observable();
	self.DialogVisible = ko.observable(false);
	self.FocusOnTitle = ko.observable(false);
	
	self.LoadInitialView = function() {
		self.ParentNode().ToggleLoadingScreen(true);
		self.ParentNode().ServerCallback('ListModules', 'category=all', function(serverData) {
			if (serverData.Success){
				self.Modules(serverData.Modules);
				self.Panes(controlPanelPanes);
				self.Containers(serverData.Containers);
				self.ModuleLocation(serverData.DefaultModuleLocation);
				
				self.ParentNode().ToggleLoadingScreen(false);
			}
			else{
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
		$('.nbr-dialog').fadeOut();
		self.SelectedModule(null);
	};
	
	self.ShowAddNewModuleDialog = function(module) {
		self.SelectedModule(module);
		self.DialogVisible(true);
		$('.nbr-dialog').fadeIn();
		
		self.FocusOnTitle(true);
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