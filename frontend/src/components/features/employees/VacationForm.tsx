"use client";

import { useForm, Controller } from "react-hook-form";
import { useTranslations } from "next-intl";
import { format } from "date-fns";
import { DateRange } from "react-day-picker";
import { Label } from "@/components/ui/shadcn/label";
import { Textarea } from "@/components/ui/shadcn/textarea";
import { DatePickerRange } from "@/components/ui/shadcn/date-picker";
import FormField from "@/components/ui/FormField";
import { VacationDto } from "@/types";

type Props = {
    formId: string;
    onSubmit: (data: VacationDto) => void;
};

type FormValues = {
    dateRange: DateRange | undefined;
    reason?: string;
};

export function VacationForm({ formId, onSubmit }: Props) {
    const t = useTranslations("timeOff");
    const { control, register, handleSubmit, formState: { errors } } = useForm<FormValues>();

    const handleFormSubmit = (data: FormValues) => {
        onSubmit({
            startDate: data.dateRange?.from ? format(data.dateRange.from, "yyyy-MM-dd") : "",
            endDate: data.dateRange?.to ? format(data.dateRange.to, "yyyy-MM-dd") : "",
            reason: data.reason,
        });
    };

    return (
        <form id={formId} onSubmit={handleSubmit(handleFormSubmit)} className="space-y-4">
            <FormField>
                <Label>
                    {t("vacationPeriod")}
                    <span className="text-red-500 ml-1">*</span>
                </Label>
                <Controller
                    control={control}
                    name="dateRange"
                    rules={{
                        validate: (v) =>
                            (v?.from && v?.to) ? true : t("dateRangeRequired"),
                    }}
                    render={({ field }) => (
                        <DatePickerRange
                            value={field.value}
                            onChange={field.onChange}
                        />
                    )}
                />
                {errors.dateRange && (
                    <p className="text-red-500 text-sm">{errors.dateRange.message}</p>
                )}
            </FormField>

            <FormField>
                <Label htmlFor={`${formId}-reason`}>{t("reasonOptional")}</Label>
                <Textarea
                    id={`${formId}-reason`}
                    placeholder={t("enterVacationReason")}
                    {...register("reason")}
                />
            </FormField>
        </form>
    );
}
