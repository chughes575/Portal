
//---- Contacts ---------------------------

$(document).ready( function ()
{	
	// validate header - standard pre submit 
	$('form').delegate('input[type=submit]', 'click', function ()
	{
		return runValidationCheck('.main-content');
	});
});