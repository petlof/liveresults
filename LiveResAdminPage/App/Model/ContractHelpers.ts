module LiveResults.Contract.Helpers {
	export class  ContractHelpers {
		public static ToModel(comp : Contract.ICompetition) : Model.Competition {
			return {
				name : comp.name,
				organizer : comp.organizer,
				date : this.parseDate(comp.date),
				id : comp.id,
				timeZoneOffset : comp.timediff+1,
				country : "SE",
				isPublic : comp.isPublic == 1,
				timezone : comp.timezone
			};
		}
		
		public static SplitControlListToModel(controls : Contract.ISplitControl[]) : LiveResults.Model.ClassSplitControlList[] {
			controls.sort((x,y) => { 
				if (x.class > y.class)
					return 1;
				else if (x.class < y.class)
					return -1;
				else 
				{
					return x.order - y.order;
				}
			});
			
			var ret = [];
			var curClass : LiveResults.Model.ClassSplitControlList = null;
			for (var i = 0; i < controls.length; i++)
			{
				if (curClass == null || controls[i].class != curClass.class)
				{
					if (curClass != null)
						ret.push(curClass);
					curClass = <LiveResults.Model.ClassSplitControlList> { class: controls[i].class, splitControls: <LiveResults.Model.SplitControl[]>[] };
				}
				curClass.splitControls.push(this.SplitControlToModel(controls[i]));
			}
			if (curClass != null)
				ret.push(curClass);
			return ret;
		}
		
		public static SplitControlToModel(control : Contract.ISplitControl) : Model.SplitControl {
			var passing = Math.floor(control.code / 1000);
			var controlCode = control.code - passing*1000;
			return {
				name : control.name,
				class : control.class,
				order : control.order,
				passing : passing,
				controlCode : controlCode
			};
		}
		
		// parse a date in yyyy-mm-dd format
 		static  parseDate(input : string) : Date {
  			var parts = input.split('-');
  			// new Date(year, month [, day [, hours[, minutes[, seconds[, ms]]]]])		
  			return new Date(+parts[0], +parts[1]-1, +parts[2]); // Note: months are 0-based
		}
	}
}