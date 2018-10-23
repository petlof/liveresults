<?
$d = "";
$sq = "";
if (isset($_GET["id"]))
	$d = $_GET["id"];
else
	die("Non id?");

if (isset($_GET["sq"]))
	$sq = $_GET["sq"];


$conn = mysqli_connect("localhost","liveresultat","web", "liveresultat");
/* check connection */
if (mysqli_connect_errno()) {
	printf("Connect failed: %s\n", mysqli_connect_error());
	exit();
}

$sql = "replace into gprsping values('$d',now(),'$sq')";
mysqli_query($conn, $sql);

mysqli_close($conn);
echo("OK");
?>