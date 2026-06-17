import {useMutation, useQuery, useQueryClient} from "@tanstack/react-query";
import api from "@/lib/api";
import {toast} from "sonner";
import {ShiftTemplate, ShiftTemplateFormValues} from "@/types";
import {shiftTemplateKeys} from "@/lib/api-keys";
import {useOrgStore} from "@/stores/orgStore";


export function useGetShiftTemplates() {
    const { currentOrgId } = useOrgStore();
    return useQuery({
        queryKey: [...shiftTemplateKeys.all, currentOrgId],
        queryFn: async () => {
            return await api.get<ShiftTemplate[]>(`/organizations/${currentOrgId}/shift-templates`);
        },
        enabled: !!currentOrgId,
    });
}

export const useCreateShiftTemplate = () => {
    const queryClient = useQueryClient();
    const { currentOrgId } = useOrgStore();
    return useMutation({
        mutationFn: (payload: ShiftTemplateFormValues) => api.post(`/organizations/${currentOrgId}/shift-templates`, payload),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: shiftTemplateKeys.all });
            toast.success("Shift template created!");
        },
        onError: () => toast.error("Error creating shift template"),
    });
}

export const useUpdateShiftTemplate = (shiftTemplateId: number) => {
    const queryClient = useQueryClient();
    const { currentOrgId } = useOrgStore();
    return useMutation({
        mutationFn: (payload: ShiftTemplateFormValues) =>
            api.put(`/organizations/${currentOrgId}/shift-templates/${shiftTemplateId}`, payload),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: shiftTemplateKeys.all });
            toast.success("Shift template updated!");
        },
        onError: () => toast.error("Error updating shift template"),
    });
}

export const useDeleteShiftTemplate = () => {
    const queryClient = useQueryClient();
    const { currentOrgId } = useOrgStore();
    return useMutation({
        mutationFn: (shiftTemplateId: number) => api.delete(`/organizations/${currentOrgId}/shift-templates/${shiftTemplateId}`),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: shiftTemplateKeys.all });
            toast.success("Shift template deleted!");
        },
        onError: () => toast.error("Error deleting shift template"),
    });
}
