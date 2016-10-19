
//---- Division Form ---------------------------

$(document).ready( function ()
{
	highlightInvalidFields();

	// validate header - standard pre submit 
	$('form').delegate('input[type=submit]', 'click', function ()
	{
		return runValidationCheck('.main-content');
	});

});
