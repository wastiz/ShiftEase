import * as React from "react"
import { TimePicker } from "./TimePicker"
import { cn } from "@/lib/utils"

interface TimeRangeProps {
    startTime?: string
    endTime?: string
    onChange?: (range: { startTime: string; endTime: string }) => void
    className?: string
}

export function TimeRange({
                              startTime,
                              endTime,
                              onChange,
                              className,
                          }: TimeRangeProps) {
    const [start, setStart] = React.useState(startTime ?? "08:00")
    const [end, setEnd] = React.useState(endTime ?? "17:00")

    React.useEffect(() => {
        if (startTime) setStart(startTime)
    }, [startTime])

    React.useEffect(() => {
        if (endTime) setEnd(endTime)
    }, [endTime])

    const handleStartChange = (time: string) => {
        setStart(time)
        onChange?.({ startTime: time, endTime: end })
    }

    const handleEndChange = (time: string) => {
        setEnd(time)
        onChange?.({ startTime: start, endTime: time })
    }

    const toMinutes = (time: string) => {
        const [h, m] = time.split(":").map(Number)
        return h * 60 + m
    }

    const duration = React.useMemo(() => {
        const diff = toMinutes(end) - toMinutes(start)
        if (diff <= 0) return null
        const h = Math.floor(diff / 60)
        const m = diff % 60
        if (h === 0) return `${m}m`
        if (m === 0) return `${h}h`
        return `${h}h ${m}m`
    }, [start, end])

    const isOvernight = toMinutes(end) <= toMinutes(start)

    return (
        <div className={cn("flex items-center gap-2", className)}>
            <TimePicker
                value={start}
                width={35}
                onChange={handleStartChange}
            />

            <span className="flex items-center gap-1.5 text-muted-foreground">-</span>

            <TimePicker
                value={end}
                width={35}
                onChange={handleEndChange}
            />

            {duration && (
                <span className="text-xs text-muted-foreground whitespace-nowrap">
                    {duration}
                </span>
            )}

            {isOvernight && (
                <span className="text-xs text-amber-500 whitespace-nowrap">
                    +1 day
                </span>
            )}
        </div>
    )
}