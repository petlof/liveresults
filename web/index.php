<?php
date_default_timezone_set("Europe/Stockholm");

include_once("./templates/emmalang_sv.php");
	include_once("./templates/classEmma.class.php");
   $lang = "sv";
   if (isset($_GET['lang']) && $_GET['lang'] != "")
   {
	$lang = $_GET['lang'];
   }
include_once("./templates/emmalang_$lang.php");

header('Content-Type: text/html; charset='.$CHARSET);

echo("<?xml version=\"1.0\" encoding=\"$CHARSET\" ?>\n");
?>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.01 Transitional//EN"
        "http://www.w3.org/TR/html4/loose.dtd">
<html>
<head><title><?=$_TITLE?></title>
<meta http-equiv="Content-Type" content="text/html;charset=<?=$CHARSET?>">
<link rel="stylesheet" type="text/css" href="css/style-eoc.css">
<link rel="stylesheet" type="text/css" href="css/ui-darkness/jquery-ui-1.8.19.custom.css">
<link rel="stylesheet" type="text/css" href="css/jquery.dataTables_themeroller-eoc.css">
<script language="javascript" type="text/javascript" src="js/jquery-1.7.2.min.js"></script>
<script language="javascript" type="text/javascript" src="js/jquery.dataTables.min.js"></script>

<script language="javascript" type="text/javascript">
function colorRow(row)
{
	var el = document.getElementById(row);
	if (el === null)
	  return;
	el.style.backgroundColor = "#C0D6FF";
}
function resetRow(row)
{
var el = document.getElementById(row);
if (el === null)
  return;
el.style.backgroundColor = "";
}

</script>
</head>
<body>
<!-- MAIN DIV -->
<div class="maindiv">
<table border="0" cellpadding="0" cellspacing="0" width="800">
  <tr>
     <td valign="bottom">

<!-- MAIN MENU FLAPS - Two rows, note that left and right styles differs from middle ones -->
     <table border="0" cellpadding="0" cellspacing="0">
          <!-- Top row with rounded corners -->
          <tr>
               <td colspan="4"></td>
          </tr>
     </table>

     </td>
     <td align="right" valign="bottom">

     </td>
  </tr>
  <tr>
    <td class="submenu" colspan="2">
       <table border="0" cellpadding="0" cellspacing="1" style="font-size: 14px">
             <tr>
               <td><a href="index.php"><?=$_CHOOSECMP?></a></td>
               <td>|</td>
               <td><a href="http://emmaclient.codeplex.com/documentation" target="_blank"><?=$_FORORGANIZERS?></a></td>
               <td>|</td>
               <td><a href="http://emmaclient.codeplex.com/wikipage?title=For%20Developers%20%28API%29" target="_blank"><?=$_FORDEVELOPERS?></a></td>
             </tr>
       </table>
     </td>
  </tr>
<!-- End SUB MENU -->
  <tr>
    <td class="searchmenu" colspan="2" style="padding: 5px;">
       <table border="0" cellpadding="0" cellspacing="0" width="800">
             <tr>
               <td>
                      | <?php echo($lang == "sv" ? "<img src='images/se.png' border='0' alt='Svenska'> Svenska" : "<a href=\"?lang=sv\" style='text-decoration: none'><img src='images/se.png' border='0' alt='Svenska'> Svenska</a>")?>
			   	   			| <?php echo($lang == "en" ? "<img src='images/en.png' border='0' alt='English'> English" : "<a href=\"?lang=en\" style='text-decoration: none'><img src='images/en.png' border='0' alt='English'> English</a>")?>
			| <?php echo($lang == "fi" ? "<img src='images/fi.png' border='0' alt='Suomeksi'> Suomeksi" : "<a href=\"?lang=fi\" style='text-decoration: none'><img src='images/fi.png' border='0' alt='Suomeksi'> Suomeksi</a>")?>
			| <?php echo($lang == "de" ? "<img src='images/de.png' border='0' alt='Deutsch'> Deutsch" : "<a href=\"?lang=de\" style='text-decoration: none'><img src='images/de.png' border='0' alt='Deutsch'> Deutsch</a>")?>
                        | <?php echo($lang == "it" ? "<img src='images/it.png' border='0' alt='Italiano'> Italiano" : "<a href=\"?lang=it\" style='text-decoration: none'><img src='images/it.png' border='0' alt='Italiano'> Italiano</a>")?> | 
			<?php echo($lang == "ru" ? "<img src='images/ru.png' border='0' alt='Русский'> Русский" : "<a href=\"?lang=ru\" style='text-decoration: none'><img src='images/ru.png' border='0' alt='Русский'> Русский</a>")?> |


						<h1 class="categoriesheader"><?=$_CHOOSECMP?></h1>
			<table border="0" cellpadding="0" cellspacing="0" width="100%" id="tblComps">
			<tr><th align="left"><?= $_DATE?></th><th align="left"><?= $_NAME?></th><th align="left"><?= $_ORGANIZER?></th></tr>
<?php
	$comps = Emma::GetCompetitions();
	foreach ($comps as $comp)
	{
	?>
		<tr id="row<?=$comp["tavid"]?>"><td><?=date("Y-m-d",strtotime($comp['compDate']))?></td>
		<td><a onmouseover="colorRow('row<?=$comp["tavid"]?>')" onmouseout="resetRow('row<?=$comp["tavid"]?>')" href="followfull.php?comp=<?=$comp["tavid"]?>&amp;lang=<?=$lang?>"><?=$comp["compName"]?></a></td>
		<td><?=$comp["organizer"]?></td>
		</tr>
	<?php
	}
	?>
			</table>
		</td>
	     </tr>
	</table>
     </td>
  </tr>

</table>
</div>
<br><br>
</body>
</html>
