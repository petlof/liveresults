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
        constructor(private $routeParams: any,
            private $scope: ICompetitionScope, private $http: ng.IHttpService) {
            this.competitionId = $routeParams["competition"];
            this.updateClasses();
            $http.get('api.php?comp=10640&method=getclassresults&unformattedTimes=true&class=W21&last_hash=06d8bafbe4e8edf6ef825d944828d93e').success(data => {
                $scope.results = data.results; 
            });


            $scope.selectClass = (className) => { alert(className); };

        }

        updateClasses = function() {
            this.$http.get('api.php?comp=' + this.competitionId + '&method=getclasses&last_hash=' + this.lastGetClassesHash).success(data => {
                if (data.status == "OK") {
                    this.$scope.classes = data.classes.map(x => x.className);
                    this.lastGetClassesHash = data.hash;
                }
            });
        }
}
}

angular.module('liveresControllers', [])
        .controller("CompetitionController", ["$routeParams", "$scope", "$http", LiveResults.Competition.CompetitionController]);
