// Configuring $translateProvider
liveresApp.config(['$translateProvider', ($translateProvider : ng.translate.ITranslateProvider) => {
    
    $translateProvider.translations("se", {
        //Startpage
        'NAME': "Namn", 
        'ORGANIZER' : "Arrangör",
        'DATE' : "Datum",
        'NOCOMPETITIONSTODAY': "Inga tävlingar idag",
        'LIVETODAY' : "Live idag!",
        'COMPETITIONARCHIVE' : "Tävlingsarkiv",
        'CHOOSECOMPETITION' : 'Välj tävling',
        
        //Competition
        'CHOOSECLASSHEADER' : 'Klass'
    });   
    
    $translateProvider.translations("en", {
        //Startpage
        'NAME': "Name", 
        'ORGANIZER' : "Organizer",
        'DATE' : "Date",
        'NOCOMPETITIONSTODAY': "No competitions today",
        'LIVETODAY' : "Live today!",
        'COMPETITIONARCHIVE' : "Archive",
        'CHOOSECOMPETITION' : 'Choose Competition',
        
        //Competition
        'CHOOSECLASSHEADER' : 'Class'
    });   
    
    $translateProvider.preferredLanguage('se');
}]);

