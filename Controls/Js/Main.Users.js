
var nBraneAdminSuiteUsersViewModel = function () {
    var self = this;
	self.Users = ko.observableArray();
	self.SelectedUser = ko.observable();
	self.DefaultAction = ko.observable('edit');
	
	self.UserName = ko.observable('');
	self.DisplayName = ko.observable('');
	self.EmailAddress = ko.observable('');
	self.Password = ko.observable('');
	
	self.LoadInitialView = function() {
		self.ParentNode().ToggleLoadingScreen(true);
		
		self.ParentNode().ServerCallback('ListUsers', 'filter=', function(serverData) {
			if (serverData.Success){
				self.Users(serverData.CustomObject);

				self.ParentNode().ToggleLoadingScreen(false);
			}
			else{
				self.ParentNode().ToggleConfirmScreen('Sorry, We ran into a problem.', 'Please try again');
			}
		});
	};

	self.ShowUserDetailsDialog = function(user){
		self.SelectedUser(user);
		self.DisplayName(user.Name);
		$('.nbr-dialog').fadeIn();
	};
	
	self.ShowAddNewUserDialog = function() {
		self.SelectedUser(null);
		
		$('.nbr-dialog').fadeIn();
	};
	
	self.AddUser = function(){
		
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
    var usersSubModuleNode = document.getElementById('nbr-admin-suite-users');
    if (usersSubModuleNode) {
        ko.cleanNode(usersSubModuleNode);

        var myItem = new nBraneAdminSuiteUsersViewModel();
        ko.applyBindings(myItem, usersSubModuleNode);
    }
});