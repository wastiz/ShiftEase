import { useState } from "react"
import { Button } from "@/components/ui/shadcn/button"
import { Input } from "@/components/ui/shadcn/input"
import { Label } from "@/components/ui/shadcn/label"
import {
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue,
} from "@/components/ui/shadcn/select"
import {
    Tooltip,
    TooltipContent,
    TooltipProvider,
    TooltipTrigger,
} from "@/components/ui/shadcn/tooltip"
import { X, Info } from "lucide-react"
import { Checkbox } from "@/components/ui/shadcn/checkbox"
import { Switch } from "@/components/ui/shadcn/switch"
import { Holiday } from '@/types'
import { TimePicker } from "@/components/ui/inputs/TimePicker"

const MONTHS = [
    { value: 1, label: "January" },
    { value: 2, label: "February" },
    { value: 3, label: "March" },
    { value: 4, label: "April" },
    { value: 5, label: "May" },
    { value: 6, label: "June" },
    { value: 7, label: "July" },
    { value: 8, label: "August" },
    { value: 9, label: "September" },
    { value: 10, label: "October" },
    { value: 11, label: "November" },
    { value: 12, label: "December" },
]

const DEFAULT_HOLIDAYS = [
    { holidayName: "New Year's Day", month: 1, day: 1 },
    { holidayName: "Independence Day", month: 2, day: 24 },
    { holidayName: "International Workers' Day", month: 5, day: 1 },
    { holidayName: "Christmas Eve", month: 12, day: 24 },
    { holidayName: "Christmas Day", month: 12, day: 25 },
    { holidayName: "New Year's Eve", month: 12, day: 31 },
]

const getDaysInMonth = (month: number) => {
    const days31 = [1, 3, 5, 7, 8, 10, 12]
    const days30 = [4, 6, 9, 11]

    if (days31.includes(month)) return 31
    if (days30.includes(month)) return 30
    return 29
}

interface HolidaySelectorProps {
    selectedHolidays: Holiday[]
    setSelectedHolidays: (holidays: Holiday[]) => void
}

