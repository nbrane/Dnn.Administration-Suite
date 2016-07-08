
var nBraneAdminSuiteConfigureViewModel = function () {
    var self = this;
    self.DetermineVisibility = ko.observable(false);

    self.InitialDetermineVisibility = ko.observable(false);

    self.LoadInitialView = function () {
        self.ParentNode().ToggleLoadingScreen(true);

        self.ParentNode().ServerCallback('', 'LoadConfigurationSettings', null, function (serverData) {
            if (serverData.Success) {
                self.DetermineVisibility(serverData.CustomObject.DetermineVisibility);
                self.InitialDetermineVisibility(serverData.CustomObject.DetermineVisibility);

                $('.nbr-right-dialog').fadeIn(400, function () { self.ParentNode().ToggleLoadingScreen(false); });
            }
            else {
                self.ParentNode().ToggleConfirmScreen('Sorry, We ran into a problem.', 'Please try again');
            }
        });
    };

    

    self.UpdateConfiguration = function () {
        self.CloseDialog();
        self.ParentNode().ToggleLoadingScreen(true);

        var configObject = {};
        configObject.DetermineVisibility = self.DetermineVisibility();

        self.ParentNode().ServerCallback('', 'UpdateConfiguration', JSON.stringify(configObject), function (serverData) {
            if (serverData.Success) {
                self.ParentNode().ToggleLoadingScreen(false);
                self.CloseDialog();
            }
            else {
                self.ParentNode().ToggleConfirmScreen('Sorry, We ran into a problem.', 'Please try again');
            }
        }, null, null, 'POST');
    };

    self.CloseDialog = function () {
        $('.nbr-upper-control-panel li.selected').removeClass('selected');
        $('.nbr-right-dialog').fadeOut(400);
        $('#nbr-admin-suite-configure').remove();
    };

    self.ParentNode = function () {
        return ko.contextFor(nBraneAdminSuiteNode).$data;
    };

    self.LoadInitialView();
};

$(document).ready(function () {
    var configureSubModuleNode = document.getElementById('nbr-admin-suite-configure');
    if (configureSubModuleNode) {
        ko.cleanNode(configureSubModuleNode);

        var myItem = new nBraneAdminSuiteConfigureViewModel();
        ko.applyBindings(myItem, configureSubModuleNode);
    }
});