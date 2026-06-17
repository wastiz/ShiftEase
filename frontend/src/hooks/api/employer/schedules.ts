import {useMutation, useQuery, useQueryClient} from "@tanstack/react-query";
import api from "@/lib/api";
import {toast} from "sonner";
import {isApiError} from "@/hooks/useApiError";
import {
    AcoGaScheduleGenerateRequest,
    AcoScheduleGenerateRequest, ApiError,
    GaScheduleGenerateRequest,
    Schedule,
    ScheduleEditorData,
    ScheduleGenerateRequest,
    ScheduleGenerateResult,
    SchedulePost,
    ScheduleRequest,
    ScheduleSummary,
    Shift
} from "@/types";
import {scheduleKeys} from "@/lib/api-keys";
import {useOrgStore} from "@/stores/orgStore";

export function useScheduleSummaries(enabled: boolean) {
    const { currentOrgId } = useOrgStore();
    return useQuery<ScheduleSummary>({
        queryKey: [...scheduleKeys.all, currentOrgId],
        queryFn: async () => {
            return await api.get<ScheduleSummary>(`/organizations/${currentOrgId}/schedules/schedule-summaries`);
        },
        enabled: enabled && !!currentOrgId,
    });
}

export function useGetConfirmedSchedule(month: number, year: number, departmentId?: number) {
    const { currentOrgId } = useOrgStore();
    return useQuery<Schedule>({
        queryKey: [...scheduleKeys.confirmed(month, year), departmentId, currentOrgId],
        queryFn: async () => {
            return await api.get<Schedule>(
                `/organizations/${currentOrgId}/schedules/confirmed?month=${month}&year=${year}&departmentId=${departmentId}`
            );
        },
        enabled: !!currentOrgId,
    });
}

export function useScheduleManagementData({ month, year }: ScheduleRequest) {
    const { currentOrgId } = useOrgStore();
    return useQuery<ScheduleEditorData>({
        queryKey: [...scheduleKeys.data(month, year), currentOrgId],
        queryFn: async () => {
            return await api.get<ScheduleEditorData>(
                `/organizations/${currentOrgId}/schedules/schedule-management?month=${month}&year=${year}`
            );
        },
        enabled: !!currentOrgId,
    });
}

type SaveScheduleParams = {
    startDate: string;
    endDate: string;
    shiftsData: Shift[];
};

export function useSaveSchedule({ startDate, endDate, shiftsData }: SaveScheduleParams) {
    const queryClient = useQueryClient();
    const { currentOrgId } = useOrgStore();

    return useMutation<void, Error, boolean>({
        mutationFn: async (isConfirmed: boolean) => {
            const payload: SchedulePost = {
                startDate,
                endDate,
                isConfirmed,
                shifts: shiftsData.map((s) => ({
                    shiftTypeId: s.shiftTypeId,
                    date: s.date,
                    employees: s.employees.map((e) => ({
                        id: e.id,
                        note: e.note || "",
                    })),
                })),
            };

            await api.post(`/organizations/${currentOrgId}/schedules/update-schedule`, payload);
        },
        onSuccess: () => {
            toast.success('Schedule saved!');
            queryClient.invalidateQueries({ queryKey: scheduleKeys.dataAll() });
        },
        onError: (error: unknown) => {
            if (isApiError(error) && error.errorCode === 'SCHEDULE_GENERATION_ERROR') {
                toast.error(`Schedule generation failed: ${error.message}`);
            } else {
                toast.error(isApiError(error) ? error.message : "Failed to save schedule");
            }
        }
    });
}

export function useAutoSaveSchedule() {
    const { currentOrgId } = useOrgStore();
    return useMutation<void, Error, { startDate: string; endDate: string; shifts: Shift[]; isConfirmed: boolean }>({
        mutationFn: async ({ startDate, endDate, shifts, isConfirmed }) => {
            const payload: SchedulePost = {
                startDate,
                endDate,
                isConfirmed,
                shifts: shifts.map((s) => ({
                    shiftTypeId: s.shiftTypeId,
                    date: s.date,
                    employees: s.employees.map((e) => ({
                        id: e.id,
                        note: e.note || "",
                    })),
                })),
            };
            await api.post(`/organizations/${currentOrgId}/schedules/update-schedule`, payload);
        },
    });
}

export function useUnconfirmSchedule() {
    const queryClient = useQueryClient();
    const { currentOrgId } = useOrgStore();

    return useMutation({
        mutationFn: async (scheduleId: number) => {
            return await api.post(`/organizations/${currentOrgId}/schedules/${scheduleId}/unconfirm`);
        },
        onSuccess: () => {
            toast.success('Schedule unconfirmed!');
            queryClient.invalidateQueries({ queryKey: scheduleKeys.dataAll() });
        },
        onError: () => {
            toast.error('Failed to unconfirm schedule');
        },
    });
}

export function useGenerateSchedule() {
    const queryClient = useQueryClient();
    const { currentOrgId } = useOrgStore();

    return useMutation<ScheduleGenerateResult, Error, Omit<ScheduleGenerateRequest, 'id'>>({
        mutationFn: async ({
            startDate,
            endDate,
            TotalHours,
            HardTotalHours,
        }) => {
            return await api.post<ScheduleGenerateResult>(`/organizations/${currentOrgId}/schedule-generator/generate-greedy`, {
                startDate,
                endDate,
                TotalHours,
                HardTotalHours,
            });
        },
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: scheduleKeys.dataAll() });
        },
    });
}

export function useGenerateAcoSchedule() {
    const queryClient = useQueryClient();
    const { currentOrgId } = useOrgStore();

    return useMutation<ScheduleGenerateResult, Error, AcoScheduleGenerateRequest>({
        mutationFn: async (payload) => {
            return await api.post<ScheduleGenerateResult>(`/organizations/${currentOrgId}/schedule-generator/generate-aco`, payload);
        },
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: scheduleKeys.dataAll() });
        },
    });
}

export function useGenerateGaSchedule() {
    const queryClient = useQueryClient();
    const { currentOrgId } = useOrgStore();

    return useMutation<ScheduleGenerateResult, Error, GaScheduleGenerateRequest>({
        mutationFn: async (payload) => {
            return await api.post<ScheduleGenerateResult>(`/organizations/${currentOrgId}/schedule-generator/generate-ga`, payload);
        },
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: scheduleKeys.dataAll() });
        },
    });
}

export function useGenerateAcoGaSchedule() {
    const queryClient = useQueryClient();
    const { currentOrgId } = useOrgStore();

    return useMutation<ScheduleGenerateResult, Error, AcoGaScheduleGenerateRequest>({
        mutationFn: async (payload) => {
            return await api.post<ScheduleGenerateResult>(`/organizations/${currentOrgId}/schedule-generator/generate-aco-ga`, payload);
        },
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: scheduleKeys.dataAll() });
        },
    });
}

export function useExportSchedule() {
    const { currentOrgId } = useOrgStore();
    return useMutation({
        mutationFn: async (scheduleId: number) => {
            await api.download(`/organizations/${currentOrgId}/schedules/${scheduleId}/export`, `schedule_${scheduleId}.xlsx`);
        },
    });
}
