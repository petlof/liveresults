
module LiveResults.Competition {
    export class ResultItem {
    }

    export interface ICompetitionScope extends ng.IScope {
        results: ResultItem[];
        classes : string[];
        tableHeight : string;
        selectClass(className : string);
        selectedClass : string;
        splitControls : Model.SplitControl[];
        lastUpdated : Date;
    }


    export class CompetitionController {
        competitionId: number;
        lastGetClassesHash: string;
        lastGetResultsHash : string;
        resultsUpdateTimerRef : any;
        isActive : boolean = false;

        static $inject = ["$routeParams", "$scope", "$http", "apiUrl","$log","$location"];

        constructor(private $routeParams: any,
            private $scope: ICompetitionScope, private $http: ng.IHttpService, private apiUrl: string, private $log : ng.ILogService, private $location : ng.ILocationService) {
            this.competitionId = $routeParams["competition"];
            this.updateClasses();

            if ($routeParams["className"]) {
                this.$scope.selectedClass = $routeParams["className"];
                this.updateResults();
            }


            $scope.selectClass = (className: string) => {
                this.$location.path($routeParams["lang"] + "/comp/" + $routeParams["competition"] + "/" + className);
            };

            $scope.$on('$destroy', () => {
                this.$log.debug("Result-view Destroyed");
                this.isActive = false;
                if (this.resultsUpdateTimerRef) {
                    {
                        clearTimeout(this.resultsUpdateTimerRef);
                        this.resultsUpdateTimerRef = null;
                    }
                }
            });

        }

        private updateResults() {
            if (this.$scope.selectedClass) {
                this.$http.get(this.apiUrl + '?comp=' + this.competitionId + '&method=getclassresults&unformattedTimes=true&class=' + this.$scope.selectedClass + '&last_hash=' + this.lastGetResultsHash).success((data: IResultsResponse) => {

                    this.$scope.lastUpdated = new Date();
                    if (data && data.status && data.status == "NOT MODIFIED") {

                    } else {
                        this.lastGetResultsHash = data.hash;
                        if (data && data.results) {
                            Utils.TimeUtils.calculateRankOnSplits(data);
                            data.results.sort(Utils.TimeUtils.resultSorter);
                        }
                        this.$scope.splitControls = data.splitcontrols;
                        this.$scope.results = data.results;
                    }
                    this.scheduleResultUpdate();
                }).error(() => {
                    
                    this.scheduleResultUpdate();
                });
            }
        }

        //Schedules an update of the results..
        private scheduleResultUpdate() {
            if (this.resultsUpdateTimerRef)
                clearTimeout(this.resultsUpdateTimerRef);

            this.resultsUpdateTimerRef = setTimeout(() => { this.updateResults(); }, AppSettings.resultsUpdateInterval);
        }


        updateClasses = function() {
            this.$http.get(this.apiUrl + '?comp=' + this.competitionId + '&method=getclasses&last_hash=' + this.lastGetClassesHash).success(data => {
                if (data.status == "OK") {  
                    this.$scope.classes = data.classes.map(x => x.className);
                    this.lastGetClassesHash = data.hash;
                }
            });
        }
}
}

