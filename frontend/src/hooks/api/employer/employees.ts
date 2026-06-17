import {useMutation, useQuery, useQueryClient} from "@tanstack/react-query";
import api from "@/lib/api";
import {BulkCreateResult, Employee, EmployeeTimeOff, GetEmployeesTimeOffsParams} from "@/types";
import {employeeKeys} from "@/lib/api-keys";
import {useOrgStore} from "@/stores/orgStore";

export function useGetEmployees() {
    const { currentOrgId } = useOrgStore();
    return useQuery({
        queryKey: [...employeeKeys.all, currentOrgId],
        queryFn: async () => {
            return await api.get<Employee[]>(`/organizations/${currentOrgId}/employees`);
        },
        enabled: !!currentOrgId,
    });
}

export function useGetEmployee(id: number) {
    const { currentOrgId } = useOrgStore();
    return useQuery({
        queryKey: [...employeeKeys.detail(id), currentOrgId],
        queryFn: async () => {
            return await api.get<Employee>(`/organizations/${currentOrgId}/employees/${id}`);
        },
        enabled: !!id && !!currentOrgId,
    });
}

export function useCreateEmployee() {
    const queryClient = useQueryClient();
    const { currentOrgId } = useOrgStore();
    return useMutation({
        mutationFn: async (employee: Partial<Employee>) => {
            return await api.post(`/organizations/${currentOrgId}/employees`, employee);
        },
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: employeeKeys.all });
        },
    });
}

export function useBulkCreateEmployees() {
    const { currentOrgId } = useOrgStore();
    return useMutation({
        mutationFn: async (employees: Partial<Employee>[]) => {
            return await api.post<BulkCreateResult>(`/organizations/${currentOrgId}/employees/bulk`, employees);
        }
    });
}

export function useUpdateEmployee(id: number) {
    const queryClient = useQueryClient();
    const { currentOrgId } = useOrgStore();
    return useMutation({
        mutationFn: async (employee: Partial<Employee>) => {
            return await api.put(`/organizations/${currentOrgId}/employees/${id}`, employee);
        },
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: employeeKeys.all });
        },
    });
}

export function useGetEmployeesTimeOffs(params: GetEmployeesTimeOffsParams = {}, enabled = true) {
    const { year, month, employeeIds } = params;
    const { currentOrgId } = useOrgStore();
    return useQuery<EmployeeTimeOff[]>({
        queryKey: [...employeeKeys.timeOffs(year, month, employeeIds), currentOrgId],
        queryFn: async () => {
            return await api.post<EmployeeTimeOff[]>(`/organizations/${currentOrgId}/employees/time-offs`, {
                ...(year !== undefined && {year}),
                ...(month !== undefined && {month}),
                ...(employeeIds?.length && {employeeIds}),
            });
        },
        enabled: enabled && !!currentOrgId,
    });
}

export function useDeleteEmployee(id: number) {
    const queryClient = useQueryClient();
    const { currentOrgId } = useOrgStore();
    return useMutation({
        mutationFn: async () => {
            return await api.delete(`/organizations/${currentOrgId}/employees/${id}`);
        },
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: employeeKeys.all });
        },
    });
}
