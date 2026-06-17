export const authKeys = {
    all: ["auth", "me"] as const,
    currentUser: () => [...authKeys.all, "currentUser"] as const,
    user: (userId: number) => [...authKeys.all, "user", userId] as const,
};

export const employeeKeys = {
    all: ["employees"] as const,
    detail: (id: number) => [...employeeKeys.all, id] as const,
    timeOffs: (year?: number, month?: number, employeeIds?: number[]) =>
        ["employees", "time-offs", year, month, employeeIds] as const,
};

export const organizationKeys = {
    all: ["organizations"] as const,
    detail: (id: string) => ["organization", id] as const,
    entities: () => ["entities"] as const,
    dashboardData: (id: string) => ["organizationDashboardData", id] as const,
};

export const departmentKeys = {
    all: ["departments"] as const,
};

export const shiftTemplateKeys = {
    all: ["shift-templates"] as const,
};

export const scheduleKeys = {
    all: ["schedules"] as const,
    confirmed: (month: number, year: number) => ["schedule", "confirmed", month, year] as const,
    data: (month: number, year: number) => ["scheduleData", month, year] as const,
    dataAll: () => ["scheduleData"] as const,
};

export const notificationKeys = {
    all: ["notifications"] as const,
    unreadCount: () => ["notifications", "unread-count"] as const,
};

export const vacationKeys = {
    all: ["vacations"] as const,
    byEmployee: (employeeId: number) => ["vacations", employeeId] as const,
    requests: (employeeId: number) => ["vacation-requests", employeeId] as const,
    pendingRequests: () => ["pending-vacation-requests"] as const,
};

export const sickLeaveKeys = {
    all: ["sick-leaves"] as const,
    byEmployee: (employeeId: number) => ["sick-leaves", employeeId] as const,
    requests: (employeeId: number) => ["sick-leave-requests", employeeId] as const,
    pendingRequests: () => ["pending-sick-leave-requests"] as const,
};

export const personalDayKeys = {
    all: ["personal-days"] as const,
    byEmployee: (employeeId: number) => ["personal-days", employeeId] as const,
    requests: (employeeId: number) => ["personal-day-requests", employeeId] as const,
    pendingRequests: () => ["pending-personal-day-requests"] as const,
};

export const preferenceKeys = {
    all: ["preferences"] as const,
    byEmployee: (employeeId: number) => ["preferences", employeeId] as const,
    weekDays: (employeeId: number) => ["week-days", employeeId] as const,
};