import { useAuthStore } from "@/stores/authStore";

const BASE_URL = process.env.NEXT_PUBLIC_API_URL;

let refreshPromise: Promise<boolean> | null = null;

async function apiFetch<T>(path: string, options: RequestInit = {}): Promise<T> {
    const res = await fetch(`${BASE_URL}${path}`, {
        ...options,
        credentials: "include",
        headers: { "Content-Type": "application/json", ...options.headers },
    });

    if (res.status === 401) {
        let errorMessage = "Unauthorized";
        let errorCode = "UNAUTHORIZED";
        try {
            const body = await res.clone().json();
            errorMessage = body.message ?? errorMessage;
            errorCode = body.errorCode ?? errorCode;
        } catch {}

        // Login and register endpoints return 401 for credential/verification
        // failures — never for an expired token. Skip refresh for these so that
        // a stale session cookie cannot silently re-authenticate and redirect
        // the user unexpectedly.
        const isCredentialEndpoint = /\/(login|register)$/.test(path);

        if (!isCredentialEndpoint) {
            const refreshed = await tryRefresh();
            if (refreshed) {
                const retry = await fetch(`${BASE_URL}${path}`, {
                    ...options,
                    credentials: "include",
                    headers: { "Content-Type": "application/json", ...options.headers },
                });
                if (!retry.ok) throw new ApiError(retry.status, await retry.text());
                if (retry.status === 204) return null as T;
                return retry.json();
            }
            useAuthStore.getState().clear();
            window.location.href = "/sign-in";
        }

        throw new ApiError(401, errorMessage, errorCode);
    }

    if (res.status === 204) return null as T;
    if (!res.ok) {
        let body: { message?: string; errorCode?: string; errors?: Record<string, string[]> } | null = null;
        try {
            body = await res.json();
        } catch {}
        throw new ApiError(
            res.status,
            body?.message ?? "Unknown error",
            body?.errorCode ?? "UNKNOWN_ERROR",
            body?.errors ?? null,
        );
    }
    return res.json();
}

async function tryRefresh(): Promise<boolean> {
    if (refreshPromise) return refreshPromise;
    refreshPromise = fetch(`${BASE_URL}/auth/refresh`, {
        method: "POST",
        credentials: "include",
    })
        .then(res => res.ok)
        .catch(() => false)
        .finally(() => { refreshPromise = null; });
    return refreshPromise;
}

export class ApiError extends Error {
    constructor(
        public status: number,
        message: string,
        public errorCode: string = 'UNKNOWN_ERROR',
        public errors: Record<string, string[]> | null = null,
    ) {
        super(message);
        this.name = 'ApiError';
    }
}

const api = {
    get: <T>(path: string, options?: RequestInit) =>
        apiFetch<T>(path, { ...options, method: "GET" }),

    post: <T>(path: string, body?: unknown, options?: RequestInit) =>
        apiFetch<T>(path, { ...options, method: "POST", body: JSON.stringify(body) }),

    put: <T>(path: string, body?: unknown, options?: RequestInit) =>
        apiFetch<T>(path, { ...options, method: "PUT", body: JSON.stringify(body) }),

    patch: <T>(path: string, body?: unknown, options?: RequestInit) =>
        apiFetch<T>(path, { ...options, method: "PATCH", body: JSON.stringify(body) }),

    delete: <T>(path: string, body?: unknown, options?: RequestInit) =>
        apiFetch<T>(path, { ...options, method: "DELETE", ...(body !== undefined ? { body: JSON.stringify(body) } : {}) }),

    download: async (path: string, filename: string) => {
        const res = await fetch(`${BASE_URL}${path}`, {
            credentials: "include",
        });

        if (!res.ok) throw new ApiError(res.status, await res.text());

        const blob = await res.blob();
        const url = URL.createObjectURL(blob);

        const a = document.createElement("a");
        a.href = url;
        a.download = filename;
        a.click();

        URL.revokeObjectURL(url);
    },
};

export default api;