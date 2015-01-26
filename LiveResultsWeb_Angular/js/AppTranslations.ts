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
        'CHOOSECLASSHEADER' : 'Klass',
        'NOCLASSCHOSEN' : "Välj en klass att se på",
        'RES_COL_RANK' : '#',
        'RES_COL_NAME' : 'Namn',
        'RES_COL_CLUB' : 'Klubb',
        'RES_COL_TIME' : 'Mål',
        'RES_COL_TIMEPLUS' : '',
        'RES_COL_START' : 'Start',
        'RES_LASTUPDATED' : 'Senast uppdaterad'
    });

    $translateProvider.translations("en", {
        //Startpage
        'NAME': "Name",
        'ORGANIZER': "Organizer",
        'DATE': "Date",
        'NOCOMPETITIONSTODAY': "No competitions today",
        'LIVETODAY': "Live today!",
        'COMPETITIONARCHIVE': "Archive",
        'CHOOSECOMPETITION': 'Choose Competition',

        //Competition
        'CHOOSECLASSHEADER': 'Class',
        'NOCLASSCHOSEN': "Chose class to view",
        'RES_COL_RANK': '#',
        'RES_COL_NAME': 'Name',
        'RES_COL_CLUB': 'Organization',
        'RES_COL_TIME': 'Finish',
        'RES_COL_TIMEPLUS': '',
        'RES_COL_START': 'Start',
        'RES_LASTUPDATED': 'LAST UPDATED:'
});   
    
    $translateProvider.preferredLanguage('se');
}]);

