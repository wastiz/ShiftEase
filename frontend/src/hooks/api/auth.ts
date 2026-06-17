import {useMutation, useQuery, useQueryClient} from "@tanstack/react-query";
import {useEffect} from "react";
import api from "@/lib/api";
import {useAuthStore} from "@/stores/authStore";
import {useOrgStore} from "@/stores/orgStore";
import {authKeys} from "@/lib/api-keys";
import {useRouter} from "next/navigation";
import type {
    ForgotPasswordPayload,
    LoginPayload,
    LoginResponse,
    MeResponse,
    RegisterPayload,
    RegisterResponse,
    UserRole,
} from "@/types";

const AUTH_STALE_TIME = 15 * 60 * 1000;

export function useGetMe({ isEnabled }: { isEnabled: boolean }) {
    const { setUser } = useAuthStore();

    const query = useQuery({
        queryKey: authKeys.currentUser(),
        queryFn: async () => await api.get<MeResponse>("/auth/me"),
        enabled: isEnabled,
        retry: false,
        staleTime: AUTH_STALE_TIME,
        refetchOnWindowFocus: false,
    });

    useEffect(() => {
        if (query.data) {
            setUser(query.data);
        }
    }, [query.data]);

    return query;
}

export function useEmployerRegister() {
    return useMutation<RegisterResponse, Error, RegisterPayload>({
        mutationFn: async (payload) => {
            return await api.post<RegisterResponse>("/auth/employer/register", payload);
        },
    });
}

export function useLogin(role: UserRole) {
    const queryClient = useQueryClient();
    const router = useRouter();

    return useMutation<LoginResponse, Error, LoginPayload>({
        mutationFn: async (payload) => {
            return await api.post<LoginResponse>(
                `/auth/${role.toLowerCase()}/login`,
                payload
            );
        },
        onSuccess: async () => {
            await queryClient.invalidateQueries({ queryKey: authKeys.currentUser() });
            if (role === "Employer") {
                const { currentOrgId } = useOrgStore.getState();
                router.push(currentOrgId ? "/dashboard" : "/organizations");
            } else {
                router.push("/my-shifts");
            }
        },
    });
}

export function useLogout() {
    const router = useRouter();
    const queryClient = useQueryClient();
    const { clear } = useAuthStore();
    const { clearCurrentOrg } = useOrgStore();

    return useMutation<void, Error, void>({
        mutationFn: () => api.post("/auth/logout"),
        onSuccess: () => {
            clear();
            clearCurrentOrg();
            queryClient.clear();
            router.push("/sign-in");
        },
        onError: () => {
            clear();
            clearCurrentOrg();
            queryClient.clear();
            router.push("/sign-in");
        },
    });
}

export function useForgotPassword() {
    return useMutation<void, Error, ForgotPasswordPayload>({
        mutationFn: async (payload) => {
            await api.post("/auth/forgot-password", payload);
        },
    });
}

export function useResetPassword() {
    const router = useRouter();

    return useMutation<void, Error, { token: string; newPassword: string }>({
        mutationFn: async (payload) => {
            await api.post("/auth/reset-password", payload);
        },
        onSuccess: () => {
            router.push("/sign-in");
        },
    });
}

export function useChangePassword() {
    return useMutation<void, Error, { currentPassword: string; newPassword: string }>({
        mutationFn: async (payload) => {
            await api.patch("/auth/change-password", payload);
        },
    });
}

export function useUpdatePhone() {
    const { setUser, user } = useAuthStore();
    return useMutation<void, Error, { phone: string }>({
        mutationFn: async (payload) => {
            await api.patch("/auth/profile", payload);
        },
        onSuccess: (_, { phone }) => {
            if (user) setUser({ ...user, phone });
        },
    });
}

export function useDeleteAccount() {
    const router = useRouter();
    const queryClient = useQueryClient();
    const { clear } = useAuthStore();
    const { clearCurrentOrg } = useOrgStore();

    return useMutation<void, Error, { password: string }>({
        mutationFn: async ({ password }) => {
            await api.delete("/auth/account", { password });
        },
        onSuccess: () => {
            clear();
            clearCurrentOrg();
            queryClient.clear();
            router.push("/sign-in");
        },
    });
}