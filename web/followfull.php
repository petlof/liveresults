<?php
date_default_timezone_set("Europe/Stockholm");
$lang = "sv";
if (isset($_GET['lang']))
 $lang = $_GET['lang'];

include_once("templates/emmalang_en.php");
include_once("templates/emmalang_$lang.php");
include_once("templates/classEmma.class.php");
$currentComp = new Emma($_GET['comp']);

$RunnerStatus = Array("1" =>  $_STATUSDNS, "2" => $_STATUSDNF, "11" =>  $_STATUSWO, "12" => $_STATUSMOVEDUP, "9" => $_STATUSNOTSTARTED,"0" => $_STATUSOK, "3" => $_STATUSMP, "4" => $_STATUSDSQ, "5" => $_STATUSOT);
echo("<?xml version=\"1.0\" encoding=\"iso-8859-1\" ?>");
?>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.01 Transitional//EN"
        "http://www.w3.org/TR/html4/loose.dtd">
<html>
<head><title><?=$_TITLE?> :: <?=$currentComp->CompName()?> [<?=$currentComp->CompDate()?>]</title>
<META HTTP-EQUIV="expires" CONTENT="-1">
<meta http-equiv="Content-Type" content="text/html">
<link rel="stylesheet" type="text/css" href="css/style.css"><link rel="stylesheet" type="text/css" href="css/ui-darkness/jquery-ui-1.8.19.custom.css">
<link rel="stylesheet" type="text/css" href="css/jquery.dataTables_themeroller.css">
<script language="javascript" type="text/javascript" src="js/jquery-1.7.2.min.js"></script>
<script language="javascript" type="text/javascript" src="js/jquery-ui-1.8.19.custom.min.js"></script>
<script language="javascript" type="text/javascript" src="js/jquery.dataTables.min.js"></script>

<script language="javascript" type="text/javascript">var updateAutomatically = true;
var updateInterval = 15000;
var classUpdateInterval = 60000;

var classUpdateTimer = null;
var passingsUpdateTimer = null;

$(document).ready(function()
{
	$("#divClasses").html("Laddar klasser...");
	updateClassList();
	updateLastPassings();
});

function updateClassList()
{
	if (updateAutomatically)
	{
		$.ajax({
			  url: "api.php",
			  //cache: false,
			  data: "comp=<?=$_GET['comp']?>&method=getclasses&last_hash="+lastClassListHash,
			  success: resp_updateClassList,
			  dataType: "json"
		});
	}

}

function updateLastPassings()
{
	if (updateAutomatically)
	{
		$.ajax({
			  url: "api.php",
			  //cache: false,
			  data: "comp=<?=$_GET['comp']?>&method=getlastpassings&last_hash="+lastPassingsUpdateHash,
			  success: resp_updateLastPassings,
			  dataType: "json"
		});
	}

}

var lastClassListHash = "";
var lastPassingsUpdateHash = "";
function resp_updateClassList(data)
{
	if (data != null && data.status == "OK")
	{
		if (data.classes != null)
		{
			str = ""
			$.each(data.classes,
				function(key, value)
				{
					str += "<a href=\"javascript:chooseClass('" + value.className + "')\">" + value.className + "</a><br/>";
				}
			);
			$("#divClasses").html(str);
			lastClassListHash = data.hash;
		}
	}

	classUpdateTimer = setTimeout(updateClassList,classUpdateInterval);
}

function resp_updateLastPassings(data)
{
	if (data != null && data.status == "OK")
	{
		if (data.passings != null)
		{
			str = ""
			$.each(data.passings,
				function(key, value)
				{
					str += value.passtime + ": " + value.runnerName + " (<a href=\"javascript:chooseClass('" + value["class"] + "')\">" + value["class"] + "</a>) " + (value.control == 1000 ? "<?=$_LASTPASSFINISHED?>" : "<?=$_LASTPASSPASSED?> " + value["controlName"]) + " <?=$_LASTPASSWITHTIME ?> " + value["time"] + "<br/>";
				}
			);
			$("#divLastPassings").html(str);
			lastPassingsUpdateHash = data.hash;
		}
	}

	passingsUpdateTimer = setTimeout(updateLastPassings,updateInterval);
}

var currentTable = null;
var resUpdateTimeout = null;
function chooseClass(className)
{
	if (currentTable != null)
		currentTable.fnDestroy();

	clearTimeout(resUpdateTimeout);

	$('#divResults').html('');
	curClassName = className;
	$('#resultsHeader').html('<?=$_LOADINGRESULTS?>');
	$.ajax({
		  url: "api.php",
		  //cache: false,
		  data: "comp=<?=$_GET['comp']?>&method=getclassresults&unformattedTimes=true&class="+className,
		  success: updateClassResults,
		  dataType: "json"
	});
	resUpdateTimeout = setTimeout(checkForClassUpdate,updateInterval);
}

