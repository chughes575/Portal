
//----	Comms Manager	---------------------------

$(document).ready( function ()
{
	// set default attachments read only
	$('.attachment-checkbox input').each( function ()
	{
		if ($(this).parent().data('default-attachment') == 'True')
		{
			$(this).prop('disabled', 'disabled');
		}
	});
	
	// toggle select all
	$('form').delegate('#toggle-select', 'click', function ()
	{
		var saProp = $('#toggle-select').prop('checked');

		$('.attachment-checkbox input').each(function ()
		{
			if ($(this).parent().data('default-attachment') != 'True')
			{
				$(this).prop('checked', saProp);
			}
		});
	});

	//var CKEDITOR_BASEPATH = '/debit-notes-portal/Scripts/MP/ckeditor_440/';
	var CKEDITOR_BASEPATH = 'Scripts/MP/ckeditor_440/';
	//CKEDITOR.replace('MainContent_txBody',{toolbar : 'Min_Toolbar_Admin'});
	CKEDITOR.replace('MainContent_txBody', { toolbar: 'Min_Toolbar' });

});

$(document).on('submit', 'form', function ()
{
	// remove html from post as this causes a security warning.
	// btn click asp event reset the field value as html
	var id = $('#' + clientIDPrefix + 'hdnID').val();
	var action = $('#' + clientIDPrefix + 'hdnAction').val();

	var txBody = $('#' + clientIDPrefix + 'txBody').val();
	$('#' + clientIDPrefix + 'txBody').val(encode(txBody));
});