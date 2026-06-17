"use client";

import { useGetMe } from "@/hooks/api/auth";
import { usePathname } from "next/navigation";
import { isPublicRoute } from "@/lib/routes";
import { useAuthStore } from "@/stores/authStore";
import Loader from "@/components/ui/Loader";

export function AuthProvider({ children }: { children: React.ReactNode }) {
    const pathname = usePathname();
    const { user } = useAuthStore();
    const notPublicRoute = !isPublicRoute(pathname);

    useGetMe({ isEnabled: notPublicRoute });

    if (!user && notPublicRoute) {
        return <Loader></Loader>
    }

    return <>{children}</>;
}