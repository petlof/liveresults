<?
session_start();
include_once("templates/classEmma.class.php");
$currentComp = new Emma($_GET['comp']);

if (isset($_POST['newUpdTime']))
{
  $_SESSION['updTime'] = $_POST['newUpdTime'];
}
else if (!isset($_SESSION['updTime']))
{
  $_SESSION['updTime'] = "60";
}

$times = Array("15" => "Var 15:e sekund", "30" => "Var 30:e sekund", "60" => "Varje minut", "120" => "Varannan minut", "-1" => "Aldrig");

?>
<html>
<head><title>Liveresultat på nätet :: <?=$currentComp->CompName()?> [<?=$currentComp->CompDate()?>] <?= ($_GET['class'] != '' ? ">> ".$_GET['class'] : "")?></title>
<?if ($_SESSION['updTime'] != "-1") {?> <META HTTP-EQUIV=Refresh CONTENT="<?=$_SESSION['updTime']?>;"><?}?>
<META HTTP-EQUIV="expires" CONTENT="-1">
<link rel="stylesheet" type="text/css" href="css/style.css">
<script language="javascript">
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
  document.getElementById('updTime').value=el.options[el.selectedIndex].value;
  document.form1.submit();
}
</script>
</head>
<body topmargin="0" leftmargin="0">
<form name="form1" action="follow.php?comp=<?=$_GET['comp']?><?= ($_GET['class'] != '' ? '&class='.urlencode($_GET['class']) : '')?><?=($_GET['split'] != '' ? '&split='.$_GET['split'] : '')?>" method="post">
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
               <td colspan="4"><span class="mttop"></td>
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
               <td><a href="index.php">Välj tävling</a> >> <?=$currentComp->CompName()?> [<?=$currentComp->CompDate()?>] <?= ($_GET['class'] != '' ? ">> ".$_GET['class'] : "") ?></td>
<td align=right>Automatisk Uppdatering:
<select id="selUpdTime" onchange="changeUpdateTime()">
<?foreach ($times as $key => $value)
{
echo("<option value='$key' ".($key == $_SESSION['updTime'] ? "selected" : "").">$value</option>");
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
			<? if (!isset($_GET['comp']))
			{
			?>
				<h1 class="categoriesheader">Ett fel uppstod? Har du valt tävling?</h1>
			<?
			}
			else
			{
			?>
			<h1 class="categoriesheader"><?=$currentComp->CompName()?> [<?=$currentComp->CompDate()?>], <?=$_GET['class']?></h1>
			<table border="0" cellpadding="0" cellspacing="0" width="100%">
			<tr>
			<td valign=top><b>Senaste passeringarna</b><br>
<?
			   			   $lastPassings = $currentComp->getLastPassings(3);
			foreach ($lastPassings as $pass)
			  {
			    echo(date("H:i:s",strtotime($pass['Changed'])).": ".$pass['Name']." (".$pass['class'].") ". ($pass['Control'] == "1000" ? "finished" : "passed ".$pass['pname'])." with time " .formatTime($pass['Time'],$pass['Status'])." <br/>");
			   			  }
?>
</td></tr></table><br/>
			<table border="0" cellpadding="0" cellspacing="0" width="100%">
			<tr>
			<td width=70 valign=top><b>Välj klass</b><br>
<?
	$currentClass = $_GET['class'];
	$classes = $currentComp->Classes();
	if (sizeof($classes) == 0)
	{
		echo("Inga klasser än");
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
			echo("<a href='follow.php?comp=". $_GET['comp'] ."&class=". urlencode($cl['Class']).(($_GET['split'] != '' ? '&split='.$_GET['split'] : '')). "'>".$cl['Class']."</a><br>");
		}
		}
	}
?>
</td>
			
			<td valign=top>
|
<?
    $splitControls = $currentComp->getSplitControlsForClass($currentClass);
	$i = 1;
	foreach($splitControls as $val)
	  {
	    if ($val['code'] == $_GET['split'])
	      {
		echo("<b>". $val['name'] . "</b> | ");
	      }
	    else
	      {
	    echo("<a href='follow.php?comp=". $_GET['comp'] ."&class=". urlencode($_GET['class']). "&split=".$val['code']."'>".$val['name']."</a> | ");
	      }
	  }
	if (!isset($_GET['split']) || $_GET['split'] == "1000")
	  {
	    echo("<b>Finish</b> |");
	  }
	else
	  {
	    echo("<a href='follow.php?comp=". $_GET['comp'] ."&class=". urlencode($_GET['class']). "&split=1000'>Finish</a> |");
	  }
?>
<table border="0" cellpadding="0" cellspacing="0" width="100%">
<tr>
<td style="border-bottom: 1px solid black;"><b>#</td>
<td style="border-bottom: 1px solid black;"><b>Namn</td>
<td style="border-bottom: 1px solid black;"><b>Klubb</td>
<td style="border-bottom: 1px solid black;"><b>Tid</td>
<td style="border-bottom: 1px solid black;">&nbsp;</td>
</TR>
<?
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

 echo("<td>". ($res['Status'] == "0" ? $place : "") ."</td><td>".$res['Name']."</td><td>".$res['Club']."</td><td>".formatTime($res['Time'],$res['Status'])."</td><td>".($res['Status'] == "0" ? "+".formatTime($res['Time'] - $winnerTime,"0") : "")."</td>");
 echo("</tr>");
 $lastTime = $res['Time'];
 	
}
}
else
{
?>
<tr bgcolor="ffffff">
<td colspan=5>Ingen klass vald!</td>
</tr>
<?
}
?>
</table>
<font color="AAAAAA">* Rödmarkerade resultat är nya sedan 2 minuter</font>
</td>
			</tr>
			</table>
			<?}?>
		</td>
	     </tr>
	</table>

     </td>
  </tr>
     
</table>
<p align=center>&copy;2006, Obeservera att resultat på denna sida ej är officiella. För officiella resultat hänvisas till arrangörens egna hemsida</p>

</div>

</body>
</html>
<?
function formatTime($time,$status)
{
  $RunnerStatus = Array("1" =>  "dns", "2" => "dnf", "11" =>  "Återbud", "12" => "Uppflyttad", "9" => "Ej startat","0" => "Godkänd", "3" => "Felst.", "4" => "disq.", "5" => "ot");
  if ($status != "0")
  {
    return $RunnerStatus[$status]; //$status;
  }
  $minutes = floor($time/6000);
  $seconds = floor(($time-$minutes*6000)/100);
  return str_pad("".$minutes,2,"0",STR_PAD_LEFT) .":".str_pad("".$seconds,2,"0",STR_PAD_LEFT);
}
?>