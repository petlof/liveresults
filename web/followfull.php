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

<script language="javascript" type="text/javascript">
$(document).ready(function()
{
	$("#divClasses").html("Laddar klasser...");
	$.ajax({
	  url: "api.php",
	  data: "comp=<?=$_GET['comp']?>&method=getclasses",
	  success: updateClassList,
	  dataType: "json"
	});
	currentTable = $('#divResults').dataTable( {
			"bPaginate": false,
	        "bLengthChange": false,
	        "bFilter": false,
	        "bSort": true,
	        "bInfo" : false,
        "bAutoWidth": false,
	        "aaData": [
	        ],
	        "aoColumns": [
	            { "sTitle": "#" },
	            { "sTitle": "<?=$_NAME?>" },
	            { "sTitle": "<?=$_CLUB?>" },
	            { "sTitle": "Tid", "sClass": "center" },
	            { "sTitle": "Status", "bVisible": false },
	            { "sTitle": "Tid+", "sClass": "center" }
	        ]
    } );

});

function updateClassList(data)
{
	if (data != null && data.classes != null)
	{
	str = ""
		$.each(data.classes,
			function(key, value)
			{
				str += "<a href=\"javascript:chooseClass('" + value.className + "')\">" + value.className + "</a><br/>";
			}
		);
		$("#divClasses").html(str);
	}

}

var currentTable = null;
function chooseClass(className)
{
	if (currentTable != null)
		currentTable.fnDestroy();
	$.ajax({
		  url: "api.php",
		  data: "comp=<?=$_GET['comp']?>&method=getclassresults&resultsAsArray=true&class="+className,
		  success: updateClassResults,
		  dataType: "json"
	});
}
function updateClassResults(data)
{
	if (data.results != null)
	{
		currentTable = $('#divResults').dataTable( {
				"bPaginate": false,
				"bLengthChange": false,
				"bFilter": false,
				"bSort": true,
				"bInfo" : false,
				"bAutoWidth": false,
				"aaData": data.results,
				"aaSorting" : [[4,"asc"],[3, "asc"]],
				"aoColumnDefs": [
					{ "sTitle": "#", "bSortable" : false, "aTargets" : [0] },
					{ "sTitle": "<?=$_NAME?>","bSortable" : false,"aTargets" : [1] },
					{ "sTitle": "<?=$_CLUB?>","bSortable" : false ,"aTargets" : [2]},
					{ "sTitle": "Tid", "sClass": "center", "sType": "numeric","aDataSort": [ 4, 3 ], "aTargets" : [3],"bUseRendered": false,
						"fnRender": function ( o, val )
						{
          					return formatTime(o.aData[3],o.aData[4]);
        				},
        			},
        			{ "sTitle": "Status", "bVisible" : false,"aTargets" : [4],"sType": "numeric"},

					{ "sTitle": "Tid+", "sClass": "center","bSortable" : false,"aTargets" : [5],
						"fnRender": function ( o, val )
											{
												if (o.aData[4] != 0)
													return "";
												else
					          						return "+" + formatTime(o.aData[5],o.aData[4]);
        				}
					}
				]
		} );
    }
}

var runnerStatus = Array();
runnerStatus[0]= "<?=$_STATUSOK?>";
runnerStatus[1]= "<?=$_STATUSDNS?>";
runnerStatus[11] =  "<?=$_STATUSWO?>";
runnerStatus[12] = "<?=$_STATUSMOVEDUP?>";
runnerStatus[9] = "<?=$_STATUSNOTSTARTED?>";
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
               <td><a href="../index.php?lang=<?=$lang?>&"><?=$_CHOOSECMP?></a> >> <?=$currentComp->CompName()?> [<?=$currentComp->CompDate()?>]</td>
<td align=right></td>
             </tr>
       </table>
     </td>
  </tr>
<!-- End SUB MENU -->
  <tr>
    <td class="searchmenu" colspan="2" style="padding: 5px;" valign=top>
       <table border="0" cellpadding="0" cellspacing="0" width="600">
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
			<td valign=top><b><?=$_LASTPASSINGS?></b><br>
<?php
			   			   $lastPassings = $currentComp->getLastPassings(3);
			foreach ($lastPassings as $pass)
			  {
			    echo(date("H:i:s",strtotime($pass['Changed'])).": ".$pass['Name']." (".$pass['class'].") ". ($pass['Control'] == "1000" ? $_LASTPASSFINISHED : $_LASTPASSPASSED." ".$pass['pname'])." $_LASTPASSWITHTIME " .formatTime($pass['Time'],$pass['Status'],$RunnerStatus)." <br>");
			   			  }
?>
</td></tr></table><br>
			<table border="0" cellpadding="0" cellspacing="0" width="100%">
			<tr>
			<td width=70 valign=top><b><?=$_CHOOSECLASS?></b><br>
<div id="divClasses">
</div>
</td>

			<td valign=top><table id="divResults" width="100%">
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