<?php
$CHARSET = 'utf-8';
class Emma
{

	public static $db_server = "127.0.0.1";
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
   var $m_TwitterFeed = "";
   
	var $m_Conn;

	private static function openConnection() {
		$conn = mysqli_connect(self::$db_server, self::$db_user, self::$db_pw, self::$db_database);
		if (mysqli_connect_errno()) {
			printf("Connect failed: %s\n", mysqli_connect_error());
			exit();
		}
		mysqli_set_charset($conn, self::$MYSQL_CHARSET);
		return $conn;
	}
        public static function GetCompetitions()

        {
            $conn = self::openConnection();

	 $result = mysqli_query($conn, "select compName, compDate,tavid,organizer,timediff,multidaystage,multidayparent from login where public = 1 order by compDate desc");

         $ret = Array();

         while ($tmp = mysqli_fetch_array($result))

	 {

 		$ret[] = $tmp;

         }

		mysqli_free_result($result);

 	return $ret;

        }

public static function GetCompetitionsToday()

        {
            $conn = self::openConnection();

	 $result = mysqli_query($conn, "select compName, compDate,tavid,organizer,timediff,multidaystage,multidayparent from login where public = 1 and compDate = '".date("Y-m-d")."'");

         $ret = Array();

         while ($tmp = mysqli_fetch_array($result))

	 {

 		$ret[] = $tmp;

         }

		mysqli_free_result($result);

 	return $ret;

        }



public static function GetRadioControls($compid)

        {
            $conn = self::openConnection();

	 $result = mysqli_query($conn, "select * from splitcontrols where tavid=$compid order by corder");

         $ret = Array();

         while ($tmp = mysqli_fetch_array($result))

	 {

 		$ret[] = $tmp;

         }

		mysqli_free_result($result);

 	return $ret;

        }
public static function DelRadioControl($compid,$code,$classname)

        {
        $conn = self::openConnection();

	 mysqli_query($conn, "delete from splitcontrols where tavid=$compid and code=$code and classname='$classname'");

        }

        public static function DelAllRadioControls($compid)
		{
        $conn = self::openConnection();
			mysqli_query($conn, "delete from splitcontrols where tavid=$compid");
   }



	public static function CreateCompetition($name,$org,$date)
        {
        $conn = self::openConnection();
	 $res = mysqli_query($conn, "select max(tavid)+1 from login");
	 list($id) = mysqli_fetch_row($res);
	if ($id < 10000)
		$id = 10000;


	 mysqli_query($conn, "insert into login(tavid,user,pass,compName,organizer,compDate,public) values(".$id.",'".md5($name.$org.$date)."','".md5("liveresultat")."','".$name."','".$org."','".$date."',0)") or die(mysqli_error($conn));

	}

	public static function CreateCompetitionFull($name,$org,$date, $email, $password, $country)
    	{
        $conn = self::openConnection();
	 $res = mysqli_query($conn, "select max(tavid)+1 from login");
	 list($id) = mysqli_fetch_row($res);
	 if ($id < 10000)
		$id = 10000;


	 mysqli_query($conn, "insert into login(tavid,user,pass,compName,organizer,compDate,public, country) values(".$id.",'".$email."','".md5($password)."','".$name."','".$org."','".$date."',0,'".$country."')") or die(mysqli_error($conn));
	 	return $id;
	}


	public static function AddRadioControl($compid,$classname,$name,$code)

        {
        $conn = self::openConnection();
	 $res = mysqli_query($conn, "select count(*)+1 from splitcontrols where classname='$classname' and tavid=$compid");
	 list($id) = mysqli_fetch_row($res);

	 mysqli_query($conn, "insert into splitcontrols(tavid,classname,name,code,corder) values($compid,'$classname','$name',$code,$id)") or die(mysqli_error($conn));

	}

public static function UpdateCompetition($id,$name,$org,$date,$public,$timediff)

        {
        $conn = self::openConnection();
	 $sql = "update login set compName = '$name', organizer='$org', compDate ='$date',timediff=$timediff, public=". (!isset($public) ? "0":"1") ." where tavid=$id";

	 mysqli_query($conn, $sql) or die(mysqli_error($conn));

	}

	public static function GetAllCompetitions()

        {
        $conn = self::openConnection();

	 $result = mysqli_query($conn, "select compName, compDate,tavid,timediff,organizer,public from login order by compDate desc");

         $ret = Array();

         while ($tmp = mysqli_fetch_array($result))

	 {

 		$ret[] = $tmp;

         }

		mysqli_free_result($result);

 	return $ret;

        }

