
//---- Global---------------------------

var pathname;
var page;
var ext;
var pageView;
var postBackCount;

var clientIDPrefix			= "MainContent_";

$(document).ready(function ()
{
	// detect / flag page
	pathname				= $(location).attr('pathname');
	page					= pathname.substring(pathname.lastIndexOf('/') + 1, pathname.length);
	ext						= page.substring(page.lastIndexOf('.'), page.length);
	pageView				= page.replace(ext, '').toLowerCase();

	if (typeof pageView === "undefined" || pageView == "index" || pageView == "") { pageView = "homepage"; } // catch homepage exception
	$('body').addClass(pageView); // label message page

	// assign  plugins
	$('#topNav').onePageNav(
	{
		currentClass: 'current',
		changeHash: false,
		scrollSpeed: 750,
		scrollOffset: 30,
		scrollThreshold: 0.5,
		filter: '',
		easing: 'swing'
	});

	uiMessage();
	redrawPaging();

	// assign listenres

	$('#hdnSystemMessage').on('change', function () // check for changes to system message
	{
		uiMessage();
	});

	$('#content').delegate('#dvSystemMessageNotification', 'click', function () // user requested system message
	{
		uiMessage();
	});

	$('#content').delegate('a.reload-page', 'click', function ()
	{
		loadPage(window.location.href);
	});

});


function postBackEvent(postBackNumber)
{
	if (postBackNumber > 0)
	{
		switch (pageView)
		{
			case "claim-listing":
				redrawPaging();
				break;
			case "claim-form":
				redrawPaging();
				break;
			case "division-listing":
				redrawPaging();
				break;
			case "contact-listing":
				redrawPaging();
				break;
		}
	}
}