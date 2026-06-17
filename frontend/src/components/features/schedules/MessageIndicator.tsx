import { AlertTriangle, XCircle } from "lucide-react";
import { Button } from "@/components/ui/shadcn/button";
import {
    Tooltip,
    TooltipContent,
    TooltipProvider,
    TooltipTrigger,
} from "@/components/ui/shadcn/tooltip";
import clsx from "clsx";

type MessageType = "error" | "warning";

interface MessageIndicatorProps {
    message: string;
    messageType: MessageType;
}

export function MessageIndicator({
                                     message,
                                     messageType,
                                 }: MessageIndicatorProps) {
    const Icon = messageType === "error" ? XCircle : AlertTriangle;

    return (
        <TooltipProvider>
            <Tooltip>
                <TooltipTrigger asChild>
                    <Button
                        variant="ghost"
                        size="icon"
                        className={clsx(
                            "h-6 w-6 p-0",
                            messageType === "error" && "text-destructive",
                            messageType === "warning" && "text-yellow-500"
                        )}
                    >
                        <Icon className="h-4 w-4" />
                    </Button>
                </TooltipTrigger>

                <TooltipContent side="bottom" align="center">
                    <p className="max-w-xs text-sm">{message}</p>
                </TooltipContent>
            </Tooltip>
        </TooltipProvider>
    );
}
