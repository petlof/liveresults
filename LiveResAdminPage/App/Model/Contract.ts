module LiveResults.Contract {
    export interface ICountry {
      name : string;
      code : string;
    }
    
    export interface  ICompetition {
        date: string;
        name: string;
        organizer: string;
        id: number;
        timediff : number;
        timezone : string;
        isPublic : number;
    }
    
    export interface  ISplitControl {
        class: string;
        name: string;
        code: number;
        order: number;
    }

    export interface IResultsResponse {
        status: string;
        className: string;
        hash: string;
        results: IResultsResponseResult[];
        splitcontrols: IResultsReponseSplitControl[];
    }

    export interface IResultsResponseResult {
        club: string;
        name: string;
        place: string;
        result: string;
        start: number;
        status: number;
        timeplus: string;
        splits: any;
    }

    export interface IResultsReponseSplitControl {
        name: string;
        code: number;
    }
} 