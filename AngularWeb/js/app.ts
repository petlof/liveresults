/// <reference path="../Scripts/typings/angularjs/angular.d.ts"/>
/// <reference path="../Scripts/typings/angularjs/angular-route.d.ts"/>

var liveresApp = angular.module("liveresApp", ['ngRoute', 'ngGrid', 'liveresControllers'])
    .config(['$routeProvider', ($routeprovider: ng.route.IRouteProvider) => {
    $routeprovider.when('/', { templateUrl: 'Views/home.html', controller: 'HomeController' });
    $routeprovider.when('/comp/:competition/:className?', { templateUrl: 'Views/competition.html', controller: 'CompetitionController' });
    $routeprovider.otherwise({ redirectTo: '/' });
}]);