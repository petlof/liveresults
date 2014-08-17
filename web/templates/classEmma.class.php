<?php
$CHARSET = 'utf-8';
class Emma
{

	public static $db_server = "liveresultat.orientering.se";
	public static $db_database = "liveresultat";
	public static $db_user = "liveresultat";
	public static $db_pw= "web";
	public static $MYSQL_CHARSET = "utf8";
	var $m_CompId;

   var $m_CompName;

   var $m_CompDate;
   var $m_TimeDiff = 0;
   var $m_IsMultiDayEvent = false;
   var $m_MultiDayStage = -1;
   var $m_MultiDayParent = -1;
   
   var $m_VideoFormat = "";
   var $m_VideoUrl = "";
   
	var $m_Conn;

        public static function GetCompetitions()

        {

          $conn = mysql_connect(self::$db_server,self::$db_user,self::$db_pw);

	  mysql_select_db(self::$db_database);
	  mysql_set_charset(self::$MYSQL_CHARSET,$conn);
	  if (mysql_errno()) {

	   		printf("Connect failed: %s\n", mysql_error());

	   		exit();

		}

	 $result = mysql_query("select compName, compDate,tavid,organizer,timediff,multidaystage,multidayparent from login where public = 1 order by compDate desc",$conn);

         $ret = Array();

         while ($tmp = mysql_fetch_array($result))

	 {

 		$ret[] = $tmp;

         }

		mysql_free_result($result);

 	return $ret;

        }

public static function GetRadioControls($compid)

        {
	$conn = mysql_connect(self::$db_server,self::$db_user,self::$db_pw);

	  mysql_select_db(self::$db_database);
	  mysql_set_charset(self::$MYSQL_CHARSET,$conn);

	  if (mysql_errno()) {

	   		printf("Connect failed: %s\n", mysql_error());

	   		exit();

		}

	 $result = mysql_query("select * from splitcontrols where tavid=$compid order by corder",$conn);

         $ret = Array();

         while ($tmp = mysql_fetch_array($result))

	 {

 		$ret[] = $tmp;

         }

		mysql_free_result($result);

 	return $ret;

        }
public static function DelRadioControl($compid,$code,$classname)

        {
$conn = mysql_connect(self::$db_server,self::$db_user,self::$db_pw);

	  mysql_select_db(self::$db_database);

	  if (mysql_errno()) {

	   		printf("Connect failed: %s\n", mysql_error());

	   		exit();

		}

	 mysql_query("delete from splitcontrols where tavid=$compid and code=$code and classname='$classname'",$conn);

        }


	public static function CreateCompetition($name,$org,$date)

        {

        $conn = mysql_connect(self::$db_server,self::$db_user,self::$db_pw);

	  mysql_select_db(self::$db_database);
	  mysql_set_charset(self::$MYSQL_CHARSET,$conn);

	  if (mysql_errno()) {

	   		printf("Connect failed: %s\n", mysql_error());

	   		exit();

		}
	 $res = mysql_query("select max(tavid)+1 from login",$conn);
	 $id = mysql_result($res,0,0);
	if ($id < 10000)
		$id = 10000;


	 mysql_query("insert into login(tavid,user,pass,compName,organizer,compDate,public) values(".$id.",'".md5($name.$org.$date)."','".md5("liveresultat")."','".$name."','".$org."','".$date."',0)" ,$conn) or die(mysql_error());

	}
	public static function AddRadioControl($compid,$classname,$name,$code)

        {

          $conn = mysql_connect(self::$db_server,self::$db_user,self::$db_pw);

	  mysql_select_db(self::$db_database);
	  mysql_set_charset(self::$MYSQL_CHARSET,$conn);

	  if (mysql_errno()) {

	   		printf("Connect failed: %s\n", mysql_error());

	   		exit();

		}
	 $res = mysql_query("select count(*)+1 from splitcontrols where classname='$classname' and tavid=$compid",$conn);
	 $id = mysql_result($res,0,0);

	 mysql_query("insert into splitcontrols(tavid,classname,name,code,corder) values($compid,'$classname','$name',$code,$id)" ,$conn) or die(mysql_error());

	}

public static function UpdateCompetition($id,$name,$org,$date,$public,$timediff)

        {

          $conn = mysql_connect(self::$db_server,self::$db_user,self::$db_pw);

	  mysql_select_db(self::$db_database);
	  mysql_set_charset(self::$MYSQL_CHARSET,$conn);

	  if (mysql_errno()) {

	   		printf("Connect failed: %s\n", mysql_error());

	   		exit();

		}
	 $sql = "update login set compName = '$name', organizer='$org', compDate ='$date',timediff=$timediff, public=". (!isset($public) ? "0":"1") ." where tavid=$id";

	 mysql_query($sql ,$conn) or die(mysql_error());

	}

