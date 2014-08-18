///<reference path="../Scripts/typings/angularjs/angular.d.ts/>

module LiveResults.Competition {
    export class ResultItem {
    }

    export interface ICompetitionScope extends ng.IScope {
        results: ResultItem[];
        classes : string[];
        gridOptions: ngGrid.IGridOptions;
        tableHeight : string;
        selectClass(className : string);
    }


    export class CompetitionController {
        competitionId : number;
        lastGetClassesHash : string;
        
        static $inject = ["$routeParams", "$scope", "$http", "API_URL"];
        constructor(private $routeParams: any,
            private $scope: ICompetitionScope, private $http: ng.IHttpService, private API_URL : string) {
            this.competitionId = $routeParams["competition"];
            this.updateClasses();
            $http.get(this.API_URL + '?comp=10640&method=getclassresults&unformattedTimes=true&class=W21&last_hash=06d8bafbe4e8edf6ef825d944828d93e').success((data : any) => {
                $scope.results = data.results; 
            });

            $scope.selectClass = (className) => { alert(className); };

        }

        updateClasses = function() {
            this.$http.get(this.API_URL + '?comp=' + this.competitionId + '&method=getclasses&last_hash=' + this.lastGetClassesHash).success(data => {
                if (data.status == "OK") {  
                    this.$scope.classes = data.classes.map(x => x.className);
                    this.lastGetClassesHash = data.hash;
                }
            });
        }
}
}

