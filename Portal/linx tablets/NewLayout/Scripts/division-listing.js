
//---- Division Listing ---------------------------

$(document).ready( function ()
{
	// validate gridview row
	$('.divisions').delegate('.row-selected input.validate', 'keyup', function ()
	{
		// update gridview attr
		validateGVRow($(this).attr('name'), 'name');
	});
});

//-- Standard Navigation

function gotoDivisionForm(thisID)
{
	var divisionID = $('#' + thisID).parents('tr:first').data('division-id');
	var nextURL = 'division-form.aspx?division-id=' + divisionID;
	window.location.href = nextURL;
}

function gotoDuplicateForm(thisID)
{
	var divisionID = $('#' + thisID).parents('tr:first').data('division-id');
	var nextURL = 'division-form.aspx?division-id=' + divisionID + '&cmd=duplicate';
	window.location.href = nextURL;
}