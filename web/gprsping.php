<?
$d = "";
$sq = "";
if (isset($_GET["id"]))
	$d = $_GET["id"];
else
	die("Non id?");

if (isset($_GET["sq"]))
	$sq = $_GET["sq"];


$conn = mysql_connect("localhost","liveresultat","web") or die(mysql_error());
mysql_select_db("liveresultat");
		/* check connection */ 
		if (mysql_errno()) {
	   		printf("Connect failed: %s\n", mysql_error());
	   		exit();
		}

$sql = "replace into gprsping values('$d',now(),'$sq')";
mysql_query($sql,$conn);

mysql_close($conn);
echo("OK");
?>