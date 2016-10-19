
//----	Claim Listing	---------------------------

// MODAL NAVIGATION

function modalViewAssets(thisID)
{
	var claimID = $('#' + thisID).parents('tr:first').data('claim-id');
	var modalURL = 'asset-manager.aspx?dir-group=claim&dir-id=' + claimID + '&dir-label=claim-header';
	$('#' + thisID).colorbox({ href: modalURL, iframe: true, width: "1200px", height: "700px" });
}

function modalViewHistory(thisID) 
{
	var claimID = $('#' + thisID).parents('tr:first').data('claim-id');
	var modalURL;
	modalURL = 'history.aspx?object-id=' + claimID + '&section=Claim-Header';
	$('#' + thisID).colorbox({ href: modalURL, iframe: true, width: "1200px", height: "700px" });
}

// STANDARD NAVIGATION

function gotoClaimForm(thisID) 
{
	var claimID = $('#' + thisID).parents('tr:first').data('claim-id');
	var nextURL = 'claim-form.aspx?id=' + claimID;
	window.location.href = nextURL;
}