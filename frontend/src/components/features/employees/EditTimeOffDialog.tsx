"use client";

import { useEffect } from "react";
import { useForm, Controller } from "react-hook-form";
import { format } from "date-fns";
import { DateRange } from "react-day-picker";
import { toast } from "sonner";
import { useQueryClient } from "@tanstack/react-query";
import {
    Dialog,
    DialogContent,
    DialogHeader,
    DialogTitle,
    DialogFooter,
} from "@/components/ui/shadcn/dialog";
import { Button } from "@/components/ui/shadcn/button";
import { Label } from "@/components/ui/shadcn/label";
import { Textarea } from "@/components/ui/shadcn/textarea";
import { Input } from "@/components/ui/shadcn/input";
import { DatePickerRange } from "@/components/ui/shadcn/date-picker";
import { DatePicker } from "@/components/ui/shadcn/date-picker";
import FormField from "@/components/ui/FormField";
import { EmployeeTimeOff, TimeOffType } from "@/types/schedule";
import { employeeKeys } from "@/lib/api-keys";
import api from "@/lib/api";

type Props = {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    entry: EmployeeTimeOff;
    year: number;
};

type VacationFormValues = {
    dateRange: DateRange | undefined;
    reason?: string;
};

type SickLeaveFormValues = {
    dateRange: DateRange | undefined;
    diagnosis?: string;
    documentNumber?: string;
};

type PersonalDayFormValues = {
    date: Date | undefined;
    reason?: string;
};

export function EditTimeOffDialog({ open, onOpenChange, entry, year }: Props) {
    if (entry.type === TimeOffType.Vacation) {
        return (
            <EditVacationDialog open={open} onOpenChange={onOpenChange} entry={entry} year={year} />
        );
    }
    if (entry.type === TimeOffType.SickLeave) {
        return (
            <EditSickLeaveDialog open={open} onOpenChange={onOpenChange} entry={entry} year={year} />
        );
    }
    return (
        <EditPersonalDayDialog open={open} onOpenChange={onOpenChange} entry={entry} year={year} />
    );
}

function EditVacationDialog({ open, onOpenChange, entry, year }: Props) {
    const qc = useQueryClient();
    const { control, register, handleSubmit, reset, formState: { errors, isSubmitting } } =
        useForm<VacationFormValues>();

    useEffect(() => {
        if (open) {
            reset({
                dateRange: {
                    from: new Date(entry.startDate),
                    to: new Date(entry.endDate),
                },
                reason: undefined,
            });
        }
    }, [open, entry, reset]);

    const onSubmit = async (data: VacationFormValues) => {
        try {
            await api.delete(`vacations/${entry.employeeId}/${entry.id}`);
            await api.post(`vacations/employer/${entry.employeeId}`, {
                startDate: data.dateRange?.from ? format(data.dateRange.from, "yyyy-MM-dd") : "",
                endDate: data.dateRange?.to ? format(data.dateRange.to, "yyyy-MM-dd") : "",
                reason: data.reason,
            });
            qc.invalidateQueries({ queryKey: employeeKeys.timeOffs(year) });
            toast.success("Vacation updated");
            onOpenChange(false);
        } catch {
            toast.error("Failed to update vacation");
        }
    };

    return (
        <Dialog open={open} onOpenChange={onOpenChange}>
            <DialogContent>
                <DialogHeader>
                    <DialogTitle>Edit Vacation — {entry.employeeName}</DialogTitle>
                </DialogHeader>
                <form id="edit-vacation" onSubmit={handleSubmit(onSubmit)} className="space-y-4 py-2">
                    <FormField>
                        <Label>Vacation period <span className="text-red-500">*</span></Label>
                        <Controller
                            control={control}
                            name="dateRange"
                            rules={{ validate: (v) => (v?.from && v?.to) ? true : "Date range is required" }}
                            render={({ field }) => (
                                <DatePickerRange value={field.value} onChange={field.onChange} />
                            )}
                        />
                        {errors.dateRange && (
                            <p className="text-red-500 text-sm">{errors.dateRange.message}</p>
                        )}
                    </FormField>
                    <FormField>
                        <Label>Reason (optional)</Label>
                        <Textarea placeholder="Enter reason" {...register("reason")} />
                    </FormField>
                </form>
                <DialogFooter>
                    <Button variant="outline" onClick={() => onOpenChange(false)}>Cancel</Button>
                    <Button type="submit" form="edit-vacation" disabled={isSubmitting}>
                        {isSubmitting ? "Saving..." : "Save"}
                    </Button>
                </DialogFooter>
            </DialogContent>
        </Dialog>
    );
}

