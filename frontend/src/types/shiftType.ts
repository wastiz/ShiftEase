export type ShiftTemplateFormValues = {
    name: string;
    startTime: string;
    endTime: string;
    breakDuration?: string | null; // HH:MM:SS format for API
    minEmployees: number;
    maxEmployees: number;
    color: string;
    departmentId: number;
}

// Used internally in the form — stores break as minutes (number) for the input field
export type ShiftTemplateInternalFormValues = Omit<ShiftTemplateFormValues, 'breakDuration'> & {
    breakDurationMinutes?: number | '';
}

export type PendingShiftTemplate = Omit<ShiftTemplateFormValues, 'departmentId'>

export type ShiftTemplate = {
    id: number;
    name: string;
    startTime: string;
    endTime: string;
    breakDuration?: string | null; // HH:MM:SS from backend
    minEmployees: number;
    maxEmployees: number;
    color: string;
    departmentId: number;
};