	public static function GetAllCompetitions()

        {

         $conn = mysql_connect(self::$db_server,self::$db_user,self::$db_pw);

	  mysql_select_db(self::$db_database);
	  mysql_set_charset(self::$MYSQL_CHARSET,$conn);

	  if (mysql_errno()) {

	   		printf("Connect failed: %s\n", mysql_error());

	   		exit();

		}

	 $result = mysql_query("select compName, compDate,tavid,timediff,organizer,public from login order by compDate desc",$conn);

         $ret = Array();

         while ($tmp = mysql_fetch_array($result))

	 {

 		$ret[] = $tmp;

         }

		mysql_free_result($result);

 	return $ret;

        }

	public static function GetCompetition($compid)

        {

         $conn = mysql_connect(self::$db_server,self::$db_user,self::$db_pw);

	  mysql_select_db(self::$db_database);
	  mysql_set_charset(self::$MYSQL_CHARSET,$conn);

	  if (mysql_errno()) {

	   		printf("Connect failed: %s\n", mysql_error());

	   		exit();

		}

	 $result = mysql_query("select compName, compDate,tavid,organizer,public,timediff, videourl, videotype,multidaystage,multidayparent from login where tavid=$compid",$conn);

         $ret = null;

         while ($tmp = mysql_fetch_array($result))

	 {

 		$ret = $tmp;

         }

		mysql_free_result($result);

 	return $ret;

        }


	function Emma($compID)

	{

		$this->m_CompId = $compID;

		$this->m_Conn = mysql_connect(self::$db_server,self::$db_user,self::$db_pw);

		mysql_select_db(self::$db_database,$this->m_Conn);
		mysql_set_charset(self::$MYSQL_CHARSET,$this->m_Conn);

		/* check connection */

		if (mysql_errno()) {

	   		printf("Connect failed: %s\n", mysql_error());

	   		exit();

		}



		$result = mysql_query("select * from login where tavid = $compID",$this->m_Conn);

		if ($tmp = mysql_fetch_array($result))

		  {

		    $this->m_CompName = $tmp["compName"];

		    $this->m_CompDate = date("Y-m-d",strtotime($tmp["compDate"]));

		    $this->m_TimeDiff = $tmp["timediff"]*3600;

		    if (isset($tmp["videourl"]))
		    	$this->m_VideoUrl = $tmp["videourl"];

		    if (isset($tmp["videotype"]))
				$this->m_VideoFormat= $tmp["videotype"];
        
		    if (isset($tmp['multidaystage']))
		    {
		    	if ($tmp['multidaystage'] != null && $tmp['multidayparent'] != null)
		    	{
		    		$this->m_IsMultiDayEvent = true;
		    		$this->m_MultiDayStage = $tmp['multidaystage'];
		    		$this->m_MultiDayParent = $tmp['multidayparent'];
		    	}
		    }

		  }

	}

	function IsMultiDayEvent()
	{
		return $this->m_IsMultiDayEvent;
	}
  
  function HasVideo()
	{
		return $this->m_VideoFormat != "";
	}

	function GetVideoEmbedCode()
	{
		if ($this->m_VideoFormat == "bambuser")
		{
			return '<iframe src="http://embed.bambuser.com/channel/' . $this->m_VideoUrl . '" width="460" height="403" frameborder="0">Your browser does not support iframes.</iframe>';
		}
		return "";
	}


	function CompName()

	{
	  return $this->m_CompName;
	}

	function CompDate()

	{

	  return $this->m_CompDate;

	}

	function Classes()
	{

		$ret = Array();

		$q = "SELECT Class From runners where TavId = ". $this->m_CompId ." Group By Class";

		if ($result = mysql_query($q,$this->m_Conn))

		{

			while ($row = mysql_fetch_array($result))

			{

				$ret[] = $row;

			}

			mysql_free_result($result);

		}

		else

			die(mysql_error());

		return $ret;



	}



  function getSplitControlsForClass($className)

  {

    $ret = Array();

    $q = "SELECT Control from results, runners where results.TavID = ". $this->m_CompId . " and runners.TavID = " . $this->m_CompId . " and results.dbid = runners.dbid and runners.class = '" . $className ."' and results.Control != 1000 Group by Control";

    $q = "SELECT code, name from splitcontrols where tavid = " .$this->m_CompId. " and classname = '" . $className ."' order by corder";

    if ($result = mysql_query($q))

      {

	while($tmp = mysql_fetch_array($result))

	  {

	    $ret[] = $tmp;

	  }

	mysql_free_result($result);



      } else

	{ echo(mysql_error());

	}

    return $ret;

  }

	function getResultsForClass($className)

  {

   return $this->getSplitsForClass($className,1000);

  }



  function getLastPassings($num)

