
var nBraneAdminSuiteUsersViewModel = function () {
    var self = this;

	self.LoadInitialView = function() {
		self.ParentNode().ToggleLoadingScreen(true);
		
		self.ParentNode().ServerCallback('ListUsers', 'filter=', function(serverData) {
			if (serverData.Success){
				$('#nbr-admin-suite-users > li:not(:first)').remove();
		
				for (i = 0; i < serverData.CustomObject.length; i++) { 
					$('#nbr-admin-suite-users').append('<li data-id="' + serverData.CustomObject[i].Value + '"><span>' + serverData.CustomObject[i].Name + '</span></li>');
				}
				self.ParentNode().ToggleLoadingScreen(false);
			}
			else{
				self.ParentNode().ToggleConfirmScreen('Sorry, We ran into a problem.', 'Please try again');
			}
		});
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
    var usersSubModuleNode = document.getElementById('nbr-admin-suite-users');
    if (usersSubModuleNode) {
        ko.cleanNode(usersSubModuleNode);

        var myItem = new nBraneAdminSuiteUsersViewModel();
        ko.applyBindings(myItem, usersSubModuleNode);
    }
});