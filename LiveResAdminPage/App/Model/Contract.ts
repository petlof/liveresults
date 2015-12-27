module LiveResults.Contract {
    export interface  ICompetition {
        date: Date;
        name: string;
        organizer: string;
        id: number;
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