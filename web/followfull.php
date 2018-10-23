<?php
date_default_timezone_set("Europe/Stockholm");
$lang = "sv";

if (isset($_GET['lang']))
 $lang = $_GET['lang'];

include_once("templates/emmalang_en.php");
include_once("templates/emmalang_$lang.php");
include_once("templates/classEmma.class.php");

header('Content-Type: text/html; charset='.$CHARSET);

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

$showTimePrediction = true;

echo("<?xml version=\"1.0\" encoding=\"$CHARSET\" ?>\n");
?>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.01 Transitional//EN"
        "http://www.w3.org/TR/html4/loose.dtd">
<html>
<head><title><?=$_TITLE?> :: <?=$currentComp->CompName()?> [<?=$currentComp->CompDate()?>]</title>

<META HTTP-EQUIV="expires" CONTENT="-1">
<meta http-equiv="Content-Type" content="text/html;charset=<?=$CHARSET?>">

<meta name="viewport" content="width=1200,initial-scale=1.0">
<meta name="apple-mobile-web-app-capable" content="yes">
<meta name="mobile-web-app-capable" content="yes">
<meta name="theme-color" content="#555556">
<link rel="stylesheet" type="text/css" href="css/style-eoc.css">
<link rel="stylesheet" type="text/css" href="css/ui-darkness/jquery-ui-1.8.19.custom.css">
<link rel="stylesheet" type="text/css" href="css/jquery.dataTables_themeroller-eoc.css">
<script type="text/javascript">
window.mobilecheck = function() {
  var check = false;
  (function(a){if(/(android|bb\d+|meego).+mobile|avantgo|bada\/|blackberry|blazer|compal|elaine|fennec|hiptop|iemobile|ip(hone|od)|iris|kindle|lge |maemo|midp|mmp|mobile.+firefox|netfront|opera m(ob|in)i|palm( os)?|phone|p(ixi|re)\/|plucker|pocket|psp|series(4|6)0|symbian|treo|up\.(browser|link)|vodafone|wap|windows ce|xda|xiino/i.test(a)||/1207|6310|6590|3gso|4thp|50[1-6]i|770s|802s|a wa|abac|ac(er|oo|s\-)|ai(ko|rn)|al(av|ca|co)|amoi|an(ex|ny|yw)|aptu|ar(ch|go)|as(te|us)|attw|au(di|\-m|r |s )|avan|be(ck|ll|nq)|bi(lb|rd)|bl(ac|az)|br(e|v)w|bumb|bw\-(n|u)|c55\/|capi|ccwa|cdm\-|cell|chtm|cldc|cmd\-|co(mp|nd)|craw|da(it|ll|ng)|dbte|dc\-s|devi|dica|dmob|do(c|p)o|ds(12|\-d)|el(49|ai)|em(l2|ul)|er(ic|k0)|esl8|ez([4-7]0|os|wa|ze)|fetc|fly(\-|_)|g1 u|g560|gene|gf\-5|g\-mo|go(\.w|od)|gr(ad|un)|haie|hcit|hd\-(m|p|t)|hei\-|hi(pt|ta)|hp( i|ip)|hs\-c|ht(c(\-| |_|a|g|p|s|t)|tp)|hu(aw|tc)|i\-(20|go|ma)|i230|iac( |\-|\/)|ibro|idea|ig01|ikom|im1k|inno|ipaq|iris|ja(t|v)a|jbro|jemu|jigs|kddi|keji|kgt( |\/)|klon|kpt |kwc\-|kyo(c|k)|le(no|xi)|lg( g|\/(k|l|u)|50|54|\-[a-w])|libw|lynx|m1\-w|m3ga|m50\/|ma(te|ui|xo)|mc(01|21|ca)|m\-cr|me(rc|ri)|mi(o8|oa|ts)|mmef|mo(01|02|bi|de|do|t(\-| |o|v)|zz)|mt(50|p1|v )|mwbp|mywa|n10[0-2]|n20[2-3]|n30(0|2)|n50(0|2|5)|n7(0(0|1)|10)|ne((c|m)\-|on|tf|wf|wg|wt)|nok(6|i)|nzph|o2im|op(ti|wv)|oran|owg1|p800|pan(a|d|t)|pdxg|pg(13|\-([1-8]|c))|phil|pire|pl(ay|uc)|pn\-2|po(ck|rt|se)|prox|psio|pt\-g|qa\-a|qc(07|12|21|32|60|\-[2-7]|i\-)|qtek|r380|r600|raks|rim9|ro(ve|zo)|s55\/|sa(ge|ma|mm|ms|ny|va)|sc(01|h\-|oo|p\-)|sdk\/|se(c(\-|0|1)|47|mc|nd|ri)|sgh\-|shar|sie(\-|m)|sk\-0|sl(45|id)|sm(al|ar|b3|it|t5)|so(ft|ny)|sp(01|h\-|v\-|v )|sy(01|mb)|t2(18|50)|t6(00|10|18)|ta(gt|lk)|tcl\-|tdg\-|tel(i|m)|tim\-|t\-mo|to(pl|sh)|ts(70|m\-|m3|m5)|tx\-9|up(\.b|g1|si)|utst|v400|v750|veri|vi(rg|te)|vk(40|5[0-3]|\-v)|vm40|voda|vulc|vx(52|53|60|61|70|80|81|83|85|98)|w3c(\-| )|webc|whit|wi(g |nc|nw)|wmlb|wonu|x700|yas\-|your|zeto|zte\-/i.test(a.substr(0,4)))check = true})(navigator.userAgent||navigator.vendor||window.opera);
  return check;
}
</script>

