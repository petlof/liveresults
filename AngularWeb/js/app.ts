/// <reference path="../Scripts/typings/angularjs/angular.d.ts"/>
/// <reference path="../Scripts/typings/angularjs/angular-route.d.ts"/>
/// <reference path="HomeController.ts"/>
/// <reference path="CompetitionController.ts"/>


var liveresApp = angular.module("liveresApp", ['ngRoute', 'ngGrid', 'liveresControllers','ngTranslate'])
    .config(['$routeProvider', ($routeprovider: ng.route.IRouteProvider) => {
    $routeprovider.when('/:lang?', { templateUrl: 'Views/home.html', controller: 'HomeController' });
    $routeprovider.when('/:lang?/comp/:competition/:className?', { templateUrl: 'Views/competition.html', controller: 'CompetitionController' });
    $routeprovider.otherwise({ redirectTo: '/' });
}]);

angular.module('liveresControllers', ['LiveResults.Config'])
        .controller("CompetitionController", ["$routeParams", "$scope", "$http", "API_URL", LiveResults.Competition.CompetitionController])
        .controller("HomeController", ["$scope", "$http", "$location","$filter", "API_URL", LiveResults.Index.HomeController]);

var config = angular.module('LiveResults.Config', [])
    .constant('APP_NAME','EmmaClient LiveResults')
    .constant('APP_VERSION','0.2')
    .constant('API_URL','/web/api.php');