
var liveresAdminApp = angular.module("liveresAdminApp", ['ngRoute', 'liveresAdminControllers',
'liveresAdminApp.directives', 'pascalprecht.translate', 'ngSanitize','ngStorage',
'ui.bootstrap','vcRecaptcha'])
    .config(['$routeProvider', ($routeprovider: ng.route.IRouteProvider) => {
        $routeprovider.when('/:lang', { templateUrl: 'App/Home/Home.html', controller: 'HomeController' });
        $routeprovider.when('/:lang/Edit/:competitionId', { templateUrl: 'App/EditComp/EditComp.html', controller: 'EditCompController' });
        $routeprovider.otherwise({ redirectTo: '/se' });
    }]);


angular.module('liveresAdminControllers', ['LiveResults.Admin.Config', 'pascalprecht.translate'])
    .controller("HomeController", <any>LiveResults.Admin.Home.HomeController)
    .controller("EditCompController", <any>LiveResults.Admin.EditComp.EditCompController)
    .controller("AppServices", <any>LiveResults.Admin.App.AppServices);
    
angular.module('liveresAdminApp.directives', [])
  .directive('pwCheck', [function () {
    return {
      require: 'ngModel',
      link: function (scope, elem, attrs, ctrl) {
        var firstPassword = '#' + attrs.pwCheck;
        elem.add(firstPassword).on('keyup', function () {
          scope.$apply(function () {
            var v = elem.val()===$(firstPassword).val();
            ctrl.$setValidity('pwmatch', v);
          });
        });
      }
    }
  }]);



var config = angular.module('LiveResults.Admin.Config', [])
    .constant('APP_NAME', 'EmmaClient LiveResults Admin')
    .constant('APP_VERSION', '0.2')
    .constant('apiUrl', 'http://liveresultat.orientering.se/api.php')
    .constant('COUNTRY_LIST',[ 
  {name: 'Afghanistan', code: 'AF'}, 
  {name: 'Åland Islands', code: 'AX'}, 
  {name: 'Albania', code: 'AL'}, 
  {name: 'Algeria', code: 'DZ'}, 
  {name: 'Andorra', code: 'AD'}, 
  {name: 'Angola', code: 'AO'}, 
  {name: 'Antarctica', code: 'AQ'}, 
  {name: 'Argentina', code: 'AR'}, 
  {name: 'Armenia', code: 'AM'}, 
  {name: 'Australia', code: 'AU'}, 
  {name: 'Austria', code: 'AT'}, 
  {name: 'Azerbaijan', code: 'AZ'}, 
  {name: 'Belarus', code: 'BY'}, 
  {name: 'Belgium', code: 'BE'}, 
  {name: 'Bolivia', code: 'BO'}, 
  {name: 'Bosnia and Herzegovina', code: 'BA'}, 
  {name: 'Brazil', code: 'BR'}, 
  {name: 'Bulgaria', code: 'BG'}, 
  {name: 'Canada', code: 'CA'}, 
  {name: 'Cape Verde', code: 'CV'}, 
  {name: 'China', code: 'CN'}, 
  {name: 'Croatia', code: 'HR'}, 
  {name: 'Cyprus', code: 'CY'}, 
  {name: 'Czech Republic', code: 'CZ'}, 
  {name: 'Denmark', code: 'DK'}, 
  {name: 'Estonia', code: 'EE'}, 
  {name: 'Finland', code: 'FI'}, 
  {name: 'France', code: 'FR'}, 
  {name: 'Georgia', code: 'GE'}, 
  {name: 'Germany', code: 'DE'}, 
  {name: 'Greece', code: 'GR'}, 
  {name: 'Greenland', code: 'GL'}, 
  {name: 'Hong Kong', code: 'HK'}, 
  {name: 'Hungary', code: 'HU'}, 
  {name: 'Iceland', code: 'IS'}, 
  {name: 'Ireland', code: 'IE'}, 
  {name: 'Israel', code: 'IL'}, 
  {name: 'Italy', code: 'IT'}, 
  {name: 'Japan', code: 'JP'}, 
  {name: 'Kazakhstan', code: 'KZ'}, 
  {name: 'Kyrgyzstan', code: 'KG'}, 
  {name: 'Latvia', code: 'LV'}, 
  {name: 'Liechtenstein', code: 'LI'}, 
  {name: 'Lithuania', code: 'LT'}, 
  {name: 'Luxembourg', code: 'LU'}, 
  {name: 'Macedonia', code: 'MK'},
  {name: 'Malta', code: 'MT'}, 
  {name: 'Moldova', code: 'MD'}, 
  {name: 'Monaco', code: 'MC'}, 
  {name: 'Morocco', code: 'MA'}, 
  {name: 'Netherlands', code: 'NL'}, 
  {name: 'New Zealand', code: 'NZ'}, 
  {name: 'Norway', code: 'NO'}, 
  {name: 'Poland', code: 'PL'}, 
  {name: 'Portugal', code: 'PT'}, 
  {name: 'Romania', code: 'RO'}, 
  {name: 'Russian Federation', code: 'RU'}, 
  {name: 'Serbia and Montenegro', code: 'CS'}, 
  {name: 'Slovakia', code: 'SK'}, 
  {name: 'Slovenia', code: 'SI'}, 
  {name: 'South Africa', code: 'ZA'}, 
  {name: 'Spain', code: 'ES'}, 
  {name: 'Sweden', code: 'SE'}, 
  {name: 'Switzerland', code: 'CH'}, 
  {name: 'Tajikistan', code: 'TJ'}, 
  {name: 'Turkey', code: 'TR'}, 
  {name: 'Turkmenistan', code: 'TM'}, 
  {name: 'Ukraine', code: 'UA'}, 
  {name: 'United Kingdom', code: 'GB'}, 
  {name: 'United States', code: 'US'},
  {name: 'Uzbekistan', code: 'UZ'}
]);