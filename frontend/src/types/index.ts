export * from "./auth";
export * from "./organizations";
export * from "./schedule";
export * from "./employee";
export * from "./employee-options";
export * from "./department";
export * from "./shiftType";
export * from "./notification";
export * from "./onboarding";


export interface ApiError {
    errorCode: string;
    message: string;
    errors: Record<string, string[]> | null;
}

export type Mode = "login" | "register" | "forgot"
export type DayOfWeek = 0 | 1 | 2 | 3 | 4 | 5 | 6;
export { SchedulePattern } from "./department";

export const dayNameToEnum: Record<string, number> = {
    sunday: 0,
    monday: 1,
    tuesday: 2,
    wednesday: 3,
    thursday: 4,
    friday: 5,
    saturday: 6,
};

export const enumToDayName: Record<number, string> = {
    0: 'sunday',
    1: 'monday',
    2: 'tuesday',
    3: 'wednesday',
    4: 'thursday',
    5: 'friday',
    6: 'saturday',
};

export type DateData = {
    isoDate: string;
    label: string;
};

export type CheckEntitiesResult = {
    departments: boolean;
    employees: boolean;
    shiftTypes: boolean;
    schedules: boolean;
}
