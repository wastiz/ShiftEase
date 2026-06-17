import { useEffect } from "react"
import { useForm } from "react-hook-form"
import { useTranslations } from "next-intl"
import { Department, ShiftTemplate, ShiftTemplateFormValues, ShiftTemplateInternalFormValues } from "@/types"
import { AsideDrawer } from "@/components/ui/AsideDrawer"
import { Button } from "@/components/ui/shadcn/button"
import { ShiftTemplateForm } from "./ShiftTypeForm"
import { minutesToTimeSpan, parseBreakToMinutes } from "@/helpers/scheduleHelper"

interface ShiftTemplateAsideFormProps {
    open: boolean
    onOpenChange: (open: boolean) => void
    selectedShift: ShiftTemplate | null
    departments: Department[]
    defaultDepartmentId?: number
    onCreate: (data: ShiftTemplateFormValues) => void
    onUpdate: (id: number, data: ShiftTemplateFormValues) => void
    onDelete: (id: number) => void
    isCreating?: boolean
    isUpdating?: boolean
    isDeleting?: boolean
}

export function ShiftTemplateAsideForm({
    open,
    onOpenChange,
    selectedShift,
    departments,
    defaultDepartmentId,
    onCreate,
    onUpdate,
    onDelete,
    isCreating,
    isUpdating,
    isDeleting,
}: ShiftTemplateAsideFormProps) {
    const t = useTranslations('employer.shiftTypes')
    const tCommon = useTranslations('common')
    const formId = "shift-type-form"

    const {
        register,
        control,
        handleSubmit,
        reset,
        watch,
        formState: { errors },
    } = useForm<ShiftTemplateInternalFormValues>({
        defaultValues: {
            name: "",
            startTime: "09:00",
            endTime: "17:00",
            breakDurationMinutes: '',
            minEmployees: 1,
            maxEmployees: 1,
            color: "#3b82f6",
        },
    })

    const minEmployees = watch("minEmployees")

    useEffect(() => {
        if (open) {
            if (selectedShift) {
                reset({
                    name: selectedShift.name,
                    startTime: selectedShift.startTime,
                    endTime: selectedShift.endTime,
                    breakDurationMinutes: selectedShift.breakDuration
                        ? parseBreakToMinutes(selectedShift.breakDuration)
                        : '',
                    minEmployees: selectedShift.minEmployees,
                    maxEmployees: selectedShift.maxEmployees,
                    color: selectedShift.color,
                    departmentId: selectedShift.departmentId,
                })
            } else {
                reset({
                    name: "",
                    startTime: "09:00",
                    endTime: "17:00",
                    breakDurationMinutes: '',
                    minEmployees: 1,
                    maxEmployees: 1,
                    color: "#3b82f6",
                    departmentId: defaultDepartmentId,
                })
            }
        }
    }, [open, selectedShift, reset])

    const onSubmit = (data: ShiftTemplateInternalFormValues) => {
        const { breakDurationMinutes, ...rest } = data;
        const payload: ShiftTemplateFormValues = {
            ...rest,
            breakDuration: breakDurationMinutes != null && breakDurationMinutes !== ''
                ? minutesToTimeSpan(Number(breakDurationMinutes))
                : null,
        };

        if (selectedShift) {
            onUpdate(selectedShift.id, payload)
        } else {
            onCreate(payload)
        }
    }

    return (
        <AsideDrawer
            open={open}
            onOpenChange={onOpenChange}
            title={selectedShift ? t('editShiftTemplate') : t('addShiftTemplate')}
            description={t('configureDetails')}
            footer={
                <>
                    <Button
                        type="submit"
                        form={formId}
                        disabled={isCreating || isUpdating}
                    >
                        {isCreating || isUpdating
                            ? tCommon('saving')
                            : selectedShift
                              ? tCommon('update')
                              : tCommon('create')}
                    </Button>
                    {selectedShift && (
                        <Button
                            type="button"
                            variant="destructive"
                            disabled={isDeleting}
                            onClick={() => onDelete(selectedShift.id)}
                        >
                            {isDeleting ? tCommon('deleting') : tCommon('delete')}
                        </Button>
                    )}
                </>
            }
        >
            <ShiftTemplateForm
                formId={formId}
                register={register}
                control={control}
                errors={errors}
                minEmployees={minEmployees}
                departments={departments}
                onSubmit={handleSubmit(onSubmit)}
            />
        </AsideDrawer>
    )
}
