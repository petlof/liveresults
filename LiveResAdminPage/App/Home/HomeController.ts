/// <reference path="../Model/Competition.ts" />
/// <reference path="../Model/Contract.ts" />

module LiveResults.Admin.Home {
    export interface IHomeScope extends ng.IScope {
        competitions: LiveResults.Model.Competition[];
        selectedCompetitionId: number;
        password: string;
        login();
        newComp : { name : string,organizer : string, password : string, password2 : string, date : Date, country : string,
            email : string};
        countries : LiveResults.Contract.ICountry[];
        formatCompetition(comp : LiveResults.Model.Competition);
        createCompetition();
    }

    export class HomeController {
        static $inject = ["$scope", "$http", "$location", "$filter", "apiUrl", "$routeParams","$sessionStorage",'COUNTRY_LIST'];
        constructor(
            private $scope: IHomeScope, private $http: ng.IHttpService, private $location: ng.ILocationService, 
            private $filter: ng.IFilterService, apiUrl: string, private $routeParams: any, 
            private $sessionStorage : any, COUNTRY_LIST : LiveResults.Contract.ICountry[]) {
            $scope.newComp = { date : new Date(), country : 'SE', name : "", organizer : "", email : "", password: "", password2 :""};
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
            
            $scope.createCompetition = () => {
                this.$http.post(apiUrl + '?method=createcompetition', 
                { name: this.$scope.newComp.name,
                  organizer : this.$scope.newComp.organizer,
                  date : this.$scope.newComp.date,
                  country : this.$scope.newComp.country,
                  email : this.$scope.newComp.email,
                  password : this.$scope.newComp.password}
                ).success((data: { status : string, competitionid : number, message : string }) => {
                    if (data.status != "OK")
                    {
                        alert("Could not create competition: " + data.message );
                    }
                    else 
                    {
                        this.$sessionStorage.competitionid = data.competitionid;
                        this.$sessionStorage.password = this.$scope.password;
                        this.$location.path(this.$routeParams["lang"] + "/Edit/" + this.$sessionStorage.competitionid);
                    }
                });
            };
        }
    }
}