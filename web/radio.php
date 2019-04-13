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
<script language="javascript" type="text/javascript">

var noSleep = new NoSleep();

function enableNoSleep() {
  noSleep.enable();
  document.removeEventListener('click', enableNoSleep, false);
}

document.addEventListener('click', enableNoSleep, false);
var res = null;
var Resources = null;
var runnerStatus = null;


$(document).ready(function()
{
	res = new LiveResults.AjaxViewer(<?= $_GET['comp']?>,"<?= $lang?>","divClasses","divLastPassings","resultsHeader","resultsControls","divResults","txtResetSorting",Resources,"false","true","setAutomaticUpdateText","setCompactViewText", runnerStatus, "true","divRadioPassings");
    res.updateRadioPassings(<?= $_GET['code']?>);
	
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
<tr><td valign="top"><b><?=$currentComp->CompName()?> [<?=$currentComp->CompDate()?>]</b></tr>
<tr><td valign="top"><b>Meldepost med kode: <?= $_GET['code']?></b><br>
<td valign="top" align="right"><b>Generell medling: <a href="https://freidig.idrett.no/o/liveres_helpers/meld.php?lopid=(<?=$_GET['comp']?>) <?=$currentComp->CompName()?>&amp;Navn=Generell melding">Send</a></b><br></tr>
</table>
<table id="divRadioPassings" ></table>
</table>

<?php }?>
</body>
</html>

