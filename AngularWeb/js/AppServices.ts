/// <reference path="../Scripts/typings/angularjs/angular.d.ts"/>

module LiveResults.App
{
    export class AppServices {
    
        static $inject = ["$scope", "$location","$routeParams"];
        constructor(private $scope : any, private $location : ng.ILocationService, private $routeParams : any)
        {
            $scope.$on('$routeChangeSuccess', function() {
                $scope.curlang = $routeParams["lang"];
            });
            
            $scope.GoHome = () => this.$location.path("/" + this.$routeParams["lang"]);
            $scope.SetLang = (lang : string) => {
                var curLang = this.$routeParams["lang"];
                var path = this.$location.path();
                if (curLang && curLang.length > 0)
                {
                    path = path.substr(curLang.length+1);
                }
                this.$location.path(lang + "" + path);
            };
            $scope.languages = [["se","Svenska"],["en","English"],["de","Deutsch"],["fi","Suomeksi"],["it","Italiano"],["ru","Русский"]];
        }
    }
}