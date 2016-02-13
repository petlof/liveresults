/// <reference path="../Model/Competition.ts" />
/// <reference path="../Model/Contract.ts" />

module LiveResults.Admin.Home {
    export interface IHomeScope extends ng.IScope {
        competitions: LiveResults.Model.Competition[];
        selectedCompetitionId: number;
        password: string;
        login();
        newComp : { date : Date, country : string};
        countries : LiveResults.Contract.ICountry[];
        formatCompetition(comp : LiveResults.Model.Competition);
    }

    export class HomeController {
        static $inject = ["$scope", "$http", "$location", "$filter", "apiUrl", "$routeParams","$sessionStorage",'COUNTRY_LIST'];
        constructor(
            private $scope: IHomeScope, $http: ng.IHttpService, private $location: ng.ILocationService, private $filter: ng.IFilterService, apiUrl: string, private $routeParams: any, 
            private $sessionStorage : any, COUNTRY_LIST : LiveResults.Contract.ICountry[]) {
            $scope.newComp = { date : new Date(), country : 'SE'};
            $scope.countries = COUNTRY_LIST;
            $http.get(apiUrl + '?method=getcompetitions').success((data: { competitions: LiveResults.Contract.ICompetition[] }) => {
                    this.$scope.competitions = data.competitions.map((comp) => Contract.Helpers.ContractHelpers.ToModel(comp));
                    this.$scope.selectedCompetitionId = this.$scope.competitions[0].id;
            });
            $scope.login = () => {
                this.$sessionStorage.competitionid = this.$scope.selectedCompetitionId;
                this.$sessionStorage.password = this.$scope.password;
                this.$location.path(this.$routeParams["lang"] + "/Edit/" + this.$sessionStorage.competitionid);
            };
            $scope.formatCompetition = (comp : LiveResults.Model.Competition) => 
            {
                return comp.date.toLocaleDateString() + ": "+ comp.name + ", " + comp.organizer;  
            };
        }
    }
}