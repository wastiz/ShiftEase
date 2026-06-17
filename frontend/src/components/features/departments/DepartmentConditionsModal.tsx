import React, { useState } from "react"
import { useTranslations } from "next-intl"
import { UseFormReturn } from "react-hook-form"
import { Trash } from "lucide-react"
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from "@/components/ui/shadcn/dialog"
import { Button } from "@/components/ui/shadcn/button"
import { Label } from "@/components/ui/shadcn/label"
import { Input } from "@/components/ui/shadcn/input"
import { Checkbox } from "@/components/ui/shadcn/checkbox"
import { TimePicker } from "@/components/ui/inputs/TimePicker"
import ColorPicker from "@/components/ui/inputs/ColorPicker"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/shadcn/select"
import { DepartmentFormValues, DayOfWeek, PendingShiftTemplate } from "@/types"
import { SchedulePattern } from "@/types/department"
import { useGetShiftTemplates, useCreateShiftTemplate, useDeleteShiftTemplate } from "@/hooks/api/employer/shift-templates"
import {WEEK_DAYS} from "@/helpers/dateHelper";

const timeToPercent = (timeStr: string) => {
    if (!timeStr) return 0;
    const [h, m] = timeStr.split(':').map(Number);
    return ((h * 60 + (m || 0)) / (24 * 60)) * 100;
};

interface DepartmentConditionsModalProps {
    open: boolean
    onOpenChange: (open: boolean) => void
    form: UseFormReturn<DepartmentFormValues>
    departmentId?: number
    pendingTemplates: PendingShiftTemplate[]
    onPendingTemplatesChange: (templates: PendingShiftTemplate[]) => void
}

type DisplayTemplate = {
    id: number
    name: string
    startTime: string
    endTime: string
    minEmployees: number
    maxEmployees: number
    color: string
}

const ALL_DAYS = WEEK_DAYS.map(d => d.value)

