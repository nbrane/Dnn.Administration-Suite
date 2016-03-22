var nBraneAdminSuiteMainViewModel = function () {
    var self = this;

    self.Load = function (item, event) {
        self.ToggleLoadingScreen(true);

        var listItem = event.target;
        if (listItem.nodeName != "li" && listItem.nodeName != "LI") {
            listItem = event.target.parentNode;
        }

        self.CloseSubMenu();
        $(listItem).addClass('selected');

        self.ServerCallback('load', 'Name=' + $(listItem).data('action'), function (serverData) {
				var decoded = $('<div/>').html(serverData.HTML).text();
				$('.nbr-admin-suite').append(decoded);
				
				if (serverData.JS){
					$('<script>').attr('type', 'text/javascript')
						.text(serverData.JS)
						.appendTo('head');
				}

			}, function(xhr, status, thrown){

				self.ToggleConfirmScreen('Sorry, We ran into a problem.', status + ': ' + thrown);
				self.CloseSubMenu();
			}
		);
    };

    self.CloseSubMenu = function () {
        $('.nbr-sub-menu').remove();
        $('.nbr-control-panel li').removeClass('selected');
    };

    self.ToggleLoadingScreen = function (visibility) {
        if (visibility) {
            $('html').block({
                 css: {
					'position': 'fixed',
                    'top': '30%',
                    'border': '#009344 7px solid',
                    'width': '200px',
                    'height': '140px !important',
                    'padding': '15px 15px 30px',
                    'backgroundColor': '#fff',
                    'color': '#000',
                    '-webkit-border-radius': '15px',
                    '-moz-border-radius': '15px'
                },
                message: $(".nbr-admin-suite-loading"),
                'centerX': true,
                'centerY': false
            });
        } else {
            $('html').unblock();
        }
    };

	self.ToggleConfirmScreen = function(message, subMessage, okAction) {
        $('html').block({
            css: {
				'position': 'fixed',
				'top': '30%',
				'border': '#FFCA00 7px solid',
				'width': '450px',
				'height': '140px !important',
				'padding': '15px 15px 30px',
				'backgroundColor': '#fff',
				'color': '#000',
				'-webkit-border-radius': '15px',
				'-moz-border-radius': '15px'
            },
            message: $(".nbr-admin-suite-confirm"),
                'centerX': true,
                'centerY': false
        });

        $("#messagePlaceholder").html(message);
        $("#subMessagePlaceholder").html(subMessage);

        $('#yesButton').click(function (event) {
            event.preventDefault();
            $('html').unblock();

            if (okAction && $.isFunction(okAction)) {
                okAction();
            }

            $(this).unbind('click');
        });

        $('#noButton').click(function (event) {
            event.preventDefault();
            $('html').unblock();
        });
		
		if (!okAction){
			$('#yesButton').hide();
			$('#noButton').text('Close & Continue');
		} else {
			$('#yesButton').show();
			$('#noButton').text('No');
		}
    };

    self.ServerCallback = function (func, parameters, success, failure, complete, method) {

        var service = $.dnnSF();
        var serviceUrl = service.getServiceRoot('nbrane/administrationsuite') + 'ControlPanel/' + func;

        method = method || "GET";

        $.ajax({
            url: serviceUrl,
            beforeSend: service.setModuleHeaders,
            cache: false,
            contentType: 'application/json; charset=UTF-8',
            type: method,
            data: parameters,
            success: success,
            error: failure,
            complete: complete
        });
    }
};

var nBraneAdminSuiteNode = null;
$(document).ready(function () {
    nBraneAdminSuiteNode = document.getElementById('nbr-admin-suite');
    if (nBraneAdminSuiteNode) {
        ko.cleanNode(nBraneAdminSuiteNode);

        var koViewModel = new nBraneAdminSuiteMainViewModel();
        ko.applyBindings(koViewModel, nBraneAdminSuiteNode);
    }
});