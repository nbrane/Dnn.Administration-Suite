
var nBraneAdminSuitePagesViewModel = function () {
    var self = this;

	self.LoadInitialView = function() {
		self.ParentNode().ToggleLoadingScreen(true);
		
		self.ParentNode().ServerCallback('ListPages', 'parent=all', function(serverData) {
			if (serverData.Success){
				$('#nbr-admin-suite-pages > li:not(:first)').remove();
		
				for (i = 0; i < serverData.CustomObject.length; i++) { 
					$('#nbr-admin-suite-pages').append('<li data-id="' + serverData.CustomObject[i].Value + '"><span>' + serverData.CustomObject[i].Name + '</span></li>');
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
    var pagesSubModuleNode = document.getElementById('nbr-admin-suite-pages');
    if (pagesSubModuleNode) {
        ko.cleanNode(pagesSubModuleNode);

        var myItem = new nBraneAdminSuitePagesViewModel();
        ko.applyBindings(myItem, pagesSubModuleNode);
    }
});