var curClassName = "";
var lastClassHash = "";
function checkForClassUpdate()
{
	if (updateAutomatically)
	{
		if (currentTable != null)
		{
			$.ajax({
				  url: "api.php",
				  //cache: false,
				  data: "comp=<?=$_GET['comp']?>&method=getclassresults&unformattedTimes=true&class="+curClassName + "&last_hash=" +lastClassHash ,
				  success: resp_updateClassResults,
				  dataType: "json"
			});
		}
	}

}

function setAutomaticUpdate(val)
{
	updateAutomatically = val;
	if (updateAutomatically)
	{
		$("#setAutomaticUpdateText").html("<b><?=$_AUTOUPDATE?>:</b> <?=$_ON?> | <a href=\"javascript:setAutomaticUpdate(false);\"><?=$_OFF?></a>");
		checkForClassUpdate();
		updateLastPassings();
		checkForClassUpdate();

	}
	else
	{
		clearTimeout(resUpdateTimeout);
		clearTimeout(passingsUpdateTimer);
		clearTimeout(classUpdateTimer);
		$("#setAutomaticUpdateText").html("<b><?=$_AUTOUPDATE?>:</b> <a href=\"javascript:setAutomaticUpdate(true);\"><?=$_ON?></a> | <?=$_OFF?>");
	}
}

var curClassSplits = null;
function updateResultVirtualPosition(data)
{

	if (curClassSplits != null)
	{
		for (s = 0; s < curClassSplits.length; s++)
		{
			splitCode = curClassSplits[s].code;
			data.sort(function(a,b) { return a.splits[splitCode] - b.splits[splitCode];});

			lastPos = 1;
			posCnt = 1;
			lastTime = -1;
			for (i = 0; i < data.length; i++)
			{
				if (data[i].splits[splitCode] != "")
				{
					if (lastTime == data[i].splits[splitCode])
						data[i].splits[splitCode + "-place"] = lastPos;
					else
					{
						data[i].splits[splitCode + "-place"] = posCnt;
						lastPos = posCnt;
					}
					lastTime = data[i].splits[splitCode];
					posCnt++;
				}
				else
				{
				data[i].splits[splitCode + "-place"] = "";
				}
			}

		}
	}

	data.sort(resultSorter);

	/* move down runners that have not finished to the correct place*/
	firstFinishedIdx = 0;
	for (i = 0; i < data.length; i++)
	{
		if (data[i].place != "")
		{
			firstFinishedIdx = i;
			break;
		}
	}

	tmp = Array();
	for (i = 0; i < firstFinishedIdx; i++)
		tmp.push(data[i]);

	data.splice(0,firstFinishedIdx);

	tmp.sort(sortByDist);

	for (i = 0; i < tmp.length; i++)
	{
		if (curClassSplits != null)
		{
			insertIntoResults(tmp[i],data);
		}
		else
		{
			data.push(tmp[i]);
		}
	}


	for (i = 0; i < data.length; i++)
	{
		data[i].virtual_position = i;
	}

}

function sortByDist(a,b)
{
	for (s = curClassSplits.length-1; s >= 0; s--)
	{
		splitCode = curClassSplits[s].code;
		if (a.splits[splitCode] == "" && b.splits[splitCode] != "")
			return 1;
		else if (a.splits[splitCode] != "" && b.splits[splitCode] == "")
			return -1;
	}
	return 0;
}

function insertIntoResults(result, data)
{
	for (s = curClassSplits.length-1; s >= 0; s--)
	{
		splitCode = curClassSplits[s].code;
		if (result.splits[splitCode] != "")
		{
			for (d = 0; d < data.length; d++)
			{
				if (data[d].place == "-" || (data[d].splits[splitCode] != "" && data[d].splits[splitCode] > result.splits[splitCode]))
				{
					data.splice(d,0,result);
					return;
				}
			}
		}
	}
}

function resultSorter(a,b)
{
	if (a.place != "" && b.place != "")
	{
		if (a.status != b.status)
			return a.status - b.status;
		else
		{
			if (a.result == b.result)
			{
				if (a.place == "=" && b.place != "=")
				{
					return 1;
				}
				else if (b.place == "=" && a.place != "=")
				{
					return -1;
				}
			}
			else
			{
				return a.result - b.result;
			}
		}

	}
	else if (a.place == "-" || a.place != "" )
	{
		return 1;
	}
	else if (b.place == "-" || b.place != "" )
	{
			return -1;
	}
	else
	{

		return 0;
	}
}

