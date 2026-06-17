import {
    Drawer,
    DrawerContent,
    DrawerDescription, DrawerFooter,
    DrawerHeader,
    DrawerTitle,
    DrawerTrigger
} from "@/components/ui/shadcn/drawer";
import { useIsMobile } from "@/hooks/use-mobile";

interface AsideDrawerProps {
    open: boolean
    onOpenChange: (open: boolean) => void
    title: string
    description?: string
    trigger?: React.ReactNode
    footer?: React.ReactNode
    children: React.ReactNode
}

export function AsideDrawer({ open, onOpenChange, title, description, trigger, footer, children }: AsideDrawerProps) {

    const isMobile = useIsMobile();

    return (
        <Drawer
            open={open}
            onOpenChange={onOpenChange}
            direction={isMobile ? "bottom" : "right"}
        >
            {trigger && <DrawerTrigger asChild>{trigger}</DrawerTrigger>}

            <DrawerContent
                onCloseAutoFocus={(e) => e.preventDefault()}
                className={
                    isMobile
                        ? "inset-x-0"
                        : "fixed inset-y-0 right-0 mt-0 w-96 flex h-full flex-col rounded-l-[10px] border bg-background"
                }
            >
                {/* Header */}
                <DrawerHeader className="gap-1">
                    <DrawerTitle>{title}</DrawerTitle>
                    {description && (
                        <DrawerDescription>{description}</DrawerDescription>
                    )}
                </DrawerHeader>

                {/* Content */}
                <div className={`flex-1 overflow-y-auto p-4`}>
                    {children}
                </div>

                {/* Footer */}
                {footer && (
                    <DrawerFooter className="flex gap-2 justify-end">
                        {footer}
                    </DrawerFooter>
                )}
            </DrawerContent>
        </Drawer>
    );
}
