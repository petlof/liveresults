
module LiveResults.Admin.EditComp {
    export interface IEditCompScope extends ng.IScope {
        competition: LiveResults.Model.Competition;
        competitionId: number;
    }

    export class EditCompController {
        static $inject = ["$scope", "$http", "$location", "$filter", "apiUrl", "$routeParams"];
        constructor(
            private $scope: IEditCompScope, $http: ng.IHttpService, private $location: ng.ILocationService, private $filter: ng.IFilterService, apiUrl: string,
            private $routeParams: any) {

            $scope.competitionId = $routeParams["competitionId"];
        
            $http.get(apiUrl + '?method=getcompetitioninfo&comp=' + $scope.competitionId).success((data: LiveResults.Contract.ICompetition) => {
                this.$scope.competition = data;
            });
        }
    }
}