import {useMutation, useQuery, useQueryClient} from "@tanstack/react-query";
import {SickLeaveDto, SickLeaveRequestCreateDto, SickLeaveRequestDto} from "@/types";
import api from "@/lib/api";
import {sickLeaveKeys, notificationKeys} from "@/lib/api-keys";
import {useOrgStore} from "@/stores/orgStore";

export const useSickLeaves = (employeeId: number) => {
    const { currentOrgId } = useOrgStore();
    return useQuery<SickLeaveDto[]>({
        queryKey: [...sickLeaveKeys.byEmployee(employeeId), currentOrgId],
        queryFn: async () => {
            return await api.get(`/organizations/${currentOrgId}/sick-leaves/${employeeId}`);
        },
        enabled: !!currentOrgId,
    });
};

export const useSickLeaveRequests = (employeeId: number) => {
    const { currentOrgId } = useOrgStore();
    return useQuery<SickLeaveRequestDto[]>({
        queryKey: [...sickLeaveKeys.requests(employeeId), currentOrgId],
        queryFn: async () => {
            return await api.get(`/organizations/${currentOrgId}/sick-leaves/requests/${employeeId}`);
        },
        enabled: !!currentOrgId,
    });
};

export const usePendingSickLeaveRequests = () => {
    const { currentOrgId } = useOrgStore();
    return useQuery<SickLeaveRequestDto[]>({
        queryKey: [...sickLeaveKeys.pendingRequests(), currentOrgId],
        queryFn: async () => {
            return await api.get(`/organizations/${currentOrgId}/sick-leaves/pending-requests`);
        },
        enabled: !!currentOrgId,
    });
};

export const useAddApprovedSickLeave = (employeeId: number) => {
    const queryClient = useQueryClient();
    const { currentOrgId } = useOrgStore();

    return useMutation({
        mutationFn: async (sickLeave: SickLeaveDto) => {
            return await api.post(`/organizations/${currentOrgId}/sick-leaves/${employeeId}`, sickLeave);
        },
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: sickLeaveKeys.byEmployee(employeeId) });
        },
    });
};

export const useDeleteSickLeave = (employeeId: number) => {
    const queryClient = useQueryClient();
    const { currentOrgId } = useOrgStore();

    return useMutation({
        mutationFn: async (id: number) => {
            await api.delete(`/organizations/${currentOrgId}/sick-leaves/${employeeId}/${id}`);
        },
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: sickLeaveKeys.byEmployee(employeeId) });
        },
    });
};

export const useAddSickLeaveRequest = (employeeId: number) => {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: async (dto: SickLeaveRequestCreateDto) => {
            return await api.post(`/sick-leaves/requests`, dto);
        },
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: sickLeaveKeys.requests(employeeId) });
        },
    });
};

export const useDeleteSickLeaveRequest = (employeeId: number) => {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: async (id: number) => {
            await api.delete(`/sick-leaves/requests/${id}`);
        },
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: sickLeaveKeys.requests(employeeId) });
        },
    });
};

export const useApproveSickLeaveRequest = () => {
    const qc = useQueryClient();
    const { currentOrgId } = useOrgStore();

    return useMutation({
        mutationFn: (id: number) =>
            api.post(`/organizations/${currentOrgId}/sick-leaves/requests/${id}/approve`),
        onSuccess: () => {
            qc.invalidateQueries({ queryKey: sickLeaveKeys.pendingRequests() });
            qc.invalidateQueries({ queryKey: sickLeaveKeys.all });
            qc.invalidateQueries({ queryKey: notificationKeys.all });
            qc.invalidateQueries({ queryKey: notificationKeys.unreadCount() });
        },
    });
};

export const useRejectSickLeaveRequest = () => {
    const qc = useQueryClient();
    const { currentOrgId } = useOrgStore();

    return useMutation({
        mutationFn: (id: number) =>
            api.post(`/organizations/${currentOrgId}/sick-leaves/requests/${id}/reject`),
        onSuccess: () => {
            qc.invalidateQueries({ queryKey: sickLeaveKeys.pendingRequests() });
            qc.invalidateQueries({ queryKey: sickLeaveKeys.all });
            qc.invalidateQueries({ queryKey: notificationKeys.all });
            qc.invalidateQueries({ queryKey: notificationKeys.unreadCount() });
        },
    });
};

export const useGetSickLeaves = (employeeId: number) => useSickLeaves(employeeId);
