module LiveResults.Model {
    export class SplitControl {
        class: string;
        name: string;
        controlCode: number;
        passing: number;
        order : number;
    }
    
    export class ClassSplitControlList {
        class : string;
        splitControls : SplitControl[];
    }
}