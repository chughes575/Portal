
//---- Utils ---------------------------

var js_debug = true;

/* encoding */
function encode(input)
{
	return $('<div/>').text(input).html();
}
function decode(input)
{
	return $('<div/>').html(input).text();
}

/* overlay content */
function showOverlay(msg)
{
	$('#dvOverlay #message').text(msg);
	$('#dvOverlay').css('visibility','visible');
	$('#dvOverlay').css('z-index','99');
};

function hideOverlay()
{
	$('#dvOverlay').css('visibility','hidden');
	$('#dvOverlay').css('z-index','-1');
};

/* date extension */
Date.prototype.yyyymmdd = function ()
{
	var yyyy = this.getFullYear().toString();
	var mm = (this.getMonth()+1).toString();										// getMonth() is zero-based
	var dd  = this.getDate().toString();
	return yyyy + (mm[1]?mm:"0"+mm[0]) + (dd[1]?dd:"0"+dd[0]);	// padding
};

/* replaceAll	*/
function replaceAll(txt, replace, with_this)
{
	return txt.replace(new RegExp(replace, 'g'),with_this);
}

/* strpos | search string for value & return index */
function strpos(haystack, needle, offset)
{
  var i = (haystack+'').indexOf(needle, (offset || 0));
  return i === -1 ? false : i;
}

/* querystring_lookup | passed key, returns value */
function qs_lookup(qs_label)
{
	var qs;
	var arr_qs;
	var ft;
	var qs_value;

	qs = window.location.search.substring(1);
	arr_qs = qs.split("&");

	for (i = 0; i < arr_qs.length; i++)
	{
		qs_value = arr_qs[i].split("=");
		if (qs_value[0] == qs_label)
		{
			return qs_value[1];
		}
	}
}

/* random */
jQuery.jQueryRandom = 0;
jQuery.extend(jQuery.expr[":"],
{
	random: function (a, i, m, r)
	{
		if (i == 0) {
			jQuery.jQueryRandom = Math.floor(Math.random() * r.length);
		};
		return i == jQuery.jQueryRandom;
	}
});

/* log & msg */
jQuery.log = function (message)
{
	if (js_debug)
	{
		if (window.console)
		{
			console.debug(message);
		}
		else
		{
			alert(message);
		}
	}
};
jQuery.msg = function (message)
{
	if (debug_msg == true)
	{
		alert(message);
	}
};

/* dump | print_r equivalent */
function dump(arr, level)
{
	var dumped_text = "";
	if (!level) level = 0;

	//The padding given at the beginning of the line.
	var level_padding = "";
	for (var j = 0; j < level + 1; j++) level_padding += "    ";

	if (typeof (arr) == 'object') //Array/Hashes/Objects
	{
		for (var item in arr)
		{
			var value = arr[item];

			if (typeof (value) == 'object') //If it is an array,
			{
				dumped_text += level_padding + "'" + item + "' ...\n";
				dumped_text += dump(value, level + 1);
			}
			else
			{
				dumped_text += level_padding + "'" + item + "' => \"" + value + "\"\n";
			}
		}
	}
	else
	{ //Stings/Chars/Numbers etc.
		dumped_text = "===>" + arr + "<===(" + typeof (arr) + ")";
	}
	return dumped_text;
}