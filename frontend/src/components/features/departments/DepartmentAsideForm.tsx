import { useEffect } from "react"
import { useForm, useWatch } from "react-hook-form"
import { useTranslations } from "next-intl"
import {
    DayOfWeek, Department,
    DepartmentFormValues,
} from "@/types"
import { SchedulePattern } from "@/types/department"
import { AsideDrawer } from "@/components/ui/AsideDrawer"
import { Button } from "@/components/ui/shadcn/button"
import FormField from "@/components/ui/FormField"
import { Label } from "@/components/ui/shadcn/label"
import { Input } from "@/components/ui/shadcn/input"
import { Textarea } from "@/components/ui/shadcn/textarea"
import ColorPicker from "@/components/ui/inputs/ColorPicker"
import {
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue,
} from "@/components/ui/shadcn/select"
import { WEEK_DAYS } from "@/helpers/dateHelper"
import { TimeRange } from "@/components/ui/inputs/TimeRange"

interface DepartmentAsideFormProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    selectedDepartment: Department | null;
    onCreate: (data: DepartmentFormValues) => void;
    onUpdate: (id: number, data: DepartmentFormValues) => void;
    onDelete: (id: number) => void;
    isCreating: boolean;
    isUpdating: boolean;
    isDeleting: boolean;
}

