<?php
date_default_timezone_set("Europe/Stockholm");
$lang = "no";
$compid   = $_GET['comp'];
$hightime = 60;
$refreshTime = 5;


if (isset($_GET['lang']))
 $lang = $_GET['lang'];

include_once("../templates/emmalang_en.php");
include_once("../templates/emmalang_$lang.php");
include_once("../templates/classEmma.class.php");


$RunnerStatus = Array("1" =>  $_STATUSDNS, "2" => $_STATUSDNF, "11" =>  $_STATUSWO, "12" => $_STATUSMOVEDUP, "9" => $_STATUSNOTSTARTED,"0" => $_STATUSOK, "3" => $_STATUSMP, "4" => $_STATUSDSQ, "5" => $_STATUSOT, "9" => "", "10" => "", "13" => $_STATUSFINISHED);

header('content-type: application/json; charset='.$CHARSET);
header('Access-Control-Allow-Origin: *');
header('cache-control: max-age='+($refreshTime-1));
header('Expires: '.gmdate('D, d M Y H:i:s \G\M\T', time() + ($refreshTime-1)));

if (!isset($_GET['method']))
{
    $_GET['method'] = null;
}

$pretty = isset($_GET['pretty']);
$br = $pretty ? "\n" : "";
///Method returns all competitions available

if ($_GET['method'] == 'getradiopassings')
{
		$currentComp = new Emma($_GET['comp']);
		$code = $_GET['code'];
		$calltime = $_GET['calltime'];
		$lastPassings = $currentComp->getRadioPassings($code,$calltime);

		$first = true;
		$ret = "";
		$laststatus = 0;
		$lasttime = 0;
		foreach ($lastPassings as $pass)
		{
			$age = time()-strtotime($pass['Changed']);
			$modified = $age < $hightime ? 1:0;
			
			$status = $pass['Status'];
			$time   = $pass['Time'];

			if (!$first)
				$ret .=",$br";
			
			$ret .= "{\"passtime\": \"".date("H:i:s",strtotime($pass['Changed']))."\",
					\"runnerName\": \"".$pass['Name']."\",
					\"club\": \"".$pass['Club']."\",
					\"class\": \"".$pass['class']."\",
					\"control\": ".$pass['Control'].",
					\"controlName\" : \"".$pass['pname']."\",
					\"time\": \"" .formatTime($time,0,$RunnerStatus)."\",
					\"compName\": \"".$pass['compName']."\" ";
			
			if ($pass['Control']==100)	
			{
				if ($pass['class']=="NOCLAS")
				{
					$ret .= ",$br \"DT_RowClass\": \"new_result\"";
				}
				elseif (($lasttime - $time > 5900) && ($status == 10)) 
				{
					$ret .= ",$br \"DT_RowClass\": \"start_entrynew\"";
				}
				elseif ($status == 10)
				{
					$ret .= ",$br \"DT_RowClass\": \"start_entry\"";
				}
				elseif ($modified)
				{
					$ret .= ",$br \"DT_RowClass\": \"OK_entry\"";
				}
				elseif ($laststatus == 10) 
				{
					$ret .= ",$br \"DT_RowClass\": \" firstnonqualifier\"";
				}
			}
			elseif ($modified) // New result, all but start 
			{
				$ret .= ",$br \"DT_RowClass\": \"new_result\"";
			}
			
			$ret .= "$br}";
			$first = false;
			$laststatus = $status;
			$lasttime = $time;
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
else
{
    $protocol = (isset($_SERVER['SERVER_PROTOCOL']) ? $_SERVER['SERVER_PROTOCOL'] : 'HTTP/1.0');
    header($protocol . ' ' . 400 . ' Bad Request');

	echo("{ \"status\": \"ERR\", \"message\": \"No method given\"}");
}



function formatTime($time,$status,& $RunnerStatus)

{
  global $lang;

  if ($status != "0")
  {
    return $RunnerStatus[$status]; //$status;
  }

  if ($time < 0)
  	return "*";
  else {
   if ($lang == "no" or $lang == "fi")
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
