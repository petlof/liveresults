interface IHomeScope extends ng.IScope {
    competitions: Competition[];
    today : string;
    selectComp(compId: number);
}

class HomeController {
    constructor(
        private $scope: IHomeScope, $http: ng.IHttpService, $location: ng.ILocationService, $filter : ng.IFilterService) {

        $http.get('api.php?method=getcompetitions').success(data => {
            $scope.competitions = data.competitions;
            $scope.today = $filter('date')(Date.now(), 'yyyy-MM-dd');
        });

        $scope.selectComp = (compId: number) => {
            $location.path('/comp/' + compId);
        };
    }
}

angular.module('liveresControllers', [])
    .controller("HomeController", ["$scope", "$http", "$location","$filter", HomeController]);