  {

    $ret = Array();

	$q = "SELECT runners.Name, runners.class, runners.Club, results.Time,results.Status, results.Changed, results.Control, splitcontrols.name as pname From results inner join runners on results.DbId = runners.DbId left join splitcontrols on (splitcontrols.code = results.Control and splitcontrols.tavid=".$this->m_CompId." and runners.class = splitcontrols.classname) where results.TavId =".$this->m_CompId." AND runners.TavId = results.TavId and results.Status <> -1 AND results.Time <> -1 AND results.Status <> 9 and results.Status <> 10 and results.control <> 100 and (results.control = 1000 or splitcontrols.tavid is not null) ORDER BY results.changed desc limit 3";

		if ($result = mysql_query($q,$this->m_Conn))

		{

			while ($row = mysql_fetch_array($result))

			{

				$ret[] = $row;
				if ($this->m_TimeDiff != 0)
				{
					$ret[sizeof($ret)-1]["Changed"] = date("Y-m-d H:i:s",strtotime($ret[sizeof($ret)-1]["Changed"])+$this->m_TimeDiff);
				}

			}

			mysql_free_result($result);

		}

		else

			die(mysql_error());

		return $ret;

  }

	function getSplitsForClass($className,$split)

	{

		$ret = Array();

		$q = "SELECT runners.Name, runners.Club, results.Time,results.Status, results.Changed From runners,results where results.DbID = runners.DbId AND results.TavId = ". $this->m_CompId ." AND runners.TavId = ".$this->m_CompId ." AND runners.Class = '".$className."' and results.Status <> -1 AND (results.Time <> -1 or (results.Time = -1 and (results.Status = 2 or results.Status=3))) AND results.Control = $split ORDER BY results.Status, results.Time";

		if ($result = mysql_query($q,$this->m_Conn))

		{

			while ($row = mysql_fetch_array($result))

			{

				$ret[] = $row;

			}

			mysql_free_result($result);

		}

		else

			die(mysql_error());

		return $ret;
	}

		function getClubResults($compId, $club)

		{

			$ret = Array();


			$q = "SELECT runners.Name, runners.Club, results.Time, runners.Class ,results.Status, results.Changed, results.DbID, results.Control ";
			$q .= ", (select count(*)+1 from results sr, runners sru where sr.tavid=sru.tavid and sr.dbid=sru.dbid and sr.tavid=results.TavId and sru.class = runners.class and sr.status = 0 and sr.time < results.time and sr.Control=1000) as place ";
			$q .= ", results.Time - (select min(time) from results sr, runners sru where sr.tavid=sru.tavid and sr.dbid=sru.dbid and sr.tavid=results.TavId and sru.class = runners.class and sr.status = 0 and sr.Control=1000) as timeplus ";
			$q .= "From runners,results where ";
			$q .= "results.DbID = runners.DbId AND results.TavId = ". $this->m_CompId ." AND runners.TavId = ".$this->m_CompId ." and runners.Club = '$club' and (results.Control=1000 or results.Control=100) ORDER BY runners.Class, runners.Name";

			if ($result = mysql_query($q,$this->m_Conn))

			{

				while ($row = mysql_fetch_array($result))

				{
					$dbId = $row['DbID'];
					if (!isset($ret[$dbId]))
					{

						$ret[$dbId] = Array();
						$ret[$dbId]["DbId"] = $dbId;
						$ret[$dbId]["Name"] = $row['Name'];
						$ret[$dbId]["Club"] = $row['Club'];
						$ret[$dbId]["Class"] = $row['Class'];
						$ret[$dbId]["Time"] = "";
						$ret[$dbId]["TimePlus"] = "";
						$ret[$dbId]["Status"] = "9";
						$ret[$dbId]["Changed"] = "";
						$ret[$dbId]["Place"]  = "";
					}

					$split = $row['Control'];
					if ($split == 1000)
					{
						$ret[$dbId]["Time"] = $row['Time'];
						$ret[$dbId]["Status"] = $row['Status'];
						$ret[$dbId]["Changed"] = $row['Changed'];
						$ret[$dbId]["Place"] = $row['place'];
						$ret[$dbId]["TimePlus"] = $row['timeplus'];

					}
					elseif ($split == 100)
					{
						$ret[$dbId]["start"] = $row['Time'];
					}
				}

				mysql_free_result($result);

			}

			else

				die(mysql_error());


			return $ret;
	}

		function getAllSplitsForClass($className)

