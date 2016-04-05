
var nBraneAdminSuiteUsersViewModel = function () {
    var self = this;
	self.Users = ko.observableArray();
	self.SelectedUser = ko.observable();
	self.DefaultAction = ko.observable('edit');
	
	self.Id = ko.observable(-1);
	self.UserName = ko.observable('');
	self.DisplayName = ko.observable('');
	self.FirstName = ko.observable('');
	self.LastName = ko.observable('');
	self.EmailAddress = ko.observable('');
	self.Password = ko.observable('');
	self.LastLogin = ko.observable('');
	self.Approved = ko.observable(true);
	self.Locked = ko.observable(false);
	
	self.filteredUsers = ko.computed(function() {
        if(self.DefaultAction() == 'login') {
            return self.Users(); 
        } else {
            return ko.utils.arrayFilter(self.Users(), function(usr) {
                return usr.Value != 0;
            });
        }
    });
	
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
		
		var userObject = {};
		userObject.Id = self.SelectedUser().Value;
		
		if (self.DefaultAction() == 'edit') {
			self.ParentNode().ServerCallback('LoadUserDetails', 'Id=' + self.SelectedUser().Value, function(serverData) {
				if (serverData.Success){
					self.Id(serverData.CustomObject.Id);
					self.UserName(serverData.CustomObject.UserName);
					self.DisplayName(serverData.CustomObject.DisplayName);
					self.FirstName(serverData.CustomObject.FirstName);
					self.LastName(serverData.CustomObject.LastName);
					self.EmailAddress(serverData.CustomObject.EmailAddress);
					self.Approved(serverData.CustomObject.Authorized);
					self.Locked(serverData.CustomObject.Locked);
					self.LastLogin(serverData.CustomObject.LastLogin);
					$('.nbr-dialog').fadeIn();
				}
				else{
					self.ParentNode().ToggleConfirmScreen('Sorry, We ran into a problem.', 'Please try again');
				}
			});
		} else if (self.DefaultAction() == 'login') {
			self.ParentNode().ServerCallback('Impersonate', 'Id=' + self.SelectedUser().Value, function(serverData) {
				if (serverData.Success){
					location.reload();
				}
				else{
					self.ParentNode().ToggleConfirmScreen('Sorry, We ran into a problem.', 'Please try again');
				}
			});
		}
		 else if (self.DefaultAction() == 'delete') {
			self.ParentNode().ServerCallback('DeleteUser', JSON.stringify(userObject), function(serverData) {
				if (serverData.Success){
					location.reload();
				}
				else{
					self.ParentNode().ToggleConfirmScreen('Sorry, We ran into a problem.', 'Please try again');
				}
			}, null,null, 'POST');
		}
	};
	
	self.ShowAddNewUserDialog = function() {
		self.SelectedUser(null);
		self.Id(-1);
		self.UserName('');
		self.DisplayName('');
		self.FirstName('');
		self.LastName('');
		self.EmailAddress('');
		self.Approved(true);
		self.Locked(false);
		self.LastLogin('');
		
		$('.nbr-dialog').fadeIn();
	};
	
	self.AddUser = function(){
		self.ParentNode().ToggleLoadingScreen(true);
		
		if (self.DefaultAction() == 'edit') {
			var userObject = {};
			userObject.Id = self.Id();
			userObject.DisplayName = self.DisplayName();
			userObject.FirstName = self.FirstName();
			userObject.LastName = self.LastName();
			userObject.EmailAddress = self.EmailAddress();
			
			if (self.Id() == -1){
				userObject.Password = self.Password();
				userObject.UserName = self.UserName();
			}
			
			self.ParentNode().ServerCallback('SaveUser', JSON.stringify(userObject), function(serverData) {
				if (serverData.Success){
					$('.nbr-dialog').fadeOut(400, function() {self.LoadInitialView();});
				}
				else{
					self.ParentNode().ToggleConfirmScreen('Sorry, We ran into a problem.', 'Please try again');
				}
			}, null,null, 'POST');
			
		}
	};
	
	self.SetDefaultAction = function(data, event){
		var listItem = event.target;
        if (listItem.nodeName != "li" && listItem.nodeName != "LI") {
            listItem = event.target.parentNode;
        }
		self.DefaultAction($(listItem).data('action'));
	};
	
	self.CloseDialog = function(module) {
		$('.nbr-dialog').fadeOut(400, function() {self.SelectedUser(null);});
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