function resp_updateClassResults(data)
{
	if (data.status == "OK")
	{
		if (currentTable != null)
		{
			updateResultVirtualPosition(data.results);
			currentTable.fnClearTable();
			currentTable.fnAddData(data.results,true);
			lastClassHash = data.hash;
		}
	}
	resUpdateTimeout = setTimeout(checkForClassUpdate,updateInterval);
}

function updateClassResults(data)
{
	if (data.status == "OK")
	{
		if (data.className != null)
		{
			$('#resultsHeader').html('<b>'+data.className + '</b>');
			$('#resultsControls').show();
		}

		if (data.results != null)
		{
			columns = Array();
			columns.push({ "sTitle": "#", "bSortable" : false, "aTargets" : [0], "mDataProp": "place" });
			columns.push({ "sTitle": "<?=$_NAME?>","bSortable" : false,"aTargets" : [1], "mDataProp": "name" });
			columns.push({ "sTitle": "<?=$_CLUB?>","bSortable" : false ,"aTargets" : [2], "mDataProp": "club"});

			curClassSplits = data.splitcontrols;

			updateResultVirtualPosition(data.results);

			var col = 3;
			if (data.splitcontrols != null)
			{
				$.each(data.splitcontrols,
					function(key,value)
					{
						columns.push({ "sTitle": value.name, "sClass": "left","sType": "numeric","aDataSort" : [col+1,col],"aTargets" : [col], "bUseRendered" : false, "mDataProp" : "splits." + value.code, "fnRender": function ( o, val )
							{
									if (o.aData.splits[value.code+"_status"] != 0)
										return "";
									else
										return formatTime(o.aData.splits[value.code],0) + " (" +o.aData.splits[value.code + "-place"] + ")";
							}
						});
						col++
						columns.push({ "sTitle": value.name + "_Status", "bVisible" : false,"aTargets" : [col++],"sType": "numeric", "mDataProp": "splits." + value.code + "_status"});
					});
			}

			timecol = col;
			columns.push({ "sTitle": "<?=$_CONTROLFINISH?>", "sClass": "left", "sType": "numeric","aDataSort": [ col+1, col ], "aTargets" : [col],"bUseRendered": false, "mDataProp": "result",
							"fnRender": function ( o, val )
							{
								if (o.aData.place == "-" || o.aData.place == "")
								{
									return formatTime(o.aData.result,o.aData.status);
								}
								else
								{
									return formatTime(o.aData.result,o.aData.status) +" (" + o.aData.place +")";
								}
							}
						});

			col++;
			columns.push({ "sTitle": "Status", "bVisible" : false,"aTargets" : [col++],"sType": "numeric", "mDataProp": "status"});
			columns.push({ "sTitle": "", "sClass": "center","bSortable" : false,"aTargets" : [col++],"mDataProp": "timeplus",
							"fnRender": function ( o, val )
												{
													if (o.aData.status != 0)
														return "";
													else
														return "+" + formatTime(o.aData.timeplus,o.aData.status);
							}
						});

			columns.push({ "sTitle": "VP", "bVisible" : true, "aTargets" : [col++], "mDataProp": "virtual_position" });

			currentTable = $('#divResults').dataTable( {
					"bPaginate": false,
					"bLengthChange": false,
					"bFilter": false,
					"bSort": true,
					"bInfo" : false,
					"bAutoWidth": false,
					"aaData": data.results,
					//"aaSorting" : [[timecol+1,"asc"],[timecol, "asc"]],
					"aaSorting" : [[col-1, "asc"]],
					"aoColumnDefs": columns
			} );

			lastClassHash = data.hash;
		}
    }
}

var runnerStatus = Array();
runnerStatus[0]= "<?=$_STATUSOK?>";
runnerStatus[1]= "<?=$_STATUSDNS?>";
runnerStatus[11] =  "<?=$_STATUSWO?>";
runnerStatus[12] = "<?=$_STATUSMOVEDUP?>";
runnerStatus[9] = "";
runnerStatus[3] = "<?=$_STATUSMP?>";
runnerStatus[4] = "<?=$_STATUSDSQ?>";
runnerStatus[5] = "<?=$_STATUSOT?>";

function formatTime(time,status)
{
	if (status != 0)
  	{
  		return runnerStatus[status];
  	}
  	else
  	{
  	 minutes = Math.floor(time/6000);
	 seconds = Math.floor((time-minutes*6000)/100);

  	 return str_pad(minutes,2) +":" +str_pad(seconds,2);
  	}
}

