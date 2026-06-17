import { Holiday, WorkDay } from "@/types/organizations";
import { EmployeeMinData } from "@/types/employee";
import { Department } from "@/types/department";
import { ShiftTemplate } from "@/types/shiftType";

export interface ScheduleSummary {
    confirmedSchedules: ScheduleItem[];
    unconfirmedSchedules: ScheduleItem[];
}

export interface ScheduleItem {
    id: number;
    month: string;
    year: string;
    coverage: string;
    warnings: DayIssue[];
    totalHours: number;
    totalShifts: number;
}

export interface DayIssue {
    isoDate: string;
    label: string;
    assigned: number;
    required: number;
    severity: string;
}

export type ScheduleEditorData = {
    employees: EmployeeMinData[];
    shiftTypes: ShiftTemplate[];
    departments: Department[];
    organizationHolidays: Holiday[];
    organizationSchedule: WorkDay[];
    employeeTimeOffs: EmployeeTimeOff[]
    schedule: Schedule
};

export type Schedule = {
    id: number;
    startDate: Date;
    endDate: Date;
    isConfirmed: boolean;
    shifts: Shift[];
}

export type EmployeeTimeOff = {
    id: number;
    employeeId: number;
    employeeName: string;
    startDate: Date;
    endDate: Date;
    type: TimeOffType;
}

export enum TimeOffType {
    Vacation = "Vacation",
    SickLeave = "SickLeave",
    PersonalDay = "PersonalDay",
}

export type Shift = {
    id: number;
    date: string;
    shiftTypeId: number;
    shiftTypeName: string;
    startTime: string;
    endTime: string;
    color: string;
    employeeNeeded: number;
    employees: EmployeeShiftAssignment[];
}

export type EmployeeShiftAssignment = {
    id: number;
    name: string;
    departmentName?: string;
    note?: string;
}

export interface ScheduleRequest {
    month: number
    year: number
}

export type SchedulePost = {
    startDate: string
    endDate: string
    isConfirmed: boolean
    shifts: ShiftPost[];
}

export type ShiftPost = {
    shiftTypeId: number;
    date: string;
    employees: EmployeeIdWithNote[];
}

export type EmployeeIdWithNote = {
    id: number;
    note: string;
}

export enum SchedulePattern {
    Custom = 0,
    TwoOnTwoOff = 1,
    FiveOnTwoOff = 2,
    ThreeOnThreeOff = 3,
    FourOnFourOff = 4
}

export type ScheduleGenerateRequest = {
    id: number
    startDate: string
    endDate: string
    TotalHours?: number | null;
    HardTotalHours?: boolean;
}

export type AcoScheduleGenerateRequest = {
    startDate: string;
    endDate: string;
    NumAnts: number;
    NumIterations: number;
    TotalHours?: number | null;
    HardTotalHours?: boolean;
}

export type GaScheduleGenerateRequest = {
    startDate: string;
    endDate: string;
    PopulationSize: number;
    NumGenerations: number;
    TotalHours?: number | null;
    HardTotalHours?: boolean;
}

export type AcoGaScheduleGenerateRequest = {
    startDate: string;
    endDate: string;
    NumAnts: number;
    NumAcoIterations: number;
    PopulationSize: number;
    NumGaGenerations: number;
    TotalHours?: number | null;
    HardTotalHours?: boolean;
}

export type ScheduleGenerateResult =
    | {
        status: GenerateStatus.Success;
        shifts: Shift[];
        warnings: [];
        error?: never;
    }
    | {
        status: GenerateStatus.Warning;
        shifts: Shift[];
        warnings: GenerateWarningCode[];
        error?: never;
    }
    | {
        status: GenerateStatus.Error;
        error: GenerateErrorCode;
        warnings: [];
        shifts?: never;
    };


export enum GenerateStatus {
    Success = "Success",
    Warning = "Warning",
    Error = "Error",
}

export enum GenerateErrorCode {
    DepartmentNotFound = "DepartmentNotFound",
    OrganizationNotFound = "OrganizationNotFound",
    NoEmployeesInDepartment = "NoEmployeesInDepartment",
    NoShiftTypes = "NoShiftTypes",
    SelectedShiftTypesNotFound = "SelectedShiftTypesNotFound",
    NoWorkDaysConfigured = "NoWorkDaysConfigured",
    InvalidDateRange = "InvalidDateRange",
    AllDaysAreHolidays = "AllDaysAreHolidays",
    AllDaysAreNonWorking = "AllDaysAreNonWorking",
    ShiftTypesDontFitSchedule = "ShiftTypesDontFitSchedule",
}

export enum GenerateWarningCode {
    NoSuitableShiftTypes = "NoSuitableShiftTypes",
    NotEnoughEmployeesForMinimum = "NotEnoughEmployeesForMinimum",
    EmployeesAssignedWithConstraintViolations = "EmployeesAssignedWithConstraintViolations",
    AllEmployeesOnTimeOff = "AllEmployeesOnTimeOff",
    HighWorkloadDetected = "HighWorkloadDetected",
    SomeDaysWithoutShifts = "SomeDaysWithoutShifts",
    BudgetExhausted = "BudgetExhausted",
}

