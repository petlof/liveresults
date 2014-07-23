declare module LiveResults.Model {
    class Competition {
        date: Date;
        name: string;
        organizer: string;
        id: number;
    }

    class Result {
        place: number;
        name: string;
        club: string;
        result: number;
        status: number;
        timeplus: number;
        splits: KeyPair[]

    }
}