		{

			$ret = Array();


			$q = "SELECT runners.Name, runners.Club, results.Time ,results.Status, results.Changed, results.DbID, results.Control From runners,results where results.DbID = runners.DbId AND results.TavId = ". $this->m_CompId ." AND runners.TavId = ".$this->m_CompId ." AND runners.Class = '".$className."'  ORDER BY results.Dbid";

			if ($result = mysql_query($q,$this->m_Conn))

			{

				while ($row = mysql_fetch_array($result))

				{
					$dbId = $row['DbID'];

					if (!isset($ret[$dbId]))
					{
						$ret[$dbId] = Array();
						$ret[$dbId]["DbId"] = $dbId;
						$ret[$dbId]["Name"] = $row['Name'];
						$ret[$dbId]["Club"] = $row['Club'];
						$ret[$dbId]["Time"] = "";
						$ret[$dbId]["Status"] = "9";
						$ret[$dbId]["Changed"] = "";
					}

					$split = $row['Control'];
					if ($split == 1000)
					{
						$ret[$dbId]["Time"] = $row['Time'];
						$ret[$dbId]["Status"] = $row['Status'];
						$ret[$dbId]["Changed"] = $row['Changed'];

					}
					elseif ($split == 100)
					{
						$ret[$dbId]["start"] = $row['Time'];

					}
					else
					{
						$ret[$dbId][$split."_time"] = $row['Time'];
						$ret[$dbId][$split."_status"] = $row['Status'];
						$ret[$dbId][$split."_changed"] = $row['Changed'];
					}
				}

				mysql_free_result($result);

			}

			else

				die(mysql_error());



				function timeSorter($a,$b)
					{
						if ($a['Status'] != $b['Status'])
							return $a['Status'] - $b['Status'];
						else
							return $a['Time'] - $b['Time'];
					}

			usort($ret,'timeSorter');
			return $ret;

	}



	function getTotalResultsForClass($className)
	{
				$ret = Array();

				$q = "Select TavId,multidaystage from login where MultiDayParent = ".$this->m_MultiDayParent." and MultiDayStage <=".$this->m_MultiDayStage." order by multidaystage";

				$ar = Array();
				$comps = "(";
				if ($result = mysql_query($q,$this->m_Conn))
				{
					$f = 1;
					while ($row = mysql_fetch_array($result))
					{
						$ar[$row["TavId"]] = $row["TavId"];
						if ($f == 0)
							$comps .=",";
						$comps .= $row["TavId"];
						$f = 0;
					}
				}
				mysql_free_result($result);
				$comps .= ")";


				$q = "SELECT results.Time, results.Status, results.TavId, results.DbID From runners,results where results.Control = 1000 and results.DbID = runners.DbId AND results.TavId in $comps AND runners.TavId = results.TavId AND runners.Class = '".$className."'  ORDER BY results.Dbid";

				if ($result = mysql_query($q,$this->m_Conn))

				{
					while ($row = mysql_fetch_array($result))

					{
						$dbId = $row['DbID'];

						if (!isset($ret[$dbId]))
						{
							$ret[$dbId] = Array();
							$ret[$dbId]["DbId"]  = $dbId;
							$ret[$dbId]["Time"] = 0;
							$ret[$dbId]["Status"] = 0;
							foreach ($ar as $c)
							{
								$ret[$dbId]["c_".$c] = false;
							}
						}

						$ret[$dbId]["Time"] += (int)$row['Time'];
						$status = (int)$row['Status'];
						if ($status > $ret[$dbId]["Status"] )
						{
							$ret[$dbId]["Status"] = $status;
						}
						$ret[$dbId]["c_".$row['TavId']] = true;
					}

					mysql_free_result($result);

					//print_r($ret);

					/*set DNS on those missing any comp*/
					foreach($ret as $key => $val)
					{
						$haveAll = true;
						foreach ($ar as $c)
						{
							if (!$val["c_".$c] )
							{
								$haveAll = false;
								break;
							}
						}
						if (!$haveAll)
						{
							$ret[$key]['Status'] = 1;
						}
					}

				}

				else

					die(mysql_error());



			$sres = Array();
			foreach ($ret as $key=>$res)
			{
					$sres[$res["DbId"]]["DbId"] = $res["DbId"];
					$sres[$res["DbId"]]["Time"] = $res["Time"];
					$sres[$res["DbId"]]["Status"] = $res["Status"];
			}


			usort($sres,'timeSorter');

			$pl = 0;
			$lastTime = -1;
			$bestTime = -1;


			foreach ($sres as $tr)
			{

				if ($tr['Status'] == 0)
				{
					if ($bestTime == -1)
						$bestTime = $tr['Time'];

					if ($tr['Time'] > $lastTime)
						$pl++;
					$ret[$tr['DbId']]["Place"] = $pl;
					$ret[$tr['DbId']]["TotalPlus"] = $tr['Time'] - $bestTime;
				}
				else
				{
					$ret[$tr['DbId']]["Place"] = "-";
					$ret[$tr['DbId']]["TotalPlus"] = 0;
				}
			}

//print_r($ret);
			return $ret;
	}


}



?>