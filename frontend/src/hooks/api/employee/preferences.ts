import {useMutation, useQuery, useQueryClient} from "@tanstack/react-query";
import {PreferenceBundle, WeekDayPreferenceDto} from "@/types";
import api from "@/lib/api";
import {preferenceKeys} from "@/lib/api-keys";
import {useOrgStore} from "@/stores/orgStore";

export const usePreferences = (employeeId: number) => {
    const { currentOrgId } = useOrgStore();
    return useQuery<PreferenceBundle>({
        queryKey: [...preferenceKeys.byEmployee(employeeId), currentOrgId],
        queryFn: async () => {
            return await api.get(`/organizations/${currentOrgId}/preferences/${employeeId}`);
        },
        enabled: !!currentOrgId,
    });
};

export const useSavePreferences = (employeeId: number) => {
    const qc = useQueryClient();
    const { currentOrgId } = useOrgStore();

    return useMutation({
        mutationFn: (dto: PreferenceBundle) =>
            api.post(`/organizations/${currentOrgId}/preferences/${employeeId}`, dto),
        onSuccess: () =>
            qc.invalidateQueries({ queryKey: preferenceKeys.byEmployee(employeeId) }),
    });
};

export const useWeekDays = (employeeId: number) => {
    const { currentOrgId } = useOrgStore();
    return useQuery<WeekDayPreferenceDto[]>({
        queryKey: [...preferenceKeys.weekDays(employeeId), currentOrgId],
        queryFn: async () => {
            return await api.get(`/organizations/${currentOrgId}/preferences/${employeeId}/week-days`);
        },
        enabled: !!currentOrgId,
    });
};
