"use client";

import React from "react";
import {AppSidebar} from "@/components/ui/shadcn/app-sidebar";
import {SidebarInset, SidebarProvider} from "@/components/ui/shadcn/sidebar";
import {useTranslations} from 'next-intl';
import {useAuthStore} from "@/stores/authStore";

export default function EmployeeLayout({
    children,
}: {
    children: React.ReactNode;
}) {
    const t = useTranslations('nav');
    const { user: userData } = useAuthStore();

    const user = {
        name: userData?.name ?? "User",
        email: userData?.email ?? "",
        avatar: "/images/avatar_placeholder.png",
    };

    const navMain = [
        { title: t('overview'), url: "/overview", icon: "LayoutDashboard" as const, isActive: true },
        { title: t('preferences'), url: "/preferences", icon: "Bolt" as const },
        { title: t('sickLeave'), url: "/sick-leave", icon: "CalendarPlus2" as const },
        { title: t('vacation'), url: "/vacation", icon: "TreePalm" as const },
        { title: t('personalDays'), url: "/personal-day", icon: "CalendarPlus2" as const },
    ];

    return (
        <SidebarProvider>
            <AppSidebar user={user} navMain={navMain} />
            <SidebarInset className="h-screen overflow-y-auto">
                {children}
            </SidebarInset>
        </SidebarProvider>
    );
}
