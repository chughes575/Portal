
//---- Claim Form ---------------------------

$(document).ready( function ()
{	
	// HEADER	

	highlightInvalidFields();

	// validate header - standard pre submit 
	$('form').delegate('input[type=submit]', 'click', function ()
	{
		if ($(this).val() != "Delete")
		{
			return runValidationCheck('.claim-header');
		}
	});

	// data picker formating
	$('.claim-header').delegate('.datepicker', 'click', function ()
	{
		$(this).datepicker({
			'dateFormat': 'yy-mm-dd',
			'changeMonth': 'true',
			'changeYear': 'true'
		});
		$(this).datepicker("show");
	});

	// show ~ hide additional recipients
	$('.claim-header').delegate('#' + clientIDPrefix + 'additional-recipients', 'change', function ()
	{
		if ($('#' + clientIDPrefix + 'additional-recipients').children().length > 0)
		{
			$('#' + clientIDPrefix + 'lblAdditionalRecipients').removeClass('hidden');
			$('#' + clientIDPrefix + 'additional-recipients').removeClass('hidden');
		}
		else
		{
			$('#' + clientIDPrefix + 'lblAdditionalRecipients').addClass('hidden');
			$('#' + clientIDPrefix + 'additional-recipients').addClass('hidden');
		}
	});
	$('#' + clientIDPrefix + 'additional-recipients').trigger('change');

	// LINES

	// validate gridview row
	$('.claim-lines').delegate('.row-selected input.validate', 'keyup', function ()
	{
		// update gridview attr
		validateGVRow($(this).attr('name'), 'name');
	});

	$('.claim-lines').delegate('.row-selected input.validate', 'blur', function ()
	{
		// update gridview attr
		validateGVRow($(this).attr('name'), 'name');
	});


	// calculate & display line totals
	
	// add flags - element in new claim selected row has been amended
	$('.claim-lines').delegate('#' + clientIDPrefix + 'NewClaimLineGV input', 'change', function ()
	{
		$(this).parents('tr:first').attr('data-row-amended', 'true');
	});

	// add flags - element in existing claim row has been amended
	$('.claim-lines').delegate('#' + clientIDPrefix + 'ExistingClaimLineGV input', 'change', function ()
	{
		$(this).parents('tr:first').attr('data-row-amended', 'true');
	});

	// calculate new claim line values
	$('.claim-lines').delegate('#' + clientIDPrefix + 'NewClaimLineGV .value-calculation input', 'keyup', function (e)
	{
		var code = (e.keyCode ? e.keyCode : e.which);
		var inputValue = $(this).val();
		if ((code == 8 || code == 46) && inputValue == '') { // user pressed backspace | del & current value < 10
			// do nothing 
		} else {
			if ($(this).attr('title') != 'line_total') {
				updateLineValues(clientIDPrefix + 'NewClaimLineGV');
			}
		}
	});
	
	// calculate existing claim line values
	$('.claim-lines').delegate('#' + clientIDPrefix + 'ExistingClaimLineGV .value-calculation input', 'keyup', function (e)
	{
		var code = (e.keyCode ? e.keyCode : e.which);
		var inputValue = $(this).val();
		if ((code == 8 || code == 46) && inputValue == '') { // user pressed backspace | del & current value < 10
			// do nothing 
		} else {
			if ($(this).attr('title') != 'line_total') {
				updateLineValues(clientIDPrefix + 'ExistingClaimLineGV');
			}
		}
	});

	// flag invalid fields
	var invalidFields = $('#' + clientIDPrefix + 'hdnInvalidFields').val();
	if (invalidFields != '')
	{
		var invalidFieldArr = invalidFields.split('|');
		for (var i = 0; i < invalidFieldArr.length; i++)
		{
			$('#' + clientIDPrefix + invalidFieldArr[i]).addClass('invalid field-error');
		}
	}
	
}); // doc.ready

// FORM FUNCTIONS

// manipulate values | claim lines

