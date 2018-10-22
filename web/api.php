<?php
date_default_timezone_set("Europe/Stockholm");
$lang = "sv";

if (isset($_GET['lang']))
 $lang = $_GET['lang'];

include_once("templates/emmalang_en.php");
include_once("templates/emmalang_$lang.php");
include_once("templates/classEmma.class.php");


$RunnerStatus = Array("1" =>  $_STATUSDNS, "2" => $_STATUSDNF, "11" =>  $_STATUSWO, "12" => $_STATUSMOVEDUP, "9" => $_STATUSNOTSTARTED,"0" => $_STATUSOK, "3" => $_STATUSMP, "4" => $_STATUSDSQ, "5" => $_STATUSOT, "9" => "", "10" => "");

header('content-type: application/json; charset='.$CHARSET);
header('Access-Control-Allow-Origin: *');
header('cache-control: max-age=15');
header('pragma: public');
header('Expires: '.gmdate('D, d M Y H:i:s \G\M\T', time() + 15));

if (!isset($_GET['method']))
{
    $_GET['method'] = null;
}

$pretty = isset($_GET['pretty']);
$br = $pretty ? "\n" : "";
///Method returns all competitions available
if ($_GET['method'] == 'getcompetitions')
{
		$comps = Emma::GetCompetitions();
		echo("{ \"competitions\": [$br");
		$first = true;
		foreach ($comps as $comp)
			{
				if (!$first)
					echo(",");
				echo("{\"id\": ".$comp["tavid"].", \"name\": \"".$comp["compName"]."\", \"organizer\": \"".$comp["organizer"]."\", \"date\": \"".date("Y-m-d",strtotime($comp['compDate']))."\"");

                                echo (", \"timediff\": ".$comp["timediff"]);
                                if ($comp["multidaystage"] != "")
                                {
                                    echo(", \"multidaystage\": ".$comp["multidaystage"].", \"multidayfirstday\": ".$comp["multidayparent"]);
                                }


                                echo("}$br");
				$first = false;
			}
		echo("]}");
}
else if ($_GET['method'] == 'setcompetitioninfo')
{
        $compid = $_POST['comp'];
		Emma::UpdateCompetition($compid,$_POST["compName"],$_POST["organizer"],$_POST["date"],$_POST["public"],$_POST["timediff"]);
		echo("{\"status\": \"OK\"");
}
else if ($_GET['method'] == 'createcompetition')
{
		$data = json_decode($HTTP_RAW_POST_DATA);
		if (!isset($data->name))
			echo("{\"status\": \"Error\", \"message\": \"name not set\"}");
		if (!isset($data->organizer))
			echo("{\"status\": \"Error\", \"message\": \"organizer not set\"}");
		if (!isset($data->date))
			echo("{\"status\": \"Error\", \"message\": \"date not set\"}");
		if (!isset($data->country))
			echo("{\"status\": \"Error\", \"message\": \"country not set\"}");
		if (!isset($data->email))
			echo("{\"status\": \"Error\", \"message\": \"email not set\"}");
		if (!isset($data->password))
			echo("{\"status\": \"Error\", \"message\": \"password not set\"}");

		$id = Emma::CreateCompetitionFull($data->name,$data->organizer,$data->date,$data->email,$data->password,$data->country);

		if ($id > 0)
		{
			echo("{\"status\": \"OK\", \"competitionid\": ".$id." }");
		}
		else
		{
			echo("{\"status\": \"Error\", \"message\": \"Error adding competition\" }");
		}
}
else if ($_GET['method'] == 'getcompetitioninfo')
{
                $compid = $_GET['comp'];
		$comp = Emma::GetCompetition($compid);
          if (isset($comp["tavid"]))
          {
                echo("{\"id\": ".$comp["tavid"].", \"name\": \"".$comp["compName"]."\", \"organizer\": \"".$comp["organizer"]."\", \"date\": \"".date("Y-m-d",strtotime($comp['compDate']))."\"");

                echo (", \"timediff\": ".$comp["timediff"]);
				echo (", \"timezone\": \"".$comp["timezone"]."\"");
				echo (", \"isPublic\": ".(isset($comp["public"]) ? $comp["public"] : false));
                if ($comp["multidaystage"] != "")
                {
                    echo(", \"multidaystage\": ".$comp["multidaystage"].", \"multidayfirstday\": ".$comp["multidayparent"]);
                }

                echo("}");
          }
          else
          {
             echo("{\"id\": ".$_GET["comp"]."}");
          }
}
elseif ($_GET['method'] == 'getlastpassings')
{

		$currentComp = new Emma($_GET['comp']);
		$lastPassings = $currentComp->getLastPassings(5);

		$first = true;
		$ret = "";
		foreach ($lastPassings as $pass)
		{
			if (!$first)
				$ret .=",$br";
			$ret .= "{\"passtime\": \"".date("H:i:s",strtotime($pass['Changed']))."\",
					\"runnerName\": \"".$pass['Name']."\",
					\"class\": \"".$pass['class']."\",
					\"control\": ".$pass['Control'].",
					\"controlName\" : \"".$pass['pname']."\",
					\"time\": \"" .formatTime($pass['Time'],$pass['Status'],$RunnerStatus)."\" }";
			$first = false;
		}

		$hash = MD5($ret);
		if (isset($_GET['last_hash']) && $_GET['last_hash'] == $hash)
		{
			echo("{ \"status\": \"NOT MODIFIED\"}");
		}
		else
		{
			echo("{ \"status\": \"OK\", $br\"passings\" : [$br$ret$br],$br \"hash\": \"$hash\"}");
		}
}
elseif ($_GET['method'] == 'getclasses')
{

		$currentComp = new Emma($_GET['comp']);
		$classes = $currentComp->Classes();
		$ret = "";
		$first = true;

		foreach ($classes as $class)
		{
			if (!$first)
				$ret.=",$br";
			$ret .="{\"className\": \"".$class['Class']."\"}";
			$first = false;
		}

		$hash = MD5($ret);

		if (isset($_GET['last_hash']) && $_GET['last_hash'] == $hash)
		{
			echo("{ \"status\": \"NOT MODIFIED\"}");
		}
		else
		{
			echo("{ \"status\": \"OK\", \"classes\" : [$br$ret$br]");
			echo(",$br \"hash\": \"". $hash."\"}");
		}

}
elseif ($_GET['method'] == 'getclubresults')
{
		$currentComp = new Emma($_GET['comp']);
		$club = $_GET['club'];
		$results = $currentComp->getClubResults($_GET['comp'], $club);
		$ret = "";
		$unformattedTimes = false;
		$first = true;

		if (isset($_GET['unformattedTimes']) && $_GET['unformattedTimes'] == "true")
		{
			$unformattedTimes = true;
		}

		foreach ($results as $res)
		{
			$time = $res['Time'];
			$status = $res['Status'];

			if ($time == "")
				$status = 9;

			$cp = $res['Place'];
			if ($status == 9 || $status == 10)
			{
				$cp = "";

			}
			elseif ($status != 0 || $time < 0)
			{
				$cp = "-";
			}

			$timeplus = $res['TimePlus'];

			$age = time()-strtotime($res['Changed']);
			$modified = $age < 120 ? 1:0;

			if (!$unformattedTimes)
			{
				$time = formatTime($res['Time'],$res['Status'],$RunnerStatus);
				$timeplus = "+".formatTime($timeplus,$res['Status'],$RunnerStatus);

			}

			if (!$first)
				$ret .= ",$br";

			$ret .= "{\"place\": \"$cp\", \"name\": \"".$res['Name']."\", \"club\": \"".$res['Club']."\",\"class\": \"".$res['Class']."\", \"result\": \"".$time."\",\"status\" : ".$status.", \"timeplus\": \"$timeplus\"";


			if (isset($res["start"]))
			{
				$ret .= ",$br \"start\": ".$res["start"];
			}
			else
			{
				$ret .= ",$br \"start\": \"\"";
			}

			if ($modified)
			{
				$ret .= ",$br \"DT_RowClass\": \"new_result\"";
			}

			$ret .= "$br}";

			$first = false;
		}

		$hash = MD5($ret);
		if (isset($_GET['last_hash']) && $_GET['last_hash'] == $hash)
		{
			echo("{ \"status\": \"NOT MODIFIED\"}");
		}
		else
		{
			echo("{ \"status\": \"OK\",$br \"clubName\": \"".$club."\", $br\"results\": [$br$ret$br]");
			echo(", $br \"hash\": \"". $hash."\"}");
		}
}
elseif ($_GET['method'] == 'getsplitcontrols')
{
	$currentComp = new Emma($_GET['comp']);
	$splits = $currentComp->getAllSplitControls();
	$splitJSON = "[$br";
	$first = true;
	foreach ($splits as $split)
	{
		if (!$first)
			$splitJSON .=",$br";
		$splitJSON .= "{ \"class\": ".$split['className'].", \"code\": ".$split['code'] .", \"name\": \"".$split['name']."\", \"order\": \"".$split['corder']."\"}";
		$first = false;
	}
	$splitJSON .="$br]";
	$hash = MD5($splitJSON);
	if (isset($_GET['last_hash']) && $_GET['last_hash'] == $hash)
	{
		echo("{ \"status\": \"NOT MODIFIED\"}");
	}
	else
	{
		echo("{ \"status\": \"OK\",$br \"splitcontrols\": $splitJSON");
		echo(",$br \"hash\": \"". $hash."\"}");
	}
}
elseif ($_GET['method'] == 'getclassresults')
{
		$class = $_GET['class'];
		$currentComp = new Emma($_GET['comp']);
		$results = $currentComp->getAllSplitsForClass($class);
		$splits = $currentComp->getSplitControlsForClass($class);

		$total = null;
		$retTotal = false;
		if (isset($_GET['includetotal']) && $_GET['includetotal'] == "true")
		{
			$retTotal = true;
			$total = $currentComp->getTotalResultsForClass($class);

			foreach ($results as $key=>$res)
			{
				$id = $res['DbId'];

				$results[$key]["totaltime"] = $total[$id]["Time"];
				$results[$key]["totalstatus"] = $total[$id]["Status"];
				$results[$key]["totalplace"] = $total[$id]["Place"];
				$results[$key]["totalplus"] = $total[$id]["TotalPlus"];
			}
		}


		$ret = "";
		$first = true;
		$place = 1;
		$lastTime = -9999;
		$winnerTime = 0;
		$resultsAsArray = false;
		$unformattedTimes = false;
		if (isset($_GET['resultsAsArray']))
			$resultsAsArray  = true;

		if (isset($_GET['unformattedTimes']) && $_GET['unformattedTimes'] == "true")
		{
			$unformattedTimes = true;
		}

		$splitJSON = "[$br";
		foreach ($splits as $split)
		{
			if (!$first)
				$splitJSON .=",$br";
			$splitJSON .= "{ \"code\": ".$split['code'] .", \"name\": \"".$split['name']."\"}";
			$first = false;
      usort($results, function ($a,$b) use($split) {
        if (!isset($a[$split['code']."_time"]) && isset($b[$split['code']."_time"]))
        {
          return 1;
        }
        if (isset($a[$split['code']."_time"]) && !isset($b[$split['code']."_time"]))
        {
          return -1;
        }
        if (!isset($a[$split['code']."_time"]) && !isset($b[$split['code']."_time"]))
        {
          return 0;
        }
        if (isset($a[$split['code']."_time"]) && isset($b[$split['code']."_time"]))
        {
          if ($b[$split['code']."_time"] == $a[$split['code']."_time"])
            return 0;
          else
            return $a[$split['code']."_time"] < $b[$split['code']."_time"] ? -1 : 1;
        }
      }
      );

      $splitplace = 1;
      $cursplitplace = 1;
      $cursplittime = "";
      $bestsplittime = -1;
      foreach ($results as $key => $res)
      {
        $sp_time = "";
        $raceTime = $res['Time'];
        $raceStatus = $res['Status'];
        if ($raceTime == "")
				  $raceStatus = 9;


          if (isset($res[$split['code']."_time"]))
          {
            $sp_time = $res[$split['code']."_time"];
		    if ($bestsplittime < 0 && ($raceStatus == 0 || $raceStatus == 9 || $raceStatus == 10))
		    {
              $bestsplittime = $sp_time;
            }
          }

          if ($sp_time != "")
          {
            $results[$key][$split['code']."_timeplus"] =$sp_time - $bestsplittime;
          }
          else
          {
            $results[$key][$split['code']."_timeplus"] = -1;
          }

          if ($cursplittime != $sp_time)
          {
            $cursplitplace = $splitplace;
          }
          if ($raceStatus == 0 || $raceStatus == 9 || $raceStatus == 10)
          {
            $results[$key][$split['code']."_place"] = $cursplitplace;
            $splitplace++;
            if (isset($res[$split['code']."_time"]))
              $cursplittime = $res[$split['code']."_time"];
          }
          else
          {
            $results[$key][$split['code']."_place"] = "\"-\"";
          }
      }
		}

    usort($results,"sortByResult");
		$splitJSON .= "$br]";

		$first = true;
		$firstNonQualifierSet = false;
		foreach ($results as $res)
		{
			if (!$first)
				$ret .=",";
			$time = $res['Time'];

			if ($first)
				$winnerTime =$time;

			$status = $res['Status'];
			$cp = $place;
      $progress = 0;

			if ($time == "")
				$status = 9;

			if ($status == 9 || $status == 10)
			{
				$cp = "";

        if (count($splits) == 0)
        {
          $progress = 0;
        }
        else
        {
          $passedSplits = 0;
          $splitCnt = 0;
          foreach ($splits as $split)
		      {
            $splitCnt++;
            if (isset($res[$split['code']."_time"]))
              $passedSplits = $splitCnt;
          }
          $progress = ($passedSplits * 100.0) / (count($splits)+1);
        }
			}
			elseif ($status != 0 || $time < 0)
			{
				$cp = "-";
        $progress = 100;
			}
			elseif ($time == $lastTime)
			{
				$cp = "=";
        $progress = 100;
			}

			$timeplus = "";

			if ($time > 0 && $status == 0)
			{
				$timeplus = $time-$winnerTime;
        $progress = 100;
			}

			$age = time()-strtotime($res['Changed']);
			$modified = $age < 120 ? 1:0;

			if (!$unformattedTimes)
			{
				$time = formatTime($res['Time'],$res['Status'],$RunnerStatus);
				$timeplus = "+".formatTime($timeplus,$res['Status'],$RunnerStatus);

			}

			$tot = "";
			if ($retTotal)
			{
				$tot = ", \"totalresult\": ".($res['totaltime']). ", \"totalstatus\": ".$res['totalstatus']. ", \"totalplace\": \"".$res['totalplace']."\", \"totalplus\": ".($res['totalplus']);
			}


			if($resultsAsArray)
			{
				$ret .= "[\"$cp\", \"".$res['Name']."\",$br \"".str_replace("\"","'",$res['Club'])."\",$br ".$res['Time'].",$br ".$status.",$br ".($time-$winnerTime).",$modified]";
			}
			else
			{
				$ret .= "{\"place\": \"$cp\",$br \"name\": \"".$res['Name']."\",$br \"club\": \"".str_replace("\"","'",$res['Club'])."\",$br \"result\": \"".$time."\",$br \"status\" : ".$status.",$br \"timeplus\": \"$timeplus\",$br \"progress\": $progress $tot";

				if (count($splits) > 0)
				{
					$ret .= ",$br \"splits\": {";
					$firstspl = true;
					foreach ($splits as $split)
					{
						if (!$firstspl)
								$ret .=",$br";
						if (isset($res[$split['code']."_time"]))
						{
              $splitStatus = $status;
              if ($status == 9 || $status == 10)
                $splitStatus = 0;

							$ret .= "\"".$split['code']."\": ".$res[$split['code']."_time"] .",\"".$split['code']."_status\": ".$splitStatus.",\"".$split['code']."_place\": ".$res[$split['code']."_place"].",\"".$split['code']."_timeplus\": ".$res[$split['code']."_timeplus"];
							$spage = time()-strtotime($res[$split['code'].'_changed']);
							if ($spage < 120)
								$modified = true;
						}
						else
						{
							$ret .= "\"".$split['code']."\": \"\",\"".$split['code']."_status\": 1,\"".$split['code']."_place\": \"\"";
						}

						$firstspl = false;
					}

					$ret .="}";
				}

				if (isset($res["start"]))
				{
					$ret .= ",$br \"start\": ".$res["start"];
				}
				else
				{
					$ret .= ",$br \"start\": \"\"";
				}

				$rowClass="";
				if ($modified)
				{
					$rowClass="new_result";
					$ret .= ",$br \"DT_RowClass\": \"new_result\"";
				}

				if (strlen($rowClass) > 0)
				{
					 $ret .= ",$br \"DT_RowClass\": \"$rowClass\"";
				}


				$ret .= "$br}";
			}
			$first = false;
			$place++;
			$lastTime = $time;
		}

		$hash = MD5($ret);
		if (isset($_GET['last_hash']) && $_GET['last_hash'] == $hash)
		{
			echo("{ \"status\": \"NOT MODIFIED\"}");
		}
		else
		{
			echo("{ \"status\": \"OK\",$br \"className\": \"".$class."\",$br \"splitcontrols\": $splitJSON,$br \"results\": [$br$ret$br]");
			 if ($_GET['comp'] == "11838" || $_GET['comp'] == "12096" || $_GET['comp'] == "12793" || $_GET['comp'] == "12998" )
                        {
                           echo(",$br \"IsMassStartRace\": true");
                        }

			echo(",$br \"hash\": \"". $hash."\"}");
		}
}
else
{
    $protocol = (isset($_SERVER['SERVER_PROTOCOL']) ? $_SERVER['SERVER_PROTOCOL'] : 'HTTP/1.0');
    header($protocol . ' ' . 400 . ' Bad Request');

	echo("{ \"status\": \"ERR\", \"message\": \"No method given\"}");
}

