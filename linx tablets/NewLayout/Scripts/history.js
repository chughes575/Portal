
//---- History ---------------------------

$(document).ready( function ()
{
	// show | hide content based on checkbox filter

	$('.header').delegate('#dvGVFilters div', 'click', function ()
	{
		var filter_value;

		if ($(this).hasClass('true'))
		{
			$(this).removeClass('true');
			$('#hdnGVFilter_' + $(this).attr('id')).val('False');
			filter_value = "false";
		}
		else
		{
			$(this).addClass('true');
			$(this).removeClass('false');
			$('#hdnGVFilter_' + $(this).attr('id')).val('True');
			filter_value = "true";
		}		
		$('#txGVFilter').val($(this).attr('id') + '-' + filter_value);
		__doPostBack('txGVFilter', ''); // .net postback
	});
});