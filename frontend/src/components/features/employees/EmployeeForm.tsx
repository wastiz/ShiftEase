import { Control, Controller, FieldErrors, UseFormRegister } from "react-hook-form"
import { Info, X } from "lucide-react"
import { useTranslations } from "next-intl"
import FormField from "@/components/ui/FormField"
import { Label } from "@/components/ui/shadcn/label"
import { Input } from "@/components/ui/shadcn/input"
import { Tooltip, TooltipContent, TooltipProvider, TooltipTrigger } from "@/components/ui/shadcn/tooltip"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/shadcn/select"
import { Badge } from "@/components/ui/shadcn/badge"
import { Checkbox } from "@/components/ui/shadcn/checkbox"
import { Department } from "@/types"

export type EmployeeFormValues = {
    firstName: string
    lastName: string
    email: string
    position: string
    phone?: string
    hourlyRate?: number
    employmentRate?: number
    priority?: "high" | "medium" | "low"
    departmentIds: number[]
    primaryDepartmentId?: number | null
}

interface EmployeeFormProps {
    formId: string
    register: UseFormRegister<EmployeeFormValues>
    control: Control<EmployeeFormValues>
    errors: FieldErrors<EmployeeFormValues>
    departments: Department[]
    selectedDepartmentIds: number[]
    onSubmit: (e: React.FormEvent) => void
}

