
module LiveResults.Competition {
    export class ResultItem {
    }

    export interface ICompetitionScope extends ng.IScope {
        results: ResultItem[];
        classes : string[];
        tableHeight : string;
        selectClass(className : string);
        selectedClass : string;
    }


    export class CompetitionController {
        competitionId: number;
        lastGetClassesHash: string;
        lastGetResultsHash : string;
        resultsUpdateTimerRef : any;

        static $inject = ["$routeParams", "$scope", "$http", "apiUrl","$log"];

        constructor(private $routeParams: any,
            private $scope: ICompetitionScope, private $http: ng.IHttpService, private apiUrl: string, private $log : ng.ILogService) {
            this.competitionId = $routeParams["competition"];
            this.updateClasses();


            $scope.selectClass = (className: string) => {
                this.$scope.selectedClass = className;
                this.updateResults();
            };

        }

        private updateResults() {
            if (this.$scope.selectedClass) {
                this.$http.get(this.apiUrl + '?comp=' + this.competitionId + '&method=getclassresults&unformattedTimes=true&class=' + this.$scope.selectedClass + '&last_hash=' + this.lastGetResultsHash).success((data: any) => {

                    if (data.results) {
                        
                    }

                    this.$scope.results = data.results;
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

