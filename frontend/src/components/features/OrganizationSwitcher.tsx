"use client"

import { useState } from "react"
import { useRouter } from "next/navigation"
import { Building2, ChevronDown, Check } from "lucide-react"
import { useOrgStore } from "@/stores/orgStore"
import { useGetOrganizations } from "@/hooks/api"
import {
    SidebarMenuButton,
    SidebarMenuItem,
    useSidebar,
} from "@/components/ui/shadcn/sidebar"
import { Popover, PopoverContent, PopoverTrigger } from "@/components/ui/shadcn/popover"

export function OrganizationSwitcher() {
    const router = useRouter()
    const { state } = useSidebar()
    const isCollapsed = state === "collapsed"
    const { currentOrgId, setCurrentOrg } = useOrgStore()
    const { data: organizations } = useGetOrganizations()
    const [open, setOpen] = useState(false)

    const currentOrg = organizations?.find(org => org.id === currentOrgId)

    const handleSwitch = (orgId: number) => {
        localStorage.setItem("orgId", String(orgId))
        setCurrentOrg(orgId)
        setOpen(false)
        router.push("/dashboard")
    }

    return (
        <SidebarMenuItem>
            <Popover open={open} onOpenChange={setOpen}>
                <PopoverTrigger asChild>
                    <SidebarMenuButton tooltip={currentOrg?.name ?? "Organization"}>
                        <Building2 className="shrink-0" />
                        <span className="truncate">{currentOrg?.name ?? "..."}</span>
                        {!isCollapsed && (
                            <ChevronDown className="ml-auto h-4 w-4 shrink-0 opacity-50" />
                        )}
                    </SidebarMenuButton>
                </PopoverTrigger>
                <PopoverContent side="right" align="end" sideOffset={8} className="w-56 p-1">
                    {organizations?.map(org => (
                        <button
                            key={org.id}
                            className="flex w-full items-center gap-2 rounded-sm px-2 py-1.5 text-sm hover:bg-accent transition-colors"
                            onClick={() => handleSwitch(org.id)}
                        >
                            <Check
                                className={`h-4 w-4 shrink-0 ${org.id === currentOrgId ? "opacity-100" : "opacity-0"}`}
                            />
                            <span className="truncate">{org.name}</span>
                        </button>
                    ))}
                </PopoverContent>
            </Popover>
        </SidebarMenuItem>
    )
}
