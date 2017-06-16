<?php
include_once("../templates/classEmma.class.php");

if (isset($_POST['btnSubmit']))
{
	Emma::CreateCompetition($_POST['name'],$_POST['org'],$_POST['date']);
	header("Location: admincompetitions.php");
	exit;
}


include_once("../templates/emmalang_sv.php");

   $lang = "en";

   if (isset($_GET['lang']) && $_GET['lang'] != "")

   {

	$lang = $_GET['lang'];

   }

include_once("../templates/emmalang_$lang.php");




header('Content-Type: text/html; charset='.$CHARSET);

?>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.01 Transitional//EN"
        "http://www.w3.org/TR/html4/loose.dtd">

<html>

<head><title><?=$_TITLE?></title>

<link rel="stylesheet" type="text/css" href="../css/style.css">
<meta name="robots" content="noindex">
<meta http-equiv="Content-Type" content="text/html;charset=<?=$CHARSET?>">

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
              <td><a href="admincompetitions.php">Adminpage Competitionindex</a> | </td>
               <td><a href="../index.php"><?=$_CHOOSECMP?> to view</a></td>

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
<form name="form1" action="createComp.php" method="post">
<h1 class="categoriesheader">New competition</h1>
<b>Competitions Name</b><br/>
<input type="text" name="name" size="15"/><br/>
<b>Organizer</b><br/>
<input type="text" name="org" size="15"/><br/>
<b>Date (format yyyy-mm-dd)</b><br/>
<input type="text" name="date" size="15"/> (ex. 2008-02-03)<br/>
<input type="submit" name="btnSubmit" value="Create"/>
</form>

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