export function DepartmentConditionsModal({
    open,
    onOpenChange,
    form,
    departmentId,
    pendingTemplates,
    onPendingTemplatesChange,
}: DepartmentConditionsModalProps) {
    const t = useTranslations('employer.departments')
    const tST = useTranslations("employer.shiftTypes")
    const tCommon = useTranslations('common')

    // Shift Types API
    const { data: allShiftTypes = [] } = useGetShiftTemplates()
    const createShiftTypeMutation = useCreateShiftTemplate()
    const deleteShiftTypeMutation = useDeleteShiftTemplate()

    const departmentShiftTypes = allShiftTypes.filter(st => st.departmentId === departmentId)

    // For display: use API templates when editing, pending templates when creating
    const displayTemplates: DisplayTemplate[] = departmentId
        ? departmentShiftTypes
        : pendingTemplates.map((t, i) => ({ ...t, id: -(i + 1) }))

    // Form Watches
    const formWorkingDays = form.watch("workingDays") as unknown as string[];
    const formDefaultPattern = form.watch("defaultSchedulePattern") ?? SchedulePattern.Flexible
    const formDefaultPatternKey = SchedulePattern[formDefaultPattern] as string
    const formStartTime = form.watch("startTime") || "09:00"
    const formEndTime = form.watch("endTime") || "17:00"

    // Local State
    const [workingDays, setWorkingDays] = useState<string[]>(formWorkingDays)
    const [schedulePatternKey, setSchedulePatternKey] = useState<string>(formDefaultPatternKey)
    const [startTime, setStartTime] = useState<string>(formStartTime)
    const [endTime, setEndTime] = useState<string>(formEndTime)

    // New Shift Type State
    const [newShiftName, setNewShiftName] = useState("")
    const [newShiftStart, setNewShiftStart] = useState("09:00")
    const [newShiftEnd, setNewShiftEnd] = useState("17:00")
    const [newShiftMin, setNewShiftMin] = useState(1)
    const [newShiftMax, setNewShiftMax] = useState(1)
    const [newShiftColor, setNewShiftColor] = useState("#3b82f6")

    React.useEffect(() => {
        if (open) {
            setWorkingDays(formWorkingDays?.length ? formWorkingDays : ALL_DAYS)
            const pattern = form.getValues("defaultSchedulePattern") ?? SchedulePattern.Flexible
            setSchedulePatternKey(SchedulePattern[pattern] as string)
            setStartTime(form.getValues("startTime") || "09:00")
            setEndTime(form.getValues("endTime") || "17:00")
        }
    }, [open, formWorkingDays, form])

    const handleSave = () => {
        form.setValue("workingDays", workingDays as unknown as DayOfWeek[], { shouldValidate: true, shouldDirty: true })
        form.setValue("defaultSchedulePattern", SchedulePattern[schedulePatternKey as keyof typeof SchedulePattern], { shouldValidate: true, shouldDirty: true })
        form.setValue("startTime", startTime, { shouldValidate: true, shouldDirty: true })
        form.setValue("endTime", endTime, { shouldValidate: true, shouldDirty: true })
        onOpenChange(false)
    }

    const toggleDay = (day: string) => {
        if (workingDays.includes(day)) {
            setWorkingDays(workingDays.filter(d => d !== day))
        } else {
            const newDays = [...workingDays, day]
            setWorkingDays(ALL_DAYS.filter(d => newDays.includes(d)))
        }
    }

    const handleAddShiftType = () => {
        if (!newShiftName.trim()) return

        if (departmentId) {
            createShiftTypeMutation.mutate({
                name: newShiftName,
                startTime: newShiftStart,
                endTime: newShiftEnd,
                minEmployees: newShiftMin,
                maxEmployees: newShiftMax,
                color: newShiftColor,
                departmentId,
            }, {
                onSuccess: () => setNewShiftName("")
            })
        } else {
            onPendingTemplatesChange([...pendingTemplates, {
                name: newShiftName,
                startTime: newShiftStart,
                endTime: newShiftEnd,
                minEmployees: newShiftMin,
                maxEmployees: newShiftMax,
                color: newShiftColor,
            }])
            setNewShiftName("")
        }
    }

    const handleDeleteShiftType = (id: number) => {
        if (departmentId) {
            deleteShiftTypeMutation.mutate(id)
        } else {
            // id is negative: index = -(id) - 1
            const idx = -(id) - 1
            onPendingTemplatesChange(pendingTemplates.filter((_, i) => i !== idx))
        }
    }

    // Overlap logic for preview
    const getShiftStyle = (st: DisplayTemplate, idx: number, allInDay: DisplayTemplate[]) => {
        const top = timeToPercent(st.startTime);
        const bottom = timeToPercent(st.endTime);
        let height = bottom - top;
        if (height < 0) height = (100 - top) + bottom;
        if (height <= 0) height = 5;

        const overlaps = allInDay.filter(other => {
            if (other.id === st.id) return false;
            const oTop = timeToPercent(other.startTime);
            const oBottom = timeToPercent(other.endTime);
            let oHeight = oBottom - oTop;
            if (oHeight < 0) oHeight = (100 - oTop) + oBottom;
            return (top < oTop + oHeight) && (top + height > oTop);
        });

        const overlapCount = overlaps.length;
        const myIndex = allInDay.indexOf(st) % (overlapCount + 1);
        const width = overlapCount > 0 ? (100 / (overlapCount + 1)) : 100;
        const left = overlapCount > 0 ? (myIndex * width) : 0;

        return {
            top: `${top}%`,
            height: `${height}%`,
            width: `${width}%`,
            left: `${left}%`,
            backgroundColor: `${st.color}40`,
            borderColor: st.color,
            zIndex: 10 + idx
        };
    };

    return (
        <Dialog open={open} onOpenChange={onOpenChange}>
            <DialogContent className="sm:max-w-[1000px] h-[95vh] flex flex-col p-6">
                <DialogHeader>
                    <DialogTitle>{t('departmentConditions')}</DialogTitle>
                </DialogHeader>

                <div className="flex-1 overflow-y-auto pr-4 space-y-8">
                    <div className="text-sm text-muted-foreground pb-4 border-b">
                        {t('departmentConditionsTooltip')}
                    </div>

                    <div className="space-y-4">
                        <div className="flex gap-4">
                            <div className="space-y-2">
                                <Label>Schedule Pattern</Label>
                                <Select
                                    value={schedulePatternKey}
                                    onValueChange={setSchedulePatternKey}
                                >
                                    <SelectTrigger>
                                        <SelectValue placeholder="Select Pattern" />
                                    </SelectTrigger>
                                    <SelectContent>
                                        <SelectItem value="Flexible">Flexible</SelectItem>
                                        <SelectItem value="TwoOnTwoOff">2 on / 2 off</SelectItem>
                                        <SelectItem value="FiveOnTwoOff">5 on / 2 off</SelectItem>
                                        <SelectItem value="ThreeOnThreeOff">3 on / 3 off</SelectItem>
                                        <SelectItem value="FourOnFourOff">4 on / 4 off</SelectItem>
                                    </SelectContent>
                                </Select>
                            </div>
                            <div className="space-y-2">
                                <Label>Start Time</Label>
                                <TimePicker value={startTime} onChange={setStartTime} />
                            </div>
                            <div className="space-y-2">
                                <Label>End Time</Label>
                                <TimePicker value={endTime} onChange={setEndTime} />
                            </div>
                        </div>
                    </div>

                    <div className="space-y-6">
                        {/* Quick Shift Type Form */}
                        <div className="p-4 border rounded-md bg-muted/30 space-y-4">
                            <Label className="font-semibold pb-2 border-b block">{tST("addShiftTemplate")}</Label>
                            <div className="grid grid-cols-6 gap-4 items-end">
                                <div className="col-span-2 space-y-2">
                                    <Label className="text-xs">{t("name")}</Label>
                                    <Input value={newShiftName} onChange={e => setNewShiftName(e.target.value)} placeholder="e.g. Morning Shift" />
                                </div>
                                <div className="col-span-1 space-y-2">
                                    <Label className="text-xs">{t("startTime")}</Label>
                                    <TimePicker value={newShiftStart} onChange={setNewShiftStart} />
                                </div>
                                <div className="col-span-1 space-y-2">
                                    <Label className="text-xs">{t("endTime")}</Label>
                                    <TimePicker value={newShiftEnd} onChange={setNewShiftEnd} />
                                </div>
                                <div className="col-span-1 space-y-2">
                                    <ColorPicker label={tCommon("color")} value={newShiftColor} onChange={setNewShiftColor} />
                                </div>
                                <div className="col-span-1">
                                    <Button
                                        className="w-full"
                                        onClick={handleAddShiftType}
                                        disabled={!newShiftName || createShiftTypeMutation.isPending}
                                    >
                                        Add
                                    </Button>
                                </div>
                            </div>
                            <div className="grid grid-cols-4 gap-4 items-center">
                                <div className="space-y-2">
                                    <Label className="text-xs">{tST("minEmployees")}</Label>
                                    <Input type="number" min={1} value={newShiftMin} onChange={e => setNewShiftMin(parseInt(e.target.value))} />
                                </div>
                                <div className="space-y-2">
                                    <Label className="text-xs">{tST("maxEmployees")}</Label>
                                    <Input type="number" min={1} value={newShiftMax} onChange={e => setNewShiftMax(parseInt(e.target.value))} />
                                </div>
                            </div>
                        </div>

                        {/* Templates list */}
                        {displayTemplates.length > 0 && (
                            <div className="flex flex-wrap gap-2">
                                {displayTemplates.map(st => (
                                    <div key={st.id} className="flex items-center gap-2 px-3 py-1.5 rounded-full text-xs font-medium border" style={{ borderColor: st.color, backgroundColor: `${st.color}20`, color: st.color }}>
                                        <span>{st.name} ({st.startTime} - {st.endTime})</span>
                                        <button onClick={() => handleDeleteShiftType(st.id)} className="opacity-70 hover:opacity-100 transition-opacity">
                                            <Trash className="w-3 h-3" />
                                        </button>
                                    </div>
                                ))}
                            </div>
                        )}

                        {/* Weekly Calendar View */}
                        <div className="border rounded-md overflow-hidden bg-background shadow-sm">
                            <div className="grid grid-cols-7 border-b bg-muted/50">
                                {WEEK_DAYS.map(day => {
                                    const isSelected = workingDays.includes(day.value)
                                    return (
                                        <div key={day.value} className="p-3 text-center text-xs font-semibold border-r last:border-r-0 flex flex-col items-center justify-center gap-2 bg-muted/30">
                                            <div className="flex items-center gap-2">
                                                <Checkbox
                                                    id={`day-check-${day.value}`}
                                                    checked={isSelected}
                                                    onCheckedChange={() => toggleDay(day.value)}
                                                />
                                                <Label htmlFor={`day-check-${day.value}`} className={`cursor-pointer ${isSelected ? 'text-foreground' : 'text-muted-foreground opacity-50'}`}>
                                                    {day.label}
                                                </Label>
                                            </div>
                                        </div>
                                    )
                                })}
                            </div>
                            <div className="grid grid-cols-7 h-[400px]">
                                {WEEK_DAYS.map(day => {
                                    const isWorkingDay = workingDays.includes(day.value)
                                    return (
                                        <div key={day.value} className={`relative border-r last:border-r-0 ${isWorkingDay ? 'bg-background' : 'bg-muted/10 pattern-diagonal-lines'}`}>
                                            {isWorkingDay && displayTemplates.map((st, idx) => (
                                                <div
                                                    key={st.id}
                                                    className="absolute rounded-sm border shadow-sm p-1 overflow-hidden transition-all hover:z-30 cursor-default group"
                                                    style={getShiftStyle(st, idx, displayTemplates)}
                                                    title={`${st.name} (${st.startTime} - ${st.endTime})`}
                                                >
                                                    <div className="text-[10px] font-bold leading-tight px-0.5" style={{ color: st.color }}>{st.name}</div>
                                                    <div className="text-[9px] text-muted-foreground font-medium transition-opacity px-0.5">
                                                        <span>{st.minEmployees} - {st.maxEmployees}</span>
                                                    </div>
                                                </div>
                                            ))}
                                            {!isWorkingDay && (
                                                <div className="absolute inset-0 flex items-center justify-center pointer-events-none">
                                                    <span className="text-[10px] font-bold uppercase tracking-wider text-muted-foreground/30 rotate-[-45deg]">
                                                        No work
                                                    </span>
                                                </div>
                                            )}
                                        </div>
                                    )
                                })}
                            </div>
                        </div>
                    </div>
                </div>
                <DialogFooter>
                    <Button variant="outline" className="w-32" onClick={() => onOpenChange(false)}>
                        {tCommon('cancel')}
                    </Button>
                    <Button className="w-32" onClick={handleSave}>
                        {tCommon('save')}
                    </Button>
                </DialogFooter>
            </DialogContent>
        </Dialog>
    )
}
