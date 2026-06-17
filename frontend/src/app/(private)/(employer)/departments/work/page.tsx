"use client"

import { useEffect, useState } from "react"
import { useForm, Controller } from "react-hook-form"
import { toast } from "sonner"
import { Trash } from "lucide-react"
import { useTranslations } from "next-intl"

import Header from "@/components/ui/headers/Header"
import Loader from "@/components/ui/Loader"
import { Button } from "@/components/ui/shadcn/button"
import { Label } from "@/components/ui/shadcn/label"
import { TimePicker } from "@/components/ui/inputs/TimePicker"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/shadcn/select"
import { ShiftTemplateAsideForm } from "@/components/features/shift-types/ShiftTypeAsideForm"

import { Department, DepartmentFormValues, ShiftTemplate, ShiftTemplateFormValues } from "@/types"
import { SchedulePattern } from "@/types/department"
import { useGetDepartments, useUpdateDepartment } from "@/hooks/api"
import {
    useGetShiftTemplates,
    useCreateShiftTemplate,
    useUpdateShiftTemplate,
    useDeleteShiftTemplate,
} from "@/hooks/api/employer/shift-templates"
import {WEEK_DAYS} from "@/helpers/dateHelper";

const timeToPercent = (timeStr: string) => {
    if (!timeStr) return 0
    const [h, m] = timeStr.split(":").map(Number)
    return ((h * 60 + (m || 0)) / (24 * 60)) * 100
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

const getShiftStyle = (st: DisplayTemplate, idx: number, all: DisplayTemplate[]) => {
    const top = timeToPercent(st.startTime)
    const bottom = timeToPercent(st.endTime)
    let height = bottom - top
    if (height < 0) height = 100 - top + bottom
    if (height <= 0) height = 5

    const overlaps = all.filter(other => {
        if (other.id === st.id) return false
        const oTop = timeToPercent(other.startTime)
        const oBottom = timeToPercent(other.endTime)
        let oH = oBottom - oTop
        if (oH < 0) oH = 100 - oTop + oBottom
        return top < oTop + oH && top + height > oTop
    })

    const overlapCount = overlaps.length
    const myIndex = all.indexOf(st) % (overlapCount + 1)
    const width = overlapCount > 0 ? 100 / (overlapCount + 1) : 100
    const left = overlapCount > 0 ? myIndex * width : 0

    return {
        top: `${top}%`,
        height: `${height}%`,
        width: `${width}%`,
        left: `${left}%`,
        backgroundColor: `${st.color}40`,
        borderColor: st.color,
        zIndex: 10 + idx,
    }
}

export default function DepartmentsWork() {
    const t = useTranslations("employer.departments")
    const tCommon = useTranslations("common")

    const [selectedDepartmentId, setSelectedDepartmentId] = useState<number | null>(null)
    const [shiftFormOpen, setShiftFormOpen] = useState(false)
    const [selectedShift, setSelectedShift] = useState<ShiftTemplate | null>(null)

    const { data: departments = [], isLoading } = useGetDepartments()
    const { data: allShiftTemplates = [] } = useGetShiftTemplates()

    const selectedDepartment = departments.find(d => d.id === selectedDepartmentId) ?? null
    const departmentTemplates = allShiftTemplates.filter(st => st.departmentId === selectedDepartmentId)

    const updateDeptMutation = useUpdateDepartment(selectedDepartmentId ?? 0)
    const createShiftMutation = useCreateShiftTemplate()
    const updateShiftMutation = useUpdateShiftTemplate(selectedShift?.id ?? 0)
    const deleteShiftMutation = useDeleteShiftTemplate()

    const form = useForm<DepartmentFormValues>({
        defaultValues: {
            startTime: "09:00",
            endTime: "17:00",
            workingDays: [],
            defaultSchedulePattern: SchedulePattern.Flexible,
        },
    })

    useEffect(() => {
        if (selectedDepartment) {
            form.reset({
                name: selectedDepartment.name,
                description: selectedDepartment.description ?? "",
                color: selectedDepartment.color,
                startTime: selectedDepartment.startTime ?? "09:00",
                endTime: selectedDepartment.endTime ?? "17:00",
                workingDays: selectedDepartment.workingDays ?? [],
                defaultSchedulePattern: selectedDepartment.defaultSchedulePattern ?? SchedulePattern.Flexible,
            })
        }
    }, [selectedDepartmentId, selectedDepartment, form])

    useEffect(() => {
        if (!selectedDepartmentId && departments.length > 0) {
            setSelectedDepartmentId(departments[0].id)
        }
    }, [departments, selectedDepartmentId])

    const handleSaveSettings = form.handleSubmit((data) => {
        updateDeptMutation.mutate(data, {
            onSuccess: () => toast.success(t("updated")),
            onError: () => toast.error(t("failedToUpdate")),
        })
    })

    const openShiftForm = (shift: ShiftTemplate | null) => {
        setSelectedShift(shift)
        setShiftFormOpen(true)
    }

    const workingDays = (form.watch("workingDays") ?? []) as unknown as string[]

    if (isLoading) return <Loader />

    return (
        <>
            <Header title={t("departmentsWork")}>
                {selectedDepartment && (
                    <Button onClick={() => openShiftForm(null)}>
                        + {t("addShiftTemplate") || "Add Shift Template"}
                    </Button>
                )}
            </Header>

            <div className="flex h-[calc(100vh-56px)] overflow-hidden">
                {/* Left: department list */}
                <aside className="w-52 border-r flex flex-col overflow-y-auto shrink-0">
                    {departments.length === 0 ? (
                        <p className="p-4 text-sm text-muted-foreground">{t("noDepartments")}</p>
                    ) : (
                        departments.map(dep => (
                            <button
                                key={dep.id}
                                onClick={() => setSelectedDepartmentId(dep.id)}
                                className={`text-left px-4 py-3 text-sm border-b transition-colors ${
                                    selectedDepartmentId === dep.id
                                        ? "bg-accent text-accent-foreground"
                                        : "hover:bg-muted/50 text-foreground"
                                }`}
                            >
                                <div className="flex items-center gap-2">
                                    <div
                                        className="w-2.5 h-2.5 rounded-full shrink-0"
                                        style={{ backgroundColor: dep.color }}
                                    />
                                    <span className="truncate">{dep.name}</span>
                                </div>
                            </button>
                        ))
                    )}
                </aside>

                {/* Right: content */}
                {!selectedDepartment ? (
                    <div className="flex-1 flex items-center justify-center text-muted-foreground text-sm">
                        {t("selectDepartmentFirst")}
                    </div>
                ) : (
                    <div className="flex-1 overflow-y-auto p-4 space-y-4">
                        {/* Settings form */}
                        <form onSubmit={handleSaveSettings}>
                            <div className="p-4 border rounded-lg bg-muted/20 space-y-3">
                                <div className="flex flex-wrap gap-4 items-end">
                                    <div className="space-y-1">
                                        <Label className="text-xs text-muted-foreground">{tCommon("startTime")}</Label>
                                        <Controller
                                            name="startTime"
                                            control={form.control}
                                            render={({ field }) => (
                                                <TimePicker value={field.value ?? "09:00"} onChange={field.onChange} />
                                            )}
                                        />
                                    </div>
                                    <div className="space-y-1">
                                        <Label className="text-xs text-muted-foreground">{tCommon("endTime")}</Label>
                                        <Controller
                                            name="endTime"
                                            control={form.control}
                                            render={({ field }) => (
                                                <TimePicker value={field.value ?? "17:00"} onChange={field.onChange} />
                                            )}
                                        />
                                    </div>
                                    <div className="space-y-1">
                                        <Label className="text-xs text-muted-foreground">{t("schedulePattern")}</Label>
                                        <Controller
                                            name="defaultSchedulePattern"
                                            control={form.control}
                                            render={({ field }) => (
                                                <Select
                                                    value={field.value != null ? SchedulePattern[field.value] : "Flexible"}
                                                    onValueChange={(val) =>
                                                        field.onChange(SchedulePattern[val as keyof typeof SchedulePattern])
                                                    }
                                                >
                                                    <SelectTrigger>
                                                        <SelectValue />
                                                    </SelectTrigger>
                                                    <SelectContent>
                                                        <SelectItem value="Flexible">Flexible</SelectItem>
                                                        <SelectItem value="TwoOnTwoOff">2 on / 2 off</SelectItem>
                                                        <SelectItem value="FiveOnTwoOff">5 on / 2 off</SelectItem>
                                                        <SelectItem value="ThreeOnThreeOff">3 on / 3 off</SelectItem>
                                                        <SelectItem value="FourOnFourOff">4 on / 4 off</SelectItem>
                                                    </SelectContent>
                                                </Select>
                                            )}
                                        />
                                    </div>
                                    <Button
                                        type="submit"
                                        disabled={updateDeptMutation.isPending}
                                        size="sm"
                                    >
                                        {updateDeptMutation.isPending ? tCommon("saving") : t("saveSettings")}
                                    </Button>
                                </div>
                            </div>
                        </form>

                        {/* Shift templates list */}
                        {departmentTemplates.length > 0 ? (
                            <div className="flex flex-wrap gap-2">
                                {departmentTemplates.map(st => (
                                    <div
                                        key={st.id}
                                        className="flex items-center gap-2 px-3 py-1.5 rounded-full text-xs font-medium border cursor-pointer hover:opacity-80 transition-opacity"
                                        style={{ borderColor: st.color, backgroundColor: `${st.color}20`, color: st.color }}
                                        onClick={() => openShiftForm(st)}
                                    >
                                        <span>{st.name} ({st.startTime} – {st.endTime})</span>
                                        <button
                                            onClick={(e) => {
                                                e.stopPropagation()
                                                deleteShiftMutation.mutate(st.id)
                                            }}
                                            className="opacity-70 hover:opacity-100 transition-opacity"
                                        >
                                            <Trash className="w-3 h-3" />
                                        </button>
                                    </div>
                                ))}
                            </div>
                        ) : (
                            <p className="text-sm text-muted-foreground">{t("noShiftTemplates")}</p>
                        )}

                        {/* Weekly calendar visualization */}
                        <div className="border rounded-md overflow-hidden bg-background shadow-sm">
                            {/* Day headers */}
                            <div className="grid grid-cols-7 border-b bg-muted/50">
                                {WEEK_DAYS.map(day => {
                                    const isWorking = workingDays.includes(day.value)
                                    return (
                                        <button
                                            key={day.value}
                                            type="button"
                                            onClick={() => {
                                                const current = form.getValues("workingDays") as string[]

                                                const next = current.includes(day.value)
                                                    ? current.filter(d => d !== day.value)
                                                    : [...current, day.value]

                                                form.setValue("workingDays", next, {
                                                    shouldDirty: true,
                                                    shouldTouch: true,
                                                })
                                            }}
                                            className={`
                                                p-3 text-center text-xs font-semibold border-r last:border-r-0
                                                transition-colors
                                                ${isWorking
                                                ? "bg-primary text-primary-foreground"
                                                : "bg-muted/30 text-muted-foreground hover:bg-muted/50"
                                            }
    `}
                                        >
                                            {day.label}
                                        </button>
                                    )
                                })}
                            </div>

                            {/* Day columns */}
                            <div className="grid grid-cols-7 h-[400px]">
                                {WEEK_DAYS.map(day => {
                                    const isWorking = workingDays.includes(day.value)
                                    const templates: DisplayTemplate[] = departmentTemplates.map(st => ({
                                        id: st.id,
                                        name: st.name,
                                        startTime: st.startTime,
                                        endTime: st.endTime,
                                        minEmployees: st.minEmployees,
                                        maxEmployees: st.maxEmployees,
                                        color: st.color,
                                    }))
                                    return (
                                        <div
                                            key={day.value}
                                            className={`relative border-r last:border-r-0 ${
                                                isWorking ? "bg-background" : "bg-muted/10"
                                            }`}
                                        >
                                            {isWorking
                                                ? templates.map((st, idx) => (
                                                    <div
                                                        key={st.id}
                                                        className="absolute rounded-sm border shadow-sm p-1 overflow-hidden hover:z-30 cursor-pointer transition-all"
                                                        style={getShiftStyle(st, idx, templates)}
                                                        title={`${st.name} (${st.startTime} – ${st.endTime})`}
                                                        onClick={() => openShiftForm(departmentTemplates.find(t => t.id === st.id) ?? null)}
                                                    >
                                                        <div
                                                            className="text-[10px] font-bold leading-tight px-0.5"
                                                            style={{ color: st.color }}
                                                        >
                                                            {st.name}
                                                        </div>
                                                        <div className="text-[9px] text-muted-foreground px-0.5">
                                                            {st.minEmployees}–{st.maxEmployees}
                                                        </div>
                                                    </div>
                                                ))
                                                : (
                                                    <div className="absolute inset-0 flex items-center justify-center pointer-events-none">
                                                        <span className="text-[10px] font-bold uppercase tracking-wider text-muted-foreground/30 rotate-[-45deg]">
                                                            No work
                                                        </span>
                                                    </div>
                                                )
                                            }
                                        </div>
                                    )
                                })}
                            </div>
                        </div>
                    </div>
                )}
            </div>

            {/* Shift template aside form */}
            <ShiftTemplateAsideForm
                open={shiftFormOpen}
                onOpenChange={setShiftFormOpen}
                selectedShift={selectedShift}
                departments={selectedDepartment ? [selectedDepartment] : departments}
                defaultDepartmentId={selectedDepartment?.id}
                onCreate={(data: ShiftTemplateFormValues) =>
                    createShiftMutation.mutate(data, { onSuccess: () => setShiftFormOpen(false) })
                }
                onUpdate={(id: number, data: ShiftTemplateFormValues) =>
                    updateShiftMutation.mutate(data, { onSuccess: () => setShiftFormOpen(false) })
                }
                onDelete={(id: number) =>
                    deleteShiftMutation.mutate(id, { onSuccess: () => setShiftFormOpen(false) })
                }
                isCreating={createShiftMutation.isPending}
                isUpdating={updateShiftMutation.isPending}
                isDeleting={deleteShiftMutation.isPending}
            />
        </>
    )
}
