import {useMutation, useQuery, useQueryClient} from "@tanstack/react-query";
import api from "@/lib/api";
import {Organization, OrganizationDashboardData, OrganizationFormValues} from "@/types/organizations";
import {CheckEntitiesResult} from "@/types";
import {organizationKeys} from "@/lib/api-keys";
import {useOrgStore} from "@/stores/orgStore";

export function useCheckEntities() {
    const { currentOrgId } = useOrgStore();
    return useQuery<CheckEntitiesResult>({
        queryKey: [...organizationKeys.entities(), currentOrgId],
        queryFn: async () => {
            return await api.get<CheckEntitiesResult>(`/organizations/${currentOrgId}/check-entities`);
        },
        enabled: !!currentOrgId,
    })
}

export function useGetOrganizations() {
    return useQuery({
        queryKey: organizationKeys.all,
        queryFn: async () => {
            return await api.get<Organization[]>("/organizations")
        },
    });
}

export function useGetOrganization(id: string, p0: { enabled: boolean; }) {
    return useQuery({
        queryKey: organizationKeys.detail(id),
        queryFn: async () => {
            return await api.get<Organization>(`/organizations/${id}`);
        },
        enabled: !!id && id !== "create",
    });
}

export function useGetOrganizationData(id?: string) {
    return useQuery({
        queryKey: organizationKeys.dashboardData(id!),
        queryFn: async () => {
            return await api.get<OrganizationDashboardData>(`/organizations/data/${id}`);
        },
        enabled: !!id
    });
}

export function useAddOrganization() {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: async (dto: OrganizationFormValues) => {
            return await api.post('/organizations', dto);
        },
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: organizationKeys.all });
        },
    });
}
export function useUpdateOrganization() {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: async (dto: OrganizationFormValues & { id: string | number }) => {
            const { id, ...data } = dto;
            return await api.put(`/organizations/${id}`, data);
        },
        onSuccess: (_, variables) => {
            queryClient.invalidateQueries({ queryKey: organizationKeys.all });
            queryClient.invalidateQueries({ queryKey: organizationKeys.detail(String(variables.id)) });
        },
    });
}

export function useDeleteOrganization() {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: async ({ id, password }: { id: string | number; password: string }) => {
            return await api.delete(`/organizations/${id}`, { password });
        },
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: organizationKeys.all });
        },
    });
}