	public static function GetCompetition($compid)

        {
        $conn = self::openConnection();

	 $result = mysqli_query($conn, "select compName, compDate,tavid,organizer,public,timediff, timezone, videourl, videotype,multidaystage,multidayparent from login where tavid=$compid");

         $ret = null;

         while ($tmp = mysqli_fetch_array($result))

	 {

 		$ret = $tmp;

         }

		mysqli_free_result($result);

 	return $ret;

        }


	function Emma($compID)

	{

		$this->m_CompId = $compID;

		$this->m_Conn = self::openConnection();



		$result = mysqli_query($this->m_Conn, "select * from login where tavid = $compID");

		if ($tmp = mysqli_fetch_array($result))

		  {

		    $this->m_CompName = $tmp["compName"];

		    $this->m_CompDate = date("Y-m-d",strtotime($tmp["compDate"]));

		    $this->m_TimeDiff = $tmp["timediff"]*3600;

		    if (isset($tmp["videourl"]))
		    	$this->m_VideoUrl = $tmp["videourl"];

		    if (isset($tmp["videotype"]))
				$this->m_VideoFormat= $tmp["videotype"];
        
         if (isset($tmp["twitter"]))
				$this->m_TwitterFeed= $tmp["twitter"];
        
		    if (isset($tmp['multidaystage']))
		    {
		    	if ($tmp['multidaystage'] != null && $tmp['multidayparent'] != null && $tmp['multidaystage'] > 1)
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
  
   function HasTwitter()
	{
		return $this->m_TwitterFeed != "";
	}

	function GetVideoEmbedCode()
	{
		if ($this->m_VideoFormat == "bambuser")
		{
			return '<iframe src="http://embed.bambuser.com/channel/' . $this->m_VideoUrl . '" width="460" height="403" frameborder="0">Your browser does not support iframes.</iframe>';
		}
		return "";
	}
  
  function GetTwitterFeed()
  {
    return $this->m_TwitterFeed;
  }


	function CompName()

	{
	  return $this->m_CompName;
	}

	function CompDate()

	{

	  return $this->m_CompDate;

	}
  
  function TimeZoneDiff()
  {
    return $this->m_TimeDiff/3600;
  }
  

	function Classes()
	{

		$ret = Array();

		$q = "SELECT Class From runners where TavId = ". $this->m_CompId ." Group By Class";

		if ($result = mysqli_query($this->m_Conn, $q))

		{

			while ($row = mysqli_fetch_array($result))

			{

				$ret[] = $row;

			}

			mysqli_free_result($result);

		}

		else

			die(mysqli_error($this->m_Conn));

		return $ret;



	}

function getAllSplitControls()
{
	$ret = Array();
	$q = "SELECT code, name, classname, corder from splitcontrols where tavid = " .$this->m_CompId. " order by corder";
	if ($result = mysqli_query($this->m_Conn, $q))
	{
		while($tmp = mysqli_fetch_array($result))
		{
			$ret[] = $tmp;
		}
		mysqli_free_result($result);
	} 
	else
	{ 
		echo(mysqli_error($this->m_Conn));
	}
	
	return $ret;
}


  function getSplitControlsForClass($className)

  {

    $ret = Array();

    $q = "SELECT Control from results, runners where results.TavID = ". $this->m_CompId . " and runners.TavID = " . $this->m_CompId . " and results.dbid = runners.dbid and runners.class = '" . mysqli_real_escape_string($this->m_Conn, $className) ."' and results.Control != 1000 Group by Control";

    $q = "SELECT code, name from splitcontrols where tavid = " .$this->m_CompId. " and classname = '" . mysqli_real_escape_string($this->m_Conn, $className) ."' order by corder";

    if ($result = mysqli_query($this->m_Conn, $q))

      {

	while($tmp = mysqli_fetch_array($result))

	  {

	    $ret[] = $tmp;

	  }

	mysqli_free_result($result);



      } else

	{ echo(mysqli_error($this->m_Conn));

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

		if ($result = mysqli_query($this->m_Conn, $q))

		{

			while ($row = mysqli_fetch_array($result))

			{

				$ret[] = $row;
				if ($this->m_TimeDiff != 0)
				{
					$ret[sizeof($ret)-1]["Changed"] = date("Y-m-d H:i:s",strtotime($ret[sizeof($ret)-1]["Changed"])+$this->m_TimeDiff);
				}

			}

			mysqli_free_result($result);

		}

		else

			die(mysqli_error($this->m_Conn));

		return $ret;

  }

	function getSplitsForClass($className,$split)

	{

		$ret = Array();

		$q = "SELECT runners.Name, runners.Club, results.Time,results.Status, results.Changed From runners,results where results.DbID = runners.DbId AND results.TavId = ". $this->m_CompId ." AND runners.TavId = ".$this->m_CompId ." AND runners.Class = '".$className."' and results.Status <> -1 AND (results.Time <> -1 or (results.Time = -1 and (results.Status = 2 or results.Status=3))) AND results.Control = $split ORDER BY results.Status, results.Time";

		if ($result = mysqli_query($this->m_Conn, $q))

		{

			while ($row = mysqli_fetch_array($result))

			{

				$ret[] = $row;

			}

			mysqli_free_result($result);

		}

		else

			die(mysqli_error($this->m_Conn));

		return $ret;
	}

		function getClubResults($compId, $club)

		{

			$ret = Array();


			$q = "SELECT runners.Name, runners.Club, results.Time, runners.Class ,results.Status, results.Changed, results.DbID, results.Control ";
			$q .= ", (select count(*)+1 from results sr, runners sru where sr.tavid=sru.tavid and sr.dbid=sru.dbid and sr.tavid=results.TavId and sru.class = runners.class and sr.status = 0 and sr.time < results.time and sr.Control=1000) as place ";
			$q .= ", results.Time - (select min(time) from results sr, runners sru where sr.tavid=sru.tavid and sr.dbid=sru.dbid and sr.tavid=results.TavId and sru.class = runners.class and sr.status = 0 and sr.Control=1000) as timeplus ";
			$q .= "From runners,results where ";
			$q .= "results.DbID = runners.DbId AND results.TavId = ". $this->m_CompId ." AND runners.TavId = ".$this->m_CompId ." and runners.Club = '". mysqli_real_escape_string($this->m_Conn, $club) ."' and (results.Control=1000 or results.Control=100) ORDER BY runners.Class, runners.Name";

			if ($result = mysqli_query($this->m_Conn, $q))

			{

				while ($row = mysqli_fetch_array($result))

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

				mysqli_free_result($result);

			}

			else

				die(mysqli_error($this->m_Conn));


			return $ret;
	}

		function getAllSplitsForClass($className)

		{

			$ret = Array();


			$q = "SELECT runners.Name, runners.Club, results.Time ,results.Status, results.Changed, results.DbID, results.Control From runners,results where results.DbID = runners.DbId AND results.TavId = ". $this->m_CompId ." AND runners.TavId = ".$this->m_CompId ." AND runners.Class = '". mysqli_real_escape_string($this->m_Conn, $className)."'  ORDER BY results.Dbid";

			if ($result = mysqli_query($this->m_Conn, $q))

			{

				while ($row = mysqli_fetch_array($result))

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

				mysqli_free_result($result);

			}

			else

				die(mysqli_error($this->m_Conn));



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

				$ar = Array();
				if ($this->m_MultiDayParent == -1)
				{
				     $comps = "(".$this->m_CompId.")";
	    			} else {
    					$q = "Select TavId,multidaystage from login where MultiDayParent = ".$this->m_MultiDayParent." and MultiDayStage <=".$this->m_MultiDayStage." order by multidaystage";

					$comps = "(";
					if ($result = mysqli_query($this->m_Conn, $q))
					{
						$f = 1;
						while ($row = mysqli_fetch_array($result))
						{
							$ar[$row["TavId"]] = $row["TavId"];
							if ($f == 0)
								$comps .=",";
							$comps .= $row["TavId"];
							$f = 0;
						}
					}
					mysqli_free_result($result);
					$comps .= ")";
				}


				$q = "SELECT results.Time, results.Status, results.TavId, results.DbID From runners,results where results.Control = 1000 and results.DbID = runners.DbId AND results.TavId in $comps AND runners.TavId = results.TavId AND runners.Class = '".mysqli_real_escape_string($this->m_Conn, $className)."'  ORDER BY results.Dbid";

				if ($result = mysqli_query($this->m_Conn, $q))

				{
					while ($row = mysqli_fetch_array($result))

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

					mysqli_free_result($result);

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

					die(mysqli_error($this->m_Conn));



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