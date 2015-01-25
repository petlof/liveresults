module LiveResults.App
{
    export class AppServices {
    
        static $inject = ["$scope", "$location","$routeParams","$translate"];
        constructor(private $scope : any, private $location : ng.ILocationService, private $routeParams : any, private $translateProvider : ng.translate.ITranslateProvider)
        {
            $scope.$on('$routeChangeSuccess', () => {
                $scope.curlang = $routeParams["lang"];
                $translateProvider.use($scope.curlang);
            });
            
            $scope.GoHome = () => this.$location.path("/" + this.$routeParams["lang"]);
            $scope.SetLang = (lang : string) => {
                var curLang = this.$routeParams["lang"];
                var path = this.$location.path();
                if (curLang && curLang.length > 0)
                {
                    path = path.substr(curLang.length+1);
                }
                this.$translateProvider.use(lang);
                this.$location.path(lang + "" + path);
            };
            $scope.languages = [["se","Svenska"],["en","English"],["de","Deutsch"],["fi","Suomeksi"],["it","Italiano"],["ru","Русский"]];
        }
    }
}