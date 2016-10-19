
//---- Framework ---------------------------

function uiMessage() // display UI message to user
{
	if (($('#hdnSystemMessage').val()).length > 1)
	{
		$('#dvSystemMessageNotification').css('display', 'block');
		var system_message					= $('#hdnSystemMessage').val();
		var message_array					= system_message.split('|||');

		switch (message_array[0])
		{
			case "Oracle Validation":
				var message_content			= message_array[2].split('|')
				if (message_content[0] == 'H')
				{
					system_message			= "Header Error\n\nField : " + message_content[2] + "\nError : " + message_content[3];
				}
				else
				{
					system_message			= "Line Error - " + message_content[1] + "\n\nField : " + message_content[2] + "\nError : " + message_content[3];
				}
				message_array[0] + ' ' + system_message
				break;

			default:
				system_message				= message_array[2];
				break;
		}
		$.notify(system_message, message_array[1].toLowerCase(), { autoHide: false, clickToHide: true });
	}
	else
	{
		$('#dvSystemMessageNotification').css('display', 'none');
	}
	
}

function redrawPaging() // fix gridview paging dom structure
{	
	if ($('.gridview .paging').length > 0)
	{
		var $pagingContainer			= $('.gridview .paging table tr');
		var $pagingControls				= $('<td class="paged">');
		$pagingContainer.find('td').each(function ()
		{
			if (!$(this).hasClass("paged"))
			{
				$pagingControls.append('<div class="td">' + $(this).html() + '</div>');
			}
		});
		$pagingContainer.html($pagingControls);
	}
}

function loadPage(_URL)
{
	window.location.href				= _URL;
}

function reloadPage()
{
	window.location.reload(true);
}

function closeModal()
{
	$.colorbox.close();
}