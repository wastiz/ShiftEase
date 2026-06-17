import React from 'react';
import { MoveUpRight, MoveDownRight } from 'lucide-react';

type Trend = {
    value: number;
    type: 'increase' | 'decrease';
};

interface TrendBadgeProps {
    trend: Trend;
}

export function TrendBadge({ trend }: TrendBadgeProps) {
    const isIncrease = trend.type === 'increase';

    return (
        <div
            className={`flex items-center gap-1 text-xs font-semibold px-2 py-1 rounded ${
                isIncrease
                    ? 'text-orange-400 bg-orange-500/10'
                    : 'text-red-400 bg-red-500/10'
            }`}
        >
            {isIncrease ? (
                <MoveUpRight className="size-3" />
            ) : (
                <MoveDownRight className="size-3" />
            )}
            {Math.abs(trend.value)}%
        </div>
    );
}
