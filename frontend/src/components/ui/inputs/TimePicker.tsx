import * as React from "react"
import {
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue,
} from "@/components/ui/shadcn/select"
import { Input } from "@/components/ui/shadcn/input"
import { cn } from "@/lib/utils"
import {
    Popover,
    PopoverContent,
    PopoverTrigger,
} from "@/components/ui/shadcn/popover"
import { Button } from "@/components/ui/shadcn/button"
import { Clock } from "lucide-react"

interface TimePickerProps {
    value?: string
    onChange?: (time: string) => void
    width?: number
    className?: string
}

export function TimePicker({ value, onChange, width, className }: TimePickerProps) {
    const [hours, setHours] = React.useState("08")
    const [minutes, setMinutes] = React.useState("00")
    const [manualInput, setManualInput] = React.useState("")
    const [open, setOpen] = React.useState(false)

    React.useEffect(() => {
        if (value && value.includes(':')) {
            const [h, m] = value.split(":")
            const formattedH = h?.padStart(2, '0') || "08"
            const formattedM = m?.padStart(2, '0') || "00"
            setHours(formattedH)
            setMinutes(formattedM)
            setManualInput(`${formattedH}:${formattedM}`)
        }
    }, [value])

    const handleHoursChange = (h: string) => {
        setHours(h)
        const newTime = `${h}:${minutes}`
        setManualInput(newTime)
        onChange?.(newTime)
    }

    const handleMinutesChange = (m: string) => {
        setMinutes(m)
        const newTime = `${hours}:${m}`
        setManualInput(newTime)
        onChange?.(newTime)
    }

    const handleManualInput = (e: React.ChangeEvent<HTMLInputElement>) => {
        let input = e.target.value.replace(/[^\d:]/g, '') // Только цифры и двоеточие

        if (input.length === 2 && !input.includes(':')) {
            input = input + ':'
        }

        if (input.length > 5) {
            input = input.slice(0, 5)
        }

        setManualInput(input)

        const timeRegex = /^([0-1]?[0-9]|2[0-3]):([0-5][0-9])$/
        if (timeRegex.test(input)) {
            const [h, m] = input.split(":")
            const formattedH = h.padStart(2, '0')
            const formattedM = m.padStart(2, '0')
            setHours(formattedH)
            setMinutes(formattedM)
            onChange?.(`${formattedH}:${formattedM}`)
        }
    }

    const handleManualBlur = () => {
        const timeRegex = /^([0-1]?[0-9]|2[0-3]):([0-5][0-9])$/
        if (!timeRegex.test(manualInput)) {
            const formatted = `${hours}:${minutes}`
            setManualInput(formatted)
        }
    }

    const handleKeyDown = (e: React.KeyboardEvent<HTMLInputElement>) => {
        if (
            e.key === 'Backspace' ||
            e.key === 'Delete' ||
            e.key === 'ArrowLeft' ||
            e.key === 'ArrowRight' ||
            e.key === 'Tab' ||
            e.key === 'Enter'
        ) {
            return
        }

        if (!/^\d$/.test(e.key)) {
            e.preventDefault()
        }
    }

    const hourOptions = Array.from({ length: 24 }, (_, i) =>
        i.toString().padStart(2, "0")
    )

    const minuteOptions = Array.from({ length: 60 }, (_, i) =>
        i.toString().padStart(2, "0")
    )

    return (
        <Popover open={open} onOpenChange={setOpen} modal={true}>
            <PopoverTrigger asChild>
                <Button
                    variant="outline"
                    className={cn(
                        `w-${width || 35} justify-start text-left font-normal`,
                        !value && "text-muted-foreground",
                        className
                    )}
                >
                    <Clock className="mr-2 h-4 w-4" />
                    {value || "Pick time"}
                </Button>
            </PopoverTrigger>
            <PopoverContent
                className="w-auto p-4"
                align="start"
            >
                <div className="space-y-4">
                    <div className="space-y-2">
                        <label className="text-sm font-medium">Manual Input</label>
                        <Input
                            value={manualInput}
                            onChange={handleManualInput}
                            onBlur={handleManualBlur}
                            onKeyDown={handleKeyDown}
                            placeholder="HH:MM"
                            className="w-full"
                            maxLength={5}
                            autoComplete="off"
                        />
                        <p className="text-xs text-muted-foreground">
                            Type hours and minutes (e.g., 0830 or 08:30)
                        </p>
                    </div>

                    <div className="flex items-center gap-2">
                        <div className="h-px flex-1 bg-border" />
                        <span className="text-xs text-muted-foreground">or select</span>
                        <div className="h-px flex-1 bg-border" />
                    </div>

                    <div className="flex items-center gap-2">
                        <Select value={hours} onValueChange={handleHoursChange}>
                            <SelectTrigger className="w-[80px]">
                                <SelectValue placeholder="HH" />
                            </SelectTrigger>
                            <SelectContent className="max-h-[200px]">
                                {hourOptions.map((hour) => (
                                    <SelectItem key={hour} value={hour}>
                                        {hour}
                                    </SelectItem>
                                ))}
                            </SelectContent>
                        </Select>

                        <span className="text-muted-foreground">:</span>

                        <Select value={minutes} onValueChange={handleMinutesChange}>
                            <SelectTrigger className="w-[80px]">
                                <SelectValue placeholder="MM" />
                            </SelectTrigger>
                            <SelectContent className="max-h-[200px]">
                                {minuteOptions.map((minute) => (
                                    <SelectItem key={minute} value={minute}>
                                        {minute}
                                    </SelectItem>
                                ))}
                            </SelectContent>
                        </Select>
                    </div>

                    <div className="flex justify-end pt-2">
                        <Button
                            size="sm"
                            onClick={() => setOpen(false)}
                        >
                            Done
                        </Button>
                    </div>
                </div>
            </PopoverContent>
        </Popover>
    )
}
