
//---- Contact Listing---------------------------

$(document).ready( function ()
{
	// validate gridview row
	$('.contacts').delegate('.row-selected input.validate', 'keyup', function () // update gridview attr
	{		
		validateGVRow($(this).attr('name'), 'name');
	});
	
});

//-- STANDARD NAVIGATION

function gotoContactForm(thisID)
{
	var contactID = $('#' + thisID).parents('tr:first').data('contact-id');
	var nextURL = 'contact-form.aspx?contact-id=' + contactID;
	window.location.href = nextURL;
}