import {useMutation, useQuery, useQueryClient} from "@tanstack/react-query";
import api from "@/lib/api";
import {VacationDto, VacationRequestCreateDto, VacationRequestDto} from "@/types";
import {vacationKeys, notificationKeys} from "@/lib/api-keys";
import {useOrgStore} from "@/stores/orgStore";

export const useEmployeeVacations = (employeeId: number) => {
    const { currentOrgId } = useOrgStore();
    return useQuery<VacationDto[]>({
        queryKey: [...vacationKeys.byEmployee(employeeId), currentOrgId],
        queryFn: async () => {
            return await api.get(`/organizations/${currentOrgId}/vacations/${employeeId}`);
        },
        enabled: !!currentOrgId,
    });
};

export const useDeleteVacation = (employeeId: number) => {
    const qc = useQueryClient();
    const { currentOrgId } = useOrgStore();

    return useMutation({
        mutationFn: (id: number) =>
            api.delete(`/organizations/${currentOrgId}/vacations/${employeeId}/${id}`),
        onSuccess: () =>
            qc.invalidateQueries({ queryKey: vacationKeys.byEmployee(employeeId) }),
    });
};

export const useVacationRequests = (employeeId: number) => {
    const { currentOrgId } = useOrgStore();
    return useQuery<VacationRequestDto[]>({
        queryKey: [...vacationKeys.requests(employeeId), currentOrgId],
        queryFn: async () => {
            return await api.get(`/organizations/${currentOrgId}/vacations/requests/${employeeId}`);
        },
        enabled: !!currentOrgId,
    });
};

export const useAddVacationRequest = (employeeId: number) => {
    const qc = useQueryClient();

    return useMutation({
        mutationFn: (dto: VacationRequestCreateDto) =>
            api.post(`/vacations/requests`, dto),
        onSuccess: () =>
            qc.invalidateQueries({ queryKey: vacationKeys.requests(employeeId) }),
    });
};

export const useDeleteVacationRequest = (employeeId: number) => {
    const qc = useQueryClient();

    return useMutation({
        mutationFn: (id: number) =>
            api.delete(`/vacations/requests/${id}`),
        onSuccess: () =>
            qc.invalidateQueries({ queryKey: vacationKeys.requests(employeeId) }),
    });
};

//Employer hooks
export const usePendingVacationRequests = () => {
    const { currentOrgId } = useOrgStore();
    return useQuery<VacationRequestDto[]>({
        queryKey: [...vacationKeys.pendingRequests(), currentOrgId],
        queryFn: async () => {
            return await api.get(`/organizations/${currentOrgId}/vacations/pending-requests`);
        },
        enabled: !!currentOrgId,
    });
};

export const useApproveVacationRequest = () => {
    const qc = useQueryClient();
    const { currentOrgId } = useOrgStore();

    return useMutation({
        mutationFn: (id: number) =>
            api.post(`/organizations/${currentOrgId}/vacations/requests/${id}/approve`),
        onSuccess: () => {
            qc.invalidateQueries({ queryKey: vacationKeys.pendingRequests() });
            qc.invalidateQueries({ queryKey: vacationKeys.all });
            qc.invalidateQueries({ queryKey: notificationKeys.all });
            qc.invalidateQueries({ queryKey: notificationKeys.unreadCount() });
        },
    });
};

export const useAddApprovedVacation = (employeeId: number) => {
    const queryClient = useQueryClient();
    const { currentOrgId } = useOrgStore();

    return useMutation({
        mutationFn: async (vacation: VacationDto) => {
            return await api.post(`/organizations/${currentOrgId}/vacations/${employeeId}`, vacation);
        },
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: vacationKeys.byEmployee(employeeId) });
        },
    });
};

export const useRejectVacationRequest = () => {
    const qc = useQueryClient();
    const { currentOrgId } = useOrgStore();

    return useMutation({
        mutationFn: (id: number) =>
            api.post(`/organizations/${currentOrgId}/vacations/requests/${id}/reject`),
        onSuccess: () => {
            qc.invalidateQueries({ queryKey: vacationKeys.pendingRequests() });
            qc.invalidateQueries({ queryKey: vacationKeys.all });
            qc.invalidateQueries({ queryKey: notificationKeys.all });
            qc.invalidateQueries({ queryKey: notificationKeys.unreadCount() });
        },
    });
};

export const useGetVacations = (employeeId: number) => useEmployeeVacations(employeeId);
