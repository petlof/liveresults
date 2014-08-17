/// <reference path="../Scripts/typings/angularjs/angular.d.ts"/>
/// <reference path="../Scripts/typings/angularjs/angular-route.d.ts"/>
/// <reference path="HomeController.ts"/>
/// <reference path="CompetitionController.ts"/>


var liveresApp = angular.module("liveresApp", ['ngRoute', 'ngGrid', 'liveresControllers'])
    .config(['$routeProvider', ($routeprovider: ng.route.IRouteProvider) => {
    $routeprovider.when('/', { templateUrl: 'Views/home.html', controller: 'HomeController' });
    $routeprovider.when('/comp/:competition/:className?', { templateUrl: 'Views/competition.html', controller: 'CompetitionController' });
    $routeprovider.otherwise({ redirectTo: '/' });
}]);

angular.module('liveresControllers', [])
        .controller("CompetitionController", ["$routeParams", "$scope", "$http", LiveResults.Competition.CompetitionController])
        .controller("HomeController", ["$scope", "$http", "$location","$filter", LiveResults.Index.HomeController]);
