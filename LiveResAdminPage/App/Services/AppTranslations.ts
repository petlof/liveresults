// Configuring $translateProvider
liveresAdminApp.config(['$translateProvider', ($translateProvider: ng.translate.ITranslateProvider) => {

    $translateProvider.translations("se", {
        //Startpage
        'ADMINEXISTING': "Administrera existerande tävling",
        'ADMINPASSWORD': "Lösenord",
        'ADMINLOGIN': "Logga in",
        'ADMINPASSWORDHINT': "För tävlingar skapade 2015 eller tidigare, ange blankt lösenord. För tävlingar 2016 eller senare ange lösenordet som angavs då tävlingen skapades",
        'LIVETODAY': "Live idag!",
        'COMPETITIONARCHIVE': "Tävlingsarkiv",
        'CHOOSECOMPETITION': 'Välj tävling',
        
      
    });

    $translateProvider.translations("en", {
        //Startpage
        'ADMINEXISTING': "Administrate existing competition",
        'ADMINPASSWORD': "Password",
        'ADMINLOGIN': "Login",
        'ADMINPASSWORDHINT': "For competitions created 2015 or earlier, leave password empty. For competitions 2016 and later, give the password that was supplied while creating the competition",
        'LIVETODAY': "Live today!",
        'COMPETITIONARCHIVE': "Archive",
        'CHOOSECOMPETITION': 'Choose Competition',

        
    });

    $translateProvider.preferredLanguage('se');
    $translateProvider.useSanitizeValueStrategy('escape');
}]);