function sortByResult($a,$b)
{
  if ($a["Status"] == 0 && $b["Status"] != 0)
  {
    return -1;
  }
  else if ($a["Status"] != 0 && $b["Status"] == 0)
  {
    return 1;
  }
  else if ($a["Status"] != $b["Status"])
  {
    return $a["Status"] - $b["Status"];
  }
  else
  {
    return $a["Time"] - $b["Time"];
  }
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

  if ($hours > 0)
  {
  	return $hours .":" .str_pad("".$minutes,2,"0",STR_PAD_LEFT) .":".str_pad("".$seconds,2,"0",STR_PAD_LEFT);
  }
  else
  {
  	return $minutes.":".str_pad("".$seconds,2,"0",STR_PAD_LEFT);
  }

}

else

{





  $minutes = floor($time/6000);

  $seconds = floor(($time-$minutes*6000)/100);

  return str_pad("".$minutes,2,"0",STR_PAD_LEFT) .":".str_pad("".$seconds,2,"0",STR_PAD_LEFT);

}

}

function urlRawDecode($raw_url_encoded)
{
    # Hex conversion table
    $hex_table = array(
        0 => 0x00,
        1 => 0x01,
        2 => 0x02,
        3 => 0x03,
        4 => 0x04,
        5 => 0x05,
        6 => 0x06,
        7 => 0x07,
        8 => 0x08,
        9 => 0x09,
        "A"=> 0x0a,
        "B"=> 0x0b,
        "C"=> 0x0c,
        "D"=> 0x0d,
        "E"=> 0x0e,
        "F"=> 0x0f
    );

    # Fixin' latin character problem
        if(preg_match_all("/\%C3\%([A-Z0-9]{2})/i", $raw_url_encoded,$res))
        {
            $res = array_unique($res = $res[1]);
            $arr_unicoded = array();
            foreach($res as $key => $value){
                $arr_unicoded[] = chr(
                        (0xc0 | ($hex_table[substr($value,0,1)]<<4))
                       | (0x03 & $hex_table[substr($value,1,1)])
                );
                $res[$key] = "%C3%" . $value;
            }

            $raw_url_encoded = str_replace(
                                    $res,
                                    $arr_unicoded,
                                    $raw_url_encoded
                        );
        }

        # Return decoded  raw url encoded data
        return rawurldecode($raw_url_encoded);
}


?>
