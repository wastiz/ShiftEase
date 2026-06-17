"use client";

import React, { useEffect } from "react";
import {AppSidebar} from "@/components/ui/shadcn/app-sidebar";
import {SidebarInset, SidebarProvider} from "@/components/ui/shadcn/sidebar";
import {useTranslations} from 'next-intl';
import {useAuthStore} from "@/stores/authStore";
import {useOrgStore} from "@/stores/orgStore";
import {useRouter} from "next/navigation";
import Loader from "@/components/ui/Loader";

export default function EmployerLayout({
    children,
}: {
    children: React.ReactNode;
}) {
    const t = useTranslations('nav');
    const { user } = useAuthStore();
    const { currentOrgId, _hasHydrated } = useOrgStore();
    const router = useRouter();

    useEffect(() => {
        if (_hasHydrated && !currentOrgId) {
            router.replace("/organizations");
        }
    }, [_hasHydrated, currentOrgId, router]);

    if (!_hasHydrated || !currentOrgId) {
        return (
            <div className="flex min-h-screen items-center justify-center">
                <Loader />
            </div>
        );
    }

    const navMain = [
        { title: t('backToOrganizations'), url: "/organizations", icon: "ArrowBigLeft" as const, isActive: true },
        { title: t('dashboard'), url: "/dashboard", icon: "LayoutDashboard" as const, isActive: true },
        { title: t('schedules'), url: "/schedules", icon: "CalendarCheck" as const },
        { title: t('employees'), url: "/employees", icon: "IdCardLanyard" as const },
        { title: t('timeOff'), url: "/employees/time-off", icon: "TreePalm" as const },
        { title: t('departments'), url: "/departments", icon: "Building2" as const },
        { title: t('departmentsWork'), url: "/departments/work", icon: "CalendarDays" as const },
        { title: t('shiftTemplates'), url: "/shift-templates", icon: "FileType" as const },
    ];

    return (
        <SidebarProvider>
            <AppSidebar user={user} navMain={navMain} showOrgSwitcher />
            <SidebarInset className="h-screen overflow-y-auto">
                {children}
            </SidebarInset>
        </SidebarProvider>
    );
}
