module LiveResults.Contract.Helpers {
	export class  ContractHelpers {
		public static ToModel(comp : Contract.ICompetition) : Model.Competition {
			return {
				name : comp.name,
				organizer : comp.organizer,
				date : comp.date,
				id : comp.id,
				timeZoneOffset : comp.timediff+1,
				country : "SE"
			};
		}
	}
}