<?php
date_default_timezone_set("Europe/Oslo");
$lang = "no";

if (isset($_GET['lang']))
 $lang = $_GET['lang'];

include_once("templates/emmalang_en.php");
include_once("templates/emmalang_$lang.php");
include_once("templates/classEmma.class.php");
header('Content-Type: text/html; charset='.$CHARSET);

$currentComp = new Emma($_GET['comp']);
$code = isset($_GET['code']);

echo("<?xml version=\"1.0\" encoding=\"$CHARSET\" ?>\n");
?>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.01 Transitional//EN"
        "http://www.w3.org/TR/html4/loose.dtd">
<html>
<head><title>LiveRes Radio :: <?=$currentComp->CompName()?> [<?=$currentComp->CompDate()?>]</title>

<META HTTP-EQUIV="expires" CONTENT="-1">
<meta http-equiv="Content-Type" content="text/html;charset=<?=$CHARSET?>">

<meta name="viewport" content="width=1200,initial-scale=1.0">
<meta name="apple-mobile-web-app-capable" content="yes">
<meta name="mobile-web-app-capable" content="yes">
<meta name="theme-color" content="#555556">
<link rel="stylesheet" type="text/css" href="css/style-eoc.css?as">
<link rel="stylesheet" type="text/css" href="css/ui-darkness/jquery-ui-1.8.19.custom.css">
<link rel="stylesheet" type="text/css" href="css/jquery.dataTables_themeroller-eoc.css">


<?php
//$debug = isset($_GET['debug']) && $_GET["debug"] == "true";
$debug = true;
if ($debug)
{
?>
<!-- DEBUG -->
<script language="javascript" type="text/javascript" src="js/jquery-1.7.2.min.js"></script>
<script language="javascript" type="text/javascript" src="js/jquery.dataTables.min.js"></script>
<script language="javascript" type="text/javascript" src="js/jquery.ba-hashchange.min.js"></script>
<script language="javascript" type="text/javascript" src="js/liveresults.js"></script> 
<!-- <script language="javascript" type="text/javascript" src="js/LiveResults.debug.js?rnd=<?=time()?>"></script>
-->
<?php }
else
{?>
<!-- RELEASE-->
<script language="javascript" type="text/javascript" src="js/liveresults.min.20170627.js"></script>

<?php }?>
<script language="javascript" type="text/javascript" src="js/NoSleep.min.js"></script>
<script type="text/javascript" src="//w.24timezones.com/l.js" async></script>
<script language="javascript" type="text/javascript">
var noSleep = new NoSleep();

function enableNoSleep() {
  noSleep.enable();
  document.removeEventListener('click', enableNoSleep, false);
}

document.addEventListener('click', enableNoSleep, false);
var res = null;
var res2 = null;
var Resources = null;
var runnerStatus = null;
var calltime = 3;

$(document).ready(function()
{
	
	<?php 
		if (isset($_GET['calltime']))
			echo 'calltime = ', $_GET['calltime'] ,';'
	?>
	
	if (<?= $_GET['code']?>==0)
	{ 
		var clockElement = document.getElementById( "clock" );
		function updateClock ( clock ) 
		{
			var currTime = new Date();
			var preTime = new Date(currTime.valueOf()+calltime*60*1000);
			var HTMLstring = " Opprop: " + preTime.toLocaleTimeString('en-GB') + "&nbsp;"
			clock.innerHTML = HTMLstring;
		}
		setInterval(function () {updateClock( clockElement );}, 1000);
	}
	
	res = new LiveResults.AjaxViewer(<?= $_GET['comp']?>,"<?= $lang?>","divClasses","divLastPassings","resultsHeader","resultsControls","divResults","txtResetSorting",Resources,"false","true","setAutomaticUpdateText","setCompactViewText", runnerStatus, "true","divRadioPassings");
    res.updateRadioPassings(<?= $_GET['code']?>,calltime);
	
	if (!('<?= $_GET['code2']?>'==''))
	{
		res2 = new LiveResults.AjaxViewer(<?= $_GET['comp']?>,"<?= $lang?>","divClasses","divLastPassings","resultsHeader","resultsControls","divResults","txtResetSorting",Resources,"false","true","setAutomaticUpdateText","setCompactViewText", runnerStatus, "true","divRadioPassings2");
		res2.updateRadioPassings(<?php if (isset($_GET['code2'])) echo $_GET['code2'] ,',', $_GET['calltime'] ?>);
	}
});


</script>
</head>

<body>

	<?php if (!isset($_GET['comp']) || !isset($_GET['code'])) 
	{
	?>
		<h1 class="categoriesheader">Feil. Har du satt compID og postkode? Eks: radio.php?comp=15109&code=100 </h1>
	<?php 
	}
	else
	{
	?>
<table border="0" cellpadding="0" cellspacing="0"><td valign=top> 
<table border="0" cellpadding="0" cellspacing="0" width="100%" style="background-color:#555556; color:#FFF; padding: 10px; margin-top: 3px;border-radius: 5px">
<tr><td valign="top"><b><?=$currentComp->CompName()?> [<?=$currentComp->CompDate()?>]</b><td align="right"><b>GPS tid: </b><iframe src="https://freesecure.timeanddate.com/clock/i6ryyd5b/n2601/tlno10/fs11/fcfff/tct/pct/ftb/th1" frameborder="0" width="47" height="11" allowTransparency="true"></iframe>
</b><br></tr>
<?php if ($_GET['code']==0) {
	?>
	<tr><td><td align="right"><b><span id="clock"></span></b></tr>
	<?php }?>
<tr><td valign="top"><b>Meldepost med kode: <?= $_GET['code']?></b><br>
<td valign="top" align="right"><b>Generell medling: <a href="https://freidig.idrett.no/o/liveres_helpers/meld.php?lopid=(<?=$_GET['comp']?>) <?=$currentComp->CompName()?>&amp;Navn=Generell melding">Send</a>&nbsp;</b></td>
<table id="divRadioPassings"></table>

<?php if (isset($_GET['code2'])) { ?>

<td valign=top>

<table border="0" cellpadding="0" cellspacing="0" width="100%" style="background-color:#555556; color:#FFF; padding: 10px; margin-top: 3px;border-radius: 5px">
<tr><td valign="top"><b><?=$currentComp->CompName()?> [<?=$currentComp->CompDate()?>]</b><td align="right"><b>GPS tid: </b><iframe src="https://freesecure.timeanddate.com/clock/i6ryyd5b/n2601/tlno10/fs11/fcfff/tct/pct/ftb/th1" frameborder="0" width="47" height="11" allowTransparency="true"></iframe>
</b><br></tr>
<?php if ($_GET['code2']==0) { ?>
	<tr><td><td align="right"><b><span id="clock"></span></b></tr>
	<?php }?>
<tr><td valign="top"><b>Meldepost med kode: <?= $_GET['code2']?></b><br>
<td valign="top" align="right"><b>Generell medling: <a href="https://freidig.idrett.no/o/liveres_helpers/meld.php?lopid=(<?=$_GET['comp']?>) <?=$currentComp->CompName()?>&amp;Navn=Generell melding">Send</a>&nbsp;</b></td>
<table id="divRadioPassings2"></table>

<?php } ?>

<?php }?>
</body>
</html>

