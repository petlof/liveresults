<?php include_once("../templates/emmalang_sv.php");

	include_once("../templates/classEmma.class.php");

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
<meta http-equiv="Content-Type" content="text/html;charset=<?=$CHARSET?>">
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

               <td><a href="../index.php"><?=$_CHOOSECMP?></a></td>

             </tr>

       </table>

     </td>

  </tr>

<!-- End SUB MENU -->

  <tr>

    <td class="searchmenu" colspan="2" style="padding: 5px;">

       <table border="0" cellpadding="0" cellspacing="0">

             <tr>

               <td>
<a href="createComp.php">Create new competition</a><br/>


<h1 class="categoriesheader">Existing competitions</h1>

			<table border="0" cellpadding="0" cellspacing="0" width="100%">
<tr>
<td>Date</td><td>Name</td><td>Organizer</td><td>Public</td><td></td>
</tr>

<?php

	$comps = Emma::GetAllCompetitions();

	//echo(sizeof($comps));

	//for ($i = 0; $i < sizeof($comps); $i++)

	foreach ($comps as $comp)

	{

	?>

		<tr id="row<?=$comp["tavid"]?>"><td><?=date("Y-m-d",strtotime($comp['compDate']))?></td><td><?=$comp["compName"]?></td><td><?=$comp["organizer"]?></td><td><?=$comp["public"] == "1" ? "yes" : "false"?></td><td><a href="editComp.php?compid=<?=$comp["tavid"]?>">Edit</a></tr>

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

<br/><br/>

</body>

</html>