export function DepartmentAsideForm({
                                        open,
                                        onOpenChange,
                                        selectedDepartment,
                                        onCreate,
                                        onUpdate,
                                        onDelete,
                                        isCreating,
                                        isUpdating,
                                        isDeleting,
                                    }: DepartmentAsideFormProps) {
    const t = useTranslations("employer.departments")
    const tCommon = useTranslations("common")

    const form = useForm<DepartmentFormValues>({
        defaultValues: {
            name: "",
            description: "",
            color: "#3b82f6",
            startTime: "09:00",
            endTime: "17:00",
            workingDays: [0, 1, 2, 3, 4, 5, 6],
            defaultSchedulePattern: "Flexible",
        },
    })

    const {
        register,
        control,
        setValue,
        handleSubmit,
        reset,
        watch,
        formState: { errors },
    } = form

    const startTime = useWatch({ control, name: "startTime" })
    const endTime = useWatch({ control, name: "endTime" })
    const workingDays = useWatch({ control, name: "workingDays" })
    const schedule = useWatch({ control, name: "defaultSchedulePattern" })

    useEffect(() => {
        if (!open) return

        if (selectedDepartment) {
            reset({
                name: selectedDepartment.name,
                description: selectedDepartment.description ?? "",
                color: selectedDepartment.color,
                startTime: selectedDepartment.startTime ?? "09:00",
                endTime: selectedDepartment.endTime ?? "17:00",
                workingDays: selectedDepartment.workingDays ?? [],
                defaultSchedulePattern: SchedulePattern[selectedDepartment.defaultSchedulePattern] as unknown as string,
            })
        } else {
            reset({
                name: "",
                description: "",
                color: "#3b82f6",
                startTime: "09:00",
                endTime: "17:00",
                workingDays: [0, 1, 2, 3, 4, 5, 6],
                defaultSchedulePattern: "Flexible",
            })
        }
    }, [open, selectedDepartment, reset])

    const onSubmit = (data: DepartmentFormValues) => {
        const payload = {
            ...data,
            defaultSchedulePattern:
                SchedulePattern[
                    data.defaultSchedulePattern as keyof typeof SchedulePattern
                    ],
        }

        if (selectedDepartment) {
            onUpdate(selectedDepartment.id, payload)
        } else {
            onCreate(payload)
        }
    }

    const toggleDay = (day: DayOfWeek) => {
        const selected = workingDays ?? []
        const next = selected.includes(day)
            ? selected.filter(d => d !== day)
            : [...selected, day]

        setValue("workingDays", next, {
            shouldDirty: true,
            shouldValidate: true,
        })
    }

    return (
        <AsideDrawer
            open={open}
            onOpenChange={onOpenChange}
            title={
                selectedDepartment
                    ? t("editDepartment")
                    : t("addDepartment")
            }
            description={t("configureDetails")}
            footer={
                <>
                    {selectedDepartment && (
                        <Button
                            type="button"
                            variant="destructive"
                            disabled={isDeleting}
                            onClick={() =>
                                onDelete(selectedDepartment.id)
                            }
                        >
                            {isDeleting
                                ? tCommon("deleting")
                                : tCommon("delete")}
                        </Button>
                    )}
                    <Button
                        type="submit"
                        form="department-form"
                        disabled={isCreating || isUpdating}
                    >
                        {isCreating || isUpdating
                            ? tCommon("saving")
                            : selectedDepartment
                                ? tCommon("update")
                                : tCommon("create")}
                    </Button>
                </>
            }
        >
            <form
                id="department-form"
                onSubmit={handleSubmit(onSubmit)}
                className="space-y-4 flex-1 overflow-y-auto"
            >
                {/* NAME */}
                <FormField>
                    <Label>
                        {t("departmentName")} *
                    </Label>
                    <Input
                        {...register("name", {
                            required: t("validation.nameRequired"),
                        })}
                    />
                    {errors.name && (
                        <p className="text-sm text-red-500">
                            {errors.name.message}
                        </p>
                    )}
                </FormField>

                {/* DESCRIPTION */}
                <FormField>
                    <Label>{tCommon("description")}</Label>
                    <Textarea {...register("description")} />
                </FormField>

                {/* COLOR */}
                <FormField>
                    <Label>{tCommon("color")}</Label>
                    <ColorPicker
                        label={t("color")}
                        value={watch("color") || "#3b82f6"}
                        onChange={(v) =>
                            setValue("color", v, {
                                shouldDirty: true,
                            })
                        }
                    />
                </FormField>

                {/* TIME */}
                <FormField>
                    <Label>{t("workingHours")}</Label>
                    <TimeRange
                        startTime={startTime}
                        endTime={endTime}
                        onChange={({ startTime, endTime }) => {
                            setValue("startTime", startTime, {
                                shouldDirty: true,
                            })
                            setValue("endTime", endTime, {
                                shouldDirty: true,
                            })
                        }}
                    />
                </FormField>

                {/* DAYS */}
                <FormField>
                    <Label>Working Days</Label>
                    <div className="flex gap-1">
                        {WEEK_DAYS.map(day => (
                            <button
                                key={day.value}
                                type="button"
                                onClick={() => toggleDay(day.value)}
                                className={`flex-1 py-1.5 text-xs rounded ${
                                    workingDays?.includes(day.value)
                                        ? "bg-primary text-white"
                                        : "bg-muted"
                                }`}
                            >
                                {day.label}
                            </button>
                        ))}
                    </div>
                </FormField>

                {/* SCHEDULE */}
                <FormField>
                    <Label>Schedule Pattern</Label>

                    <Select
                        value={schedule ?? "Flexible"}
                        onValueChange={(val) =>
                            setValue("defaultSchedulePattern", val, {
                                shouldDirty: true,
                            })
                        }
                    >
                        <SelectTrigger>
                            <SelectValue />
                        </SelectTrigger>

                        <SelectContent>
                            <SelectItem value="Flexible">
                                Flexible
                            </SelectItem>
                            <SelectItem value="TwoOnTwoOff">
                                2 on / 2 off
                            </SelectItem>
                            <SelectItem value="FiveOnTwoOff">
                                5 on / 2 off
                            </SelectItem>
                            <SelectItem value="ThreeOnThreeOff">
                                3 on / 3 off
                            </SelectItem>
                            <SelectItem value="FourOnFourOff">
                                4 on / 4 off
                            </SelectItem>
                        </SelectContent>
                    </Select>
                </FormField>
            </form>
        </AsideDrawer>
    )
}