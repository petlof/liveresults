///<reference path="../Scripts/typings/angularjs/angular.d.ts/>

module LiveResults.Index {
    export interface IHomeScope extends ng.IScope {
        competitions: LiveResults.Model.Competition[];
        today: string;
        selectComp(compId: number);
    }

    export class HomeController {
        static $inject = ["$scope", "$http", "$location","$filter", "API_URL","$routeParams"];
        constructor(
            private $scope: IHomeScope, $http: ng.IHttpService, private $location: ng.ILocationService, private $filter: ng.IFilterService, API_URL : string, private $routeParams : any) {

            $http.get(API_URL +'?method=getcompetitions').success((data : any) => {
                this.$scope.competitions = data.competitions;
                this.$scope.today = this.$filter('date')(Date.now(), 'yyyy-MM-dd');
            });

            $scope.selectComp = (compId: number) => {
                this.$location.path("/" + this.$routeParams["lang"] +'/comp/' + compId);
            };
        }
    }
}