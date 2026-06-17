'use client';

import * as React from 'react';
import * as ProgressPrimitive from '@radix-ui/react-progress';

import { cn } from '@/lib/utils';

function Progress({
    className,
    value,
    ...props
}: React.ComponentProps<typeof ProgressPrimitive.Root>) {
    return (
        <ProgressPrimitive.Root
            data-slot="progress"
            className={cn(
                'bg-bgPrimary/20 relative h-2 w-full overflow-hidden rounded-full',
                className
            )}
            {...props}
        >
            <ProgressPrimitive.Indicator
                data-slot="progress-indicator"
                className="border-1 border-r-black/45 rounded-e-md bg-primary brightness-85 h-full w-full flex-1 transition-all" // bg-destructive __ h-full
                style={{ transform: `translateX(-${100 - (value || 0)}%)` }}
            />
        </ProgressPrimitive.Root>
    );
}

export { Progress };
