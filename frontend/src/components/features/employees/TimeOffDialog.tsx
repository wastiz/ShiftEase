"use client";

import { useState } from "react";
import { useForm } from "react-hook-form";
import { useTranslations, useLocale } from "next-intl";
import { toast } from "sonner";
import {
    Dialog,
    DialogContent,
    DialogHeader,
    DialogTitle,
    DialogFooter,
} from "@/components/ui/shadcn/dialog";
import { Button } from "@/components/ui/shadcn/button";
import { Label } from "@/components/ui/shadcn/label";
import { Input } from "@/components/ui/shadcn/input";
import { Textarea } from "@/components/ui/shadcn/textarea";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/shadcn/tabs";
import FormField from "@/components/ui/FormField";
import { VacationDto, SickLeaveDto, PersonalDayDto } from "@/types";
// VacationDto and SickLeaveDto are used as type parameters for mutations
import { VacationForm } from "./VacationForm";
import { SickLeaveForm } from "./SickLeaveForm";
import { Calendar, Trash2 } from "lucide-react";
import { Badge } from "@/components/ui/shadcn/badge";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/shadcn/card";
import { ScrollArea } from "@/components/ui/shadcn/scroll-area";
import {
    useAddApprovedPersonalDay,
    useAddApprovedSickLeave,
    useAddApprovedVacation, useDeletePersonalDay, useDeleteSickLeave, useDeleteVacation, useGetPersonalDays,
    useGetSickLeaves,
    useGetVacations
} from "@/hooks/api";

type TimeOffDialogProps = {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    employeeId: number;
    employeeName: string;
};

