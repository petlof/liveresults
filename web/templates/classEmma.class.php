<?php
class Emma
{
	public static $db_server = "192.168.0.183";
	public static $db_database = "liveresultat";
	public static $db_user = "liveresultat";
	public static $db_pw= "web";
	var $m_CompId;
   var $m_CompName;
   var $m_CompDate;
   var $m_TimeDiff = 0;
	var $m_Conn;
        public static function GetCompetitions()
        {
          $conn = mysql_connect(self::$db_server,self::$db_user,self::$db_pw);
	  mysql_select_db(self::$db_database);
	  if (mysql_errno()) {
	   		printf("Connect failed: %s\n", mysql_error());
	   		exit();
		}
	 $result = mysql_query("select compName, compDate,tavid,organizer from Login where public = 1 order by compDate desc",$conn);
         $ret = Array();
         while ($tmp = mysql_fetch_array($result))
	 {
 		$ret[] = $tmp;
         }
		mysql_free_result($result);
 	return $ret;
        }

public static function GetRadioControls($compid)
        {	$conn = mysql_connect(self::$db_server,self::$db_user,self::$db_pw);

	  mysql_select_db(self::$db_database);
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
        {$conn = mysql_connect(self::$db_server,self::$db_user,self::$db_pw);

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

	  if (mysql_errno()) {
	   		printf("Connect failed: %s\n", mysql_error());
	   		exit();
		}
	 $res = mysql_query("select max(tavid)+1 from Login",$conn);
	 $id = mysql_result($res,0,0);
	if ($id < 10000)
		$id = 10000;

	 mysql_query("insert into Login(tavid,user,pass,compName,organizer,compDate,public) values(".$id.",'".md5($name.$org.$date)."','".md5("liveresultat")."','".$name."','".$org."','".$date."',0)" ,$conn) or die(mysql_error());

	}
	public static function AddRadioControl($compid,$classname,$name,$code)
        {
          $conn = mysql_connect(self::$db_server,self::$db_user,self::$db_pw);
	  mysql_select_db(self::$db_database);

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

	  if (mysql_errno()) {
	   		printf("Connect failed: %s\n", mysql_error());
	   		exit();
		}
	 $sql = "update Login set compName = '$name', organizer='$org', compDate ='$date',timediff=$timediff, public=". (!isset($public) ? "0":"1") ." where tavid=$id";
	 mysql_query($sql ,$conn) or die(mysql_error());

	}

	public static function GetAllCompetitions()
        {
         $conn = mysql_connect(self::$db_server,self::$db_user,self::$db_pw);
	  mysql_select_db(self::$db_database);

	  if (mysql_errno()) {
	   		printf("Connect failed: %s\n", mysql_error());
	   		exit();
		}
	 $result = mysql_query("select compName, compDate,tavid,timediff,organizer,public from Login order by compDate desc",$conn);
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

	  if (mysql_errno()) {
	   		printf("Connect failed: %s\n", mysql_error());
	   		exit();
		}
	 $result = mysql_query("select compName, compDate,tavid,organizer,public,timediff from Login where tavid=$compid",$conn);
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
		/* check connection */
		if (mysql_errno()) {
	   		printf("Connect failed: %s\n", mysql_error());
	   		exit();
		}

		$result = mysql_query("select * from Login where tavid = $compID",$this->m_Conn);
		if ($tmp = mysql_fetch_array($result))
		  {
		    $this->m_CompName = $tmp["compName"];
		    $this->m_CompDate = date("Y-m-d",strtotime($tmp["compDate"]));

		    $this->m_TimeDiff = $tmp["timediff"]*3600;
		  }
	}

	function CompName()
	{

	  return $this->m_CompName;
	  //		return "Elitserien sprint, kval";
	}
	function CompDate()
	{
	  return $this->m_CompDate;
	  //return "2006-05-25";
	}
	function Classes()
	{
		$ret = Array();
		$q = "SELECT Class From Runners where TavId = ". $this->m_CompId ." Group By Class";
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
    $q = "SELECT Control from Results, Runners where Results.TavID = ". $this->m_CompId . " and Runners.TavID = " . $this->m_CompId . " and Results.dbid = Runners.dbid and Runners.class = '" . $className ."' and Results.Control != 1000 Group by Control";
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
	$q = "SELECT Runners.Name, Runners.class, Runners.Club, Results.Time,Results.Status, Results.Changed, Results.Control, splitcontrols.name as pname From Runners,Results left join splitcontrols on (Results.Control = splitcontrols.Code and Runners.class = splitcontrols.classname and splitcontrols.tavid=".$this->m_CompId.") where Results.DbID = Runners.DbId AND Results.TavId = ". $this->m_CompId ." AND Runners.TavId = Results.TavId and Results.Status <> -1 AND Results.Time <> -1 ORDER BY Results.changed desc limit 3";
	$q = "SELECT Runners.Name, Runners.class, Runners.Club, Results.Time,Results.Status, Results.Changed, Results.Control, splitcontrols.name as pname From Results inner join Runners on Results.DbId = Runners.DbId Left join splitcontrols on (splitcontrols.code = Results.Control and splitcontrols.tavid=".$this->m_CompId." and Runners.class = splitcontrols.classname) where Results.TavId =".$this->m_CompId." AND Runners.TavId = Results.TavId and Results.Status <> -1 AND Results.Time <> -1 ORDER BY Results.changed desc limit 3";
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
		$q = "SELECT Runners.Name, Runners.Club, Results.Time,Results.Status, Results.Changed From Runners,Results where Results.DbID = Runners.DbId AND Results.TavId = ". $this->m_CompId ." AND Runners.TavId = ".$this->m_CompId ." AND Runners.Class = '".$className."' and Results.Status <> -1 AND (Results.Time <> -1 or (Results.Time = -1 and (Results.Status = 2 or Results.Status=3))) AND Results.Control = $split ORDER BY Results.Status, Results.Time";
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

		return $ret;
	}
}

?>