function updateLineValues(thisGridView)
{	
	var line_quantity;
	var line_original_cost;
	var line_new_cost;
	var line_delta;
	var line_total;

	$('#' + thisGridView + ' .value-calculation input').each( function ()
	{
		var cellValue = $(this).val();
		var cellTitle = $(this).attr('title');

		//$.log(cellTitle + "|" + cellValue);

		if (!(typeof cellValue !== "undefined" && cellValue != ''))
		{
			// init blank fields
			cellValue = 0;
			$(this).val(cellValue);
		}
		else
		{
			// get value based on input class
			if ($(this).parent().hasClass("quantity")) {
				line_quantity = replaceAll(cellValue, ",", "");
			}

			if ($(this).parent().hasClass("original-cost")) {
				line_original_cost = replaceAll(cellValue, ",", "");
			}

			if ($(this).parent().hasClass("new-cost")) {
				line_new_cost = replaceAll(cellValue, ",", "");
			}

			if ($(this).parent().hasClass("delta")) {
				line_delta = replaceAll(cellValue, ",", "");
			}

			if ($(this).parent().hasClass("value")) {
				line_total = replaceAll(cellValue, ",", "");
			}
			//$.log(line_quantity + "|" + line_original_cost + "|" + line_new_cost + "|" + line_delta + "|" + line_total);

		}
	}); //each

	// convert type
	line_quantity = parseInt(line_quantity);
	line_original_cost = parseFloat(line_original_cost);
	line_new_cost = parseFloat(line_new_cost);
	line_delta = parseFloat(line_delta);
	line_total = parseFloat(line_total);

	// set to 2 decimal places
	line_original_cost = line_original_cost.toFixed(2);
	line_new_cost = line_new_cost.toFixed(2);
	line_delta = line_delta.toFixed(2);
	line_total = line_total.toFixed(2);

	
	// calculate & set line values
	var claimTypeID = $('#' + clientIDPrefix + 'ddClaimTypeID option:selected').val();

	//$.log(claimTypeID);
	switch (claimTypeID)
	{
		case '1': // stock base claim
			var claimLineDelta = line_original_cost - line_new_cost;
			var claimLineValue = claimLineDelta * line_quantity;

			$('#' + thisGridView + ' .value-calculation.delta input').val(claimLineDelta.toFixed(2));
			$('#' + thisGridView + ' .value-calculation.value input').val(claimLineValue.toFixed(2));
			break;

		case '2': // sales based claim
			var claimLineDelta = line_original_cost - line_new_cost;
			var claimLineValue = claimLineDelta * line_quantity;

			$('#' + thisGridView + ' .value-calculation.delta input').val(claimLineDelta.toFixed(2));
			$('#' + thisGridView + ' .value-calculation.value input').val(claimLineValue.toFixed(2));
			break;

		case '3': // marketing claim
			var claimLineValue = line_delta * line_quantity;
			$('#' + thisGridView + ' .value-calculation.value input').val(claimLineValue.toFixed(2));
			break;

		case '4': // non-marketing claim
			var claimLineValue = line_delta * line_quantity;
			$('#' + thisGridView + ' .value-calculation.value input').val(claimLineValue.toFixed(2));
			break;

		default:
			break;
	}
}

//  MODAL NAVIGATION

function modalHeaderUploadForm(thisID)
{
	var claimID = $('#' + clientIDPrefix + 'tbRequestID').val();
	var modalURL;
	modalURL = 'asset-manager.aspx?dir-group=claim&dir-id=' + claimID + '&dir-label=claim-header';
	$('#' + thisID).colorbox({ href: modalURL, iframe: true, width: "1200px", height: "700px" });
}

function modalLineUploadForm(thisID)
{
	var claimID = $('#' + clientIDPrefix + 'tbRequestID').val();
	var lineID = $('#' + thisID).parents('tr:first').attr('data-request-line-id');
	var modalURL;
	modalURL = 'asset-manager.aspx?dir-group=claim&parent-id=' + claimID + '&parent-label=claim-header&dir-id=' + lineID + '&dir-label=claim-line';
	$('#' + thisID).colorbox({ href: modalURL, iframe: true, width: "1200px", height: "700px" });
}

function modalViewHistory(thisID)
{
	var claimID = $('#' + clientIDPrefix + 'tbRequestID').val();
	var modalURL;
	modalURL = 'history.aspx?object-id=' + claimID + '&section=Claim-Header';
	$('#' + thisID).colorbox({ href: modalURL, iframe: true, width: "1200px", height: "700px" });
}