export default function TimeOffDialog({
    open,
    onOpenChange,
    employeeId,
    employeeName,
}: TimeOffDialogProps) {
    const t = useTranslations('timeOff');
    const tCommon = useTranslations('common');
    const locale = useLocale();
    const dateLocale = locale === 'ru' ? 'ru-RU' : locale === 'et' ? 'et-EE' : 'en-US';
    const [activeTab, setActiveTab] = useState<"vacation" | "sick" | "personal">("vacation");
    const [viewMode, setViewMode] = useState<"add" | "view">("add");

    const addVacationMutation = useAddApprovedVacation(employeeId);
    const addSickLeaveMutation = useAddApprovedSickLeave(employeeId);
    const addPersonalDayMutation = useAddApprovedPersonalDay(employeeId);

    const { data: vacations = [], isLoading: vacationsLoading } = useGetVacations(employeeId);
    const { data: sickLeaves = [], isLoading: sickLeavesLoading } = useGetSickLeaves(employeeId);
    const { data: personalDays = [], isLoading: personalDaysLoading } = useGetPersonalDays(employeeId);

    const deleteVacationMutation = useDeleteVacation(employeeId);
    const deleteSickLeaveMutation = useDeleteSickLeave(employeeId);
    const deletePersonalDayMutation = useDeletePersonalDay(employeeId);

    const {
        register: registerPersonal,
        handleSubmit: handleSubmitPersonal,
        reset: resetPersonal,
        formState: { errors: personalErrors },
    } = useForm<PersonalDayDto>();

    const onSubmitVacation = (data: VacationDto) => {
        addVacationMutation.mutate(data, {
            onSuccess: () => {
                toast.success(t('vacationAddedSuccess'));
                setViewMode("view");
            },
            onError: () => {
                toast.error(t('vacationAddFailed'));
            },
        });
    };

    const onSubmitSick = (data: SickLeaveDto) => {
        addSickLeaveMutation.mutate(data, {
            onSuccess: () => {
                toast.success(t('sickLeaveAddedSuccess'));
                setViewMode("view");
            },
            onError: () => {
                toast.error(t('sickLeaveAddFailed'));
            },
        });
    };

    const onSubmitPersonal = (data: PersonalDayDto) => {
        addPersonalDayMutation.mutate(data, {
            onSuccess: () => {
                toast.success(t('personalDayAddedSuccess'));
                resetPersonal();
                setViewMode("view");
            },
            onError: () => {
                toast.error(t('personalDayAddFailed'));
            },
        });
    };

    const handleDelete = (id: number, type: "vacation" | "sick" | "personal") => {
        if (type === "vacation") {
            deleteVacationMutation.mutate(id, {
                onSuccess: () => toast.success(t('vacationDeleted')),
                onError: () => toast.error(t('vacationDeleteFailed')),
            });
        } else if (type === "sick") {
            deleteSickLeaveMutation.mutate(id, {
                onSuccess: () => toast.success(t('sickLeaveDeleted')),
                onError: () => toast.error(t('sickLeaveDeleteFailed')),
            });
        } else {
            deletePersonalDayMutation.mutate(id, {
                onSuccess: () => toast.success(t('personalDayDeleted')),
                onError: () => toast.error(t('personalDayDeleteFailed')),
            });
        }
    };

    const handleClose = () => {
        resetPersonal();
        setViewMode("add");
        onOpenChange(false);
    };

    const formatDate = (dateString: string) => {
        return new Date(dateString).toLocaleDateString(dateLocale, {
            year: 'numeric',
            month: 'short',
            day: 'numeric'
        });
    };

    return (
        <Dialog open={open} onOpenChange={handleClose}>
            <DialogContent className="sm:max-w-[700px] max-h-[90vh]">
                <DialogHeader>
                    <DialogTitle>{t('manageTimeOff')} - {employeeName}</DialogTitle>
                </DialogHeader>

                <div className="flex gap-2 mb-4">
                    <Button
                        variant={viewMode === "add" ? "default" : "outline"}
                        onClick={() => setViewMode("add")}
                        size="sm"
                    >
                        {t('addTimeOff')}
                    </Button>
                    <Button
                        variant={viewMode === "view" ? "default" : "outline"}
                        onClick={() => setViewMode("view")}
                        size="sm"
                    >
                        {t('viewCurrentTimeOff')}
                    </Button>
                </div>

                {viewMode === "add" ? (
                    <Tabs value={activeTab} onValueChange={(v) => setActiveTab(v as "vacation" | "sick" | "personal")}>
                        <TabsList className="grid w-full grid-cols-3">
                            <TabsTrigger value="vacation">{t('vacation')}</TabsTrigger>
                            <TabsTrigger value="sick">{t('sickLeave')}</TabsTrigger>
                            <TabsTrigger value="personal">{t('personalDay')}</TabsTrigger>
                        </TabsList>

                        <TabsContent value="vacation" className="mt-4">
                            <VacationForm formId="vacation-form" onSubmit={onSubmitVacation} />
                            <DialogFooter className="mt-6">
                                <Button variant="outline" onClick={handleClose}>
                                    {tCommon('cancel')}
                                </Button>
                                <Button
                                    type="submit"
                                    form="vacation-form"
                                    disabled={addVacationMutation.isPending}
                                >
                                    {addVacationMutation.isPending ? t('adding') : t('addVacation')}
                                </Button>
                            </DialogFooter>
                        </TabsContent>

                        <TabsContent value="sick" className="mt-4">
                            <SickLeaveForm formId="sick-form" onSubmit={onSubmitSick} />
                            <DialogFooter className="mt-6">
                                <Button variant="outline" onClick={handleClose}>
                                    {tCommon('cancel')}
                                </Button>
                                <Button
                                    type="submit"
                                    form="sick-form"
                                    disabled={addSickLeaveMutation.isPending}
                                >
                                    {addSickLeaveMutation.isPending ? t('adding') : t('addSickLeave')}
                                </Button>
                            </DialogFooter>
                        </TabsContent>

                        <TabsContent value="personal">
                            <form
                                id="personal-form"
                                onSubmit={handleSubmitPersonal(onSubmitPersonal)}
                                className="space-y-4 mt-4"
                            >
                                <FormField>
                                    <Label htmlFor="personal-date">
                                        {t('date')}
                                        <span className="text-red-500 ml-1">*</span>
                                    </Label>
                                    <Input
                                        id="personal-date"
                                        type="date"
                                        {...registerPersonal("date", {
                                            required: t('dateRequired'),
                                        })}
                                    />
                                    {personalErrors.date && (
                                        <p className="text-red-500 text-sm">
                                            {personalErrors.date.message}
                                        </p>
                                    )}
                                </FormField>

                                <FormField>
                                    <Label htmlFor="personal-reason">{t('reasonOptional')}</Label>
                                    <Textarea
                                        id="personal-reason"
                                        placeholder={t('enterPersonalDayReason')}
                                        {...registerPersonal("reason")}
                                    />
                                </FormField>
                            </form>

                            <DialogFooter className="mt-6">
                                <Button variant="outline" onClick={handleClose}>
                                    {tCommon('cancel')}
                                </Button>
                                <Button
                                    type="submit"
                                    form="personal-form"
                                    disabled={addPersonalDayMutation.isPending}
                                >
                                    {addPersonalDayMutation.isPending ? t('adding') : t('addPersonalDay')}
                                </Button>
                            </DialogFooter>
                        </TabsContent>
                    </Tabs>
                ) : (
                    <ScrollArea className="h-[500px] pr-4">
                        <div className="space-y-6">
                            {/* Vacations */}
                            <Card>
                                <CardHeader>
                                    <CardTitle className="text-lg flex items-center gap-2">
                                        <Calendar className="h-5 w-5 text-blue-500" />
                                        {t('vacations')}
                                        <Badge variant="secondary">{vacations.length}</Badge>
                                    </CardTitle>
                                </CardHeader>
                                <CardContent>
                                    {vacationsLoading ? (
                                        <p className="text-sm text-muted-foreground">{tCommon('loading')}</p>
                                    ) : vacations.length === 0 ? (
                                        <p className="text-sm text-muted-foreground">{t('noVacationsScheduled')}</p>
                                    ) : (
                                        <div className="space-y-2">
                                            {vacations.map((vacation) => (
                                                <div
                                                    key={vacation.id}
                                                    className="flex items-start justify-between p-3 border rounded-lg hover:bg-accent"
                                                >
                                                    <div className="flex-1">
                                                        <div className="flex items-center gap-2">
                                                            <Badge variant="outline" className="bg">
                                                                {formatDate(vacation.startDate)} - {formatDate(vacation.endDate)}
                                                            </Badge>
                                                        </div>
                                                        {vacation.reason && (
                                                            <p className="text-sm text-muted-foreground mt-1">
                                                                {vacation.reason}
                                                            </p>
                                                        )}
                                                    </div>
                                                    <Button
                                                        variant="ghost"
                                                        size="sm"
                                                        onClick={() => handleDelete(vacation.id!, "vacation")}
                                                        disabled={deleteVacationMutation.isPending}
                                                    >
                                                        <Trash2 className="h-4 w-4 text-red-500" />
                                                    </Button>
                                                </div>
                                            ))}
                                        </div>
                                    )}
                                </CardContent>
                            </Card>

                            {/* Sick Leaves */}
                            <Card>
                                <CardHeader>
                                    <CardTitle className="text-lg flex items-center gap-2">
                                        <Calendar className="h-5 w-5 text-red-500" />
                                        {t('sickLeaves')}
                                        <Badge variant="secondary">{sickLeaves.length}</Badge>
                                    </CardTitle>
                                </CardHeader>
                                <CardContent>
                                    {sickLeavesLoading ? (
                                        <p className="text-sm text-muted-foreground">{tCommon('loading')}</p>
                                    ) : sickLeaves.length === 0 ? (
                                        <p className="text-sm text-muted-foreground">{t('noSickLeavesRecorded')}</p>
                                    ) : (
                                        <div className="space-y-2">
                                            {sickLeaves.map((sick) => (
                                                <div
                                                    key={sick.id}
                                                    className="flex items-start justify-between p-3 border rounded-lg hover:bg-accent"
                                                >
                                                    <div className="flex-1">
                                                        <div className="flex items-center gap-2">
                                                            <Badge variant="outline" className="bg">
                                                                {formatDate(sick.startDate)} - {formatDate(sick.endDate)}
                                                            </Badge>
                                                        </div>
                                                        {sick.diagnosis && (
                                                            <p className="text-sm text-muted-foreground mt-1">
                                                                {t('diagnosis')}: {sick.diagnosis}
                                                            </p>
                                                        )}
                                                        {sick.documentNumber && (
                                                            <p className="text-sm text-muted-foreground">
                                                                {t('docNumber')}: {sick.documentNumber}
                                                            </p>
                                                        )}
                                                    </div>
                                                    <Button
                                                        variant="ghost"
                                                        size="sm"
                                                        onClick={() => handleDelete(sick.id!, "sick")}
                                                        disabled={deleteSickLeaveMutation.isPending}
                                                    >
                                                        <Trash2 className="h-4 w-4 text-red-500" />
                                                    </Button>
                                                </div>
                                            ))}
                                        </div>
                                    )}
                                </CardContent>
                            </Card>

                            {/* Personal Days */}
                            <Card>
                                <CardHeader>
                                    <CardTitle className="text-lg flex items-center gap-2">
                                        <Calendar className="h-5 w-5 text-amber-500" />
                                        {t('personalDays')}
                                        <Badge variant="secondary">{personalDays.length}</Badge>
                                    </CardTitle>
                                </CardHeader>
                                <CardContent>
                                    {personalDaysLoading ? (
                                        <p className="text-sm text-muted-foreground">{tCommon('loading')}</p>
                                    ) : personalDays.length === 0 ? (
                                        <p className="text-sm text-muted-foreground">{t('noPersonalDaysScheduled')}</p>
                                    ) : (
                                        <div className="space-y-2">
                                            {personalDays.map((day) => (
                                                <div
                                                    key={day.id}
                                                    className="flex items-start justify-between p-3 border rounded-lg hover:bg-accent"
                                                >
                                                    <div className="flex-1">
                                                        <div className="flex items-center gap-2">
                                                            <Badge variant="outline" className="bg">
                                                                {formatDate(day.date)}
                                                            </Badge>
                                                        </div>
                                                        {day.reason && (
                                                            <p className="text-sm text-muted-foreground mt-1">
                                                                {day.reason}
                                                            </p>
                                                        )}
                                                    </div>
                                                    <Button
                                                        variant="ghost"
                                                        size="sm"
                                                        onClick={() => handleDelete(day.id!, "personal")}
                                                        disabled={deletePersonalDayMutation.isPending}
                                                    >
                                                        <Trash2 className="h-4 w-4 text-red-500" />
                                                    </Button>
                                                </div>
                                            ))}
                                        </div>
                                    )}
                                </CardContent>
                            </Card>
                        </div>

                        <DialogFooter className="mt-6">
                            <Button variant="outline" onClick={handleClose}>
                                {tCommon('close')}
                            </Button>
                        </DialogFooter>
                    </ScrollArea>
                )}
            </DialogContent>
        </Dialog>
    );
}
