
var liveresAdminApp = angular.module("liveresAdminApp", ['ngRoute', 'liveresAdminControllers', 'pascalprecht.translate', 'ngSanitize','ngStorage'])
    .config(['$routeProvider', ($routeprovider: ng.route.IRouteProvider) => {
        $routeprovider.when('/:lang', { templateUrl: 'App/Home/Home.html', controller: 'HomeController' });
        $routeprovider.when('/:lang/Edit/:competitionId', { templateUrl: 'App/EditComp/EditComp.html', controller: 'EditCompController' });
        $routeprovider.otherwise({ redirectTo: '/se' });
    }]);


angular.module('liveresAdminControllers', ['LiveResults.Admin.Config', 'pascalprecht.translate'])
    .controller("HomeController", <any>LiveResults.Admin.Home.HomeController)
    .controller("EditCompController", <any>LiveResults.Admin.EditComp.EditCompController)
    .controller("AppServices", <any>LiveResults.Admin.App.AppServices);



var config = angular.module('LiveResults.Admin.Config', [])
    .constant('APP_NAME', 'EmmaClient LiveResults Admin')
    .constant('APP_VERSION', '0.2')
    .constant('apiUrl', 'http://liveresultat.orientering.se/api.php');
