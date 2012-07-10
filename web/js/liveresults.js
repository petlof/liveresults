var LiveResults = LiveResults || {};
LiveResults.Resources = LiveResults.Resources || {};
LiveResults.Instance = null;

LiveResults.AjaxViewer = function(compId, lang, classesDiv,lastPassingsDiv, resultsHeaderDiv,resultsControlsDiv, resultsDiv, txtResetSorting, resources, isMultiDay, isSingleClass,setAutomaticUpdateText, runnerStatus)
{
	
	var updateAutomatically = true;
	var updateInterval = 15000;
	var classUpdateInterval = 60000;

	var classUpdateTimer = null;
	var passingsUpdateTimer = null;
	
	var lastClassListHash = "";
	var lastPassingsUpdateHash = "";
	
	var currentTable = null;
	var resUpdateTimeout = null;
	
	var curClassName = "";
	var lastClassHash = "";
	
	var curClubName = "";
	var lastClubHash = "";
	
	var curClassSplits = null;


	var _classesDiv = classesDiv;
	var _lastPassingsDiv = lastPassingsDiv;
	var _resultsHeaderDiv = resultsHeaderDiv;
	var _resultsControlsDiv = resultsControlsDiv;
	var _resultsDiv = resultsDiv;
	var _txtResetSorting = txtResetSorting;
	var _setAutomaticUpdateText = setAutomaticUpdateText;
	
	var _compId = compId;
	var _lang = lang;
	var _isMultiDay = isMultiDay;
	var _isSingleClass = isSingleClass;
	
	var Resources = resources;
	
	LiveResults.Instance = this;
	
	var _runnerStatus = runnerStatus;
	
	
this.updateClassList = function ()
{
		if (updateAutomatically)
		{
			$.ajax({
				  url: "api.php",
				  data: "comp="+ _compId + "&method=getclasses&last_hash="+lastClassListHash,
				  success: resp_updateClassList,
				  error: function(xhr, ajaxOptions, thrownError) {classUpdateTimer = setTimeout(updateClassList,classUpdateInterval);},
				  dataType: "json"
			});
		}
};
	
resp_updateClassList = function (data)
{
		if (data != null && data.status == "OK")
		{
			if (data.classes != null)
			{
				str = ""
				$.each(data.classes,
					function(key, value)
					{
						str += "<a href=\"javascript:LiveResults.Instance.chooseClass('" + value.className + "')\">" + value.className + "</a><br/>";
					}
				);
				$("#" + _classesDiv).html(str);
				lastClassListHash = data.hash;
			}
		}
	
		classUpdateTimer = setTimeout(LiveResults.Instance.updateClassList,classUpdateInterval);
};

this.updateLastPassings = function ()
{
		if (updateAutomatically)
		{
			$.ajax({
				  url: "api.php",
				  data: "comp="+ _compId + "&method=getlastpassings&lang=" + _lang + "&last_hash="+lastPassingsUpdateHash,
				  success: resp_updateLastPassings,
				  error: function(xhr, ajaxOptions, thrownError) { passingsUpdateTimer = setTimeout(updateLastPassings,updateInterval);},
				  dataType: "json"
			});
		}
	
};
	
resp_updateLastPassings = function (data)
{
		if (data != null && data.status == "OK")
		{
			if (data.passings != null)
			{
				str = ""
				$.each(data.passings,
					function(key, value)
					{
						str += value.passtime + ": " + value.runnerName + " (<a href=\"javascript:LiveResults.Instance.chooseClass('" + value["class"] + "')\">" + value["class"] + "</a>) " + (value.control == 1000 ? Resources["_LASTPASSFINISHED"] : Resources["_LASTPASSPASSED"] + " " + value["controlName"]) + " " + Resources["_LASTPASSWITHTIME"] + " " + value["time"] + "<br/>";
					}
				);
				$("#" + _lastPassingsDiv).html(str);
				lastPassingsUpdateHash = data.hash;
			}
		}
	
		passingsUpdateTimer = setTimeout(LiveResults.Instance.updateLastPassings,updateInterval);
};
	
checkForClassUpdate = function ()
{
		if (updateAutomatically)
		{
			if (currentTable != null)
			{
				$.ajax({
					  url: "api.php",
					  data: "comp="+_compId+"&method=getclassresults&unformattedTimes=true&class="+curClassName + "&last_hash=" +lastClassHash + (_isMultiDay ? "&includetotal=true" : ""),
					  success: resp_updateClassResults,
					  error: function(xhr, ajaxOptions, thrownError) { resUpdateTimeout = setTimeout(checkForClassUpdate,updateInterval);},
					  dataType: "json"
				});
			}
		}
	
};

checkForClubUpdate = function ()
{
		if (updateAutomatically)
		{
			if (currentTable != null)
			{
				$.ajax({
					  url: "api.php",
					  data: "comp="+_compId+"&method=getclubresults&unformattedTimes=true&club="+encodeURIComponent(curClubName) + "&last_hash=" +lastClubHash + (_isMultiDay ? "&includetotal=true" : ""),
					  success: resp_updateClubResults,
					  error: function(xhr, ajaxOptions, thrownError) { resUpdateTimeout = setTimeout(checkForClubUpdate,updateInterval);},
					  dataType: "json"
				});
			}
		}
	
};


resp_updateClassResults = function (data)
{
	if (data.status == "OK")
	{
		if (currentTable != null)
		{
			updateResultVirtualPosition(data.results);
			currentTable.fnClearTable();
			currentTable.fnAddData(data.results,true);
			lastClassHash = data.hash;
		}
	}
	resUpdateTimeout = setTimeout(checkForClassUpdate,updateInterval);
};

resp_updateClubResults = function (data)
{
	if (data.status == "OK")
	{
		if (currentTable != null)
		{
			currentTable.fnClearTable();
			currentTable.fnAddData(data.results,true);
			lastClubHash = data.hash;
		}
	}
	resUpdateTimeout = setTimeout(checkForClubUpdate,updateInterval);
};

this.updateClassResults = function (data)
{
	if (data != null && data.status == "OK")
	{
		if (data.className != null)
		{
			$('#' + _resultsHeaderDiv).html('<b>'+data.className + '</b>');
			$('#' + _resultsControlsDiv).show();
		}

		if (data.results != null)
		{
			columns = Array();
			columns.push({ "sTitle": "#", "bSortable" : false, "aTargets" : [0], "mDataProp": "place" });
			columns.push({ "sTitle": Resources["_NAME"],"bSortable" : false,"aTargets" : [1], "mDataProp": "name" });
			columns.push({ "sTitle": Resources["_CLUB"],"bSortable" : false ,"aTargets" : [2], "mDataProp": "club",
			"fnRender": function ( o, val )
				{
					return "<a href=\"javascript:LiveResults.Instance.viewClubResults('" + o.aData.club + "')\">" + o.aData.club + "</a>";
				}
										
			});

			curClassSplits = data.splitcontrols;

			updateResultVirtualPosition(data.results);

			var col = 3;
			columns.push({ "sTitle": Resources["_START"], "sClass": "left", "sType": "numeric","aDataSort": [col], "aTargets" : [col],"bUseRendered": false, "mDataProp": "start",
			"fnRender": function ( o, val )
										{
											if (o.aData.start =="")
											{
												return "";
											}
											else
											{
												return formatTime(o.aData.start,0,true);
											}
										}
									});

			col++;

			if (data.splitcontrols != null)
			{
				$.each(data.splitcontrols,
					function(key,value)
					{
						columns.push({ "sTitle": value.name, "sClass": "left","sType": "numeric","aDataSort" : [col+1,col],"aTargets" : [col], "bUseRendered" : false, "mDataProp" : "splits." + value.code, "fnRender": function ( o, val )
							{
									if (o.aData.splits[value.code+"_status"] != 0)
										return "";
									else
										return formatTime(o.aData.splits[value.code],0) + " (" +o.aData.splits[value.code + "-place"] + ")";
							}
						});
						col++
						columns.push({ "sTitle": value.name + "_Status", "bVisible" : false,"aTargets" : [col++],"sType": "numeric", "mDataProp": "splits." + value.code + "_status"});
					});
			}

			timecol = col;
			columns.push({ "sTitle": Resources["_CONTROLFINISH"], "sClass": "left", "sType": "numeric","aDataSort": [ col+1, col, 0], "aTargets" : [col],"bUseRendered": false, "mDataProp": "result",
							"fnRender": function ( o, val )
							{
								if (o.aData.place == "-" || o.aData.place == "")
								{
									return formatTime(o.aData.result,o.aData.status);
								}
								else
								{
									return formatTime(o.aData.result,o.aData.status) +" (" + o.aData.place +")";
								}
							}
						});

			col++;
			columns.push({ "sTitle": "Status", "bVisible" : false,"aTargets" : [col++],"sType": "numeric", "mDataProp": "status"});
			columns.push({ "sTitle": "", "sClass": "center","bSortable" : false,"aTargets" : [col++],"mDataProp": "timeplus",
							"fnRender": function ( o, val )
												{
													if (o.aData.status != 0)
														return "";
													else
														return "+" + formatTime(o.aData.timeplus,o.aData.status);
							}
						});

			if (_isMultiDay)
			{
			
						columns.push({ "sTitle": Resources["_TOTAL"], "sClass": "left", "sType": "numeric","aDataSort": [ col+1, col, 0], "aTargets" : [col],"bUseRendered": false, "mDataProp": "totalresult",
										"fnRender": function ( o, val )
										{
											if (o.aData.totalplace == "-" || o.aData.totalplace == "")
											{
												return formatTime(o.aData.totalresult,o.aData.totalstatus);
											}
											else
											{
												return formatTime(o.aData.totalresult,o.aData.totalstatus) +" (" + o.aData.totalplace +")";
											}
										}
									});

						col++;
						columns.push({ "sTitle": "TotalStatus", "bVisible" : false,"aTargets" : [col++],"sType": "numeric", "mDataProp": "totalstatus"});
						columns.push({ "sTitle": "", "sClass": "center","bSortable" : false,"aTargets" : [col++],"mDataProp": "totalplus",
										"fnRender": function ( o, val )
															{
																if (o.aData.totalstatus != 0)
																	return "";
																else
																	return "+" + formatTime(o.aData.totalplus,o.aData.totalstatus);
										}
									});

			}

			columns.push({ "sTitle": "VP", "bVisible" : false, "aTargets" : [col++], "mDataProp": "virtual_position" });

			currentTable = $('#' + _resultsDiv).dataTable( {
					"bPaginate": false,
					"bLengthChange": false,
					"bFilter": false,
					"bSort": true,
					"bInfo" : false,
					"bAutoWidth": false,
					"aaData": data.results,
					"aaSorting" : [[col-1, "asc"]],
					"aoColumnDefs": columns,
					 "fnPreDrawCallback": function( oSettings ) {
					      if ( oSettings.aaSorting[0][0] != col-1) {
					        $("#" + _txtResetSorting).html("&nbsp;&nbsp;<a href=\"javascript:LiveResults.Instance.resetSorting()\"><img class=\"eR\" style=\"vertical-align: middle\" src=\"images/cleardot.gif\" border=\"0\"/> " + Resources["_RESETTODEFAULT"] + "</a>");
      					  }
      					  }
			} );

			lastClassHash = data.hash;
		}
    }
};

this.updateClubResults = function (data)
{
	if (data != null && data.status == "OK")
	{
		if (data.clubName != null)
		{
			$('#' + _resultsHeaderDiv).html('<b>'+data.clubName + '</b>');
			$('#' + _resultsControlsDiv).show();
		}

		if (data.results != null)
		{
			columns = Array();
			columns.push({ "sTitle": "#", "aTargets" : [0], "mDataProp": "place" });
			columns.push({ "sTitle": Resources["_NAME"],"aTargets" : [1], "mDataProp": "name" });
			columns.push({ "sTitle": Resources["_CLUB"],"bSortable" : false ,"aTargets" : [2], "mDataProp": "club" });
			columns.push({ "sTitle": Resources["_CLASS"],"aTargets" : [3], "mDataProp": "class",
			"fnRender": function ( o, val )
							{
								return "<a href=\"javascript:LiveResults.Instance.chooseClass('" + o.aData["class"] + "')\">" + o.aData["class"] + "</a>";
							}
													
			});

			var col = 4;
			columns.push({ "sTitle": Resources["_START"], "sClass": "left", "sType": "numeric","aDataSort": [col], "aTargets" : [col],"bUseRendered": false, "mDataProp": "start",
			"fnRender": function ( o, val )
										{
											if (o.aData.start =="")
											{
												return "";
											}
											else
											{
												return formatTime(o.aData.start,0,true);
											}
										}
									});

			col++;

			timecol = col;
			columns.push({ "sTitle": Resources["_CONTROLFINISH"], "sClass": "left", "sType": "numeric","aDataSort": [ col+1, col, 0], "aTargets" : [col],"bUseRendered": false, "mDataProp": "result",
							"fnRender": function ( o, val )
							{
								if (o.aData.place == "-" || o.aData.place == "")
								{
									return formatTime(o.aData.result,o.aData.status);
								}
								else
								{
									return formatTime(o.aData.result,o.aData.status) +" (" + o.aData.place +")";
								}
							}
						});

			col++;
			columns.push({ "sTitle": "Status", "bVisible" : false,"aTargets" : [col++],"sType": "numeric", "mDataProp": "status"});
			columns.push({ "sTitle": "", "sClass": "center","bSortable" : false,"aTargets" : [col++],"mDataProp": "timeplus",
							"fnRender": function ( o, val )
												{
													if (o.aData.status != 0)
														return "";
													else
														return "+" + formatTime(o.aData.timeplus,o.aData.status);
							}
						});

			if (false && _isMultiDay)
			{
			
						columns.push({ "sTitle": Resources["_TOTAL"], "sClass": "left", "sType": "numeric","aDataSort": [ col+1, col, 0], "aTargets" : [col],"bUseRendered": false, "mDataProp": "totalresult",
										"fnRender": function ( o, val )
										{
											if (o.aData.totalplace == "-" || o.aData.totalplace == "")
											{
												return formatTime(o.aData.totalresult,o.aData.totalstatus);
											}
											else
											{
												return formatTime(o.aData.totalresult,o.aData.totalstatus) +" (" + o.aData.totalplace +")";
											}
										}
									});

						col++;
						columns.push({ "sTitle": "TotalStatus", "bVisible" : false,"aTargets" : [col++],"sType": "numeric", "mDataProp": "totalstatus"});
						columns.push({ "sTitle": "", "sClass": "center","bSortable" : false,"aTargets" : [col++],"mDataProp": "totalplus",
										"fnRender": function ( o, val )
															{
																if (o.aData.totalstatus != 0)
																	return "";
																else
																	return "+" + formatTime(o.aData.totalplus,o.aData.totalstatus);
										}
									});

			}


			currentTable = $('#' + _resultsDiv).dataTable( {
					"bPaginate": false,
					"bLengthChange": false,
					"bFilter": false,
					"bSort": true,
					"bInfo" : false,
					"bAutoWidth": false,
					"aaData": data.results,
					"aaSorting" : [[0, "asc"]],
					"aoColumnDefs": columns,
					 "fnPreDrawCallback": function( oSettings ) {
					      if ( oSettings.aaSorting[0][0] != 0) {
					        $("#" + _txtResetSorting).html("&nbsp;&nbsp;<a href=\"javascript:LiveResults.Instance.resetSorting()\"><img class=\"eR\" style=\"vertical-align: middle\" src=\"images/cleardot.gif\" border=\"0\"/> " + Resources["_RESETTODEFAULT"] + "</a>");
      					  }
      					  }
			} );

			lastClubHash = data.hash;
		}
    }
};



this.resetSorting = function ()
{
	idxCol = 0;
	$.each(currentTable.fnSettings().aoColumns, function(idx,val)
	{
		if (val.sTitle=="VP")
		{
			idxCol = idx;
		}
	});

	currentTable.fnSort([ [idxCol,'asc']]);
	$("#" + _txtResetSorting).html("");

};

formatTime = function(time,status, showHours, padZeros)
{

	if (arguments.length==2)
	{
		if (_lang == 'fi'){
			showHours = true;
			padZeros = false;
		} else {
			showHours = false;
			padZeros = true;
		}
	}
	else if (arguments.length==3)
		{
		if (_lang == 'fi'){
				padZeros = false;
		} else {
				padZeros = true;
		}
	}

	if (status != 0)
  	{
  		return _runnerStatus[status];
  	}
  	else
  	{
  		if (showHours)
  		{
  				hours= Math.floor(time/360000);
		  		minutes = Math.floor((time-hours*360000)/6000);
				seconds = Math.floor((time-minutes*6000-hours*360000)/100);


			if (hours > 0)
			{
				if (padZeros)
					hours = str_pad(hours,2);

  	 			return hours +":" + str_pad(minutes,2) +":" +str_pad(seconds,2);
  	 		}
  	 		else
  	 		{
  	 			if (padZeros)
					minutes = str_pad(minutes,2);
  	 			return minutes +":" +str_pad(seconds,2);
  	 		}

		}
		else
		{

  	 		minutes = Math.floor(time/6000);
	 		seconds = Math.floor((time-minutes*6000)/100);

			if (padZeros)
			{
  	 			return str_pad(minutes,2) +":" +str_pad(seconds,2);
  	 		}
  	 		else
  	 		{
  	 			return minutes +":" +str_pad(seconds,2);
  	 		}
  	 	}
  	}
};


str_pad = function(number, length) {

    var str = '' + number;
    while (str.length < length) {
        str = '0' + str;
    }

    return str;
};

this.newWin = function()
{
	var url = 'followfull.php?comp=' + _compId + '&lang=' + _lang;
	
	if (curClassName != null)
		url += '&class='+encodeURIComponent(curClassName);
	else 
		url += '&club='+encodeURIComponent(curClubName);
	
	window.open(url,'','status=0,toolbar=0,location=0,menubar=0,directories=0,scrollbars=1,resizable=1,width=900,height=600');
};

this.viewClubResults = function(clubName)
{
	if (currentTable != null)
		currentTable.fnDestroy();

	clearTimeout(resUpdateTimeout);

	$('#divResults').html('');
	curClubName = clubName;
	curClassName = null;
	$('#resultsHeader').html(Resources["_LOADINGRESULTS"]);
	
	$.ajax({
			  url: "api.php",
			  data: "comp=" + compId + "&method=getclubresults&unformattedTimes=true&club="+encodeURIComponent(clubName)+ (_isMultiDay ? "&includetotal=true" : ""),
			  success: this.updateClubResults,
			  dataType: "json"
	});
	
	if (!_isSingleClass) {
		window.location.hash = "club::" + clubName;
	}
	
	resUpdateTimeout = setTimeout(checkForClubUpdate,updateInterval);
};

this.chooseClass = function(className)
{
	if (currentTable != null)
		currentTable.fnDestroy();

	clearTimeout(resUpdateTimeout);

	$('#divResults').html('');
	curClassName = className;
	curClubName = null
	$('#resultsHeader').html(Resources["_LOADINGRESULTS"]);
	$.ajax({
		  url: "api.php",
		  data: "comp=" + compId + "&method=getclassresults&unformattedTimes=true&class="+className + (_isMultiDay ? "&includetotal=true" : ""),
		  success: this.updateClassResults,
		  dataType: "json"
	});

	if (!_isSingleClass) {
		window.location.hash = className;
	}
	resUpdateTimeout = setTimeout(checkForClassUpdate,updateInterval);
};


this.setAutomaticUpdate = function(val)
{
	updateAutomatically = val;
	if (updateAutomatically)
	{
		$("#" + _setAutomaticUpdateText).html("<b>" + Resources["_AUTOUPDATE"] + ":</b> " + Resources["_ON"] + " | <a href=\"javascript:LiveResults.Instance.setAutomaticUpdate(false);\">" + Resources["_OFF"] + "</a>");
		this.checkForClassUpdate();
		this.updateLastPassings();
		this.checkForClassUpdate();

	}
	else
	{
		clearTimeout(resUpdateTimeout);
		clearTimeout(passingsUpdateTimer);
		clearTimeout(classUpdateTimer);
		$("#" + _setAutomaticUpdateText).html("<b>" + Resources["_AUTOUPDATE"] + ":</b> <a href=\"javascript:LiveResults.Instance.setAutomaticUpdate(true);\">" + Resources["_ON"] + "</a> | " + Resources["_OFF"] + "");
	}
};

updateResultVirtualPosition = function(data)
{

	if (curClassSplits != null)
	{
		for (i = 0; i < data.length; i++)
		{
			data[i].haveSplits = false;
		}

		for (s = 0; s < curClassSplits.length; s++)
		{
			splitCode = curClassSplits[s].code;
			data.sort(function(a,b) { return a.splits[splitCode] - b.splits[splitCode];});

			lastPos = 1;
			posCnt = 1;
			lastTime = -1;
			for (i = 0; i < data.length; i++)
			{
				if (data[i].splits[splitCode] != "")
				{
					data[i].haveSplits = true;

					if (lastTime == data[i].splits[splitCode])
						data[i].splits[splitCode + "-place"] = lastPos;
					else
					{
						data[i].splits[splitCode + "-place"] = posCnt;
						lastPos = posCnt;
					}
					lastTime = data[i].splits[splitCode];
					posCnt++;
				}
				else
				{
					data[i].splits[splitCode + "-place"] = "";
				}
			}

		}
	}

	data.sort(resultSorter);

	/* move down runners that have not finished to the correct place*/
	firstFinishedIdx = -1;
	for (i = 0; i < data.length; i++)
	{
		if (data[i].place != "")
		{
			firstFinishedIdx = i;
			break;
		}
	}

	if (firstFinishedIdx == -1)
		firstFinishedIdx = data.length-1;

	tmp = Array();
	for (i = 0; i < firstFinishedIdx; i++)
	{
		tmp.push(data[i]);
	}

	data.splice(0,firstFinishedIdx);

	tmp.sort(sortByDist);

	for (i = 0; i < tmp.length; i++)
	{
		if (data.length == 0)
			data.push(tmp[i]);
		else
			insertIntoResults(tmp[i],data);
	}


	for (i = 0; i < data.length; i++)
	{
		data[i].virtual_position = i;
	}

};

sortByDist = function(a,b)
{
	for (s = curClassSplits.length-1; s >= 0; s--)
	{
		splitCode = curClassSplits[s].code;
		if (a.splits[splitCode] == "" && b.splits[splitCode] != "")
			return 1;
		else if (a.splits[splitCode] != "" && b.splits[splitCode] == "")
			return -1;
	}
	return 0;
};

insertIntoResults = function(result, data)
{
	haveSplit = false;
	if (curClassSplits != null)
	{
		for (s = curClassSplits.length-1; s >= 0; s--)
		{
			splitCode = curClassSplits[s].code;
			if (result.splits[splitCode] != "")
			{
				haveSplit=true;
				for (d = 0; d < data.length; d++)
				{
					if (data[d].place == "-" || (data[d].splits[splitCode] != "" && data[d].splits[splitCode] > result.splits[splitCode]))
					{
						data.splice(d,0,result);
						return;
					}
				}
			}
		}
	}


	if (result.start != "")
	{
		for (d = 0; d < data.length; d++)
		{
			if (data[d].place == "-")
			{
				data.splice(d,0,result);
				return;
			}
			if (result.place == "" && data[d].place != "")
			{
			}
			else if (data[d].start != "" && data[d].start> result.start && !haveSplit && !data[d].haveSplits)
			{
				data.splice(d,0,result);
				return;
			}

		}
	}

	data.push(tmp[i]);
};

resultSorter = function(a,b)
{
	if (a.place != "" && b.place != "")
	{
		if (a.status != b.status)
			return a.status - b.status;
		else
		{
			if (a.result == b.result)
			{
				if (a.place == "=" && b.place != "=")
				{
					return 1;
				}
				else if (b.place == "=" && a.place != "=")
				{
					return -1;
				}
				else
				{
					return 0;
				}

			}
			else
			{
				return a.result - b.result;
			}
		}

	}
	else if (a.place == "-" || a.place != "" )
	{
		return 1;
	}
	else if (b.place == "-" || b.place != "" )
	{
			return -1;
	}
	else
	{

		return 0;
	}
};

	$(window).hashchange( function(){
		if(window.location.hash) {
			var hash = window.location.hash.substring(1);
				if (hash.indexOf('club::') >= 0)
		      		{
		      			var cl = hash.substring(6);
		      			if (cl != curClubName)
		      			{
		      				LiveResults.Instance.viewClubResults(hash.substring(6));
		      			}
		      		}
		      		else
		      		{
		      			var cl = hash;
					if (cl != curClassName)
		      			{
		      				LiveResults.Instance.chooseClass(hash);
		      			}
		      		}
      		}
	});
	
 $(window).hashchange();

};