export default function HolidaySelector({
    selectedHolidays,
    setSelectedHolidays,
}: HolidaySelectorProps) {
    const [holidayName, setHolidayName] = useState("")
    const [selectedMonth, setSelectedMonth] = useState<number>(1)
    const [selectedDay, setSelectedDay] = useState<number>(1)

    const daysInMonth = getDaysInMonth(selectedMonth)
    const dayOptions = Array.from({ length: daysInMonth }, (_, i) => i + 1)

    const isHolidaySelected = (month: number, day: number) => {
        return selectedHolidays.some(h => h.month === month && h.day === day)
    }

    const toggleDefaultHoliday = (holiday: typeof DEFAULT_HOLIDAYS[0]) => {
        if (isHolidaySelected(holiday.month, holiday.day)) {
            removeHoliday(holiday.month, holiday.day)
        } else {
            setSelectedHolidays([...selectedHolidays, holiday])
        }
    }

    const addCustomHoliday = () => {
        if (!holidayName.trim()) return

        const newHoliday = {
            holidayName: holidayName.trim(),
            month: selectedMonth,
            day: selectedDay,
        }

        if (isHolidaySelected(selectedMonth, selectedDay)) {
            alert("Holiday already exists on this date")
            return
        }

        setSelectedHolidays([...selectedHolidays, newHoliday])
        setHolidayName("")
        setSelectedMonth(1)
        setSelectedDay(1)
    }

    const removeHoliday = (month: number, day: number) => {
        setSelectedHolidays(
            selectedHolidays.filter(h => !(h.month === month && h.day === day))
        )
    }

    const updateHoliday = (month: number, day: number, updates: Partial<Holiday>) => {
        setSelectedHolidays(
            selectedHolidays.map(h =>
                h.month === month && h.day === day ? { ...h, ...updates } : h
            )
        )
    }

    return (
        <div className="space-y-4">
            <div className="flex items-center gap-2">
                <Label className="text-sm font-medium">
                    Holidays
                    <span className="text-red-500 ml-1">*</span>
                </Label>
                <TooltipProvider>
                    <Tooltip>
                        <TooltipTrigger asChild>
                            <Info className="h-4 w-4 text-muted-foreground cursor-help" />
                        </TooltipTrigger>
                        <TooltipContent className="max-w-xs">
                            <p>
                                Select only the holidays when your organization is <strong>closed</strong>.
                                If your organization operates on holidays, don't select them here.
                            </p>
                        </TooltipContent>
                    </Tooltip>
                </TooltipProvider>
            </div>

            <div className="space-y-2 p-4 border rounded-lg">
                <Label className="text-xs text-muted-foreground">Common Holidays</Label>
                <div className="grid grid-cols-1 sm:grid-cols-2 gap-2">
                    {DEFAULT_HOLIDAYS.map((holiday) => (
                        <div
                            key={`${holiday.month}-${holiday.day}`}
                            className="flex items-center space-x-2"
                        >
                            <Checkbox
                                id={`holiday-${holiday.month}-${holiday.day}`}
                                checked={isHolidaySelected(holiday.month, holiday.day)}
                                onCheckedChange={() => toggleDefaultHoliday(holiday)}
                            />
                            <label
                                htmlFor={`holiday-${holiday.month}-${holiday.day}`}
                                className="text-sm cursor-pointer"
                            >
                                {holiday.holidayName}
                                <span className="text-xs text-muted-foreground ml-1">
                                    ({MONTHS.find(m => m.value === holiday.month)?.label} {holiday.day})
                                </span>
                            </label>
                        </div>
                    ))}
                </div>
            </div>

            <div className="space-y-2">
                <Label className="text-xs text-muted-foreground">Add Custom Holiday</Label>
                <div className="flex gap-2 items-end">
                    <div className="flex-1">
                        <Input
                            value={holidayName}
                            onChange={(e) => setHolidayName(e.target.value)}
                            placeholder="Holiday name"
                        />
                    </div>

                    <Select
                        value={selectedMonth.toString()}
                        onValueChange={(v) => setSelectedMonth(Number(v))}
                    >
                        <SelectTrigger className="w-[140px]">
                            <SelectValue />
                        </SelectTrigger>
                        <SelectContent>
                            {MONTHS.map((m) => (
                                <SelectItem key={m.value} value={m.value.toString()}>
                                    {m.label}
                                </SelectItem>
                            ))}
                        </SelectContent>
                    </Select>

                    <Select
                        value={selectedDay.toString()}
                        onValueChange={(v) => setSelectedDay(Number(v))}
                    >
                        <SelectTrigger className="w-[80px]">
                            <SelectValue />
                        </SelectTrigger>
                        <SelectContent>
                            {dayOptions.map((day) => (
                                <SelectItem key={day} value={day.toString()}>
                                    {day}
                                </SelectItem>
                            ))}
                        </SelectContent>
                    </Select>

                    <Button type="button" onClick={addCustomHoliday}>
                        Add
                    </Button>
                </div>
            </div>

            {selectedHolidays.length > 0 && (
                <div className="space-y-2">
                    <Label className="text-xs text-muted-foreground">Selected Holidays</Label>
                    <div className="space-y-2">
                        {selectedHolidays
                            .sort((a, b) => a.month - b.month || a.day - b.day)
                            .map((holiday) => {
                                const isDefault = DEFAULT_HOLIDAYS.some(
                                    h => h.month === holiday.month && h.day === holiday.day
                                )
                                return (
                                    <div
                                        key={`${holiday.month}-${holiday.day}`}
                                        className="flex flex-col gap-2 p-3 border rounded-lg"
                                    >
                                        <div className="flex items-center justify-between">
                                            <div>
                                                <span className="font-medium">{holiday.holidayName}</span>
                                                <span className="text-sm text-muted-foreground ml-2">
                                                    {MONTHS.find((m) => m.value === holiday.month)?.label} {holiday.day}
                                                </span>
                                                {!isDefault && (
                                                    <span className="text-xs text-blue-500 ml-2">(Custom)</span>
                                                )}
                                            </div>
                                            <Button
                                                type="button"
                                                variant="ghost"
                                                size="sm"
                                                onClick={() => removeHoliday(holiday.month, holiday.day)}
                                            >
                                                <X className="h-4 w-4" />
                                            </Button>
                                        </div>
                                        <div className="flex items-center gap-4 mt-1">
                                            <div className="flex items-center gap-2">
                                                <Switch
                                                    id={`shortened-${holiday.month}-${holiday.day}`}
                                                    checked={holiday.isShortenedDay || false}
                                                    onCheckedChange={(checked) => updateHoliday(holiday.month, holiday.day, {
                                                        isShortenedDay: checked,
                                                        startTime: checked ? (holiday.startTime || "09:00") : undefined,
                                                        endTime: checked ? (holiday.endTime || "15:00") : undefined
                                                    })}
                                                />
                                                <Label htmlFor={`shortened-${holiday.month}-${holiday.day}`} className="text-sm cursor-pointer">
                                                    Shortened Day
                                                </Label>
                                            </div>
                                            {holiday.isShortenedDay && (
                                                <div className="flex items-center gap-2">
                                                    <TimePicker
                                                        value={holiday.startTime || "09:00"}
                                                        onChange={(time) => updateHoliday(holiday.month, holiday.day, { startTime: time })}
                                                    />
                                                    <span className="text-sm text-muted-foreground">to</span>
                                                    <TimePicker
                                                        value={holiday.endTime || "15:00"}
                                                        onChange={(time) => updateHoliday(holiday.month, holiday.day, { endTime: time })}
                                                    />
                                                </div>
                                            )}
                                        </div>
                                    </div>
                                )
                            })}
                    </div>
                </div>
            )}
        </div>
    )
}
