import {useMutation, useQuery, useQueryClient} from "@tanstack/react-query";
import api from "@/lib/api";
import {toast} from "sonner";
import {Department, DepartmentFormValues} from "@/types";
import {departmentKeys} from "@/lib/api-keys";
import {useOrgStore} from "@/stores/orgStore";

export function useGetDepartments() {
    const { currentOrgId } = useOrgStore();
    return useQuery({
        queryKey: [...departmentKeys.all, currentOrgId],
        queryFn: async () => {
            return await api.get<Department[]>(`/organizations/${currentOrgId}/departments`);
        },
        enabled: !!currentOrgId,
    });
}

export function useCreateDepartment() {
    const queryClient = useQueryClient();
    const { currentOrgId } = useOrgStore();

    return useMutation({
        mutationFn: (payload: DepartmentFormValues) => api.post(`/organizations/${currentOrgId}/departments`, payload),
        onSuccess: () => {
            toast.success("Department created!");
            queryClient.invalidateQueries({ queryKey: departmentKeys.all });
        },
        onError: () => toast.error("Error creating department"),
    });
}

export function useUpdateDepartment(departmentId: number) {
    const queryClient = useQueryClient();
    const { currentOrgId } = useOrgStore();

    return useMutation({
        mutationFn: (payload: DepartmentFormValues) => api.put(`/organizations/${currentOrgId}/departments/${departmentId}`, payload),
        onSuccess: () => {
            toast.success("Department updated!");
            queryClient.invalidateQueries({ queryKey: departmentKeys.all });
        },
        onError: () => toast.error("Error updating department"),
    });
}

export function useDeleteDepartment() {
    const queryClient = useQueryClient();
    const { currentOrgId } = useOrgStore();

    return useMutation({
        mutationFn: (departmentId: number) => api.delete(`/organizations/${currentOrgId}/departments/${departmentId}`),
        onSuccess: () => {
            toast.success("Department deleted!");
            queryClient.invalidateQueries({ queryKey: departmentKeys.all });
        },
        onError: () => toast.error("Error deleting department"),
    });
}