function str_pad(number, length) {

    var str = '' + number;
    while (str.length < length) {
        str = '0' + str;
    }

    return str;

}

function changeFontSize(val)
{
	var size = $("td").css("font-size");
	var newSize = parseInt(size.replace(/px/, "")) + val;
	$("td").css("font-size",newSize + "px");
}


</script>
</head>
<body>
<!-- MAIN DIV -->
<div class="maindiv">
<!-- LOGO AND TOP BANNER -->
<table border="0" cellpadding="0" cellspacing="0" width="100%">
  <tr>
    <td align="left"></td>
  </tr>
</table>


<table border="0" cellpadding="0" cellspacing="0" width="100%">
  <tr>
     <td valign="bottom">

<!-- MAIN MENU FLAPS - Two rows, note that left and right styles differs from middle ones -->
     <table border="0" cellpadding="0" cellspacing="0">
          <!-- Top row with rounded corners -->
          <tr>
               <td colspan="4"><span class="mttop"></span></td>
          </tr>
     </table>

     </td>
     <td align="right" valign="bottom">

     </td>
  </tr>
  <tr>
    <td class="submenu" colspan="2">
       <table border="0" cellpadding="0" cellspacing="0" width="100%">
             <tr>
               <td><a href="index.php?lang=<?=$lang?>&"><?=$_CHOOSECMP?></a> >> <?=$currentComp->CompName()?> [<?=$currentComp->CompDate()?>]</td>
<td align=right></td>
             </tr>
       </table>
     </td>
  </tr>
<!-- End SUB MENU -->
  <tr>
    <td class="searchmenu" colspan="2" style="padding: 5px;" valign=top>
       <table border="0" cellpadding="0" cellspacing="0">
             <tr>
               <td valign=top>
			<?php if (!isset($_GET['comp']))
			{
			?>
				<h1 class="categoriesheader">Ett fel uppstod? Har du valt tävling?</h1>
			<?php
			}
			else
			{
			?>
			<h1 class="categoriesheader"><?=$currentComp->CompName()?> [<?=$currentComp->CompDate()?>] <?= isset($_GET['class']) ? ", ".$_GET['class'] : "" ?></h1>
			<table border="0" cellpadding="0" cellspacing="0" width="100%">
			<tr>
			<td valign=top><b><?=$_LASTPASSINGS?></b><br><div id="divLastPassings"></div>
</td><td valign="top" style="padding-left: 5px">
<span id="setAutomaticUpdateText"><b><?=$_AUTOUPDATE?>:</b> <?=$_ON?> | <a href="javascript:setAutomaticUpdate(false);"><?=$_OFF?></a></span><br/>
<b><?=$_TEXTSIZE?>:</b> <a href="javascript:changeFontSize(1);"><?=$_LARGER?></a> | <a href="javascript:changeFontSize(-1);"><?=$_SMALLER?></a><br/>
</td>
</tr></table><br>
			<table border="0" cellpadding="0" cellspacing="0" width="100%">
			<tr>
			<td width=70 valign="top" style="padding-right: 5px"><b><?=$_CHOOSECLASS?></b><br>
<div id="divClasses">
</div>
</td>

			<td valign=top>			<div><span id="resultsHeader" style="font-size: 14px"><b><?=$_NOCLASSCHOSEN?></b></span></div><table id="divResults" width="100%">
</table><br/><br/>

<font color="AAAAAA">* <?=$_HELPREDRESULTS?></font>
</td>
			</tr>
			</table>
			<?php }?>
		</td>
	     </tr>
	</table>

     </td>
  </tr>

</table>
<p align=center>&copy;2012, Peter Löfås, <?=$_NOTICE?></p>

</div>
<br><br>
</body>
</html>
<?php

function formatTime($time,$status,& $RunnerStatus)
{
  global $lang;
  if ($status != "0")
  {
    return $RunnerStatus[$status]; //$status;
  }
   if ($lang == "fi")
{
  $hours = floor($time/360000);
  $minutes = floor(($time-$hours*360000)/6000);
  $seconds = floor(($time-$hours*360000 - $minutes*6000)/100);
  return str_pad("".$hours,2,"0",STR_PAD_LEFT) .":" .str_pad("".$minutes,2,"0",STR_PAD_LEFT) .":".str_pad("".$seconds,2,"0",STR_PAD_LEFT);
}
else
{


  $minutes = floor($time/6000);
  $seconds = floor(($time-$minutes*6000)/100);
  return str_pad("".$minutes,2,"0",STR_PAD_LEFT) .":".str_pad("".$seconds,2,"0",STR_PAD_LEFT);
}
}
?>