"use client";

import { useForm, Controller } from "react-hook-form";
import { useTranslations } from "next-intl";
import { format } from "date-fns";
import { DateRange } from "react-day-picker";
import { Label } from "@/components/ui/shadcn/label";
import { Input } from "@/components/ui/shadcn/input";
import { Textarea } from "@/components/ui/shadcn/textarea";
import { DatePickerRange } from "@/components/ui/shadcn/date-picker";
import FormField from "@/components/ui/FormField";
import { SickLeaveDto } from "@/types";

type Props = {
    formId: string;
    onSubmit: (data: SickLeaveDto) => void;
};

type FormValues = {
    dateRange: DateRange | undefined;
    diagnosis?: string;
    documentNumber?: string;
};

export function SickLeaveForm({ formId, onSubmit }: Props) {
    const t = useTranslations("timeOff");
    const { control, register, handleSubmit, formState: { errors } } = useForm<FormValues>();

    const handleFormSubmit = (data: FormValues) => {
        onSubmit({
            startDate: data.dateRange?.from ? format(data.dateRange.from, "yyyy-MM-dd") : "",
            endDate: data.dateRange?.to ? format(data.dateRange.to, "yyyy-MM-dd") : "",
            diagnosis: data.diagnosis,
            documentNumber: data.documentNumber,
        });
    };

    return (
        <form id={formId} onSubmit={handleSubmit(handleFormSubmit)} className="space-y-4">
            <FormField>
                <Label>
                    {t("sickLeavePeriod")}
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
                <Label htmlFor={`${formId}-diagnosis`}>{t("diagnosisOptional")}</Label>
                <Textarea
                    id={`${formId}-diagnosis`}
                    placeholder={t("enterDiagnosis")}
                    {...register("diagnosis")}
                />
            </FormField>

            <FormField>
                <Label htmlFor={`${formId}-documentNumber`}>{t("documentNumberOptional")}</Label>
                <Input
                    id={`${formId}-documentNumber`}
                    placeholder={t("enterDocumentNumber")}
                    {...register("documentNumber")}
                />
            </FormField>
        </form>
    );
}
