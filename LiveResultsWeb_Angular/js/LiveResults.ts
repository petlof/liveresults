﻿module LiveResults {

// ReSharper disable once InconsistentNaming
    export var Instance: AjaxViewer = null;

    export class AjaxViewer {
        // ReSharper disable once InconsistentNaming
        public static VERSION : string = "2016-08-06-01";
        private updateAutomatically: boolean = true;
        private updateInterval: number = 15000;
        private classUpdateInterval: number = 60000;

        private classUpdateTimer : any = null;
        private passingsUpdateTimer : any = null;
        private resUpdateTimeout : any = null;
        private updatePredictedTimeTimer : any = null;

        private lastClassListHash: string = "";
        private lastPassingsUpdateHash: string = "";

        private curClassName = "";
        private lastClassHash = "";
        private curClassSplits: any[] = null;
        private curClassIsMassStart = false;

        private curClubName = "";
        private lastClubHash = "";

        private currentTable : any = null;
        private serverTimeDiff : any = null;

        public eventTimeZoneDiff = 0;

        constructor(private competitionId: number, private language: string, private classesDiv: HTMLDivElement, private lastPassingsDiv: HTMLDivElement,
            private resultsHeaderDiv: HTMLDivElement, private resultsControlsDiv: HTMLDivElement, private resultsDiv: HTMLDivElement,
            private txtResetSorting: HTMLDivElement,
            private resources: any, private isMultiDayEvent: boolean, private isSingleClass: boolean, private setAutomaticUpdateText: HTMLDivElement,
            private runnerStatus: any, private showTenthOfSecond: boolean) {
            LiveResults.Instance = this;

            (<any>$(window)).hashchange(() => {
                if (window.location.hash) {
                    var hash = window.location.hash.substring(1);
                    var cl : string;
                    if (hash.indexOf('club::') >= 0) {
                        cl = decodeURIComponent(hash.substring(6));
                        if (cl != this.curClubName) {
                            LiveResults.Instance.viewClubResults(cl);
                        }
                    } else {
                        cl = decodeURIComponent(hash);
                        if (cl != this.curClassName) {
                            this.chooseClass(hash);
                        }
                    }
                }
            });

            (<any>$(window)).hashchange();

         
        }

        public startPredictionUpdate() {
            this.updatePredictedTimeTimer = setInterval(() => { this.updatePredictedTimes(); }, 1000);
        }

        //Detect if the browser is a mobile phone
        public mobilecheck() {
            var check = false;
            (function(a : string) { if (/(android|bb\d+|meego).+mobile|avantgo|bada\/|blackberry|blazer|compal|elaine|fennec|hiptop|iemobile|ip(hone|od)|iris|kindle|lge |maemo|midp|mmp|mobile.+firefox|netfront|opera m(ob|in)i|palm( os)?|phone|p(ixi|re)\/|plucker|pocket|psp|series(4|6)0|symbian|treo|up\.(browser|link)|vodafone|wap|windows (ce|phone)|xda|xiino/i.test(a) || /1207|6310|6590|3gso|4thp|50[1-6]i|770s|802s|a wa|abac|ac(er|oo|s\-)|ai(ko|rn)|al(av|ca|co)|amoi|an(ex|ny|yw)|aptu|ar(ch|go)|as(te|us)|attw|au(di|\-m|r |s )|avan|be(ck|ll|nq)|bi(lb|rd)|bl(ac|az)|br(e|v)w|bumb|bw\-(n|u)|c55\/|capi|ccwa|cdm\-|cell|chtm|cldc|cmd\-|co(mp|nd)|craw|da(it|ll|ng)|dbte|dc\-s|devi|dica|dmob|do(c|p)o|ds(12|\-d)|el(49|ai)|em(l2|ul)|er(ic|k0)|esl8|ez([4-7]0|os|wa|ze)|fetc|fly(\-|_)|g1 u|g560|gene|gf\-5|g\-mo|go(\.w|od)|gr(ad|un)|haie|hcit|hd\-(m|p|t)|hei\-|hi(pt|ta)|hp( i|ip)|hs\-c|ht(c(\-| |_|a|g|p|s|t)|tp)|hu(aw|tc)|i\-(20|go|ma)|i230|iac( |\-|\/)|ibro|idea|ig01|ikom|im1k|inno|ipaq|iris|ja(t|v)a|jbro|jemu|jigs|kddi|keji|kgt( |\/)|klon|kpt |kwc\-|kyo(c|k)|le(no|xi)|lg( g|\/(k|l|u)|50|54|\-[a-w])|libw|lynx|m1\-w|m3ga|m50\/|ma(te|ui|xo)|mc(01|21|ca)|m\-cr|me(rc|ri)|mi(o8|oa|ts)|mmef|mo(01|02|bi|de|do|t(\-| |o|v)|zz)|mt(50|p1|v )|mwbp|mywa|n10[0-2]|n20[2-3]|n30(0|2)|n50(0|2|5)|n7(0(0|1)|10)|ne((c|m)\-|on|tf|wf|wg|wt)|nok(6|i)|nzph|o2im|op(ti|wv)|oran|owg1|p800|pan(a|d|t)|pdxg|pg(13|\-([1-8]|c))|phil|pire|pl(ay|uc)|pn\-2|po(ck|rt|se)|prox|psio|pt\-g|qa\-a|qc(07|12|21|32|60|\-[2-7]|i\-)|qtek|r380|r600|raks|rim9|ro(ve|zo)|s55\/|sa(ge|ma|mm|ms|ny|va)|sc(01|h\-|oo|p\-)|sdk\/|se(c(\-|0|1)|47|mc|nd|ri)|sgh\-|shar|sie(\-|m)|sk\-0|sl(45|id)|sm(al|ar|b3|it|t5)|so(ft|ny)|sp(01|h\-|v\-|v )|sy(01|mb)|t2(18|50)|t6(00|10|18)|ta(gt|lk)|tcl\-|tdg\-|tel(i|m)|tim\-|t\-mo|to(pl|sh)|ts(70|m\-|m3|m5)|tx\-9|up(\.b|g1|si)|utst|v400|v750|veri|vi(rg|te)|vk(40|5[0-3]|\-v)|vm40|voda|vulc|vx(52|53|60|61|70|80|81|83|85|98)|w3c(\-| )|webc|whit|wi(g |nc|nw)|wmlb|wonu|x700|yas\-|your|zeto|zte\-/i.test(a.substr(0, 4))) check = true; })(navigator.userAgent || navigator.vendor || (<any>window).opera);
            return check;
        }

        ///Update the classlist
        public updateClassList() {
            if (this.updateAutomatically) {
                $.ajax({
                    url: "api.php",
                    data: "comp=" + this.competitionId + "&method=getclasses&last_hash=" + this.lastClassListHash,
                    success: (data) => {
                        this.handleUpdateClassListResponse(data);
                    },
                    error: () => { this.classUpdateTimer = setTimeout(
                    () => {
                        this.updateClassList();
                    }, this.classUpdateInterval); },
                    dataType: "json"
                });
            }
        }

        public handleUpdateClassListResponse(data : any) {
            if (data != null && data.status == "OK") {

                if (!data.classes || !$.isArray(data.classes) || data.classes.length == 0) {
                    $('#resultsHeader').html("<b>" + this.resources["_NOCLASSESYET"] + "</b>");
                }
                

                if (data.classes != null) {
                    var str = "<nowrap>";
                    $.each(data.classes,
                        (key, value) => {
                            var param = value.className;
                            if (param && param.length > 0)
                                param = param.replace('\'', '\\\'');
                            str += "<a href=\"javascript:LiveResults.Instance.chooseClass('" + param + "')\">" + value.className + "</a><br/>";
                        }
                    );
                    str += "</nowrap>";
                    $("#" + this.classesDiv).html(str);
                    this.lastClassListHash = data.hash;
                }
            }

            this.classUpdateTimer = setTimeout(() => {
                this.updateClassList();
            }, this.classUpdateInterval);
        }

        private updatePredictedTimes() {
            if (this.currentTable != null && this.curClassName != null && this.serverTimeDiff && this.updateAutomatically) {
                try {
                    var data = this.currentTable.fnGetData();
                    var dt = new Date();
                    var currentTimeZoneOffset = -1*new Date().getTimezoneOffset();
                    var eventZoneOffset = (((<any>dt).dst() ? 2 : 1) + this.eventTimeZoneDiff) * 60;
                    var timeZoneDiff = eventZoneOffset - currentTimeZoneOffset;
                    
                    var time = (dt.getSeconds() + (60 * dt.getMinutes()) + (60 * 60 * dt.getHours())) * 100 
                        - (this.serverTimeDiff / 10) + 
                        (timeZoneDiff * 6000);
                    for (var i = 0; i < data.length; i++) {
                        if ((data[i].status == 10 || data[i].status == 9) && data[i].place == "" && data[i].start != "") {
                            if (data[i].start < time) {
                                if (this.curClassSplits == null || this.curClassSplits.length == 0) {
                                    $("#" + this.resultsDiv + " tr:eq(" + (data[i].curDrawIndex + 1) + ") td:eq(4)").html("<i>(" + this.formatTime(time - data[i].start, 0, false) + ")</i>");
                                } else {

                                    //find next split to reach
                                    var nextSplit = 0;
                                    for (var sp = this.curClassSplits.length - 1; sp >= 0; sp--) {
                                        if (data[i].splits[this.curClassSplits[sp].code] != "") {
                                            {
                                                nextSplit = sp + 1;
                                                break;
                                            }

                                        }
                                    }

                                    $("#" + this.resultsDiv + " tr:eq(" + (data[i].curDrawIndex + 1) + ") td:eq(" + (3+nextSplit)+ ")").html("<i>(" + this.formatTime(time - data[i].start, 0, false) + ")</i>");

                                }
                            }
                        }
                    }
                } catch (e) {
                }
            }
        }

        //Set wether to display tenthofasecond in results
        public setShowTenth(val: boolean) {
            this.showTenthOfSecond = val;
        }

        //Request data for the last-passings div
        public updateLastPassings() {
            if (this.updateAutomatically) {
                $.ajax({
                    url: "api.php",
                    data: "comp=" + this.competitionId + "&method=getlastpassings&lang=" + this.language + "&last_hash=" + this.lastPassingsUpdateHash,
                    success: (data) => { this.handleUpdateLastPassings(data); },
                    error: () => { this.passingsUpdateTimer = setTimeout(() => {
                        this.updateLastPassings();
                    }, this.updateInterval); },
                    dataType: "json"
                });
            }

        }

        //Handle response for updating the last passings..
        private handleUpdateLastPassings(data : any) {
            if (data != null && data.status == "OK") {
                if (data.passings != null) {
                    var str = "";
                    $.each(data.passings,
                        (key, value) => {
                            var cl = value["class"];
                            if (cl && cl.length > 0)
                                cl = cl.replace('\'', '\\\'');
                            str += value.passtime + ": " + value.runnerName + " (<a href=\"javascript:LiveResults.Instance.chooseClass('" + cl + "')\">" + value["class"] + "</a>) " + (value.control == 1000 ? this.resources["_LASTPASSFINISHED"] : this.resources["_LASTPASSPASSED"] + " " + value["controlName"]) + " " + this.resources["_LASTPASSWITHTIME"] + " " + value["time"] + "<br/>";
                        }
                    );
                    $("#" + this.lastPassingsDiv).html(str);
                    this.lastPassingsUpdateHash = data.hash;
                }
            }

            this.passingsUpdateTimer = setTimeout(
            () => {
                this.updateLastPassings();
            }, this.updateInterval);
        }

        //Check for updateing of class results
        private checkForClassUpdate() {
            if (this.updateAutomatically) {
                if (this.currentTable != null) {
                    $.ajax({
                        url: "api.php",
                        data: "comp=" + this.competitionId + "&method=getclassresults&unformattedTimes=true&class=" + encodeURIComponent(this.curClassName) + "&last_hash=" + this.lastClassHash + (this.isMultiDayEvent ? "&includetotal=true" : ""),
                        success: (data, status, resp) => {

                            try {
                                var reqTime = resp.getResponseHeader("date");
                                if (reqTime) {
                                   // var d = new Date(reqTime);
                                   // d.setTime(d.getTime() + (120 + d.getTimezoneOffset()) * 60 * 1000);
                                    this.serverTimeDiff = new Date().getTime() - new Date(reqTime).getTime();
                                }
                            } catch (e) {
                            }

                            this.handleUpdateClassResults(data);
                        },
                        error: () => { this.resUpdateTimeout = setTimeout(
                        () => {
                            this.checkForClassUpdate();
                        }, this.updateInterval); },
                        dataType: "json"
                    });
                    if (typeof (ga) == "function") {
                        ga('send', 'pageview', {
                            page: '/' + this.competitionId + '/' + this.curClassName
                        });
                    }
                }
            }

        }

        //handle response from class-results-update
        private handleUpdateClassResults(data : any) {
            if (data.status == "OK") {
                if (this.currentTable != null) {
                    this.updateResultVirtualPosition(data.results);
                    this.currentTable.fnClearTable();
                    this.currentTable.fnAddData(data.results, true);
                    this.lastClassHash = data.hash;
                }
            }
            this.resUpdateTimeout = setTimeout(() => {
                this.checkForClassUpdate();
            }, this.updateInterval);
        }

        //Check for update in clubresults
        private checkForClubUpdate() {
            if (this.updateAutomatically) {
                if (this.currentTable != null) {
                    $.ajax({
                        url: "api.php",
                        data: "comp=" + this.competitionId + "&method=getclubresults&unformattedTimes=true&club=" + encodeURIComponent(this.curClubName) + "&last_hash=" + this.lastClubHash + (this.isMultiDayEvent ? "&includetotal=true" : ""),
                        success: (data) => {
                             this.handleUpdateClubResults(data); 
                        
                        },
                        error: () => { this.resUpdateTimeout = setTimeout(
                        () => {
                            this.checkForClubUpdate();
                        }, this.updateInterval); },
                        dataType: "json"
                    });

                    if (typeof (ga) == "function") {
                        ga('send', 'pageview', {
                            page: '/' + this.competitionId + '/' + this.curClubName
                        });
                    }
                }
            }

        }

        //handle the response on club-results update
        private handleUpdateClubResults(data : any) {
            if (data.status == "OK") {
                if (this.currentTable != null) {
                    this.currentTable.fnClearTable();

                    if (data && data.results) {
                        $.each(data.results, (idx: number, res: any) => {
                            res.placeSortable = res.place;
                            if (res.place == "-")
                                res.placeSortable = 999999;
                            if (res.place == "")
                                res.placeSortable = 9999;

                        });
                    }

                    this.currentTable.fnAddData(data.results, true);
                    this.lastClubHash = data.hash;
                }
            }
            this.resUpdateTimeout = setTimeout(() => {
                this.checkForClubUpdate();
            }, this.updateInterval);
        }

        public chooseClass(className : string) {
            if (this.currentTable != null) {
                try {
                    this.currentTable.fnDestroy();
                } catch (e) {

                }
            }


            clearTimeout(this.resUpdateTimeout);

            $('#divResults').html('');
            this.curClassName = className;
            this.curClubName = null;
            $('#resultsHeader').html(this.resources["_LOADINGRESULTS"]);
            $.ajax({
                url: "api.php",
                data: "comp=" + this.competitionId + "&method=getclassresults&unformattedTimes=true&class=" + encodeURIComponent(className) + (this.isMultiDayEvent ? "&includetotal=true" : ""),
                success: (data,status, resp) => {
                    try {
                        var reqTime = resp.getResponseHeader("date");
                        if (reqTime) {
                            //var d = new Date(reqTime);
                            //d.setTime(d.getTime() + (120 + d.getTimezoneOffset()) * 60 * 1000);
                            this.serverTimeDiff = new Date().getTime() - new Date(reqTime).getTime();
                        }
                    } catch (e) {
                    }
                    this.updateClassResults(data);
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
            this.resUpdateTimeout = setTimeout(() => {
                this.checkForClassUpdate();
            }, this.updateInterval);
        }

        private updateClassResults(data : any) {
            if (data != null && data.status == "OK") {
                if (data.className != null) {
                    $('#' + this.resultsHeaderDiv).html('<b>' + data.className + '</b>');
                    $('#' + this.resultsControlsDiv).show();
                }

                $('#' + this.txtResetSorting).html("");

                if (data.results != null) {
                    var columns = Array();
                    var col = 0;
                    this.curClassSplits = data.splitcontrols;
                    var haveSplitControls = data.splitcontrols != null && data.splitcontrols.length > 0;
                    columns.push({ "sTitle": "#", "bSortable": false, "aTargets": [col++], "mDataProp": "place" });
                    if (!haveSplitControls)
                        columns.push({ "sTitle": this.resources["_NAME"], "bSortable": false, "aTargets": [col++], "mDataProp": "name" });
                    columns.push({
                        "sTitle": haveSplitControls ? this.resources["_NAME"] + " / " + this.resources["_CLUB"] : this.resources["_CLUB"],
                        "bSortable": false,
                        "aTargets": [col++],
                        "mDataProp": "club",
                        "fnRender": (o : any) => {
                            var param = o.aData.club;
                            if (param && param.length > 0)
                                param = param.replace('\'', '\\\'');
                            
                            var link = "<a href=\"javascript:LiveResults.Instance.viewClubResults('" + param + "')\">" + o.aData.club + "</a>";
                            if (haveSplitControls)
                                return o.aData.name + "<br/>" + link;
                            else
                                return link;
                        }

                    });

                    
                    this.curClassIsMassStart = false;
                    if (data.IsMassStartRace)
                        this.curClassIsMassStart = data.IsMassStartRace;

                    this.updateResultVirtualPosition(data.results);

                    columns.push({
                        "sTitle": this.resources["_START"],
                        "sClass": "left",
                        "sType": "numeric",
                        "aDataSort": [col],
                        "aTargets": [col],
                        "bUseRendered": false,
                        "mDataProp": "start",
                        "fnRender": (o : any) => {
                            if (o.aData.start == "") {
                                return "";
                            } else {
                                return this.formatTime(o.aData.start, 0, false, true);
                            }
                        }
                    });

                    col++;

                    if (data.splitcontrols != null) {
                        $.each(data.splitcontrols,
                        (key, value) => {
                            columns.push({
                                "sTitle": value.name,
                                "sClass": "left",
                                "sType": "numeric",
                                "aDataSort": [col + 1, col],
                                "aTargets": [col],
                                "bUseRendered": false,
                                "mDataProp": "splits." + value.code,
                                "fnRender": (o : any) => {
                                    if (!o.aData.splits[value.code + "_place"])
                                        return "";
                                    else {
                                        
                                        var txt = this.formatTime(o.aData.splits[value.code], 0, this.showTenthOfSecond) +
                                            " (" +
                                            o.aData.splits[value.code + "_place"] +
                                            ")";
                                        if (o.aData.splits[value.code + "_timeplus"] != undefined) {
                                            txt += "<br/><span class=\"plustime\">+" + this.formatTime(o.aData.splits[value.code + "_timeplus"], 0, this.showTenthOfSecond) + "</span>";
                                        }
                                        return txt;
                                    }
                                }
                            });
                            col++;
                            columns.push({ "sTitle": value.name + "_Status", "bVisible": false, "aTargets": [col++], "sType": "numeric", "mDataProp": "splits." + value.code + "_status" });
                        });
                    }

                    var timecol = col;
                    columns.push({
                        "sTitle": this.resources["_CONTROLFINISH"],
                        "sClass": "left",
                        "sType": "numeric",
                        "aDataSort": [col + 1, col, 0],
                        "aTargets": [col],
                        "bUseRendered": false,
                        "mDataProp": "result",
                        "fnRender": (o  : any) => {
                            var res = "";
                            if (o.aData.place == "-" || o.aData.place == "") {
                                res = this.formatTime(o.aData.result, o.aData.status, this.showTenthOfSecond);
                            } else {
                                res = this.formatTime(o.aData.result, o.aData.status, this.showTenthOfSecond) + " (" + o.aData.place + ")";
                                if (haveSplitControls) {
                                    if (o.aData.status == 0)
                                        res += "<br/>" +
                                            "<span class=\"plustime\">+" +
                                            this.formatTime(o.aData.timeplus, o.aData.status, this.showTenthOfSecond) +
                                            "</span>";
                                }
                            }

                            return res;
                        }
                    });

                    col++;
                    columns.push({ "sTitle": "Status", "bVisible": false, "aTargets": [col++], "sType": "numeric", "mDataProp": "status" });
                    if (!haveSplitControls) {
                        columns.push({
                            "sTitle": "",
                            "sClass": "center",
                            "bSortable": false,
                            "aTargets": [col++],
                            "mDataProp": "timeplus",
                            "fnRender": (o: any) => {
                                if (o.aData.status != 0)
                                    return "";
                                else
                                    return "+" +
                                        this.formatTime(o.aData.timeplus, o.aData.status, this.showTenthOfSecond);
                            }
                        });
                    }

                    if (this.isMultiDayEvent) {

                        columns.push({
                            "sTitle": this.resources["_TOTAL"],
                            "sClass": "left",
                            "sType": "numeric",
                            "aDataSort": [col + 1, col, 0],
                            "aTargets": [col],
                            "bUseRendered": false,
                            "mDataProp": "totalresult",
                            "fnRender": (o: any) => {
                                if (o.aData.totalplace == "-" || o.aData.totalplace == "") {
                                    return this.formatTime(o.aData.totalresult, o.aData.totalstatus);
                                } else {
                                    var txt = this.formatTime(o.aData.totalresult, o.aData.totalstatus) +
                                        " (" +
                                        o.aData.totalplace +
                                        ")";

                                    if (haveSplitControls) {
                                        txt += "<br/><span class=\"plustime\">+" +
                                            this.formatTime(o.aData.totalplus, o.aData.totalstatus) +
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
                                "sClass": "center",
                                "bSortable": false,
                                "aTargets": [col++],
                                "mDataProp": "totalplus",
                                "fnRender": (o: any) => {
                                    if (o.aData.totalstatus != 0)
                                        return "";
                                    else
                                        return "+" + this.formatTime(o.aData.totalplus, o.aData.totalstatus);
                                }
                            });
                        }

                    }

                    columns.push({ "sTitle": "VP", "bVisible": false, "aTargets": [col++], "mDataProp": "virtual_position" });

                    this.currentTable = (<any>$('#' + this.resultsDiv)).dataTable({
                        "bPaginate": false,
                        "bLengthChange": false,
                        "bFilter": false,
                        "bSort": true,
                        "bInfo": false,
                        "bAutoWidth": false,
                        "aaData": data.results,
                        "aaSorting": [[col - 1, "asc"]],
                        "aoColumnDefs": columns,
                        "fnPreDrawCallback": (oSettings : any) => {
                            if (oSettings.aaSorting[0][0] != col - 1) {
                                $("#" + this.txtResetSorting).html("&nbsp;&nbsp;<a href=\"javascript:LiveResults.Instance.resetSorting()\"><img class=\"eR\" style=\"vertical-align: middle\" src=\"images/cleardot.gif\" border=\"0\"/> " + this.resources["_RESETTODEFAULT"] + "</a>");
                            }
                        },
                        "fnRowCallback": (nRow : any, aData : any, iDisplayIndex : number, iDisplayIndexFull :  number) => {
                            if (aData)
                                aData.curDrawIndex = iDisplayIndex;
                        },
                        "bDestroy": true
                    });

                    this.lastClassHash = data.hash;
                }
            }
        }

        public setAutomaticUpdate(val: boolean) {
            this.updateAutomatically = val;
            if (this.updateAutomatically) {
                $("#" + this.setAutomaticUpdateText).html("<b>" + this.resources["_AUTOUPDATE"] + ":</b> " + this.resources["_ON"] + " | <a href=\"javascript:LiveResults.Instance.setAutomaticUpdate(false);\">" + this.resources["_OFF"] + "</a>");
                this.checkForClassUpdate();
                this.updateLastPassings();
                this.checkForClassUpdate();

            } else {
                clearTimeout(this.resUpdateTimeout);
                clearTimeout(this.passingsUpdateTimer);
                clearTimeout(this.classUpdateTimer);
                $("#" + this.setAutomaticUpdateText).html("<b>" + this.resources["_AUTOUPDATE"] + ":</b> <a href=\"javascript:LiveResults.Instance.setAutomaticUpdate(true);\">" + this.resources["_ON"] + "</a> | " + this.resources["_OFF"] + "");
                this.serverTimeDiff = null;
                if (this.currentTable) {
                    $.each(this.currentTable.fnGetNodes(), (idx, obj) => {
                        for (var i = 4; i < obj.childNodes.length; i++) {
                            var innerHtml = obj.childNodes[i].innerHTML;
                            if (innerHtml.indexOf("<i>(") >= 0) {
                                obj.childNodes[i].innerHTML = "<td class=\"left\"></td>";
                            }
                        }
                    });
                }
            }
        }

        private formatTime(time : number, status : number, showTenthOs?: boolean, showHours?: boolean, padZeros?: boolean) {

            if (arguments.length == 2 || arguments.length == 3) {
                if (this.language == 'fi') {
                    showHours = true;
                    padZeros = false;
                } else {
                    showHours = false;
                    padZeros = true;
                }
            } else if (arguments.length == 4) {
                if (this.language == 'fi') {
                    padZeros = false;
                } else {
                    padZeros = true;
                }
            }

            if (status != 0) {
                return this.runnerStatus[status];
            } else {
                var minutes : any;
                var seconds : number;
                var tenth : number;
                if (showHours) {
                    var hours = Math.floor(time / 360000);
                    minutes = Math.floor((time - hours * 360000) / 6000);
                    seconds = Math.floor((time - minutes * 6000 - hours * 360000) / 100);
                    tenth = Math.floor((time - minutes * 6000 - hours * 360000 - seconds * 100) / 10);
                    if (hours > 0) {
                        if (padZeros)
                            hours = <any>this.strPad(hours, 2);

                        return hours + ":" + this.strPad(minutes, 2) + ":" + this.strPad(seconds, 2) + (showTenthOs ? "." + tenth : "");
                    } else {
                        if (padZeros)
                            minutes = this.strPad(minutes, 2);

                        return minutes + ":" + this.strPad(seconds, 2) + (showTenthOs ? "." + tenth : "");
                    }

                } else {

                    minutes = Math.floor(time / 6000);
                    seconds = Math.floor((time - minutes * 6000) / 100);
                    tenth = Math.floor((time - minutes * 6000 - seconds * 100) / 10);
                    if (padZeros) {
                        return this.strPad(minutes, 2) + ":" + this.strPad(seconds, 2) + (showTenthOs ? "." + tenth : "");
                    } else {
                        return minutes + ":" + this.strPad(seconds, 2) + (showTenthOs ? "." + tenth : "");
                    }
                }
            }
        }

        private strPad(num : any, length : number) {
            var str = '' + num;
            while (str.length < length) {
                str = '0' + str;
            }
            return str;
        }

        private updateResultVirtualPosition(data : any) {
            var i : number;

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
                data.sort((a, b) => { return this.sortByDistAndSplitPlace(a, b); });
                /*for (i = 0; i < tmp.length; i++) {
                    data.push(tmp[i]);
                }*/
            } else {
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
        }

        ///Sorts results by the one that have run longest on the course
        private sortByDist(a : any, b : any) {
            return b.progress - a.progress;
        }

        //Sorts results by the one that have run longest on the course, and if they are on the same split, place on that split
        //"MassStart-Sorting"
        private sortByDistAndSplitPlace(a: any, b: any) {
            var sortStatusA = a.status;
            var sortStatusB = b.status;
            if (sortStatusA == 9 || sortStatusA == 10)
                sortStatusA = 0;
            if (sortStatusB == 9 || sortStatusB == 10)
                sortStatusB = 0;
            if (sortStatusA != sortStatusB)
                return sortStatusA - sortStatusB;

            if (a.progress == 100 && b.progress == 100)
                return a.result - b.result;

            if (a.progress == 0 && b.progress == 0) {
                if (a.start && !b.start)
                    return -1;
                if (!a.start && b.start)
                    return 1;
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
        }

        //Inserts a result in the array of results.
        //The result to be inserted is assumed to have same or worse progress than all other results already in the array
        private insertIntoResults(result : any, data : any) {
            var d: number;
            if (this.curClassSplits != null) {
                for (var s = this.curClassSplits.length - 1; s >= 0; s--) {
                    var splitCode = this.curClassSplits[s].code;
                    if (result.splits[splitCode] != "") {
                        var numOthersAtSplit = 0;
                        for (d = 0; d < data.length; d++) {

                            if (data[d].splits[splitCode] != "") {
                                numOthersAtSplit++;
                            }

                            //insert result 
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
                    } else if (data[d].start != "" && data[d].start > result.start && result.progress == 0 && data[d].progress == 0) {
                        data.splice(d, 0, result);
                        return;
                    }

                }
            }

            data.push(result);
        }

        private resultSorter(a : any, b : any) {
            if (a.place != "" && b.place != "") {
                if (a.status != b.status)
                    return a.status - b.status;
                else {
                    if (a.result == b.result) {
                        if (a.place == "=" && b.place != "=") {
                            return 1;
                        } else if (b.place == "=" && a.place != "=") {
                            return -1;
                        } else {
                            return 0;
                        }

                    } else {
                        return a.result - b.result;
                    }
                }

            } else if (a.place == "-" || a.place != "") {
                return 1;
            } else if (b.place == "-" || b.place != "") {
                return -1;
            } else {

                return 0;
            }
        }

        public newWin() {
            var url = 'followfull.php?comp=' + this.competitionId + '&lang=' + this.language;
            if (this.curClassName != null)
                url += '&class=' + encodeURIComponent(this.curClassName);
            else
                url += '&club=' + encodeURIComponent(this.curClubName);

            window.open(url, '', 'status=0,toolbar=0,location=0,menubar=0,directories=0,scrollbars=1,resizable=1,width=900,height=600');
        }

        public viewClubResults(clubName : string) {
            if (this.currentTable != null) {

                try {
                    this.currentTable.fnDestroy();
                } catch (e) {
                }
            }

            clearTimeout(this.resUpdateTimeout);

            $('#divResults').html('');
            $('#' + this.txtResetSorting).html('');
            this.curClubName = clubName;
            this.curClassName = null;
            $('#resultsHeader').html(this.resources["_LOADINGRESULTS"]);

            $.ajax({
                url: "api.php",
                data: "comp=" + this.competitionId+ "&method=getclubresults&unformattedTimes=true&club=" + encodeURIComponent(clubName) + (this.isMultiDayEvent ? "&includetotal=true" : ""),
                success: (data) => {
                    this.updateClubResults(data);
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

            this.resUpdateTimeout = setTimeout(() => {
                this.checkForClubUpdate();
            }, this.updateInterval);
        }

        private updateClubResults (data : any) {
            if (data != null && data.status == "OK") {
                if (data.clubName != null) {
                    $('#' + this.resultsHeaderDiv).html('<b>' + data.clubName + '</b>');
                    $('#' + this.resultsControlsDiv).show();
                }

                if (data.results != null) {

                    $.each(data.results, (idx: number, res: any) => {
                        res.placeSortable = res.place;
                        if (res.place == "-")
                            res.placeSortable = 999999;
                        if (res.place == "")
                            res.placeSortable = 9999;

                    });

                    var columns = Array();
                    columns.push({ "sTitle": "#", "aTargets": [0], "mDataProp": "place",  });
                    columns.push({"sTitle": "placeSortable", "bVisible": false, "mDataProp": "placeSortable", "aTargets": [1]});
                    columns.push({ "sTitle": this.resources["_NAME"], "aTargets": [2], "mDataProp": "name" });
                    columns.push({ "sTitle": this.resources["_CLUB"], "bSortable": false, "aTargets": [3], "mDataProp": "club" });
                    columns.push({
                        "sTitle": this.resources["_CLASS"], "aTargets": [4], "mDataProp": "class",
                        "fnRender":  (o : any) => {
                            var param = o.aData["class"];
                            if (param && param.length > 0)
                                param = param.replace('\'', '\\\'');
                            return "<a href=\"javascript:LiveResults.Instance.chooseClass('" + param + "')\">" + o.aData["class"] + "</a>";
                        }

                    });

                    var col = 5;
                    columns.push({
                        "sTitle": this.resources["_START"], "sClass": "left", "sType": "numeric", "aDataSort": [col], "aTargets": [col], "bUseRendered": false, "mDataProp": "start",
                        "fnRender":  (o : any) => {
                            if (o.aData.start == "") {
                                return "";
                            }
                            else {
                                return this.formatTime(o.aData.start, 0, false, true);
                            }
                        }
                    });

                    col++;

                    var timecol = col;
                    columns.push({
                        "sTitle": this.resources["_CONTROLFINISH"], "sClass": "left", "sType": "numeric", "aDataSort": [col + 1, col, 0], "aTargets": [col], "bUseRendered": false, "mDataProp": "result",
                        "fnRender":  (o : any) => {
                            if (o.aData.place == "-" || o.aData.place == "") {
                                return this.formatTime(o.aData.result, o.aData.status);
                            }
                            else {
                                return this.formatTime(o.aData.result, o.aData.status) + " (" + o.aData.place + ")";
                            }
                        }
                    });

                    col++;
                    columns.push({ "sTitle": "Status", "bVisible": false, "aTargets": [col++], "sType": "numeric", "mDataProp": "status" });
                    columns.push({
                        "sTitle": "", "sClass": "center", "bSortable": false, "aTargets": [col++], "mDataProp": "timeplus",
                        "fnRender":  (o : any) => {
                            if (o.aData.status != 0)
                                return "";
                            else
                                return "+" + this.formatTime(o.aData.timeplus, o.aData.status);
                        }
                    });

                    
                    this.currentTable = (<any>$('#' + this.resultsDiv)).dataTable({
                        "bPaginate": false,
                        "bLengthChange": false,
                        "bFilter": false,
                        "bSort": true,
                        "bInfo": false,
                        "bAutoWidth": false,
                        "aaData": data.results,
                        "aaSorting": [[1, "asc"]],
                        "aoColumnDefs": columns,
                        "fnPreDrawCallback":  (oSettings : any) => {
                            if (oSettings.aaSorting[0][0] != 1) {
                                $("#" + this.txtResetSorting).html("&nbsp;&nbsp;<a href=\"javascript:LiveResults.Instance.resetSorting()\"><img class=\"eR\" style=\"vertical-align: middle\" src=\"images/cleardot.gif\" border=\"0\"/> " + this.resources["_RESETTODEFAULT"] + "</a>");
                            }
                        },
                        "bDestroy": true
                    });

                    this.lastClubHash = data.hash;
                }
            }

           
        }
        public resetSorting() {
            var idxCol = 0;
            $.each(this.currentTable.fnSettings().aoColumns, (idx, val) => {
                if (val.sTitle == "VP") {
                    idxCol = idx;
                }
            });

            this.currentTable.fnSort([[idxCol, 'asc']]);
            $("#" + this.txtResetSorting).html("");
        }
    }
}

(<any>Date.prototype).stdTimezoneOffset = function() {
    var jan = new Date(this.getFullYear(), 0, 1);
    var jul = new Date(this.getFullYear(), 6, 1);
    return Math.max(jan.getTimezoneOffset(), jul.getTimezoneOffset());
};

(<any>Date.prototype).dst = function() {
    return this.getTimezoneOffset() < this.stdTimezoneOffset();
};

//GA tracker-object
var ga: any;
