class CompetitionController {
    constructor(
        private $scope: ng.IScope) {
    }
}

angular.module('liveresControllers', [])
    .controller("CompetitionController", ["$scope", CompetitionController]);