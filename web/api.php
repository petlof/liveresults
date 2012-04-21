<?php
date_default_timezone_set("Europe/Stockholm");
$lang = "sv";

if (isset($_GET['lang']))
 $lang = $_GET['lang'];

include_once("templates/emmalang_en.php");
include_once("templates/emmalang_$lang.php");
include_once("templates/classEmma.class.php");

$RunnerStatus = Array("1" =>  $_STATUSDNS, "2" => $_STATUSDNF, "11" =>  $_STATUSWO, "12" => $_STATUSMOVEDUP, "9" => $_STATUSNOTSTARTED,"0" => $_STATUSOK, "3" => $_STATUSMP, "4" => $_STATUSDSQ, "5" => $_STATUSOT);

header('content-type: application/json; charset=iso-8859-1');
///Method returns all competitions available
if ($_GET['method'] == 'getcompetitions')
{
		$comps = Emma::GetCompetitions();
		echo("{ \"competitions\": [");
		$first = true;
		foreach ($comps as $comp)
			{
				if (!$first)
					echo(",");
				echo("{\"id\": ".$comp["tavid"].", \"name\": \"".$comp["compName"]."\", \"organizer\": \"".$comp["organizer"]."\", \"date\": \"".date("Y-m-d",strtotime($comp['compDate']))."\"}");
				$first = false;
			}
		echo("]}");
}
elseif ($_GET['method'] == 'getlastpassings')
{

		$currentComp = new Emma($_GET['comp']);
		$lastPassings = $currentComp->getLastPassings(5);
		echo("{ \"passings\": [");
		$first = true;

		foreach ($lastPassings as $pass)
		{
			if (!$first)
				echo(",");
			echo("{\"passtime\": \"".date("H:i:s",strtotime($pass['Changed']))."\",
					\"runnerName\": \"".$pass['Name']."\",
					\"class\": \"".$pass['class']."\",
					\"control\": ".$pass['Control'].",
					\"time\": \"" .formatTime($pass['Time'],$pass['Status'],$RunnerStatus)."\" }");
			$first = false;
		}
		echo("]}");
}
elseif ($_GET['method'] == 'getclasses')
{

		$currentComp = new Emma($_GET['comp']);
		$classes = $currentComp->Classes();
		echo("{ \"classes\": [");
		$first = true;

		foreach ($classes as $class)
		{
			if (!$first)
				echo(",");
			echo("{\"className\": \"".$class['Class']."\"}");
			$first = false;
		}
		echo("]}");
}
elseif ($_GET['method'] == 'getclassresults')
{

		$currentComp = new Emma($_GET['comp']);
		$results = $currentComp->getSplitsForClass($_GET['class'],1000);
		$ret = "";
		$first = true;
		$place = 1;
		$lastTime = -9999;
		$winnerTime = 0;
		$resultsAsArray = false;

		if (isset($_GET['resultsAsArray']))
			$resultsAsArray  = true;
		foreach ($results as $res)
		{
			if (!$first)
				$ret .=",";
			$time = $res['Time'];

			if ($first)
				$winnerTime =$time;

			$status = $res['Status'];
			$cp = $place;
			if ($status != 0 || $time < 0)
				$cp = "-";
			elseif ($time == $lastTime)
			{
				$cp = "=";
			}

			$timeplus = "";

			if ($time > 0 && $status == 0)
			{
				$timeplus = "+".formatTime($time-$winnerTime,$res['Status'],$RunnerStatus);
			}

			if($resultsAsArray)
			{
				$ret .= "[\"$cp\", \"".$res['Name']."\", \"".$res['Club']."\", ".$res['Time'].", ".$res['Status'].", ".($time-$winnerTime)."]";
			}
			else
			{
				$ret .= "{\"place\": \"$cp\", \"name\": \"".$res['Name']."\", \"club\": \"".$res['Club']."\", \"result\": \"".formatTime($res['Time'],$res['Status'],$RunnerStatus)."\", \"timeplus\": \"$timeplus\"}";
			}
			$first = false;
			$place++;
			$lastTime = $time;
		}
		echo("{ \"status\": \"OK\", \"results\": [$ret]");
		echo(", \"hash\": \"". MD5($ret)."\"}");
}
else
{
	echo("{ \"status\": \"ERR\", \"message\": \"No method given\"}");
}


function formatTime($time,$status,& $RunnerStatus)

{

  global $lang;

  if ($status != "0")

  {

    return $RunnerStatus[$status]; //$status;

  }

   if ($lang == "fi")

{

  $hours = floor($time/360000);

  $minutes = floor(($time-$hours*360000)/6000);

  $seconds = floor(($time-$hours*360000 - $minutes*6000)/100);

  return str_pad("".$hours,2,"0",STR_PAD_LEFT) .":" .str_pad("".$minutes,2,"0",STR_PAD_LEFT) .":".str_pad("".$seconds,2,"0",STR_PAD_LEFT);

}

else

{





  $minutes = floor($time/6000);

  $seconds = floor(($time-$minutes*6000)/100);

  return str_pad("".$minutes,2,"0",STR_PAD_LEFT) .":".str_pad("".$seconds,2,"0",STR_PAD_LEFT);

}

}


?>