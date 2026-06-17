import { DayOfWeek } from './index';

export type DepartmentFormValues = {
    name: string;
    description?: string;
    color?: string;
    startTime?: string;
    endTime?: string;
    workingDays?: DayOfWeek[];
    defaultSchedulePattern?: string;
};

export type Department = {
    id: number
    name: string
    description?: string
    color: string
    startTime?: string;
    endTime?: string;
    workingDays: DayOfWeek[];
    defaultSchedulePattern: SchedulePattern;
    autorenewSchedules: boolean
}

export enum SchedulePattern {
    Flexible = 0,
    TwoOnTwoOff = 1,
    FiveOnTwoOff = 2,
    ThreeOnThreeOff = 3,
    FourOnFourOff = 4
}

//Mapper
export const SchedulePatternOptions = [
    {
        label: "Flexible",
        value: "Flexible",
        enumValue: SchedulePattern.Flexible,
    },
    {
        label: "2 on / 2 off",
        value: "TwoOnTwoOff",
        enumValue: SchedulePattern.TwoOnTwoOff,
    },
    {
        label: "5 on / 2 off",
        value: "FiveOnTwoOff",
        enumValue: SchedulePattern.FiveOnTwoOff,
    },
]
