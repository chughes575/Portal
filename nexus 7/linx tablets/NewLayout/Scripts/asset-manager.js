
//---- Asset Manager ---------------------------

$(document).ready( function ()
{
	// tidy up file input field
	$("#fileUpload").customFileInput();

	// asset line action (view / delete / etc)
	$('form').delegate('.asset-control-icon', 'click', function ()
	{
		// get action & asset id
		var id = $(this).data('id');
		var cmd = $(this).data('action');
		var ajax = $(this).data('ajax');
		var proceed = true;

		if (cmd == "delete")
		{
			if (!window.confirm('Are you sure you want to delete this item?')) proceed = false;
		}

		if (proceed == true)
		{
			if (ajax == true)
			{
				$('#txAjaxAssetAction').val(cmd + '|' + id);
				__doPostBack('txAjaxAssetAction', '');
			}
			else
			{
				$('#txAssetAction').val(cmd + '|' + id);
				__doPostBack('txAssetAction', '');
			}
		}
	
	});	

	// asset line toggle access level
	$('form').delegate('.asset-control-checkbox input', 'change', function ()
	{
		id = ($(this).val());
		cmd = 'access-level';
		value = ($(this).prop('checked')) ? 1 : 0;
		
		$('#txAjaxAssetAction').val(cmd + '|' + id + '|' + value);
		__doPostBack('txAjaxAssetAction', '');
	});

	// manage  checkbox / icon relationship
	$('form').delegate('div.checkbox-icon', 'click', function (e)
	{
		var targetCB = $(this).attr('id').replace('Icon', '');
		var targetLB = 'lb' + targetCB.replace('cb', '');
		var $cbEle = $('#' + targetCB);

		if ($(this).hasClass('checked')) // checked = public
		{
			$(this).removeClass('checked');
			$(this).prop('title', 'Private File');
			$('#' + targetLB).text('Private File');
			$cbEle.prop('checked', false);
		}
		else
		{
			$(this).addClass('checked');
			$(this).prop('title', 'Public File');
			$('#' + targetLB).text('Public File');
			$cbEle.prop('checked', true);
		}
	});
});