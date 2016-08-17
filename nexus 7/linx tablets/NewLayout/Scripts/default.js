
//---- Default---------------------------

$(document).ready( function ()
{
	// data picker formating
	$('.claim-report').delegate('.datepicker', 'click', function ()
	{
		$(this).datepicker({
			'dateFormat': 'yy-mm-dd',
			'changeMonth': 'true',
			'changeYear': 'true'
		});
		$(this).datepicker("show");
	});

	$('.claim-report').delegate('#filter-toggle', 'click', function ()
	{
		if ($(this).hasClass("adv"))
		{
			$('.dd-wrapper').hide(1000);
			$(this).text("[+]");
		}
		else
		{
			$('.dd-wrapper').show(500);
			$(this).text("[-]");
		}
		$(this).toggleClass('adv');
	});
});