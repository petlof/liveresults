<?php
date_default_timezone_set("Europe/Stockholm");
$lang = "sv";

if (isset($_GET['lang']))
 $lang = $_GET['lang'];

include_once("templates/emmalang_en.php");
include_once("templates/emmalang_$lang.php");
include_once("templates/classEmma.class.php");

$currentComp = new Emma($_GET['comp']);

$isSingleClass = isset($_GET['class']);
$isSingleClub = isset($_GET['club']);
$showPath = true;

if (isset($_GET['showpath']) && $_GET['showpath'] == "false")
  $showPath = false;

$singleClass = "";
$singleClub = "";
if ($isSingleClass)
	$singleClass = $_GET['class'];
if ($isSingleClub)
	$singleClub = utf8_decode(rawurldecode($_GET['club']));

$showLastPassings = !($isSingleClass || $isSingleClub) || (isset($_GET['showLastPassings']) && $_GET['showLastPassings'] == "true");
$RunnerStatus = Array("1" =>  $_STATUSDNS, "2" => $_STATUSDNF, "11" =>  $_STATUSWO, "12" => $_STATUSMOVEDUP, "9" => $_STATUSNOTSTARTED,"0" => $_STATUSOK, "3" => $_STATUSMP, "4" => $_STATUSDSQ, "5" => $_STATUSOT, "9" => "", "10" => "");


echo("<?xml version=\"1.0\" encoding=\"$CHARSET\" ?>");
?>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.01 Transitional//EN"
        "http://www.w3.org/TR/html4/loose.dtd">
<html>
<head><title><?=$_TITLE?> :: <?=$currentComp->CompName()?> [<?=$currentComp->CompDate()?>]</title>

<META HTTP-EQUIV="expires" CONTENT="-1">
<meta http-equiv="Content-Type" content="text/html;charset=<?=$CHARSET?>">
<link rel="stylesheet" type="text/css" href="css/style-eoc.css">
<link rel="stylesheet" type="text/css" href="css/ui-darkness/jquery-ui-1.8.19.custom.css">
<link rel="stylesheet" type="text/css" href="css/jquery.dataTables_themeroller-eoc.css">

<?php
$debug = false;
if ($debug)
{
?>
<!-- DEBUG -->
<script language="javascript" type="text/javascript" src="js/jquery-1.7.2.min.js"></script>
<script language="javascript" type="text/javascript" src="js/jquery.dataTables.min.js"></script>
<script language="javascript" type="text/javascript" src="js/jquery.ba-hashchange.min.js"></script>
<script language="javascript" type="text/javascript" src="js/liveresults.js"></script>
<?php }
else
{?>
<!-- RELEASE-->
<script language="javascript" type="text/javascript" src="js/liveresults.min.js"></script>
<?php }?>

<script language="javascript" type="text/javascript">


var res = null;

