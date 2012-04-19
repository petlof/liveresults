<?php
date_default_timezone_set("Europe/Stockholm");
$lang = "sv";
if (isset($_GET['lang']))
 $lang = $_GET['lang'];

include_once("emmalang_en.php");
include_once("emmalang_$lang.php");
include_once("classEmma.class.php");
$currentComp = new Emma($_GET['comp']);

//if (isset($_POST['newUpdTime']))
//{
//  $_SESSION['updTime'] = $_POST['newUpdTime'];
//}
//else if (!isset($_SESSION['updTime']))
//{
//  $_SESSION['updTime'] = "60";
//}

$updTime = "60";

if (isset($_GET['updTime']))
 $updTime = $_GET['updTime'];

if ($updTime < 15)
  $updTime = "15";


$RunnerStatus = Array("1" =>  $_STATUSDNS, "2" => $_STATUSDNF, "11" =>  $_STATUSWO, "12" => $_STATUSMOVEDUP, "9" => $_STATUSNOTSTARTED,"0" => $_STATUSOK, "3" => $_STATUSMP, "4" => $_STATUSDSQ, "5" => $_STATUSOT);

$times = Array("15" => $_UP15SEK, "30" => $_UP30SEK, "60" => $_UP60SEK, "120" => $_UP120SEK, "-1" => $_UPNEVER);

echo("<?xml version=\"1.0\" encoding=\"iso-8859-1\" ?>");
?>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.01 Transitional//EN"
        "http://www.w3.org/TR/html4/loose.dtd">
<html>
<head><title><?=$_TITLE?> :: <?=$currentComp->CompName()?> [<?=$currentComp->CompDate()?>] <?= ($_GET['class'] != '' ? ">> ".$_GET['class'] : "")?></title>
<?php if ($updTime != "-1") {?> <META HTTP-EQUIV="refresh" CONTENT="<?=$updTime?>;"><?php }?>
<META HTTP-EQUIV="expires" CONTENT="-1">
<meta http-equiv="Content-Type" content="text/html; charset=iso-8859-1">
<link rel="stylesheet" type="text/css" href="../css/style.css">
<script language="javascript" type="text/javascript">
function colorRow(row)
{
var el = document.getElementById(row);
if (el == null)
  return;
el.style.backgroundColor = "#C0D6FF";
}
function resetRow(row)
{
var el = document.getElementById(row);
if (el == null)
  return;
el.style.backgroundColor = "";
}

function changeUpdateTime()
{
  var el = document.getElementById("selUpdTime");
  document.location.href='follow.php?lang=<?=$lang?>&comp=<?=$_GET['comp']?><?= ($_GET['class'] != '' ? '&class='.urlencode($_GET['class']) : '')?><?=($_GET['split'] != '' ? '&split='.$_GET['split'] : '')?>&updTime=' + el.options[el.selectedIndex].value;
}
</script>
</head>
<body>
<form name="form1" action="follow.php?lang=<?=$lang?>&comp=<?=$_GET['comp']?><?= ($_GET['class'] != '' ? '&class='.urlencode($_GET['class']) : '')?><?=($_GET['split'] != '' ? '&split='.$_GET['split'] : '')?>" method="post">
<input type=hidden id='updTime' name='newUpdTime'>
</form>
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
               <td><a href="../index.php?lang=<?=$lang?>&"><?=$_CHOOSECMP?></a> >> <?=$currentComp->CompName()?> [<?=$currentComp->CompDate()?>] <?= (isset($_GET['class']) && $_GET['class'] != '' ? ">> ".$_GET['class'] : "") ?></td>
<td align=right><?=$_AUTOUPDATE?>:
<select id="selUpdTime" onchange="changeUpdateTime()">
<?php foreach ($times as $key => $value)
{
echo("<option value='$key' ".("$key" == "$updTime" ? "selected" : "").">$value</option>");
}?>
</select></td>
             </tr>
       </table>
     </td>
  </tr>
