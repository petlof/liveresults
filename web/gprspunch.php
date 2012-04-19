<?$bip=0;$code=0;$h=0;$m=0;$s;
if (isset($_GET["bib"]))
	$bib = $_GET["bib"];
else
	die("Non Bib?");
if (isset($_GET["code"]))
	$code = $_GET["code"];
else
	die("Non code?");


if (isset($_GET["h"]))
	$h = $_GET["h"];
else
	die("Nonh?");


if (isset($_GET["m"]))
	$m = $_GET["m"];
else
	die("Non m?");


if (isset($_GET["s"]))
	$s = $_GET["s"];
else
	die("Non s?");

$conn = mysql_connect("localhost","liveresultat","") or die(mysql_error());
mysql_select_db("liveresultat");
		/* check connection */ 
		if (mysql_errno()) {
	   		printf("Connect failed: %s\n", mysql_error());
	   		exit();
		}

$time = date("Y-m-d")." ".str_pad($h,2,"0",STR_PAD_LEFT).":".str_pad($m,2,"0",STR_PAD_LEFT).":".str_pad($s,2,"0",STR_PAD_LEFT);
$sql = "insert into gprspunches values(0,$bib,$code,'$time',now())";
mysql_query($sql,$conn);

mysql_close($conn);
echo("OK");
?>