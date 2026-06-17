import {CheckCircle2} from "lucide-react";
import {cn} from "@/lib/utils";

export default function StepIndicator({
                           index,
                           isCompleted,
                           isActive,
                       }: {
    index: number;
    isCompleted: boolean;
    isActive: boolean;
}) {
    if (isCompleted) {
        return (
            <div className="flex size-7 items-center justify-center">
                <CheckCircle2
                    aria-hidden="true"
                    className="size-7 text-emerald-500"
                />
            </div>
        );
    }
    return (
        <div
            className={cn(
                'flex size-7 items-center justify-center rounded-full font-semibold text-xs',
                isActive
                    ? 'bg-primary text-primary-foreground'
                    : 'bg-muted text-muted-foreground'
            )}
        >
            {index + 1}
        </div>
    );
}
