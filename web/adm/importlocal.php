<?
//
//imports event from liveresultat.orientering.se into local instance of EmmaClient
//

include_once("../templates/classEmma.class.php");

$db_server = Emma::$db_server;
$db_database = Emma::$db_database;
$db_user = Emma::$db_user;
$db_pw = Emma::$db_pw;
$MYSQL_CHARSET = Emma::$MYSQL_CHARSET;

if (isset($_GET['id']) && $_GET['id']>0)
{
	$id = $_GET['id'];
	$json = file_get_contents("https://liveresultat.orientering.se/api.php?method=getcompetitioninfo&comp=$id");
	$info = json_decode($json);
	$name = $info->name;
	$org = $info->organizer;
	$date = $info->date;

	$conn = mysql_connect($db_server, $db_user, $db_pw);
	mysql_select_db($db_database);
	mysql_set_charset($MYSQL_CHARSET,$conn);
	if (mysql_errno())
	{
		printf("Connect failed: %s\n", mysql_error());
   		exit();
	}
	mysql_query("insert into login(tavid, user, pass, compName, organizer, compDate, public) values (".$id.",'".md5($name.$org.$date)."','".md5("liveresultat")."','".$name."','".$org."','".$date."',1)", $conn) or die(mysql_error());
	header("Location: ../");
}
else
{
	$json = file_get_contents('https://liveresultat.orientering.se/api.php?method=getcompetitions');
	$class_comps = json_decode($json);
	$arr_comps = $class_comps->competitions;
	foreach ($arr_comps as $comp)
	{
			if ($comp->date >= date("Y-m-d"))
			{
				$comps[] = $comp;
				echo "$comp->id :: $comp->date :: $comp->name :: <a href='?id=$comp->id'>import</a><br/>";
			}
	}
}

?>