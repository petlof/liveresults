<?
$link = mysqli_connect("localhost","root","vol8090ant", "EMMA3");

mysqli_query($link, "insert into gprspunches(si,code,received) values(".$_GET['sinumber'].",".$_GET['code'].",now())");
echo("OK");
mysqli_close($link);
?>