var Resources = {
	_TITLE: "<?= $_TITLE?>",
	_CHOOSECMP: "<?=$_CHOOSECMP?>",
	_AUTOUPDATE: "<?=$_AUTOUPDATE?>",
	_LASTPASSINGS: "<?=$_LASTPASSINGS?>",
	_LASTPASSFINISHED: "<?=$_LASTPASSFINISHED?>",
	_LASTPASSPASSED: "<?=$_LASTPASSPASSED?>",
	_LASTPASSWITHTIME: "<?=$_LASTPASSWITHTIME?>",
	_CHOOSECLASS: "<?=$_CHOOSECLASS?>",
	_NOCLASSESYET: "<?=$_NOCLASSESYET?>",
	_CONTROLFINISH: "<?=$_CONTROLFINISH?>",
	_NAME: "<?=$_NAME?>",
	_CLUB: "<?=$_CLUB?>",
	_TIME: "<?=$_TIME?>",
	_NOCLASSCHOSEN: "<?=$_NOCLASSCHOSEN?>",
	_HELPREDRESULTS: "<?=$_HELPREDRESULTS?>",
	_NOTICE: "<?=$_NOTICE?>",
	_STATUSDNS: "<?=$_STATUSDNS ?>",
	_STATUSDNF: "<?=$_STATUSDNF?>",
	_STATUSWO: "<?=$_STATUSWO?>",
	_STATUSMOVEDUP: "<?=$_STATUSMOVEDUP?>",
	_STATUSNOTSTARTED: "<?=$_STATUSNOTSTARTED?>",
	_STATUSOK: "<?=$_STATUSOK ?>",
	_STATUSMP: "<?=$_STATUSMP ?>",
	_STATUSDSQ: "<?=$_STATUSDSQ?>",
	_STATUSOT: "<?=$_STATUSOT?>",
	_FIRSTPAGECHOOSE: "<?=$_FIRSTPAGECHOOSE ?>",
	_FIRSTPAGEARCHIVE: "<?=$_FIRSTPAGEARCHIVE?>",
	_LOADINGRESULTS: "<?=$_LOADINGRESULTS ?>",
	_ON: "<?=$_ON ?>",
	_OFF: "<?=$_OFF?>",
	_TEXTSIZE: "<?=$_TEXTSIZE ?>",
	_LARGER: "<?=$_LARGER?>",
	_SMALLER: "<?=$_SMALLER?>",
	_OPENINNEW: "<?=$_OPENINNEW?>",
	_FORORGANIZERS: "<?=$_FORORGANIZERS ?>",
	_FORDEVELOPERS: "<?=$_FORDEVELOPERS ?>",
	_RESETTODEFAULT: "<?=$_RESETTODEFAULT?>",
	_OPENINNEWWINDOW: "<?=$_OPENINNEWWINDOW?>",
	_INSTRUCTIONSHELP: "<?=$_INSTRUCTIONSHELP?>",
	_LOADINGCLASSES: "<?=$_LOADINGCLASSES ?>",
	_START: "<?=$_START?>",
	_TOTAL: "<?=$_TOTAL?>",
	_CLASS: "<?=$_CLASS?>"
};

var runnerStatus = Array();
runnerStatus[0]= "<?=$_STATUSOK?>";
runnerStatus[1]= "<?=$_STATUSDNS?>";
runnerStatus[2]= "<?=$_STATUSDNF?>";
runnerStatus[11] =  "<?=$_STATUSWO?>";
runnerStatus[12] = "<?=$_STATUSMOVEDUP?>";
runnerStatus[9] = "";
runnerStatus[3] = "<?=$_STATUSMP?>";
runnerStatus[4] = "<?=$_STATUSDSQ?>";
runnerStatus[5] = "<?=$_STATUSOT?>";
runnerStatus[9] = "";
runnerStatus[10] = "";


$(document).ready(function()
{
	res = new LiveResults.AjaxViewer(<?= $_GET['comp']?>,"<?= $lang?>","divClasses","divLastPassings","resultsHeader","resultsControls","divResults","txtResetSorting",Resources,<?= ($currentComp->IsMultiDayEvent() ? "true" : "false")?>,<?= (($isSingleClass || $isSingleClub) ? "true": "false")?>,"setAutomaticUpdateText", runnerStatus);
	<?php if ($isSingleClass)
	{?>
		res.chooseClass('<?=$singleClass?>');
	<?php }
	else if ($isSingleClub)
	{?>
		res.viewClubResults('<?=$singleClub?>');
	<?php }
		else
	{?>
		$("#divClasses").html("<?=$_LOADINGCLASSES?>...");
		res.updateClassList();
	<?php }?>

<?php if ($showLastPassings){?>
	res.updateLastPassings();
	<?php }?>
});



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

<table border="0" cellpadding="0" cellspacing="0" width="100%">

