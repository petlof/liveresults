module LiveResults.Model {
    export class Competition {
        date: Date;
        name: string;
        organizer: string;
        id: number;
        timeZoneOffset : number;
        timezone : string;
        country : string;
        isPublic : boolean;
    }
}