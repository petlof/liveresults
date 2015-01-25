
module LiveResults.Index {
    export interface IHomeScope extends ng.IScope {
        competitions: LiveResults.Model.Competition[];
        today: string;
        selectComp(compId: number);
    }

    export class HomeController {
        static $inject = ["$scope", "$http", "$location","$filter", "apiUrl","$routeParams"];
        constructor(
            private $scope: IHomeScope, $http: ng.IHttpService, private $location: ng.ILocationService, private $filter: ng.IFilterService, apiUrl : string, private $routeParams : any) {

            $http.get(apiUrl +'?method=getcompetitions').success((data : any) => {
                this.$scope.competitions = data.competitions;
                this.$scope.today = this.$filter('date')(Date.now(), 'yyyy-MM-dd');
            });

            $scope.selectComp = (compId: number) => {
                this.$location.path("/" + this.$routeParams["lang"] +'/comp/' + compId);
            };
        }
    }
}