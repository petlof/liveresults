<?
$link = mysql_connect("localhost","root","vol8090ant");
mysql_select_db("EMMA3",$link);

mysql_query("insert into gprspunches(si,code,received) values(".$_GET['sinumber'].",".$_GET['code'].",now())");
echo("OK");
mysql_close($link);
?>