<?php
$debug = isset($_GET['debug']) && $_GET["debug"] == "true";
if ($debug)
{
?>
<!-- DEBUG -->
<script language="javascript" type="text/javascript" src="js/jquery-1.7.2.min.js"></script>
<script language="javascript" type="text/javascript" src="js/jquery.dataTables.min.js"></script>
<script language="javascript" type="text/javascript" src="js/jquery.ba-hashchange.min.js"></script>
<script language="javascript" type="text/javascript" src="js/LiveResults.debug.js?rnd=<?=time()?>"></script>
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

	<?php if ($showTimePrediction){ ?>
		res.eventTimeZoneDiff = <?=$currentComp->TimeZoneDiff();?>;
		res.startPredictionUpdate();
				
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
               <td><a href="index.php?lang=<?=$lang?>&amp;"><?=$_CHOOSECMP?></a> >> <?=$currentComp->CompName()?> [<?=$currentComp->CompDate()?>]</td>
               <td>|</td>
				<td><a href="https://liveresults.github.io/documentation/" target="_blank"><?=$_FORORGANIZERS?></a></td>
               <td>|</td>
               <td><a href="https://liveresults.github.io/documentation/#developer" target="_blank"><?=$_FORDEVELOPERS?></a></td>             </tr>
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
<h1 class="categoriesheader" style="margin-bottom: 4px; color: black"><?=$currentComp->CompName()?> [<?=$currentComp->CompDate()?>]</h1>
<?php }?>
<?php if (!$isSingleClass && !$isSingleClub) {?>
			<div id="langchooser">
| <?php echo($lang == "sv" ? "<img src='images/se.png' alt='Svenska'> Svenska" :
"<a href=\"?lang=sv&amp;comp=".$_GET['comp']."\" style='text-decoration: none'><img src='images/se.png' alt='Svenska'> Svenska</a>")?>
                                                        | <?php echo($lang == "en" ? "<img src='images/en.png' alt='English'> English" :
"<a href=\"?lang=en&amp;comp=".$_GET['comp']."\" style='text-decoration: none'><img src='images/en.png' alt='English'> English</a>")?>
                        | <?php echo($lang == "fi" ? "<img src='images/fi.png' alt='Suomeksi'> Suomeksi" :
"<a href=\"?lang=fi&amp;comp=".$_GET['comp']."\" style='text-decoration: none'><img src='images/fi.png'  alt='Suomeksi'> Suomeksi</a>")?>
                        | <?php echo($lang == "ru" ? "<img src='images/ru.png' alt='Русский'> Русский" :
"<a href=\"?lang=ru&amp;comp=".$_GET['comp']."\" style='text-decoration: none'><img src='images/ru.png' alt='Русский'> Русский</a>")?>
                        | <?php echo($lang == "cz" ? "<img src='images/cz.png' alt='Česky'> Česky" :
"<a href=\"?lang=cz&amp;comp=".$_GET['comp']."\" style='text-decoration: none'><img src='images/cz.png' alt='Česky'> Česky</a>")?>
                        | <?php echo($lang == "de" ? "<img src='images/de.png' alt='Deutsch'> Deutsch" :
"<a href=\"?lang=de&amp;comp=".$_GET['comp']."\" style='text-decoration: none'><img src='images/de.png' alt='Deutsch'> Deutsch</a>")?>
 | <?php echo($lang == "bg" ? "<img src='images/bg.png' alt='български'> български" :
"<a href=\"?lang=bg&amp;comp=".$_GET['comp']."\" style='text-decoration: none'><img src='images/bg.png' alt='български'> български</a>")?>
						| <?php echo($lang == "fr" ? "<img src='images/fr.png' alt='Français'> Français" :
"<a href=\"?lang=fr&amp;comp=".$_GET['comp']."\" style='text-decoration: none'><img src='images/fr.png' alt='Français'> Français</a>")?>
                        | <?php echo($lang == "it" ? "<img src='images/it.png' border='0' alt='Italiano'> Italiano" : 
"<a href=\"?lang=it&amp;comp=".$_GET['comp']."\" style='text-decoration: none'><img src='images/it.png' border='0' alt='Italiano'> Italiano</a>")?> 
                        | <?php echo($lang == "hu" ? "<img src='images/hu.png' border='0' alt='Magyar'> Magyar" : 
"<a href=\"?lang=hu&amp;comp=".$_GET['comp']."\" style='text-decoration: none'><img src='images/hu.png' border='0' alt='Magyar'> Magyar</a>")?> 

 | <?php echo($lang == "es" ? "<img src='images/es.png' border='0' alt='Español'> Español" :
"<a href=\"?lang=es&amp;comp=".$_GET['comp']."\" style='text-decoration: none'><img src='images/es.png' border='0' alt='Español'> Español</a>")?>
 | <?php echo($lang == "pl" ? "<img src='images/pl.png' border='0' alt='Polska'> Polska" :
"<a href=\"?lang=pl&amp;comp=".$_GET['comp']."\" style='text-decoration: none'><img src='images/pl.png' border='0' alt='Polska'> Polska</a>")?>
 | <?php echo($lang == "pt" ? "<img src='images/pt.png?a' border='0' alt='Português'> Português" :
"<a href=\"?lang=pt&amp;comp=".$_GET['comp']."\" style='text-decoration: none'><img src='images/pt.png?a' border='0' alt='Português'> Português</a>")?>

|

</div>
<?php }?>
<?php if($showLastPassings){?>
			<table border="0" cellpadding="0" cellspacing="0" width="100%" style="background-color:#555556; color:#FFF; padding: 10px; margin-top: 3px;border-radius: 5px">
			<tr>
			<!--Customized logo -->
			<!--<td width="161">
			<img src="images/fin5.png"/></td>-->
			<td valign="top"><b><?=$_LASTPASSINGS?></b><br>
<div id="divLastPassings">
</div>
</td>
<td valign="top" style="padding-left: 5px; width: 200px; text-align:right">
<span id="setAutomaticUpdateText"><b><?=$_AUTOUPDATE?>:</b> <?=$_ON?> | <a href="javascript:LiveResults.Instance.setAutomaticUpdate(false);"><?=$_OFF?></a></span><br>
<b><?=$_TEXTSIZE?>:</b> <a href="javascript:changeFontSize(1);"><?=$_LARGER?></a> | <a href="javascript:changeFontSize(-1);"><?=$_SMALLER?></a><br><br>
<a href="dok/help.php?lang=<?=$lang?>" target="_blank"><?=$_INSTRUCTIONSHELP?></a>
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



			<td valign="top">
		<div><span id="resultsHeader" style="font-size: 14px"><b><?=$_NOCLASSCHOSEN?></b></span><span style="margin-left: 10px"><?php if (!$isSingleClass) {?><a href="javascript:LiveResults.Instance.newWin()" style="text-decoration: none"><img class="eI" style="vertical-align: middle" src="images/cleardot.gif" alt="<?=$_OPENINNEWWINDOW?>" border="0" title="<?=$_OPENINNEWWINDOW?>"> <?=$_OPENINNEWWINDOW?></a> <?php }?><span id="txtResetSorting"></span></span></div>
<table id="divResults" width="100%">
<tbody>
<tr><td></td></tr>
</tbody>
</table><br><br>

<font color="AAAAAA">* <?=$_HELPREDRESULTS?></font>

</td>

			</tr>

			</table>

			<?php }?>

		</td>
<td valign="top" style="padding: 20px">
<div id="twitterfeed">
<?php
if (!$isSingleClass && !$isSingleClub && $currentComp->HasTwitter())
{?>
<script type="text/javascript">
if(!window.mobilecheck())
{
document.write('<a href="#" onclick="removeTwitter()">Remove Twitterfeed</a><br/>');
//document.write('<a class="twitter-timeline" href="https://twitter.com/tunapeter/lists/ol-liveresults?widgetId=591685055564636161&amp;chrome=noheader&amp;width=400">Related Tweets</a>');
document.write('<a class="twitter-timeline" href="<?=$currentComp->GetTwitterFeed()?>&amp;chrome=noheader&amp;width=400">Related Tweets</a>');
document.write("<script type=\"text/javascript\">!function(d,s,id){var js,fjs=d.getElementsByTagName(s)[0],p=/^http:/.test(d.location)?'http':'https';if(!d.getElementById(id)){js=d.createElement(s);js.id=id;js.src=p+\"://platform.twitter.com/widgets.js\";fjs.parentNode.insertBefore(js,fjs);}}(document,\"script\",\"twitter-wjs\");");
document.write('<\/script>');
}
function removeTwitter()
{
	$('#twitterfeed').hide();
	$('#twitter-widget-0').remove();
}
</script>
<?php
}
?>

</div>
</td>
<?php if ($currentComp->HasVideo()) {?>
<td valign="top" style="padding: 5px">
			<table border="0" cellpadding="0" cellspacing="0" width="100%" style="background-color:#000000; color:#FFF; padding: 10px; margin-top: 10px">
				<tr>
					<td valign="top"><b>Live Video/Audio</b><br>
					<?=$currentComp->GetVideoEmbedCode()?>
					</td>
				</tr>
			</table>
</td>
<?php }?>

	     </tr>

	</table>



     </td>

  </tr>



</table>

<p align="left">&copy;2012-, Liveresults (https://liveresults.github.io/documentation/), <?=$_NOTICE?></p>



</div>

<br><br>
<script>
  (function(i,s,o,g,r,a,m){i['GoogleAnalyticsObject']=r;i[r]=i[r]||function(){
  (i[r].q=i[r].q||[]).push(arguments)},i[r].l=1*new Date();a=s.createElement(o),
  m=s.getElementsByTagName(o)[0];a.async=1;a.src=g;m.parentNode.insertBefore(a,m)
  })(window,document,'script','//www.google-analytics.com/analytics.js','ga');

  ga('create', 'UA-54989483-1', 'auto');
  ga('send', 'pageview');

</script>

</body>

</html>
