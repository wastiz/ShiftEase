import {useMutation, useQuery, useQueryClient} from "@tanstack/react-query";
import {PersonalDayDto, PersonalDayRequestDto} from "@/types";
import api from "@/lib/api";
import {personalDayKeys, notificationKeys} from "@/lib/api-keys";
import {useOrgStore} from "@/stores/orgStore";

export const usePersonalDays = (employeeId: number) => {
    const { currentOrgId } = useOrgStore();
    return useQuery<PersonalDayDto[]>({
        queryKey: [...personalDayKeys.byEmployee(employeeId), currentOrgId],
        queryFn: async () => {
            return await api.get(`/organizations/${currentOrgId}/personal-days/${employeeId}`);
        },
        enabled: !!currentOrgId,
    });
};

export const usePersonalDayRequests = (employeeId: number) => {
    const { currentOrgId } = useOrgStore();
    return useQuery<PersonalDayRequestDto[]>({
        queryKey: [...personalDayKeys.requests(employeeId), currentOrgId],
        queryFn: async () => {
            return await api.get(`/organizations/${currentOrgId}/personal-days/requests/${employeeId}`);
        },
        enabled: !!currentOrgId,
    });
};

export const usePendingPersonalDayRequests = () => {
    const { currentOrgId } = useOrgStore();
    return useQuery<PersonalDayRequestDto[]>({
        queryKey: [...personalDayKeys.pendingRequests(), currentOrgId],
        queryFn: async () => {
            return await api.get(`/organizations/${currentOrgId}/personal-days/pending-requests`);
        },
        enabled: !!currentOrgId,
    });
};

export const useDeletePersonalDay = (employeeId: number) => {
    const queryClient = useQueryClient();
    const { currentOrgId } = useOrgStore();

    return useMutation({
        mutationFn: async (id: number) => {
            await api.delete(`/organizations/${currentOrgId}/personal-days/${employeeId}/${id}`);
        },
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: personalDayKeys.byEmployee(employeeId) });
        },
    });
};

export const useAddApprovedPersonalDay = (employeeId: number) => {
    const queryClient = useQueryClient();
    const { currentOrgId } = useOrgStore();

    return useMutation({
        mutationFn: async (personalDay: PersonalDayDto) => {
            return await api.post(`/organizations/${currentOrgId}/personal-days/${employeeId}`, personalDay);
        },
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: personalDayKeys.byEmployee(employeeId) });
        },
    });
};

export const useApprovePersonalDayRequest = () => {
    const qc = useQueryClient();
    const { currentOrgId } = useOrgStore();

    return useMutation({
        mutationFn: (id: number) =>
            api.post(`/organizations/${currentOrgId}/personal-days/requests/${id}/approve`),
        onSuccess: () => {
            qc.invalidateQueries({ queryKey: personalDayKeys.pendingRequests() });
            qc.invalidateQueries({ queryKey: personalDayKeys.all });
            qc.invalidateQueries({ queryKey: notificationKeys.all });
            qc.invalidateQueries({ queryKey: notificationKeys.unreadCount() });
        },
    });
};

export const useRejectPersonalDayRequest = () => {
    const qc = useQueryClient();
    const { currentOrgId } = useOrgStore();

    return useMutation({
        mutationFn: (id: number) =>
            api.post(`/organizations/${currentOrgId}/personal-days/requests/${id}/reject`),
        onSuccess: () => {
            qc.invalidateQueries({ queryKey: personalDayKeys.pendingRequests() });
            qc.invalidateQueries({ queryKey: personalDayKeys.all });
            qc.invalidateQueries({ queryKey: notificationKeys.all });
            qc.invalidateQueries({ queryKey: notificationKeys.unreadCount() });
        },
    });
};

export const useGetPersonalDays = (employeeId: number) => usePersonalDays(employeeId);
