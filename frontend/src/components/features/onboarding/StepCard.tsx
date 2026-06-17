import {cn} from "@/lib/utils";
import {Button} from "@/components/ui/shadcn/button";
import {StepData} from "@/types";
import StepIndicator from "@/components/features/onboarding/StepIndicator";

function getIconColor(isCompleted: boolean, isActive: boolean) {
    if (isCompleted) {
        return 'text-muted-foreground/30';
    }
    if (isActive) {
        return 'text-primary';
    }
    return 'text-muted-foreground/40';
}

function getTitleClass(isCompleted: boolean, isActive: boolean) {
    if (isCompleted) {
        return 'text-muted-foreground/50 line-through';
    }
    if (isActive) {
        return 'text-foreground';
    }
    return 'text-muted-foreground';
}


export default function StepCard({
                      step,
                      index,
                      isCompleted,
                      isActive,
                      onComplete,
                  }: {
    step: StepData;
    index: number;
    isCompleted: boolean;
    isActive: boolean;
    onComplete: () => void;
}) {
    const StepIcon = step.icon;

    return (
        <div
            className={cn(
                'rounded-lg border p-4 transition-colors',
                isActive && 'border-primary/30 bg-muted/50',
                !isActive && 'border-border bg-background'
            )}
        >
            <div className="flex gap-3">
                <div className="mt-0.5 shrink-0">
                    <StepIndicator
                        index={index}
                        isActive={isActive}
                        isCompleted={isCompleted}
                    />
                </div>
                <div className="min-w-0 flex-1">
                    <div className="flex items-start gap-3">
                        <div className="min-w-0 flex-1">
                            <p
                                className={cn(
                                    'font-medium leading-6',
                                    getTitleClass(isCompleted, isActive)
                                )}
                            >
                                {step.title}
                            </p>
                            <p
                                className={cn(
                                    'mt-0.5 text-sm leading-5',
                                    isActive
                                        ? 'text-muted-foreground'
                                        : 'text-muted-foreground/60'
                                )}
                            >
                                {step.description}
                            </p>
                            {isActive && (
                                <Button className="mt-3" onClick={onComplete} size="sm">
                                    <StepIcon
                                        aria-hidden="true"
                                        className="-ml-0.5 size-4 shrink-0"
                                    />
                                    {step.actionLabel}
                                </Button>
                            )}
                        </div>
                        <StepIcon
                            aria-hidden="true"
                            className={cn(
                                'mt-0.5 size-5 shrink-0',
                                getIconColor(isCompleted, isActive)
                            )}
                        />
                    </div>
                </div>
            </div>
        </div>
    );
}