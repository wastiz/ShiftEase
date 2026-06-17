"use client"

import * as React from "react"
import {
    LayoutDashboard,
    Bell,
    UserRound,
    FileType,
    IdCardLanyard, CalendarCheck, Bolt, CalendarPlus2, TreePalm, ArrowBigLeft,
    Building2, CalendarDays, AlertCircle, MessageSquare
} from "lucide-react"
import { useTranslations } from 'next-intl'
import ReportProblemModal from "@/components/features/ReportProblemModal"
import FeedbackModal from "@/components/features/FeedbackModal"
import { OrganizationSwitcher } from "@/components/features/OrganizationSwitcher"

import { NavMain } from "@/components/ui/shadcn/nav-main"
import { NavUser } from "@/components/ui/shadcn/nav-user"

import {
    Sidebar,
    SidebarContent,
    SidebarFooter, SidebarGroup,
    SidebarHeader, SidebarMenuButton, SidebarMenuItem, SidebarMenuSub, SidebarMenuSubButton, SidebarMenuSubItem,
    SidebarRail, SidebarTrigger, useSidebar,
} from "@/components/ui/shadcn/sidebar"
import Logo from "@/components/ui/Logo";
import ThemeToggle from "@/components/ui/inputs/ThemeToggle";
import NotificationWindow from "@/components/features/NotificationWindow";
import { useUnreadNotificationsCount } from "@/hooks/api/notification";
import { Popover, PopoverContent, PopoverTrigger } from "@/components/ui/shadcn/popover";
import {MeResponse} from "@/types";

export interface NavItem {
    title: string
    url: string
    icon?: keyof typeof iconMap
    isActive?: boolean
    items?: { title: string; url: string }[]
}

export interface AppSidebarProps extends React.ComponentProps<typeof Sidebar> {
    user: MeResponse | null
    navMain: NavItem[]
    showOrgSwitcher?: boolean
}

const iconMap = {
    LayoutDashboard,
    CalendarCheck,
    CalendarDays,
    IdCardLanyard,
    UserRound,
    FileType,
    Bolt,
    CalendarPlus2,
    TreePalm,
    ArrowBigLeft,
    Building2
}

export function AppSidebar({ user, navMain, showOrgSwitcher, ...props }: AppSidebarProps) {
    const { state, isMobile } = useSidebar()
    const isCollapsed = state === "collapsed"
    const tCommon = useTranslations('common')
    const tSidebar = useTranslations('sidebar')
    const [notificationsOpen, setNotificationsOpen] = React.useState(false)
    const [reportOpen, setReportOpen] = React.useState(false)
    const [feedbackOpen, setFeedbackOpen] = React.useState(false)
    const { data: unreadCount } = useUnreadNotificationsCount()

    const mappedNavMain = navMain.map(item => ({
        ...item,
        icon: item.icon ? iconMap[item.icon] : undefined,
    }))

    return (
        <Sidebar collapsible="icon" {...props}>
            <SidebarHeader>
                <Logo variant={isCollapsed ? "small" : "big"} className="mt-3 ml-1.5" />
            </SidebarHeader>

            <SidebarContent>
                <NavMain items={mappedNavMain} />
            </SidebarContent>

            <SidebarFooter>
                {showOrgSwitcher && <OrganizationSwitcher />}
                <SidebarMenuItem>
                    <Popover open={notificationsOpen} onOpenChange={setNotificationsOpen}>
                        <PopoverTrigger asChild>
                            <SidebarMenuButton
                                tooltip={tCommon('notifications')}
                                className="relative"
                            >
                                <Bell />
                                <span>{tCommon('notifications')}</span>
                                {!!unreadCount && unreadCount > 0 && (
                                    <span className="absolute top-1 left-5 w-2 h-2 bg-orange-400 rounded-full" />
                                )}
                            </SidebarMenuButton>
                        </PopoverTrigger>
                        <PopoverContent
                            side={isMobile ? "top" : "right"}
                            align="end"
                            sideOffset={8}
                            className="w-auto p-0 border-0 bg-transparent shadow-none"
                        >
                            <NotificationWindow onClose={() => setNotificationsOpen(false)} />
                        </PopoverContent>
                    </Popover>
                </SidebarMenuItem>
                <SidebarMenuItem>
                    <ThemeToggle />
                </SidebarMenuItem>
                <SidebarMenuItem>
                    <SidebarMenuButton
                        tooltip={tSidebar('reportProblem')}
                        onClick={() => setReportOpen(true)}
                    >
                        <AlertCircle />
                        <span>{tSidebar('reportProblem')}</span>
                    </SidebarMenuButton>
                </SidebarMenuItem>
                <SidebarMenuItem>
                    <SidebarMenuButton
                        tooltip={tSidebar('feedback')}
                        onClick={() => setFeedbackOpen(true)}
                    >
                        <MessageSquare />
                        <span>{tSidebar('feedback')}</span>
                    </SidebarMenuButton>
                </SidebarMenuItem>
                <NavUser user={user} />
            </SidebarFooter>

            <ReportProblemModal open={reportOpen} onClose={() => setReportOpen(false)} />
            <FeedbackModal open={feedbackOpen} onClose={() => setFeedbackOpen(false)} />

            <SidebarRail />
        </Sidebar>
    )
}
