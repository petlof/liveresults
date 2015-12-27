
module LiveResults.Admin.Home {
    export interface IHomeScope extends ng.IScope {
        competitions: LiveResults.Model.Competition[];
        selectedCompetitionId: number;
        password: string;
        login();
    }

    export class HomeController {
        static $inject = ["$scope", "$http", "$location", "$filter", "apiUrl", "$routeParams","$sessionStorage"];
        constructor(
            private $scope: IHomeScope, $http: ng.IHttpService, private $location: ng.ILocationService, private $filter: ng.IFilterService, apiUrl: string, private $routeParams: any, private $sessionStorage : any) {

            $http.get(apiUrl + '?method=getcompetitions').success((data: { competitions: LiveResults.Contract.ICompetition[] }) => {
                    this.$scope.competitions = data.competitions;
            });
            $scope.login = () => {
                this.$sessionStorage.competitionid = this.$scope.selectedCompetitionId;
                this.$sessionStorage.password = this.$scope.password;
                this.$location.path(this.$routeParams["lang"] + "/Edit/" + this.$sessionStorage.competitionid);
            };
        }
    }
}