function EditSickLeaveDialog({ open, onOpenChange, entry, year }: Props) {
    const qc = useQueryClient();
    const { control, register, handleSubmit, reset, formState: { errors, isSubmitting } } =
        useForm<SickLeaveFormValues>();

    useEffect(() => {
        if (open) {
            reset({
                dateRange: {
                    from: new Date(entry.startDate),
                    to: new Date(entry.endDate),
                },
            });
        }
    }, [open, entry, reset]);

    const onSubmit = async (data: SickLeaveFormValues) => {
        try {
            await api.delete(`sick-leaves/${entry.employeeId}/${entry.id}`);
            await api.post(`sick-leaves/employer/${entry.employeeId}`, {
                startDate: data.dateRange?.from ? format(data.dateRange.from, "yyyy-MM-dd") : "",
                endDate: data.dateRange?.to ? format(data.dateRange.to, "yyyy-MM-dd") : "",
                diagnosis: data.diagnosis,
                documentNumber: data.documentNumber,
            });
            qc.invalidateQueries({ queryKey: employeeKeys.timeOffs(year) });
            toast.success("Sick leave updated");
            onOpenChange(false);
        } catch {
            toast.error("Failed to update sick leave");
        }
    };

    return (
        <Dialog open={open} onOpenChange={onOpenChange}>
            <DialogContent>
                <DialogHeader>
                    <DialogTitle>Edit Sick Leave — {entry.employeeName}</DialogTitle>
                </DialogHeader>
                <form id="edit-sick-leave" onSubmit={handleSubmit(onSubmit)} className="space-y-4 py-2">
                    <FormField>
                        <Label>Sick leave period <span className="text-red-500">*</span></Label>
                        <Controller
                            control={control}
                            name="dateRange"
                            rules={{ validate: (v) => (v?.from && v?.to) ? true : "Date range is required" }}
                            render={({ field }) => (
                                <DatePickerRange value={field.value} onChange={field.onChange} />
                            )}
                        />
                        {errors.dateRange && (
                            <p className="text-red-500 text-sm">{errors.dateRange.message}</p>
                        )}
                    </FormField>
                    <FormField>
                        <Label>Diagnosis (optional)</Label>
                        <Textarea placeholder="Enter diagnosis" {...register("diagnosis")} />
                    </FormField>
                    <FormField>
                        <Label>Document number (optional)</Label>
                        <Input placeholder="Enter document number" {...register("documentNumber")} />
                    </FormField>
                </form>
                <DialogFooter>
                    <Button variant="outline" onClick={() => onOpenChange(false)}>Cancel</Button>
                    <Button type="submit" form="edit-sick-leave" disabled={isSubmitting}>
                        {isSubmitting ? "Saving..." : "Save"}
                    </Button>
                </DialogFooter>
            </DialogContent>
        </Dialog>
    );
}

function EditPersonalDayDialog({ open, onOpenChange, entry, year }: Props) {
    const qc = useQueryClient();
    const { control, register, handleSubmit, reset, formState: { isSubmitting } } =
        useForm<PersonalDayFormValues>();

    useEffect(() => {
        if (open) {
            reset({
                date: new Date(entry.startDate),
            });
        }
    }, [open, entry, reset]);

    const onSubmit = async (data: PersonalDayFormValues) => {
        if (!data.date) {
            toast.error("Date is required");
            return;
        }
        try {
            await api.delete(`personal-days/${entry.employeeId}/${entry.id}`);
            await api.post(`personal-days/employer/${entry.employeeId}`, {
                date: format(data.date, "yyyy-MM-dd"),
                reason: data.reason,
            });
            qc.invalidateQueries({ queryKey: employeeKeys.timeOffs(year) });
            toast.success("Personal day updated");
            onOpenChange(false);
        } catch {
            toast.error("Failed to update personal day");
        }
    };

    return (
        <Dialog open={open} onOpenChange={onOpenChange}>
            <DialogContent>
                <DialogHeader>
                    <DialogTitle>Edit Personal Day — {entry.employeeName}</DialogTitle>
                </DialogHeader>
                <form id="edit-personal-day" onSubmit={handleSubmit(onSubmit)} className="space-y-4 py-2">
                    <FormField>
                        <Label>Date <span className="text-red-500">*</span></Label>
                        <Controller
                            control={control}
                            name="date"
                            rules={{ required: "Date is required" }}
                            render={({ field }) => (
                                <DatePicker date={field.value} onChange={field.onChange} />
                            )}
                        />
                    </FormField>
                    <FormField>
                        <Label>Reason (optional)</Label>
                        <Textarea placeholder="Enter reason" {...register("reason")} />
                    </FormField>
                </form>
                <DialogFooter>
                    <Button variant="outline" onClick={() => onOpenChange(false)}>Cancel</Button>
                    <Button type="submit" form="edit-personal-day" disabled={isSubmitting}>
                        {isSubmitting ? "Saving..." : "Save"}
                    </Button>
                </DialogFooter>
            </DialogContent>
        </Dialog>
    );
}
