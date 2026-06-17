import { ReactNode } from "react";
import {SidebarTrigger} from "@/components/ui/shadcn/sidebar";
import {Separator} from "@/components/ui/shadcn/separator";

type HeaderProps = {
    title?: ReactNode;
    className?: string;
    children?: ReactNode;
}

export default function Header ({title, className, children}: HeaderProps) {
    return (
        <header className={`w-full h-1/15 flex items-center justify-between shrink-0 border-b px-4 ${className}`}>
            <div className={"flex gap-2 items-center"}>
                <SidebarTrigger className="-ml-1"/>
                <Separator
                    orientation="vertical"
                    className="mr-2 data-[orientation=vertical]:h-4 "
                />
                <h1 className={"text-xl font-bold"}>{title}</h1>
            </div>
            <div className={"flex gap-2"}>
                {children}
            </div>
        </header>
    )
}