<!-- End SUB MENU -->
  <tr>
    <td class="searchmenu" colspan="2" style="padding: 5px;" valign=top>
       <table border="0" cellpadding="0" cellspacing="0" width="500">
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
<?php
	$currentClass = isset($_GET['class']) ? $_GET['class'] : "";	$cursplit = isset($_GET['split']) ? $_GET['split'] : "";

	$classes = $currentComp->Classes();
	if (sizeof($classes) == 0)
	{
		echo($_NOCLASSESYET);
	}
	else
	{
		foreach($classes as $cl)
		{
		if ($cl['Class'] == $currentClass)
		{
			echo("<b>".$cl['Class']."</b><br>");
		}
		else
		{
			echo("<a href='follow.php?lang=$lang&comp=". $_GET['comp'] ."&class=". urlencode($cl['Class']).(($cursplit != '' ? '&split='.$_GET['split'] : '')). "'>".$cl['Class']."</a><br>");
		}
		}
	}
?>
</td>

			<td valign=top>
|
<?php
    $splitControls = $currentComp->getSplitControlsForClass($currentClass);
	$i = 1;
	foreach($splitControls as $val)
	  {
	    if ($val['code'] == $cursplit)
	      {
		echo("<b>". $val['name'] . "</b> | ");
	      }
	    else
	      {
	    echo("<a href='follow.php?lang=$lang&comp=". $_GET['comp'] ."&class=". urlencode($_GET['class']). "&split=".$val['code']."'>".$val['name']."</a> | ");
	      }
	  }
	if (!isset($_GET['split']) || $_GET['split'] == "1000")
	  {
	    echo("<b>$_CONTROLFINISH</b> |");
	  }
	else
	  {
	    echo("<a href='follow.php?lang=$lang&comp=". $_GET['comp'] ."&class=". urlencode($_GET['class']). "&split=1000'>$_CONTROLFINISH</a> |");
	  }
?>
<table border="0" cellpadding="0" cellspacing="0" width="100%">
<tr>
<td style="border-bottom: 1px solid black;"><b>#</b></td>
<td style="border-bottom: 1px solid black;"><b><?=$_NAME?></b></td>
<td style="border-bottom: 1px solid black;"><b><?=$_CLUB?></b></td>
<td style="border-bottom: 1px solid black;"><b><?=$_TIME?></b></td>
<td style="border-bottom: 1px solid black;">&nbsp;</td>
</TR>
<?php
if ($currentClass != '')
{
  if (isset($_GET['split']) && $_GET['split'] != 1000)
    {
      $results = $currentComp->getSplitsForClass($currentClass,$_GET['split']);
    }
  else
    {
      $results = $currentComp->getResultsForClass($currentClass);
    }
$evenColor = "#DDDDDD";
$newColor = "#FFDDDD";
 $place = "";
$i = 0;
$winnerTime = 0;
 $lastTime = 0;
foreach ($results as $res)
{
 $i++;
 if ((time()-strtotime($res['Changed'])) < 120)
 {
  echo("<tr bgcolor=\"$newColor\">");
 }
 else
 {
	 if ($i%2 != 0)
	 {
	  echo("<tr bgcolor=\"$evenColor\">");
	 }
	 else
	 {
	  echo("<tr>");
	 }
 }
 if ($i == 1)
  $winnerTime = $res['Time'];
 if ($lastTime == $res['Time'])
   {
     $place = "=";
   }
 else
   {
     $place = $i.".";
   }

 echo("<td>". ($res['Status'] == "0" ? $place : "") ."</td><td>".$res['Name']."</td><td>".$res['Club']."</td><td>".formatTime($res['Time'],$res['Status'],$RunnerStatus)."</td><td>".($res['Status'] == "0" ? "+".formatTime($res['Time'] - $winnerTime,"0",$RunnerStatus) : "")."</td>");
 echo("</tr>");
 $lastTime = $res['Time'];

}
}
else
{
?>
<tr bgcolor="ffffff">
<td colspan=5><?=$_NOCLASSCHOSEN?></td>
</tr>
<?php
}
?>
</table>
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
<p align=center>&copy;2006-2008, <?=$_NOTICE?></p>

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