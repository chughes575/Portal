
//---- Cut & Paste ---------------------------

$(document).ready( function ()
{	
	$(".paste-field").each(function (type)
	{
		$('body').delegate('.paste-field-label', 'click', function ()
		{
			$(this).next(".paste-field").focus();
		});

		$('body').delegate('.paste-field', 'focus', function ()
		{
			$(this).prev(".paste-field-label").addClass("in-focus");
		});

		$('body').delegate('.paste-field', 'keypress', function ()
		{
			$(this).prev(".paste-field-label").addClass("has-text").removeClass("in-focus");
		});

		$('body').delegate('.paste-field', 'blur', function ()
		{
			$(this).prev(".paste-field-label").removeClass("has-text").removeClass("in-focus");
		});

	});

	$('body').delegate('.paste-field', 'keyup', function ()
	{		
		if ($(this).val() != '')
		{
			var pastedValue = $(this).val();

			// replace new lines & tabs with delimiters
			pastedValue = pastedValue.replace(/\n/g, '~~~'); // new line
			pastedValue = pastedValue.replace(/\t/g, '|'); // tab

			var clipboardID = '#' + clientIDPrefix + 'tbClipboard';
			$(clipboardID).val(pastedValue);
			$(this).val('');

			showOverlay("the message");
			
			// .net postback
			__doPostBack('tbClipboard', '');
			//__doPostBack(clientIDPrefix + 'tbClipboard', '');
			
			// wait for #tbClipboard to be cleared (means serverside code has completed c&p task)
			var checkClipboard = function ()
			{
				if ($('#' + clientIDPrefix + 'tbClipboard').val() == '')
				{
					clearInterval(waitForPaste);
					hideOverlay();
				}
			};
			var waitForPaste = setInterval(checkClipboard, 50);

		}
	});	
});