<?php if (!$isSingleClass && !$isSingleClub && $showPath) {?>
<tr>
    <td class="submenu" colspan="2">
       <table border="0" cellpadding="0" cellspacing="1" style="font-size: 14px">
             <tr>
               <td><a href="index.php"><a href="index.php?lang=<?=$lang?>&"><?=$_CHOOSECMP?></a> >> <?=$currentComp->CompName()?> [<?=$currentComp->CompDate()?>]</a></td>
               <td>|</td>
				<td><a href="http://emmaclient.codeplex.com/documentation" target="_blank"><?=$_FORORGANIZERS?></a></td>
               <td>|</td>
               <td><a href="http://emmaclient.codeplex.com/wikipage?title=For%20Developers%20%28API%29" target="_blank"><?=$_FORDEVELOPERS?></a></td>             </tr>
       </table>
     </td>
  </tr>
<?php }?>
<!-- End SUB MENU -->

  <tr>

    <td class="searchmenu" colspan="2" style="" valign=top>

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

<?php if (!$showPath) {?>
<h1 class="categoriesheader"><?=$currentComp->CompName()?> [<?=$currentComp->CompDate()?>]</h1>
<?php }?>
<?php if (!$isSingleClass && !$isSingleClub) {?>
			| <?php echo($lang == "sv" ? "<img src='images/se.png' border='0'/> Svenska" : "<a href=\"?lang=sv&comp=".$_GET['comp']."\" style='text-decoration: none'><img src='images/se.png' border='0'/> Svenska</a>")?>
			   	   			| <?php echo($lang == "en" ? "<img src='images/en.png' border='0'/> English" : "<a href=\"?lang=en&comp=".$_GET['comp']."\" style='text-decoration: none'><img src='images/en.png' border='0'/> English</a>")?>
			| <?php echo($lang == "fi" ? "<img src='images/fi.png' border='0'/> Suomeksi" : "<a href=\"?lang=fi&comp=".$_GET['comp']."\" style='text-decoration: none'><img src='images/fi.png' border='0'/> Suomeksi</a>")?> |
<?php }?>
<?php if($showLastPassings){?>
			<table border="0" cellpadding="0" cellspacing="0" width="100%" style="background-color:#000; color:#fff; padding: 10px; margin-top: 3px">
			<tr>
			<?php //ADD CUSTOMIZED LOGO IF NEEDED<td width="161"><img src="http://www.eoc2012.se/wp-content/themes/eoc2012/images/logo.gif"/></td>?>
			<td valign=top><b><?=$_LASTPASSINGS?></b><br>
<div id="divLastPassings">
</div>
</td>
<td valign="top" style="padding-left: 5px; width: 200px; text-align:right">
<span id="setAutomaticUpdateText"><b><?=$_AUTOUPDATE?>:</b> <?=$_ON?> | <a href="javascript:LiveResults.Instance.setAutomaticUpdate(false);"><?=$_OFF?></a></span><br/>
<b><?=$_TEXTSIZE?>:</b> <a href="javascript:changeFontSize(1);"><?=$_LARGER?></a> | <a href="javascript:changeFontSize(-1);"><?=$_SMALLER?></a><br/><br/>
<a href="dok/help.html" target="_blank"><?=$_INSTRUCTIONSHELP?></a>
</td>
</tr></table><br>
<?php }?>
			<table border="0" cellpadding="0" cellspacing="0" width="100%">

			<tr>
<?php if (!$isSingleClass && !$isSingleClub){?>
			<td width=70 valign="top" style="padding-right: 5px"><b><?=$_CHOOSECLASS?></b><br>

<div id="divClasses">
</div>
</td>
<?php }?>



			<td valign=top>
		<div><span id="resultsHeader" style="font-size: 14px"><b><?=$_NOCLASSCHOSEN?></b></span><span align="right" style="margin-left: 10px"><?php if (!$isSingleClass) {?><a href="javascript:LiveResults.Instance.newWin()" style="text-decoration: none"><img class="eI" style="vertical-align: middle" id=":2q" role="button" tabindex="2" src="images/cleardot.gif" alt="<?=$_OPENINNEWWINDOW?>" border="0" title="<?=$_OPENINNEWWINDOW?>"/> <?=$_OPENINNEWWINDOW?></a> <?php }?><span id="txtResetSorting"></span></span></div>
<table id="divResults" width="100%">
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

<p align=center>&copy;2012, Liveresults (http://emmaclient.codeplex.com), <?=$_NOTICE?></p>



</div>

<br><br>

</body>

</html>