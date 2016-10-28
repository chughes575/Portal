
//---- Form Manager ---------------------------

$(document).ready( function ()
{
	// validation extention

	jQuery.validator.addMethod("phone", function (phone_number, element)
	{
		phone_number = phone_number.replace(/\s+/g, "");
		return this.optional(element) || phone_number.length > 9 && phone_number.match(/^([\+][0-9]{1,3}[\ \.\-])?([\(]{1}[0-9]{2,6}[\)])?([0-9\ \.\-\/]{3,20})((x|ext|extension)[\ ]?[0-9]{1,4})?$/);
	}, "Please specify a valid phone number");
		
	$('form').delegate('.read-only', 'focus', function () // read only fields
	{
		$(this).attr('readonly', 'readonly');
	});
});

function highlightInvalidFields()
{
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
}

function runValidationCheck(rootElement)
{
	var formValidated = true;
	var requiredValidated = true;
	
	$('form ' + rootElement + ' .validate').each( function () // find inputs to validate
	{

		var thisID = '#' + $(this).attr('id');
		
		requiredValidated = runManditoryCheck($(this)); // run 'manditory' field check		
		fieldValidated = $('form').validate().element(thisID); // validate by value type (number | email | etc)

		if (requiredValidated && fieldValidated) // flag result on input & label
		{
			$(this).removeClass('field-error');
			$(this).parent().children('.label').removeClass('field-error');
			$(this).parent().children('.label-inlined').removeClass('field-error');

		}
		else
		{
			$(this).addClass('field-error');
			$(this).parent().children('.label').addClass('field-error');
			$(this).parent().children('.label-inlined').addClass('field-error');

			formValidated = false;
		}

	});

	return formValidated; // return result

}


function validateGVRow(thisRef, thisRefType)
{
	var targetElement = ""; // target control
	var rowValidated = true; // validation result
	var gvID = ""; // gridview reference

	switch (thisRefType)
	{
		case 'id':
			targetElement = '#' + thisRef;
			break;
		case 'class':
			targetElement = '.' + thisRef;
			break;
		default:
			targetElement = 'input[' + thisRefType + "='" + thisRef + "']";
			break;
	}

	gvID = $(targetElement).parents('table:first').attr('id'); // get gridview id

	$('#' + gvID + ' .row-selected input.validate').each(function () // find inputs to validate
	{		
		$(this).attr('id', 'focus-field');
		runManditoryCheck($(this));
		$('form').validate().element('#focus-field');
		$(this).removeAttr('id');
	});

	$(targetElement).parents('table:first').find('input.m-error').each(function () // change manditory error to standard error flag
	{
		$(this).removeClass("m-error");
		$(this).removeClass("valid");
		$(this).addClass("error");
	});

	var elem = $(targetElement).parents('table:first').find('input.error');
	rowValidated = (elem.length > 0) ? 'false' : 'true';
	
	$('#' + gvID + '_hdnRowValid').val(rowValidated); // update hidden field with result
		
	return rowValidated; // return result

}

function runManditoryCheck(elem)
{
	
	var manditoryValidated = true;
	var validateCount = 0;
	var inputName = elem.attr('name');
	var thisValue = elem.val();

	if (elem.hasClass('manditory')) // does control require validation?
	{
		switch (elem.attr('type'))
		{
			case 'radio': // radio button
				$('input[name=' + inputName + ']:checked').each(function ()
				{
					validateCount++;
				});
				manditoryValidated = (validateCount > 0) ? true : false;
				break;
			case 'checkbox': // checkbox
				$('input[name=' + inputName + ']:checked').each(function ()
				{
					validateCount++;
				});
				manditoryValidated = (validateCount > 0) ? true : false;
				break;
			case 'text': // text field
				manditoryValidated = (thisValue != '') ? true : false;
				break;
			default:
				manditoryValidated = true;
		}

		if (elem.attr('type') == undefined)
		{
			switch (elem.prop('tagName'))
			{
				case 'SELECT': // listbox
					manditoryValidated = (thisValue > 0) ? true : false;
					break;
				case 'TEXTAREA': // textarea
					manditoryValidated = (thisValue != "") ? true : false;
					break;
			}
		}

		if (manditoryValidated) // flag whether the field has failed test
		{
			elem.removeClass("m-error");
		} else {
			elem.addClass("m-error");
		}
	}

	return manditoryValidated; // return result
}