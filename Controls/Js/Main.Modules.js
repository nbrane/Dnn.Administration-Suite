
var nBraneAdminSuiteModulesViewModel = function () {
    var self = this;
	self.Modules = ko.observableArray();
	self.ModuleTitle = ko.observable('');
	self.ModuleVisibility = ko.observable('');
	self.ModuleLocation = ko.observable('');
	self.ModulePosition = ko.observable('');
	self.ModuleContainer = ko.observable('');
	self.SelectedModule = ko.observable();
	
	self.LoadInitialView = function() {
		self.ParentNode().ToggleLoadingScreen(true);
		
		self.ParentNode().ServerCallback('ListModules', 'category=all', function(serverData) {
			if (serverData.Success){
				self.Modules(serverData.CustomObject);

				self.ParentNode().ToggleLoadingScreen(false);
			}
			else{
				self.ParentNode().ToggleConfirmScreen('Sorry, We ran into a problem.', 'Please try again');
			}
		});
		
		
	};
	self.AddModule = function(module) {
		console.log(module);
	};
	
	self.CloseDialog = function(module) {
		$('.nbr-dialog').fadeOut();
	};
	
	self.ShowAddNewModuleDialog = function(module) {
		self.SelectedModule(module);
		
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