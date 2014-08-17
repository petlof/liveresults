///<reference path="../Scripts/typings/angularjs/angular.d.ts/>
///<reference path="app.ts/>


// Configuring $translateProvider
liveresApp.config(['$translateProvider', function ($translateProvider) {
    
    $translateProvider.translations("se", {
        //Startpage
        'NAME': "Namn", 
        'ORGANIZER' : "Arrangör",
        'DATE' : "Datum",
        'NOCOMPETITIONSTODAY': "Inga tävlingar idag",
        'LIVETODAY' : "Live idag!",
        'COMPETITIONARCHIVE' : "Tävlingsarkiv"
    });   
    
     $translateProvider.translations("en", {
        //Startpage
        'NAME': "Name", 
        'ORGANIZER' : "Organizer",
        'DATE' : "Date",
        'NOCOMPETITIONSTODAY': "No competitions today",
        'LIVETODAY' : "Live today!",
        'COMPETITIONARCHIVE' : "Archive"
    });   
    
    $translateProvider.uses('se');
}]);

