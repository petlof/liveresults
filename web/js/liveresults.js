var LiveResults;
(function (LiveResults) {
    // ReSharper disable once InconsistentNaming
    LiveResults.Instance = null;
    var AjaxViewer = /** @class */ (function () {
        function AjaxViewer(competitionId, language, classesDiv, lastPassingsDiv, resultsHeaderDiv, resultsControlsDiv, resultsDiv, txtResetSorting, resources, isMultiDayEvent, isSingleClass, setAutomaticUpdateText, setCompactViewText, runnerStatus, showTenthOfSecond, radioPassingsDiv) {
            var _this = this;
            this.competitionId = competitionId;
            this.language = language;
            this.classesDiv = classesDiv;
            this.lastPassingsDiv = lastPassingsDiv;
            this.resultsHeaderDiv = resultsHeaderDiv;
            this.resultsControlsDiv = resultsControlsDiv;
            this.resultsDiv = resultsDiv;
            this.radioPassingsDiv = radioPassingsDiv;
            this.txtResetSorting = txtResetSorting;
            this.resources = resources;
            this.isMultiDayEvent = isMultiDayEvent;
            this.isSingleClass = isSingleClass;
            this.setAutomaticUpdateText = setAutomaticUpdateText;
			this.setCompactViewText = setCompactViewText;
            this.runnerStatus = runnerStatus;
            this.showTenthOfSecond = false;
            this.updateAutomatically = true;
			this.compactView = true;
            this.updateInterval = 10000;
			this.radioUpdateInterval = 5000;
            this.classUpdateInterval = 60000;
            this.classUpdateTimer = null;
            this.passingsUpdateTimer = null;
            this.resUpdateTimeout = null;
            this.updatePredictedTimeTimer = null;
            this.lastClassListHash = "";
            this.lastPassingsUpdateHash = "";
            this.lastRadioPassingsUpdateHash = "";
            this.curClassName = "";
            this.lastClassHash = "";
            this.curClassSplits = null;
			this.curClassSplitsBest = null;
            this.curClassIsMassStart = false;
            this.curClubName = "";
            this.lastClubHash = "";
            this.currentTable = null;
            this.serverTimeDiff = 1;
            this.eventTimeZoneDiff = 0;
            this.apiURL = "//www.freidig.idrett.no/o/liveres/api.php";
            this.radioURL = "//www.freidig.idrett.no/o/liveres/radioapi.php";
            LiveResults.Instance = this;
            $(window).hashchange(function () {
                if (window.location.hash) {
                    var hash = window.location.hash.substring(1);
                    var cl;
                    if (hash.indexOf('club::') >= 0) {
                        cl = decodeURIComponent(hash.substring(6));
                        if (cl != _this.curClubName) {
                            LiveResults.Instance.viewClubResults(cl);
                        }
                    }
                    else {
                        cl = decodeURIComponent(hash);
                        if (cl != _this.curClassName) {
                            _this.chooseClass(hash);
                        }
                    }
                }
            });
            $(window).hashchange();
        }
        AjaxViewer.prototype.startPredictionUpdate = function () {
            var _this = this;
            this.updatePredictedTimeTimer = setInterval(function () { _this.updatePredictedTimes(); }, 1000);
        };
        //Detect if the browser is a mobile phone
        AjaxViewer.prototype.mobilecheck = function () {
            var check = false;
            (function (a) { if (/(android|bb\d+|meego).+mobile|avantgo|bada\/|blackberry|blazer|compal|elaine|fennec|hiptop|iemobile|ip(hone|od)|iris|kindle|lge |maemo|midp|mmp|mobile.+firefox|netfront|opera m(ob|in)i|palm( os)?|phone|p(ixi|re)\/|plucker|pocket|psp|series(4|6)0|symbian|treo|up\.(browser|link)|vodafone|wap|windows (ce|phone)|xda|xiino/i.test(a) || /1207|6310|6590|3gso|4thp|50[1-6]i|770s|802s|a wa|abac|ac(er|oo|s\-)|ai(ko|rn)|al(av|ca|co)|amoi|an(ex|ny|yw)|aptu|ar(ch|go)|as(te|us)|attw|au(di|\-m|r |s )|avan|be(ck|ll|nq)|bi(lb|rd)|bl(ac|az)|br(e|v)w|bumb|bw\-(n|u)|c55\/|capi|ccwa|cdm\-|cell|chtm|cldc|cmd\-|co(mp|nd)|craw|da(it|ll|ng)|dbte|dc\-s|devi|dica|dmob|do(c|p)o|ds(12|\-d)|el(49|ai)|em(l2|ul)|er(ic|k0)|esl8|ez([4-7]0|os|wa|ze)|fetc|fly(\-|_)|g1 u|g560|gene|gf\-5|g\-mo|go(\.w|od)|gr(ad|un)|haie|hcit|hd\-(m|p|t)|hei\-|hi(pt|ta)|hp( i|ip)|hs\-c|ht(c(\-| |_|a|g|p|s|t)|tp)|hu(aw|tc)|i\-(20|go|ma)|i230|iac( |\-|\/)|ibro|idea|ig01|ikom|im1k|inno|ipaq|iris|ja(t|v)a|jbro|jemu|jigs|kddi|keji|kgt( |\/)|klon|kpt |kwc\-|kyo(c|k)|le(no|xi)|lg( g|\/(k|l|u)|50|54|\-[a-w])|libw|lynx|m1\-w|m3ga|m50\/|ma(te|ui|xo)|mc(01|21|ca)|m\-cr|me(rc|ri)|mi(o8|oa|ts)|mmef|mo(01|02|bi|de|do|t(\-| |o|v)|zz)|mt(50|p1|v )|mwbp|mywa|n10[0-2]|n20[2-3]|n30(0|2)|n50(0|2|5)|n7(0(0|1)|10)|ne((c|m)\-|on|tf|wf|wg|wt)|nok(6|i)|nzph|o2im|op(ti|wv)|oran|owg1|p800|pan(a|d|t)|pdxg|pg(13|\-([1-8]|c))|phil|pire|pl(ay|uc)|pn\-2|po(ck|rt|se)|prox|psio|pt\-g|qa\-a|qc(07|12|21|32|60|\-[2-7]|i\-)|qtek|r380|r600|raks|rim9|ro(ve|zo)|s55\/|sa(ge|ma|mm|ms|ny|va)|sc(01|h\-|oo|p\-)|sdk\/|se(c(\-|0|1)|47|mc|nd|ri)|sgh\-|shar|sie(\-|m)|sk\-0|sl(45|id)|sm(al|ar|b3|it|t5)|so(ft|ny)|sp(01|h\-|v\-|v )|sy(01|mb)|t2(18|50)|t6(00|10|18)|ta(gt|lk)|tcl\-|tdg\-|tel(i|m)|tim\-|t\-mo|to(pl|sh)|ts(70|m\-|m3|m5)|tx\-9|up(\.b|g1|si)|utst|v400|v750|veri|vi(rg|te)|vk(40|5[0-3]|\-v)|vm40|voda|vulc|vx(52|53|60|61|70|80|81|83|85|98)|w3c(\-| )|webc|whit|wi(g |nc|nw)|wmlb|wonu|x700|yas\-|your|zeto|zte\-/i.test(a.substr(0, 4)))
                check = true; })(navigator.userAgent || navigator.vendor || window.opera);
            return check;
        };
        
        // Update the classlist
        AjaxViewer.prototype.updateClassList = function () {
            var _this = this;
            if (this.updateAutomatically) {
                $.ajax({
                    url: this.apiURL,
                    data: "comp=" + this.competitionId + "&method=getclasses&last_hash=" + this.lastClassListHash,
                    success: function (data) {
                        _this.handleUpdateClassListResponse(data);
                    },
                    error: function () {
                        _this.classUpdateTimer = setTimeout(function () {
                            _this.updateClassList();
                        }, _this.classUpdateInterval);
                    },
                    dataType: "json"
                });
            }
        };
        AjaxViewer.prototype.handleUpdateClassListResponse = function (data) {
            var _this = this;
            if (data != null && data.status == "OK")
			{
                if (!data.classes || !$.isArray(data.classes) || data.classes.length == 0)
                    $('#resultsHeader').html("<b>" + this.resources["_NOCLASSESYET"] + "</b>");
                if (data.classes != null)
				{
					var classes = data.classes;
					classes.sort(function(a, b)
					{
						var x  = [a.className, b.className];
						for (var i=0; i<2; i++)
						{
							x[i] = x[i].replace(/(^|[^\d])(\d)([^\d])/,'$100$2$3');      // Add 00 ahead of single digits
							x[i] = x[i].replace(/(^|[^\d])(\d)(\d)([^\d])/,'$10$2$3$4'); // Add 0 ahead of double digits
							x[i] = x[i].replace(' ','');
							x[i] = x[i].replace(/n-åpen/i,'X');
							x[i] = x[i].replace(/utv/i,'Y');
							x[i] = x[i].replace(/dir/i,'Z');
							x[i] = x[i].replace(/open/i,'Z');
						}
						if (x[0] < x[1]) {return -1;}
						if (x[0] > x[1]) {return 1;}
						return 0;
					});
					var str = "<nowrap>";
					var nClass = classes.length;
					
					var relayNext = false;
					var leg = 0;
					for (var i=0; i<nClass; i++)
					{
						var relay = relayNext;
						var className = classes[i].className;
						param = className.replace('\'', '\\\'');
						if (className && className.length > 0)
						{
							var classNameClean = className.replace(/-\d$/,'');
							classNameClean = classNameClean.replace(/-All$/,'');
							if (i<(nClass-1))
							{
								var classNameCleanNext = classes[i+1].className.replace(/-\d$/,'');
								classNameCleanNext = classNameCleanNext.replace(/-All$/,'');
								if (classNameClean == classNameCleanNext) // Relay trigger
								{
									if (!relay) // First class in relay
									{
										str += "<b>" + classNameClean + "</b><br/>&nbsp";
										leg = 0;
									}
									relay = true;
									relayNext = true;
								}
								else
									relayNext = false;	
							}
							if (relay)
							{
								var legText = "";
								leg += 1;
								// var relayLeg = className.replace(classNameClean,'Etappe');
								if (className.replace(classNameClean,'') == "-All")
									legText = "<font size=\"+1\">&#" + (9398) +"</font>"
								else
								    legText = "<font size=\"+1\">&#" + (10111+leg) +"</font>"; 
								str += "<a href=\"javascript:LiveResults.Instance.chooseClass('" + param + "')\" style=\"text-decoration: none\"> " + legText + "</a>";
								if (!relayNext)
									str += "<br/>";
							}	
							else	
								str += "<a href=\"javascript:LiveResults.Instance.chooseClass('" + param + "')\">" + className + "</a><br/>";
						}
                    };
                    str += "</nowrap>";
                    $("#" + this.classesDiv).html(str);
                    this.lastClassListHash = data.hash;
                }
            }
            this.classUpdateTimer = setTimeout(function () {
                _this.updateClassList();
            }, this.classUpdateInterval);
        };
		
		// Update best split times
		AjaxViewer.prototype.updateClassSplitsBest = function (data) {
			if (data != null && data.status == "OK" && data.results != null) {
				var classSplits = data.splitcontrols;
				var classSplitsBest = Array(classSplits.length+1).fill(0);
				var relay = (classSplits.length>0 && classSplits[0].code == "0"); 
				// Best finish time
				for (var i = 0; i< data.results.length; i++)
				{
					if(data.results[i].place != undefined && data.results[i].place == 1)
					{
						if (relay)
							classSplitsBest[classSplits.length] = data.results[i].start + data.results[i].splits[classSplits[classSplits.length-1].code];
						else
							classSplitsBest[classSplits.length] = parseInt(data.results[i].result);
						break;
					}
				}
				if (classSplits.length > 0)
				{
					
			        // Best split times
					for (var sp = 0; sp < classSplits.length; sp++)
					{
						for (var i = 0; i< data.results.length ; i++)
						{
							if(data.results[i].splits[classSplits[sp].code + "_place"] != undefined && data.results[i].splits[classSplits[sp].code + "_place"] == 1)
							{
								if (relay && sp>0) // If relay store pass time stamp instead of used time. Using leg time and start time
									classSplitsBest[sp] = data.results[i].start + data.results[i].splits[classSplits[sp-1].code];
								else
									classSplitsBest[sp] = data.results[i].splits[classSplits[sp].code];
								break;
							}
						}
					}
				}
				this.curClassSplitsBests = classSplitsBest;
			}
		};
		
		// Updated predicted times
		AjaxViewer.prototype.updatePredictedTimes = function () {
            if (this.currentTable != null && this.curClassName != null && this.serverTimeDiff && this.updateAutomatically) {
                try {
                    var data = this.currentTable.fnGetData();
                    var dt = new Date();
                    var currentTimeZoneOffset = -1 * new Date().getTimezoneOffset();
                    var eventZoneOffset = ((dt.dst() ? 2 : 1) + this.eventTimeZoneDiff) * 60;
                    var timeZoneDiff = eventZoneOffset - currentTimeZoneOffset;
                    var time = 100*Math.round((dt.getSeconds() + (60 * dt.getMinutes()) + (60 * 60 * dt.getHours())) - (this.serverTimeDiff / 1000) + (timeZoneDiff * 60));
                    var i;
                    var extraCol = 0;
					var numSplits = 0;
					
					var haveSplitControls = (this.curClassSplits != null) && (this.curClassSplits.length > 0);
                    var relay = (haveSplitControls && (this.curClassSplits[0].code == "0"));
					var unranked = false;
                
                    for (var sp = this.curClassSplits.length - 1; sp >= 0; sp--) {
                        if (this.curClassSplits[sp].code == "-999") { // If not sorted
                            {
                                extraCol = 1;
								unranked = true;
                                break;
                            }
                        }
                    }
					if (this.compactView)
						extraCol = 1;
					if (relay)
						numSplits = this.curClassSplits.length/2-1;
					else
						numSplits = this.curClassSplits.length;
					var timeDiff = 0;
					var timeDiffCol = 0;
					var timeDiffStr = "";
                    for (var i = 0; i < data.length; i++) {
                        if ((data[i].status == 10 || data[i].status == 9) && data[i].place == "" && data[i].start != "") {
							var elapsedTime = time - data[i].start;
							var elapsedTimeStr = "";
							if (relay && !this.compactView)
							   elapsedTimeStr += "<br/>";
							elapsedTimeStr += "<i>(" + this.formatTime(elapsedTime, 0, false) + ")</i>";
                            if (elapsedTime>=0) {
                                if (this.curClassSplits == null || this.curClassSplits.length == 0) // No split controls
								{
                                    $("#" + this.resultsDiv + " tr:eq(" + (data[i].curDrawIndex + 1) + ") td:eq(" + 4 + ")").html(elapsedTimeStr);
									if (!unranked && this.curClassSplitsBests[0]>0)
									{
										timeDiff = elapsedTime - this.curClassSplitsBests[0]; 
										timeDiffStr = "<i>(" + (timeDiff<0 ? "-" : "+") + this.formatTime(Math.abs(timeDiff), 0, false) + ")</i>";
										$("#" + this.resultsDiv + " tr:eq(" + (data[i].curDrawIndex + 1) + ") td:eq(" + 5 + ")").html(timeDiffStr);
									}
                                }
                                else 
								{
									//Find next split to reach
                                    var nextSplit = 0;
                                    var nextSplitRef = 0;
                                    for (var sp = this.curClassSplits.length - 1; sp >= 0; sp--) {
                                        if (data[i].splits[this.curClassSplits[sp].code] != "") {
                                            {
												if (relay)
												{
													nextSplit = sp/2;
													nextSplitRef = sp + 2;
													}
													
											      else
											      {
													nextSplit = sp + 1;
													nextSplitRef = sp + 1;
													}
                                                break;
                                            }
                                        }
                                    }
									if (!unranked)
									{
										timeDiffCol = 3 + nextSplit + extraCol;
										if (nextSplit==numSplits) // Approach finish
											timeDiffCol += 1;
										if (this.curClassSplitsBests[nextSplitRef]==0)
										   $("#" + this.resultsDiv + " tr:eq(" + (data[i].curDrawIndex + 1) + ") td:eq(" + timeDiffCol + ")").html("<i>(...)<\i>");
										else
										{
											if (relay)
												timeDiff = time - this.curClassSplitsBests[nextSplitRef]; 
											else
												timeDiff = elapsedTime - this.curClassSplitsBests[nextSplitRef]; 
											timeDiffStr = "<i>(" + (timeDiff<0 ? "-" : "+") + this.formatTime(Math.abs(timeDiff), 0, false) + ")</i>";
											$("#" + this.resultsDiv + " tr:eq(" + (data[i].curDrawIndex + 1) + ") td:eq(" + timeDiffCol + ")").html(timeDiffStr);
										}
										$("#" + this.resultsDiv + " tr:eq(" + (data[i].curDrawIndex + 1) + ") td:eq(" + (3 + numSplits + extraCol) + ")").html(elapsedTimeStr);
									}
									else // unranked
										$("#" + this.resultsDiv + " tr:eq(" + (data[i].curDrawIndex + 1) + ") td:eq(" + (3 + nextSplit + extraCol) + ")").html(elapsedTimeStr);
                                }
                            }
                        }
                    }
                }
                catch (e) {
                }
            }
        };

        //Set wether to display tenth of a second in results
        AjaxViewer.prototype.setShowTenth = function (val) {
            this.showTenthOfSecond = val;
        };

        //Request data for the last-passings div
        AjaxViewer.prototype.updateLastPassings = function () {
            var _this = this;
            if (this.updateAutomatically) {
                $.ajax({
                    url: this.apiURL,
                    data: "comp=" + this.competitionId + "&method=getlastpassings&lang=" + this.language + "&last_hash=" + this.lastPassingsUpdateHash,
                    success: function (data) { _this.handleUpdateLastPassings(data); },
                    error: function () {
                        _this.passingsUpdateTimer = setTimeout(function () {
                            _this.updateLastPassings();
                        }, _this.updateInterval);
                    },
                    dataType: "json"
                });
            }
        };
        //Handle response for updating the last passings..
        AjaxViewer.prototype.handleUpdateLastPassings = function (data) {
            var _this = this;
            if (data != null && data.status == "OK") {
                if (data.passings != null) {
                    var str = "";
                    $.each(data.passings, function (key, value) {
                        var cl = value["class"];
                        if (cl && cl.length > 0)
                            cl = cl.replace('\'', '\\\'');
                        str += value.passtime + ": " + value.runnerName + " (<a href=\"javascript:LiveResults.Instance.chooseClass('" + cl + "')\">" + value["class"] + "</a>) " + (value.control == 1000 ? _this.resources["_LASTPASSFINISHED"] : _this.resources["_LASTPASSPASSED"] + " " + value["controlName"]) + " " + _this.resources["_LASTPASSWITHTIME"] + " " + value["time"] + "<br/>";
                    });
                    $("#" + this.lastPassingsDiv).html(str);
                    this.lastPassingsUpdateHash = data.hash;
                }
            }
            this.passingsUpdateTimer = setTimeout(function () {
                _this.updateLastPassings();
            }, this.updateInterval);
        };

        //Request data for the last radio passings div
        AjaxViewer.prototype.updateRadioPassings = function (code,calltime) {
            var _this = this;
            if (this.updateAutomatically) {
                $.ajax({
                    url: this.radioURL,
                    data: "comp=" + this.competitionId + "&method=getradiopassings&code=" + code + "&calltime=" + calltime +
					      "&lang=" + this.language + "&last_hash=" + this.lastRadioPassingsUpdateHash,
                    success: function (data) { _this.handleUpdateRadioPassings(data); },
                    error: function () {
                        _this.radioPassingsUpdateTimer = setTimeout(function () {
                            _this.updateRadioPassings();
                        }, _this.radioUpdateInterval);
                    },
                    dataType: "json"
                });
                this.radioPassingsUpdateTimer = setTimeout(function () {
                    _this.updateRadioPassings(code,calltime);
                }, this.radioUpdateInterval);
            }

        };
        //Handle response for updating the last radio passings..
        AjaxViewer.prototype.handleUpdateRadioPassings = function (data) {
            var _this = this;
            if (data != null && data.status == "OK") {
                if (data.passings != null) {

                    if (this.currentTable != null) {
                        this.currentTable.fnClearTable();
                        this.currentTable.fnAddData(data.passings, true);
                    }
                    else
                    {
					var columns = Array();
                    var col = 0;
					columns.push({ "sTitle": "Tidsp." , "sClass": "right" , "bSortable": false, "aTargets": [col++], "mDataProp": "passtime"});
                    columns.push({ "sTitle": "Navn", "sClass": "left", "bSortable": false, "aTargets": [col++], "mDataProp": "runnerName", 
							"fnRender": function (o) 
							{
								var link = "<a style=\"color:inherit; text-decoration:none\" href=\"https://freidig.idrett.no/o/liveres_helpers/meld.php?" +
								"lopid="  + "(" + _this.competitionId +") " + o.aData.compName +
								"&Tidsp=" + o.aData.passtime + 
								"&Navn="  + o.aData.runnerName + 
								"&T0="    + o.aData.club + 
								"&T1="    + o.aData.class +
								"&T2="    + o.aData.controlName + 
								"&T3="    + o.aData.time + "\">" + 
								o.aData.runnerName + "</a>";
								return link;
							}
						     });
                    columns.push({ "sTitle": "Klubb", "sClass": "left", "bSortable": false, "aTargets": [col++], "mDataProp": "club" });
                    columns.push({ "sTitle": "Klasse", "sClass": "left", "bSortable": false, "aTargets": [col++], "mDataProp": "class" });
                    columns.push({ "sTitle": "Post", "sClass": "left", "bSortable": false, "aTargets": [col++], "mDataProp": "controlName"}); 
                    columns.push({ "sTitle": "Tid", "sClass": "right", "bSortable": false, "aTargets": [col++], "mDataProp": "time"});
                    
                    this.currentTable = $('#' + this.radioPassingsDiv).dataTable({
                        "bPaginate": false,
                        "bLengthChange": false,
                        "bFilter": false,
                        "bSort": false,
                        "bInfo": false,
                        "bAutoWidth": false,
                        "aaData": data.passings,
                        "aaSorting": [[1, "desc"]],
                        "aoColumnDefs": columns,
                        "bDestroy": true
                        });

                    }
                    this.lastRadioPassingsUpdateHash = data.hash;
                }
            }
        };

       //Check for updating of class results 
        AjaxViewer.prototype.checkForClassUpdate = function () {
            var _this = this;
            if (this.updateAutomatically) {
                if (this.currentTable != null) {
                    $.ajax({
                        url: this.apiURL,
                        data: "comp=" + this.competitionId + "&method=getclassresults&unformattedTimes=true&class=" + encodeURIComponent(this.curClassName) + "&last_hash=" + this.lastClassHash + (this.isMultiDayEvent ? "&includetotal=true" : ""),
                        success: function (data, status, resp) {
                            try {
                                var reqTime = resp.getResponseHeader("date");
                                if (reqTime) {
                                    // var d = new Date(reqTime);
                                    // d.setTime(d.getTime() + (120 + d.getTimezoneOffset()) * 60 * 1000);
                                    _this.serverTimeDiff = new Date().getTime() - new Date(reqTime).getTime();
                                }
                            }
                            catch (e) {
                            }
                            _this.handleUpdateClassResults(data);
                        },
                        error: function () {
                            _this.resUpdateTimeout = setTimeout(function () {
                                _this.checkForClassUpdate();
                            }, _this.updateInterval);
                        },
                        dataType: "json"
                    });
                    if (typeof (ga) == "function") {
                        ga('send', 'pageview', {
                            page: '/' + this.competitionId + '/' + this.curClassName
                        });
                    }
                }
            }
        };
        //handle response from class-results-update
        AjaxViewer.prototype.handleUpdateClassResults = function (data) {
            var _this = this;
            if (data.status == "OK") {
                if (this.currentTable != null) {
					this.updateClassSplitsBest(data);
                    this.updateResultVirtualPosition(data.results);
                    this.currentTable.fnClearTable();
                    this.currentTable.fnAddData(data.results, true);
					this.updatePredictedTimes();
                    this.lastClassHash = data.hash;

                }
            }
            this.resUpdateTimeout = setTimeout(function () {
                _this.checkForClassUpdate();
            }, this.updateInterval);
        };
        //Check for update in clubresults
        AjaxViewer.prototype.checkForClubUpdate = function () {
            var _this = this;
            if (this.updateAutomatically) {
                if (this.currentTable != null) {
                    $.ajax({
                        url: this.apiURL,
                        data: "comp=" + this.competitionId + "&method=getclubresults&unformattedTimes=true&club=" + encodeURIComponent(this.curClubName) + "&last_hash=" + this.lastClubHash + (this.isMultiDayEvent ? "&includetotal=true" : ""),
                        success: function (data) {
                            _this.handleUpdateClubResults(data);
                        },
                        error: function () {
                            _this.resUpdateTimeout = setTimeout(function () {
                                _this.checkForClubUpdate();
                            }, _this.updateInterval);
                        },
                        dataType: "json"
                    });
                    if (typeof (ga) == "function") {
                        ga('send', 'pageview', {
                            page: '/' + this.competitionId + '/' + this.curClubName
                        });
                    }
                }
            }
        };
        //handle the response on club-results update
        AjaxViewer.prototype.handleUpdateClubResults = function (data) {
            var _this = this;
			clearTimeout(this.resUpdateTimeout);
            if (data.status == "OK") {
                if (this.currentTable != null) {
                    this.currentTable.fnClearTable();
                    if (data && data.results) {
                        $.each(data.results, function (idx, res) {
                            res.placeSortable = res.place;
                            if (res.place == "-")
                                res.placeSortable = 999999;
                            if (res.place == "")
                                res.placeSortable = 9999;
							if (res.place == "F")
                                res.placeSortable = 0;
                        });
                    }
                    this.currentTable.fnAddData(data.results, true);
                    this.lastClubHash = data.hash;
                }
            }
            this.resUpdateTimeout = setTimeout(function () {
                _this.checkForClubUpdate();
            }, this.updateInterval);
        };
        AjaxViewer.prototype.chooseClass = function (className) {
             if (className.length == 0)
				return;
			var _this = this;
            if (this.currentTable != null) {
                try {
                    this.currentTable.fnDestroy();
                }
                catch (e) {
                }
            }
            clearTimeout(this.resUpdateTimeout);
            $('#divResults').html('');
            this.curClassName = className;
            this.curClubName = null; 
            $('#resultsHeader').html(this.resources["_LOADINGRESULTS"]);
            $.ajax({
                url: this.apiURL,
                data: "comp=" + this.competitionId + "&method=getclassresults&unformattedTimes=true&class=" + encodeURIComponent(className) + (this.isMultiDayEvent ? "&includetotal=true" : ""),
                success: function (data, status, resp) {
                    try {
                        var reqTime = resp.getResponseHeader("date");
                        if (reqTime) {
                            //var d = new Date(reqTime);
                            //d.setTime(d.getTime() + (120 + d.getTimezoneOffset()) * 60 * 1000);
                            _this.serverTimeDiff = new Date().getTime() - new Date(reqTime).getTime();
                        }
                    }
                    catch (e) {
                    }
                    _this.updateClassResults(data);
			_this.updatePredictedTimes();
                },
                dataType: "json"
            });
            if (typeof (ga) == "function") {
                ga('send', 'pageview', {
                    page: '/' + this.competitionId + '/' + this.curClassName
                });
            }
            if (!this.isSingleClass) {
                window.location.hash = className;
            }
            this.resUpdateTimeout = setTimeout(function () {
                _this.checkForClassUpdate();
            }, this.updateInterval);
        };
        AjaxViewer.prototype.updateClassResults = function (data) {
            var _this = this;
            if (data != null && data.status == "OK") {
                if (data.className != null) {
                    $('#' + this.resultsHeaderDiv).html('<b>' + data.className + '</b>');
                    $('#' + this.resultsControlsDiv).show();
                }
                $('#' + this.txtResetSorting).html("");
                if (data.results != null) {
                    //if (data.ShowTenthOfSecond)
                    //    this.showTenthOfSecond = data.ShowTenthOfSecond;
				    this.updateClassSplitsBest(data);
                    var columns = Array();
                    var col = 0;
                    var i;
                    this.curClassSplits = data.splitcontrols;
					fullView = !this.compactView;
					var unranked = false;
                    for (var sp = this.curClassSplits.length - 1; sp >= 0; sp--) {
                        if (this.curClassSplits[sp].code == "-999") {// Indication of unranked class
                            {
                                unranked = true;
                                break;
                            }
                        }
                    }
                    var haveSplitControls = (data.splitcontrols != null) && (data.splitcontrols.length > 0);
                    var relay = (haveSplitControls && (this.curClassSplits[0].code == "0" || data.className.slice(-4) == "-All"))					

                    columns.push({
                        "sTitle": "#",
						"sClass": "right",
                        "bSortable": false,
                        "aTargets": [col++],
                        "mDataProp": "place"
                    });
                    if (!haveSplitControls || unranked || !fullView)
                        columns.push({
                            "sTitle": this.resources["_NAME"],
                            "sClass": "left",
                            "bSortable": false,
                            "aTargets": [col++],
                            "mDataProp": "name"
                        });
						
					
					columns.push({
                        "sTitle": (haveSplitControls && !unranked && fullView) ? this.resources["_NAME"] + " / " + this.resources["_CLUB"] : this.resources["_CLUB"],
                        "sClass": "left",
                        "bSortable": false,
                        "aTargets": [col++],
                        "mDataProp": "club",
                        "fnRender": function (o) {
                            var param = o.aData.club;
							var clubShort = o.aData.club;
                            if (param && param.length > 0){
                                param = param.replace('\'', '\\\'');
								clubShort = clubShort.replace('Orienterings', 'O.');
								clubShort = clubShort.replace('Orientering', 'O.');
								clubShort = clubShort.replace('Orienteering', 'O.');
								clubShort = clubShort.replace('Orienteer', 'O.');
								clubShort = clubShort.replace('Skiklubb', 'Sk.');
								clubShort = clubShort.replace('og Omegn IF', 'OIF');
								clubShort = clubShort.replace('og omegn', '');
								clubShort = clubShort.replace(/national team/i,'NT');
								clubShort = clubShort.replace('Sportklubb', 'Spk.');
								clubShort = clubShort.replace('Sportsklubb', 'Spk.');
								}
                    
                            var link = "<a href=\"javascript:LiveResults.Instance.viewClubResults('" + param + "')\">" + clubShort + "</a>";
                            if ((haveSplitControls && !unranked && fullView))
                                return o.aData.name + "<br/>" + link;
                            else
                                return link;
                        }
                    });
                    
                    this.updateResultVirtualPosition(data.results);
                    columns.push({
                        "sTitle": this.resources["_START"],
                        "sClass": "right",
                        "sType": "numeric",
                        "aDataSort": [col],
                        "aTargets": [col],
                        "bUseRendered": false,
                        "mDataProp": "start",
                        "fnRender": function (o) {
                            if (o.aData.start == "")
                                return "";
                            else
                            {
                                var txt = "";
                                if (o.aData.splits != undefined && o.aData.splits["0_place"] >= 1)
								{
									if (o.aData.splits["0_place"] == 1)
										txt += "<span class=\"besttime\">" +_this.formatTime(o.aData.start, 0, false, true)+ " (1)<\span>";
									else if (o.aData.splits["0_place"] > 1)
										txt += _this.formatTime(o.aData.start, 0, false, true) + " (" + o.aData.splits["0_place"] + ")";
									else
										txt += _this.formatTime(o.aData.start, 0, false, true); 
                                    if (fullView)
                                    {
                                        if (o.aData.splits["0_place"] == 1)
                                            txt += "<br /><span class=\"besttime\">+";
                                        else
                                            txt += "<br /><span>+";
                                        txt += _this.formatTime(o.aData.splits["0_timeplus"], 0, _this.showTenthOfSecond) + "</span>";
                                    }
								}
								else
									txt += _this.formatTime(o.aData.start, 0, false, true) 
                                return txt;
                            }
                        }
                    });
                    col++;
                    if (data.splitcontrols != null)
                    {
                        $.each(data.splitcontrols, function (key, value)
                        {
						if (value.code != 0 && value.code != 999 && !(relay && value.code>100000)) // Code = 0 for exchange, 999 for leg time, 100000+ for leg passing
                            {
                                columns.push(
                                    {
                                        "sTitle": value.name,
                                        "sClass": "right",
                                        "sType": "numeric",
                                        "aDataSort": [col + 1, col],
                                        "aTargets": [col],
                                        "bUseRendered": false,
                                        "mDataProp": "splits." + value.code,
                                        "fnRender": function (o)
                                        {
                                            if (!o.aData.splits[value.code + "_place"])
                                                return "";
                                            else
                                            {
                                                var txt = "";
												// First line
                                                if ((!fullView || relay) && (o.aData.splits[value.code + "_place"] != 1) && !unranked && (value.code > 0)) 
											    // Compact view or relay view, all but first place
                                                {
                                                    txt += "<div class=\"tooltip\">+" + _this.formatTime(o.aData.splits[value.code + "_timeplus"], 0, _this.showTenthOfSecond) + " (" + o.aData.splits[value.code + "_place"] + ")<span class=\"tooltiptext\">" + _this.formatTime(o.aData.splits[value.code], 0, _this.showTenthOfSecond) + "</span></div>";
                                                }
                                                else 
											    // Ordinary passing or first place at passing for relay (drop place if code is negative - unranked)
                                                {
													if (o.aData.splits[value.code + "_place"] == 1)
														txt += "<span class=\"besttime\">";
													else
														txt += "<span>";
                                                    txt += _this.formatTime(o.aData.splits[value.code], 0, _this.showTenthOfSecond);
                                                    if (value.code > 0) 
                                                        txt += " (" + o.aData.splits[value.code + "_place"] + ")</span>";
                                                }
												// Second line
                                                if (fullView && relay && (o.aData.splits[(value.code + 100000) + "_timeplus"] != undefined))
                                                // Relay control second line with leg time to passing 
                                                {
                                                    txt += "<br/><span class="
                                                    if (o.aData.splits[(value.code + 100000) + "_place"] == 1)
                                                        txt += "\"besttime\">";
                                                    else
                                                        txt += "\"legtime\">";
                                                    txt += _this.formatTime(o.aData.splits[(value.code + 100000)], 0, _this.showTenthOfSecond)
                                                        + " (" + o.aData.splits[(value.code + 100000) + "_place"] + ")</span>";
                                                }
                                                else if ((o.aData.splits[value.code + "_timeplus"] != undefined) && fullView && !relay && (value.code > 0))
												// Second line for ordinary passing (drop if code is negative - unranked)
                                                {
                                                    if (o.aData.splits[value.code + "_timeplus"] == 0)
                                                        txt += "<br/><span class=\"besttime\">+";
                                                    else
                                                        txt += "<br/><span class=\"plustime\">+"
                                                    txt += _this.formatTime(o.aData.splits[value.code + "_timeplus"], 0, _this.showTenthOfSecond) + "</span>";
                                                }
                                            }
                                            return txt;
                                            
                                        }
                                    });
                                col++;
                            }
							columns.push({ "sTitle": value.name + "_Status", "bVisible": false, "aTargets": [col++], "sType": "numeric", "mDataProp": "splits." + value.code + "_status" });
                        });
                    }
                    var timecol = col;
                    columns.push({
                        "sTitle": this.resources["_CONTROLFINISH"],
                        "sClass": "right",
                        "sType": "numeric",
                        "aDataSort": [col + 1, col, 0],
                        "aTargets": [col],
                        "bUseRendered": false,
                        "mDataProp": "result",
                        "fnRender": function (o) {
                            var res = "";
                            if (o.aData.place == "-" || o.aData.place == "" || o.aData.place == "F")
                                res += _this.formatTime(o.aData.result, o.aData.status, _this.showTenthOfSecond);
                            else 
							{
                                if (haveSplitControls && (o.aData.place == 1))
                                    res += "<span class=\"besttime\">";
								else
									res += "<span>";
                                res += _this.formatTime(o.aData.result, o.aData.status, _this.showTenthOfSecond) + " (" + o.aData.place + ")<\span>";
                                
                                if (haveSplitControls && fullView && !(relay) && (o.aData.status == 0))
								{
                                    if (o.aData.timeplus == 0)
                                        res += "<br/><span class=\"besttime\">+";
                                    else
                                        res += "<br/><span class=\"plustime\">+";
                                    res += _this.formatTime(o.aData.timeplus, o.aData.status, _this.showTenthOfSecond) + "</span>";
								}
							}
							if (haveSplitControls && fullView && relay && (o.aData.splits["999_place"] != undefined))
							{
								if (o.aData.splits[(999)]>0)
								{
								res += "<br/><span class=";
								if (o.aData.splits["999_place"] == 1)
									res += "\"besttime\">";
								else
									res += "\"legtime\">";
								res += _this.formatTime(o.aData.splits[(999)], 0, _this.showTenthOfSecond)
                                    + " (" + o.aData.splits["999_place"] + ")";
								}
							}
                            return res;
						}
					});
                    col++;
                    columns.push({ "sTitle": "Status", "bVisible": false, "aTargets": [col++], "sType": "numeric", "mDataProp": "status" });
                    if (!haveSplitControls || !fullView) {
                        columns.push({
                            "sTitle": "",
                            "sClass": "right",
                            "bSortable": false,
                            "aTargets": [col++],
                            "mDataProp": "timeplus",
                            "fnRender": function (o) {
                                var res = "";
                                if (o.aData.status == 0)
                                {
                                    if (haveSplitControls && (o.aData.timeplus == 0))
                                        res += "<span class=\"besttime\">+";
                                    else
                                        res += "<span class=\"plustime\">+";
                                    res += _this.formatTime(o.aData.timeplus, o.aData.status, _this.showTenthOfSecond) + "</span>";
                                }
                                return res;
                            }
                        });
                    }
					else
						if (relay && fullView){
							columns.push({
                            "sTitle": "Tot<br/><span class=\"legtime\">Etp</span>",
                            "sClass": "right",
                            "bSortable": false,
                            "aTargets": [col++],
                            "mDataProp": "timeplus",
                            "fnRender": function (o) {
                                var res = "";
                                if (o.aData.status == 0)
                                {
                                    if (o.aData.timeplus == 0)
                                        res += "<span class=\"besttime\">+";
                                    else
                                        res += "<span>+";
                                    res += _this.formatTime(o.aData.timeplus, o.aData.status, _this.showTenthOfSecond) + "</span><br />";
                                    if (o.aData.splits["999_timeplus"] == 0)
                                        res += "<span class=\"besttime\">+";
                                    else
                                        res += "<span class=\"legtime\">+";
                                    res += _this.formatTime(o.aData.splits["999_timeplus"], 0, _this.showTenthOfSecond) + "</span>";

                                }
                                return res;
                            }
                        });
						}
							
                    if (this.isMultiDayEvent) {
                        columns.push({
                            "sTitle": this.resources["_TOTAL"],
                            "sClass": "right",
                            "sType": "numeric",
                            "aDataSort": [col + 1, col, 0],
                            "aTargets": [col],
                            "bUseRendered": false,
                            "mDataProp": "totalresult",
                            "fnRender": function (o) {
                                if (o.aData.totalplace == "-" || o.aData.totalplace == "") {
                                    return _this.formatTime(o.aData.totalresult, o.aData.totalstatus);
                                }
                                else {
                                    var txt = _this.formatTime(o.aData.totalresult, o.aData.totalstatus) +
                                        " (" +
                                        o.aData.totalplace +
                                        ")";
                                    if (haveSplitControls) {
                                        txt += "<br/><span class=\"plustime\">+" +
                                            _this.formatTime(o.aData.totalplus, o.aData.totalstatus) +
                                            "</span>";
                                    }
                                    return txt;
                                }
                            }
                        });
                        col++;
                        columns.push({ "sTitle": "TotalStatus", "bVisible": false, "aTargets": [col++], "sType": "numeric", "mDataProp": "totalstatus" });
                        if (!haveSplitControls) {
                            columns.push({
                                "sTitle": "",
                                "sClass": "right",
                                "bSortable": false,
                                "aTargets": [col++],
                                "mDataProp": "totalplus",
                                "fnRender": function (o) {
                                    if (o.aData.totalstatus != 0)
                                        return "";
                                    else
                                        return "+" + _this.formatTime(o.aData.totalplus, o.aData.totalstatus);
                                }
                            });
                        }
                    }
                    columns.push({ "sTitle": "VP", "bVisible": false, "aTargets": [col++], "mDataProp": "virtual_position" });
                    this.currentTable = $('#' + this.resultsDiv).dataTable({
                        "bPaginate": false,
                        "bLengthChange": false,
                        "bFilter": false,
                        "bSort": true,
                        "bInfo": false,
                        "bAutoWidth": false,
                        "aaData": data.results,
                        "aaSorting": [[col - 1, "asc"]],
                        "aoColumnDefs": columns,
                        "fnPreDrawCallback": function (oSettings) {
                            if (oSettings.aaSorting[0][0] != col - 1) {
                                $("#" + _this.txtResetSorting).html("&nbsp;&nbsp;<a href=\"javascript:LiveResults.Instance.resetSorting()\"><img class=\"eR\" style=\"vertical-align: middle\" src=\"images/cleardot.gif\" border=\"0\"/> " + _this.resources["_RESETTODEFAULT"] + "</a>");
                            }
                        },
                        "fnRowCallback": function (nRow, aData, iDisplayIndex, iDisplayIndexFull) {
                            if (aData)
                                aData.curDrawIndex = iDisplayIndex;
                        },
                        "bDestroy": true
                    });
                    this.lastClassHash = data.hash;
                }
            }
        };
        AjaxViewer.prototype.setAutomaticUpdate = function (val) {
            this.updateAutomatically = val;
            if (this.updateAutomatically) {
                $("#" + this.setAutomaticUpdateText).html("<b>" + this.resources["_AUTOUPDATE"] + ":</b> " + this.resources["_ON"] + " | <a href=\"javascript:LiveResults.Instance.setAutomaticUpdate(false);\">" + this.resources["_OFF"] + "</a>");
                this.checkForClassUpdate();
                this.updateLastPassings();
                this.checkForClassUpdate();
            }
            else {
                clearTimeout(this.resUpdateTimeout);
                clearTimeout(this.passingsUpdateTimer);
                clearTimeout(this.classUpdateTimer);
                $("#" + this.setAutomaticUpdateText).html("<b>" + this.resources["_AUTOUPDATE"] + ":</b> <a href=\"javascript:LiveResults.Instance.setAutomaticUpdate(true);\">" + this.resources["_ON"] + "</a> | " + this.resources["_OFF"] + "");
                this.serverTimeDiff = null;
                if (this.currentTable) {
                    $.each(this.currentTable.fnGetNodes(), function (idx, obj) {
                        for (var i = 4; i < obj.childNodes.length; i++) {
                            var innerHtml = obj.childNodes[i].innerHTML;
                            if (innerHtml.indexOf("<i>(") >= 0) {
                                obj.childNodes[i].innerHTML = "<td class=\"left\"></td>";
                            }
                        }
                    });
                }
            }
        };
		AjaxViewer.prototype.setCompactView = function (val) {
            this.compactView = val;
            if (this.compactView) {
                $("#" + this.setCompactViewText).html("<b>Compact view:</b> " + this.resources["_ON"] + " | <a href=\"javascript:LiveResults.Instance.setCompactView(false);\">" + this.resources["_OFF"] + "</a>");
            }
            else {
                $("#" + this.setCompactViewText).html("<b>Compact view:</b> <a href=\"javascript:LiveResults.Instance.setCompactView(true);\">" + this.resources["_ON"] + "</a> | " + this.resources["_OFF"] + "");
            }
			if (this.curClassName!=null)
			{
				clearTimeout(this.resUpdateTimeout);
				this.chooseClass(this.curClassName);
			}
        };

        AjaxViewer.prototype.formatTime = function (time, status, showTenthOs, showHours, padZeros) {
            if (arguments.length == 2 || arguments.length == 3) {
                if (this.language == 'fi' || this.language == 'no') {
                    showHours = true;
                    padZeros = false;
                }
                else {
                    showHours = false;
                    padZeros = true;
                }
            }
            else if (arguments.length == 4) {
                if (this.language == 'fi' || this.language == 'no') {
                    padZeros = false;
                }
                else {
                    padZeros = true;
                }
            }
            if (status != 0) {
                return this.runnerStatus[status];
            }
            else if (time < 0)
                return "*"
            else {
                var minutes;
                var seconds;
                var tenth;
                if (showHours) {
                    var hours = Math.floor(time / 360000);
                    minutes = Math.floor((time - hours * 360000) / 6000);
                    seconds = Math.floor((time - minutes * 6000 - hours * 360000) / 100);
                    tenth = Math.floor((time - minutes * 6000 - hours * 360000 - seconds * 100) / 10);
                    if (hours > 0) {
                        if (padZeros)
                            hours = this.strPad(hours, 2);
                        return hours + ":" + this.strPad(minutes, 2) + ":" + this.strPad(seconds, 2) + (showTenthOs ? "." + tenth : "");
                    }
                    else {
                        if (padZeros)
                            minutes = this.strPad(minutes, 2);
                        return minutes + ":" + this.strPad(seconds, 2) + (showTenthOs ? "." + tenth : "");
                    }
                }
                else {
                    minutes = Math.floor(time / 6000);
                    seconds = Math.floor((time - minutes * 6000) / 100);
                    tenth = Math.floor((time - minutes * 6000 - seconds * 100) / 10);
                    if (padZeros) {
                        return this.strPad(minutes, 2) + ":" + this.strPad(seconds, 2) + (showTenthOs ? "." + tenth : "");
                    }
                    else {
                        return minutes + ":" + this.strPad(seconds, 2) + (showTenthOs ? "." + tenth : "");
                    }
                }
            }
        };
        AjaxViewer.prototype.strPad = function (num, length) {
            var str = '' + num;
            while (str.length < length) {
                str = '0' + str;
            }
            return str;
        };
        AjaxViewer.prototype.updateResultVirtualPosition = function (data) {
            var _this = this;
            var i;
            data.sort(this.resultSorter);
            /* move down runners that have not finished to the correct place*/
            var firstFinishedIdx = -1;
            for (i = 0; i < data.length; i++) {
                if (data[i].place != "") {
					firstFinishedIdx = i;
                    break;
                }
            }
            if (firstFinishedIdx == -1)
                firstFinishedIdx = data.length;
            if (this.curClassIsMassStart) {
                /*append results from splits backwards (by place on actual split)*/
                data.sort(function (a, b) { return _this.sortByDistAndSplitPlace(a, b); });
                /*for (i = 0; i < tmp.length; i++) {
                    data.push(tmp[i]);
                }*/
            }
            else {
                var tmp = Array();
                for (i = 0; i < firstFinishedIdx; i++) {
                    tmp.push(data[i]);
                }
                data.splice(0, firstFinishedIdx);
                //advanced virtual-sorting for individual races
                tmp.sort(this.sortByDist);
                for (i = 0; i < tmp.length; i++) {
                    if (data.length == 0)
                        data.push(tmp[i]);
                    else
                        this.insertIntoResults(tmp[i], data);
                }
            }
            for (i = 0; i < data.length; i++) {
                data[i].virtual_position = i;
            }
        };
        //Sorts results by the one that have run longest on the course
        AjaxViewer.prototype.sortByDist = function (a, b) {
            return b.progress - a.progress;
        };
        //Sorts results by the one that have run longest on the course, and if they are on the same split, place on that split
        //"MassStart-Sorting"
        AjaxViewer.prototype.sortByDistAndSplitPlace = function (a, b) {
            var sortStatusA = a.status;
            var sortStatusB = b.status;
            if (sortStatusA == 9 || sortStatusA == 10 || sortStatusA == 13 )
                sortStatusA = 0;
            if (sortStatusB == 9 || sortStatusB == 10 || sortStatusB == 13 )
                sortStatusB = 0;
            if (sortStatusA != sortStatusB) {
				if (sortStatusA == 0)
					return -1
				else if (sortStatusB == 0)
					return 1
				else
					return sortStatusB - sortStatusA;
			}
            if (a.progress == 100 && b.progress == 100)
                return a.result - b.result;
            if (a.progress == 0 && b.progress == 0) {
                if (a.start && !b.start)
                    return -1;
                if (!a.start && b.start)
                    return 1;
				if (a.start == b.start)
					return parseInt(a.name.replace("(",""),10) - parseInt(b.name.replace("(",""),10); 
                else
                    return a.start - b.start;
            }
            if (a.progress == b.progress && a.progress > 0 && a.progress < 100) {
                //Both have reached the same split
                if (this.curClassSplits != null) {
                    for (var s = this.curClassSplits.length - 1; s >= 0; s--) {
                        var splitCode = this.curClassSplits[s].code;
                        if (a.splits[splitCode] != "") {
                            return a.splits[splitCode + "_place"] - b.splits[splitCode + "_place"];
                        }
                    }
                }
            }
            return b.progress - a.progress;
        };
        //Inserts a result in the array of results.
        //The result to be inserted is assumed to have same or worse progress than all other results already in the array
        AjaxViewer.prototype.insertIntoResults = function (result, data) {
            var d;
            if (this.curClassSplits != null) {
                for (var s = this.curClassSplits.length - 1; s >= 0; s--) {
                    var splitCode = this.curClassSplits[s].code;
                    if (result.splits[splitCode] != "") {
                        var numOthersAtSplit = 0;
                        for (d = 0; d < data.length; d++) {
                            if (data[d].splits[splitCode] != "") {
                                numOthersAtSplit++;
                            }
                            // insert result 
                            // * before results with - as placemark
                            // * before the first result with worse time at this split 
                            if (data[d].place == "-" || (data[d].splits[splitCode] != "" && data[d].splits[splitCode] > result.splits[splitCode])) {
                                data.splice(d, 0, result);
                                return;
                            }
                        }
                        //If numothersatsplit there exists results at this split, but all are better than this one..
                        //Append last
                        if (numOthersAtSplit > 0) {
                            data.push(result);
                            return;
                        }
                    }
                }
            }
            if (result.start != "") {
                for (d = 0; d < data.length; d++) {
                    if (data[d].place == "-") {
                        data.splice(d, 0, result);
                        return;
                    }
                    if (result.place == "" && data[d].place != "") {
                    }
                    else if (data[d].start != "" && data[d].start > result.start && result.progress == 0 && data[d].progress == 0) {
                        data.splice(d, 0, result);
                        return;
                    }
                }
            }
            data.push(result);
        };
        AjaxViewer.prototype.resultSorter = function (a, b) {
            var aStatus = a.status;
			var bStatus = b.status;
			if (a.place == "F") {
				aStatus = 0;
            }
            else if (b.place == "F") {
                bStatus = 0;
            }
            if (a.place != "" && b.place != "") {
                if (aStatus != bStatus)
					if(aStatus == 0)
						return -1
					else if (bStatus == 0)
						return 1
					else
						return bStatus - aStatus;
				else {
                    if (a.result == b.result) {
                        if (a.place == "=" && b.place != "=") {
                            return 1;
                        }
                        else if (b.place == "=" && a.place != "=") {
                            return -1;
                        }                 
                        else {
                            return 0;
                        }
                    }
                    else {
                        return a.result - b.result;
                    }
                }
            }
            else if (a.place == "-" || a.place != "") {
                return 1;
            }
            else if (b.place == "-" || b.place != "") {
                return -1;
            }
            else {
                return 0;
            }
        };
        AjaxViewer.prototype.newWin = function () {
            var url = 'followfull.php?comp=' + this.competitionId + '&lang=' + this.language;
            if (this.curClassName != null) {
                url += '&class=' + encodeURIComponent(this.curClassName);
				if (!this.compactView)
					url += '&fullView';
				}
            else
                url += '&club=' + encodeURIComponent(this.curClubName);
            window.open(url, '', 'status=0,toolbar=0,location=0,menubar=0,directories=0,scrollbars=1,resizable=1,width=900,height=600');
        };
        AjaxViewer.prototype.viewClubResults = function (clubName) {
            var _this = this;
            if (this.currentTable != null) {
                try {
                    this.currentTable.fnDestroy();
                }
                catch (e) {
                }
            }
            clearTimeout(this.resUpdateTimeout);
            $('#divResults').html('');
            $('#' + this.txtResetSorting).html('');
            this.curClubName = clubName;
            this.curClassName = null;
            $('#resultsHeader').html(this.resources["_LOADINGRESULTS"]);
            $.ajax({
                url: this.apiURL,
                data: "comp=" + this.competitionId + "&method=getclubresults&unformattedTimes=true&club=" + encodeURIComponent(clubName) + (this.isMultiDayEvent ? "&includetotal=true" : ""),
                success: function (data) {
                    _this.updateClubResults(data);
                },
                dataType: "json"
            });
            if (typeof (ga) == "function") {
                ga('send', 'pageview', {
                    page: '/' + this.competitionId + '/' + this.curClubName
                });
            }
            if (!this.isSingleClass) {
                window.location.hash = "club::" + clubName;
            }
            this.resUpdateTimeout = setTimeout(function () {
                _this.checkForClubUpdate();
            }, this.updateInterval);
        };
        AjaxViewer.prototype.updateClubResults = function (data) {
            var _this = this;
            if (data != null && data.status == "OK") {
                if (data.clubName != null) {
                    $('#' + this.resultsHeaderDiv).html('<b>' + data.clubName + '</b>');
                    $('#' + this.resultsControlsDiv).show();
                }
                if (data.results != null) {
                    $.each(data.results, function (idx, res) {
                        res.placeSortable = res.place;
                        if (res.place == "-")
                            res.placeSortable = 999999;
                        if (res.place == "")
                            res.placeSortable = 9999;
						if (res.place == "F")
                            res.placeSortable = 0;
                    });
                    var columns = Array();
                    columns.push({ "sTitle": "#", "sClass": "right", "aTargets": [0], "mDataProp": "place" });
                    columns.push({ "sTitle": "placeSortable", "bVisible": false, "mDataProp": "placeSortable", "aTargets": [1] });
                    columns.push({ "sTitle": this.resources["_NAME"], "sClass": "left", "aTargets": [2], "mDataProp": "name" });
                    columns.push({ "sTitle": this.resources["_CLUB"], "sClass": "left", "bSortable": false, "aTargets": [3], "mDataProp": "club" });
                    columns.push({
                        "sTitle": this.resources["_CLASS"], "aTargets": [4], "mDataProp": "class",
                        "fnRender": function (o) {
                            var param = o.aData["class"];
                            if (param && param.length > 0)
                                param = param.replace('\'', '\\\'');
                            return "<a href=\"javascript:LiveResults.Instance.chooseClass('" + param + "')\">" + o.aData["class"] + "</a>";
                        }
                    });
                    var col = 5;
                    columns.push({
                        "sTitle": this.resources["_START"], "sClass": "right", "sType": "numeric", "aDataSort": [col], "aTargets": [col], "bUseRendered": false, "mDataProp": "start",
                        "fnRender": function (o) {
                            if (o.aData.start == "") {
                                return "";
                            }
                            else {
                                return _this.formatTime(o.aData.start, 0, false, true);
                            }
                        }
                    });
                    col++;
                    var timecol = col;
                    columns.push({
                        "sTitle": this.resources["_CONTROLFINISH"], "sClass": "right", "sType": "numeric", "aDataSort": [col + 1, col, 0], "aTargets": [col], "bUseRendered": false, "mDataProp": "result",
                        "fnRender": function (o) {
                            if (o.aData.place == "-" || o.aData.place == "" || o.aData.place == "F") {
                                return _this.formatTime(o.aData.result, o.aData.status);
                            }
                            else {
                                return _this.formatTime(o.aData.result, o.aData.status) + " (" + o.aData.place + ")";
                            }
                        }
                    });
                    col++;
                    columns.push({ "sTitle": "Status", "bVisible": false, "aTargets": [col++], "sType": "numeric", "mDataProp": "status" });
                    columns.push({
                        "sTitle": "", "sClass": "right", "bSortable": false, "aTargets": [col++], "mDataProp": "timeplus",
                        "fnRender": function (o) {
                            if (o.aData.status != 0)
                                return "";
                            else
                                return "+" + _this.formatTime(o.aData.timeplus, o.aData.status);
                        }
                    });
                    this.currentTable = $('#' + this.resultsDiv).dataTable({
                        "bPaginate": false,
                        "bLengthChange": false,
                        "bFilter": false,
                        "bSort": true,
                        "bInfo": false,
                        "bAutoWidth": false,
                        "aaData": data.results,
                        "aaSorting": [[1, "asc"]],
                        "aoColumnDefs": columns,
                        "fnPreDrawCallback": function (oSettings) {
                            if (oSettings.aaSorting[0][0] != 1) {
                                $("#" + _this.txtResetSorting).html("&nbsp;&nbsp;<a href=\"javascript:LiveResults.Instance.resetSorting()\"><img class=\"eR\" style=\"vertical-align: middle\" src=\"images/cleardot.gif\" border=\"0\"/> " + _this.resources["_RESETTODEFAULT"] + "</a>");
                            }
                        },
                        "bDestroy": true
                    });
                    this.lastClubHash = data.hash;
                }
            }
        };
        AjaxViewer.prototype.resetSorting = function () {
            var idxCol = 0;
            $.each(this.currentTable.fnSettings().aoColumns, function (idx, val) {
                if (val.sTitle == "VP") {
                    idxCol = idx;
                }
            });
            this.currentTable.fnSort([[idxCol, 'asc']]);
            $("#" + this.txtResetSorting).html("");
        };
        // ReSharper disable once InconsistentNaming
        AjaxViewer.VERSION = "2016-08-06-01";
        return AjaxViewer;
    }());
    LiveResults.AjaxViewer = AjaxViewer;
})(LiveResults || (LiveResults = {}));
Date.prototype.stdTimezoneOffset = function () {
    var jan = new Date(this.getFullYear(), 0, 1);
    var jul = new Date(this.getFullYear(), 6, 1);
    return Math.max(jan.getTimezoneOffset(), jul.getTimezoneOffset());
};
Date.prototype.dst = function () {
    return this.getTimezoneOffset() < this.stdTimezoneOffset();
};
//GA tracker-object
var ga;
