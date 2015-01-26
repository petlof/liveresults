module LiveResults.Utils {
    export class TimeUtils {
        private static runnerStatus = {
            "1": "DNS",
            "2": "DNF",
            "11": "WO",
            "12": "MOVEDUP",
            "9": "DNS",
            "0": "OK",
            "3": "MP",
            "4": "DSQ",
            "5": "OT",
            "10": "STARTED"
        };

        public static calculateRankOnSplits = (result: LiveResults.Contract.IResultsResponse) => {
            if (result.splitcontrols && result.splitcontrols.length > 0) {

                for (var s = 0; s < result.splitcontrols.length; s++) {
                    var splitCode = result.splitcontrols[s].code;
                    result.results.sort((a: Contract.IResultsResponseResult, b: Contract.IResultsResponseResult) => a.splits[splitCode] - b.splits[splitCode]);
                    var lastPos = 1;
                    var posCnt = 1;
                    var lastTime = -1;
                    for (var i = 0; i < result.results.length; i++) {
                        if (result.results[i].splits[splitCode] != "") {

                            if (lastTime == result.results[i].splits[splitCode])
                                result.results[i].splits[splitCode + "-rank"] = lastPos;
                            else {
                                result.results[i].splits[splitCode + "-rank"] = posCnt;
                                lastPos = posCnt;
                            }
                            lastTime = result.results[i].splits[splitCode];
                            posCnt++;
                        } else {
                            result.results[i].splits[splitCode + "-rank"] = "";
                        }
                    }

                }
            }
        }

        public static resultSorter = (a: Contract.IResultsResponseResult, b: Contract.IResultsResponseResult) => {
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
                        return (<any>a.result * 1) - (<any>b.result * 1);
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


        public static formatTime = (time : number, status: number, showHours : boolean, padZeros:boolean) => {

            if (status != 0) {
                return TimeUtils.runnerStatus[status];
            } else {
                if (showHours) {
                    var ihours = Math.floor(time / 360000);
                    var iminutes = Math.floor((time - ihours * 360000) / 6000);
                    var iseconds = Math.floor((time - iminutes * 6000 - ihours * 360000) / 100);


                    if (ihours > 0) {
                        return (padZeros ? TimeUtils.strPad(ihours, 2) : ihours + "") + ":" + TimeUtils.strPad(iminutes, 2) + ":" + TimeUtils.strPad(iseconds, 2);
                    } else {
                        return (padZeros ? TimeUtils.strPad(iminutes, 2) : iminutes + "") + ":" + TimeUtils.strPad(iseconds, 2);
                    }

                } else {

                    iminutes = Math.floor(time / 6000);
                    iseconds = Math.floor((time - iminutes * 6000) / 100);

                    if (padZeros) {
                        return TimeUtils.strPad(iminutes, 2) + ":" + TimeUtils.strPad(iseconds, 2);
                    } else {
                        return iminutes + ":" + TimeUtils.strPad(iseconds, 2);
                    }
                }
            }
        };
        public static strPad = (num: number, length: number): string => {

            var str = '' + num;
            while (str.length < length) {
                str = '0' + str;
            }

            return str;
        };
    }


}