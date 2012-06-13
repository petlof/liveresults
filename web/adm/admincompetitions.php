<? include_once("../templates/emmalang_sv.php");
	include_once("../templates/classEmma.class.php");
   $lang = "sv";
   if (isset($_GET['lang']) && $_GET['lang'] != "")
   {
	$lang = $_GET['lang'];
   }
include_once("../templates/emmalang_$lang.php");


?>
<html>
<head><title><?=$_TITLE?></title>
<meta name="robots" content="noindex">

<link rel="stylesheet" type="text/css" href="../css/style.css">
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
</script>
</head>
<body topmargin="0" leftmargin="0">
<!-- MAIN DIV -->
<div class="maindiv">
<table width="759" cellpadding="0" cellspacing="0" border="0" ID="Table6">
	<tr>


		<TR>
			<TD>
		<a href="/liveresultat/"><img src="/pics/header.jpg" alt="Svenska Orienteringsförbundet" align="left" width="759" height="91" border="0"></a>
			</TD>
		</TR>


</table>

<table border="0" cellpadding="0" cellspacing="0" width="759">
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
       <table border="0" cellpadding="0" cellspacing="0">
             <tr>
               <td><a href="index.php"><?=$_CHOOSECMP?></a></td>
             </tr>
       </table>
     </td>
  </tr>
<!-- End SUB MENU -->
  <tr>
    <td class="searchmenu" colspan="2" style="padding: 5px;">
       <table border="0" cellpadding="0" cellspacing="0" width="400">
             <tr>
               <td>
<a href="createComp.php">Create new competition</a><br/>

<h1 class="categoriesheader">Existing competitions</h1>
			<table border="0" cellpadding="0" cellspacing="0" width="100%">
<tr>
<td>Date</td><td>Name</td><td>Organizer</td><td>Public</td><td></td>
</tr>
<?
	$comps = Emma::GetAllCompetitions();
	//echo(sizeof($comps));
	//for ($i = 0; $i < sizeof($comps); $i++)
	foreach ($comps as $comp)
	{
	?>
		<tr id="row<?=$comp["tavid"]?>"><td><?=date("Y-m-d",strtotime($comp['compDate']))?></td><td><?=$comp["compName"]?></td><td><?=$comp["organizer"]?></td><td><?=$comp["public"] == "1" ? "yes" : "false"?></td><td><a href="editComp.php?compid=<?=$comp["tavid"]?>">Edit</a></tr>
	<?
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
<br/><br/>
</body>
</html>