export function EmployeeForm({
    formId,
    register,
    control,
    errors,
    departments,
    selectedDepartmentIds,
    onSubmit,
}: EmployeeFormProps) {
    const t = useTranslations('employer.employees');
    const tCommon = useTranslations('common');

    return (
        <form id={formId} onSubmit={onSubmit} className="space-y-4 w-full">
            {/* First Name */}
            <FormField>
                <Label htmlFor="firstName">
                    {t('firstName')} <span className="text-red-500 ml-1">*</span>
                </Label>
                <Input
                    id="firstName"
                    placeholder={t('enterFirstName')}
                    {...register("firstName", {
                        required: t('firstNameRequired'),
                    })}
                />
                {errors.firstName && (
                    <p className="text-sm text-red-500">{errors.firstName.message}</p>
                )}
            </FormField>

            {/* Last Name */}
            <FormField>
                <Label htmlFor="lastName">
                    {t('lastName')} <span className="text-red-500 ml-1">*</span>
                </Label>
                <Input
                    id="lastName"
                    placeholder={t('enterLastName')}
                    {...register("lastName", {
                        required: t('lastNameRequired'),
                    })}
                />
                {errors.lastName && (
                    <p className="text-sm text-red-500">{errors.lastName.message}</p>
                )}
            </FormField>

            {/* Email */}
            <FormField>
                <Label htmlFor="email">
                    {tCommon('email')} <span className="text-red-500 ml-1">*</span>
                </Label>
                <Input
                    id="email"
                    type="email"
                    placeholder={t('enterEmail')}
                    {...register("email", {
                        required: t('emailRequired'),
                    })}
                />
                {errors.email && (
                    <p className="text-sm text-red-500">{errors.email.message}</p>
                )}
            </FormField>

            {/* Position */}
            <FormField>
                <Label htmlFor="position" className="flex items-center gap-1">
                    {t('position')} <span className="text-red-500">*</span>

                    <TooltipProvider>
                        <Tooltip>
                            <TooltipTrigger asChild>
                                <Info className="h-4 w-4 text-muted-foreground cursor-help" />
                            </TooltipTrigger>
                            <TooltipContent className="max-w-xs">
                                <p>
                                    {t('positionTooltip')}
                                </p>
                            </TooltipContent>
                        </Tooltip>
                    </TooltipProvider>
                </Label>

                <Input
                    id="position"
                    placeholder={t('enterPosition')}
                    {...register("position", {
                        required: t('positionRequired'),
                    })}
                />
                {errors.position && (
                    <p className="text-sm text-red-500">{errors.position.message}</p>
                )}
            </FormField>

            {/* Phone */}
            <FormField>
                <Label htmlFor="phone">{tCommon('phone')}</Label>
                <Input
                    id="phone"
                    type="tel"
                    placeholder={t('enterPhone')}
                    {...register("phone")}
                />
            </FormField>

            <div className="grid grid-cols-2 gap-4">
                {/* Hourly Rate */}
                <FormField>
                    <Label htmlFor="hourlyRate">{t('hourlyRate')}</Label>
                    <Input
                        id="hourlyRate"
                        type="number"
                        step="0.01"
                        placeholder={t('enterHourlyRate')}
                        {...register("hourlyRate", {
                            valueAsNumber: true,
                        })}
                    />
                </FormField>

                {/* Employment Rate */}
                <FormField>
                    <Label htmlFor="employmentRate">{t('employmentRate')}</Label>
                    <Input
                        id="employmentRate"
                        type="number"
                        step="0.01"
                        min="0"
                        max="1"
                        placeholder={t('enterEmploymentRate')}
                        {...register("employmentRate", {
                            valueAsNumber: true,
                        })}
                    />
                </FormField>
            </div>

            {/* Shift Priority */}
            <FormField>
                <Label>{t('shiftPriority')}</Label>
                <Controller
                    name="priority"
                    control={control}
                    render={({ field }) => (
                        <Select value={field.value} onValueChange={field.onChange}>
                            <SelectTrigger>
                                <SelectValue placeholder={t('selectPriority')} />
                            </SelectTrigger>
                            <SelectContent>
                                <SelectItem value="high">{t('priorityHigh')}</SelectItem>
                                <SelectItem value="medium">{t('priorityMedium')}</SelectItem>
                                <SelectItem value="low">{t('priorityLow')}</SelectItem>
                            </SelectContent>
                        </Select>
                    )}
                />
            </FormField>

            {/* Departments */}
            <FormField>
                <Label>
                    {t('assignDepartments')}
                    <span className="ml-2 text-xs text-muted-foreground">({tCommon('optional')})</span>
                </Label>

                {/* Selected departments */}
                {selectedDepartmentIds.length > 0 && (
                    <div className="flex flex-wrap gap-2 mb-2">
                        <Controller
                            name="departmentIds"
                            control={control}
                            render={({ field }) => (
                                <>
                                    {selectedDepartmentIds.map((departmentId) => {
                                        const department = departments.find((g) => g.id === departmentId)
                                        if (!department) return null

                                        return (
                                            <Badge
                                                key={department.id}
                                                className="border flex items-center gap-1"
                                                style={{
                                                    backgroundColor: `${department.color}40`,
                                                    borderColor: department.color,
                                                }}
                                            >
                                                {department.name}
                                                <X
                                                    className="h-3 w-3 cursor-pointer"
                                                    onClick={() =>
                                                        field.onChange(
                                                            field.value.filter((id) => id !== department.id)
                                                        )
                                                    }
                                                />
                                            </Badge>
                                        )
                                    })}
                                </>
                            )}
                        />
                    </div>
                )}

                {/* Departments select */}
                <Controller
                    name="departmentIds"
                    control={control}
                    render={({ field }) => (
                        <Select>
                            <SelectTrigger>
                                <SelectValue
                                    placeholder={
                                        departments.length === 0
                                            ? t('noDepartmentsAvailable')
                                            : t('selectDepartments')
                                    }
                                />
                            </SelectTrigger>

                            <SelectContent>
                                {departments.length === 0 ? (
                                    <div className="p-2 text-sm text-muted-foreground">
                                        {t('noDepartmentsCreated')}
                                    </div>
                                ) : (
                                    departments.map((department) => {
                                        const selected = field.value?.includes(department.id)

                                        return (
                                            <div
                                                key={department.id}
                                                className="flex items-center gap-2 p-2 rounded-md cursor-pointer hover:bg-accent"
                                                onClick={() => {
                                                    const current = new Set(field.value ?? [])
                                                    selected
                                                        ? current.delete(department.id)
                                                        : current.add(department.id)
                                                    field.onChange(Array.from(current))
                                                }}
                                            >
                                                <Checkbox
                                                    checked={selected}
                                                    onCheckedChange={() => { }}
                                                />
                                                <span>{department.name}</span>
                                            </div>
                                        )
                                    })
                                )}
                            </SelectContent>
                        </Select>
                    )}
                />

                {selectedDepartmentIds.length === 0 && (
                    <p className="mt-1 text-xs text-muted-foreground">
                        {t('noDepartmentsSelected')}
                    </p>
                )}
            </FormField>

            {/* Primary Department */}
            <FormField>
                <Label>
                    {t('primaryDepartment')}
                    <span className="ml-2 text-xs text-muted-foreground">({tCommon('optional')})</span>
                </Label>
                <Controller
                    name="primaryDepartmentId"
                    control={control}
                    render={({ field }) => (
                        <Select
                            value={field.value?.toString() || "none"}
                            onValueChange={(val) => field.onChange(val === "none" ? null : parseInt(val))}
                            disabled={selectedDepartmentIds.length === 0}
                        >
                            <SelectTrigger>
                                <SelectValue
                                    placeholder={
                                        selectedDepartmentIds.length === 0
                                            ? t('assignDepartmentsFirst')
                                            : t('selectPrimaryDepartment')
                                    }
                                />
                            </SelectTrigger>
                            <SelectContent>
                                <SelectItem value="none">{t('noPrimaryDepartment')}</SelectItem>
                                {selectedDepartmentIds.map((departmentId) => {
                                    const department = departments.find((g) => g.id === departmentId)
                                    if (!department) return null
                                    return (
                                        <SelectItem key={department.id} value={department.id.toString()}>
                                            {department.name}
                                        </SelectItem>
                                    )
                                })}
                            </SelectContent>
                        </Select>
                    )}
                />
            </FormField>
        </form>
    )
}
