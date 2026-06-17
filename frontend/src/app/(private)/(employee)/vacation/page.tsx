'use client';

import { useState } from 'react';
import { useTranslations, useLocale } from 'next-intl';
import { Calendar, Plus, Trash2, Loader2, AlertCircle } from 'lucide-react';
import { Button } from '@/components/ui/shadcn/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/shadcn/card';
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle, DialogTrigger } from '@/components/ui/shadcn/dialog';
import { Input } from '@/components/ui/shadcn/input';
import { Label } from '@/components/ui/shadcn/label';
import { Textarea } from '@/components/ui/shadcn/textarea';
import { Badge } from '@/components/ui/shadcn/badge';
import { Alert, AlertDescription } from '@/components/ui/shadcn/alert';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/shadcn/tabs';
import {
    useEmployeeVacations,
    useVacationRequests,
    useAddVacationRequest,
    useDeleteVacationRequest,
} from '@/hooks/api';
import { EmployeeMeData } from '@/types';
import { useAuthStore } from '@/stores/authStore';
import { toast } from "sonner";
import { format } from 'date-fns';
import { enUS, ru, et } from 'date-fns/locale';
import Header from "@/components/ui/headers/Header";
import Main from "@/components/ui/Main";

export default function VacationPage() {
    const t = useTranslations('employee.vacation');
    const tCommon = useTranslations('common');
    const locale = useLocale();
    const dateLocale = locale === 'ru' ? ru : locale === 'et' ? et : enUS;

    const [isDialogOpen, setIsDialogOpen] = useState(false);
    const [startDate, setStartDate] = useState('');
    const [endDate, setEndDate] = useState('');
    const [reason, setReason] = useState('');

    const { user } = useAuthStore();
    const employeeUser = user as EmployeeMeData;
    const employeeId = employeeUser.id;

    const { data: approvedVacations, isLoading: isLoadingApproved } = useEmployeeVacations(employeeId);
    const { data: vacationRequests, isLoading: isLoadingRequests } = useVacationRequests(employeeId);
    const addVacationRequest = useAddVacationRequest(employeeId);
    const deleteVacationRequest = useDeleteVacationRequest(employeeId);

    const isLoading = isLoadingApproved || isLoadingRequests;

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();

        if (!startDate || !endDate) {
            toast.error(t('fillRequiredFields'));
            return;
        }

        try {
            await addVacationRequest.mutateAsync({
                startDate,
                endDate,
                reason,
            });

            toast.success(t('requestSubmitted'));

            setIsDialogOpen(false);
            setStartDate('');
            setEndDate('');
            setReason('');
        } catch (error) {
            toast.error(t('submitFailed'));
        }
    };

    const handleDeleteRequest = async (id: number) => {
        if (!confirm(t('confirmDelete'))) return;

        try {
            await deleteVacationRequest.mutateAsync(id);
            toast.success(t('requestDeleted'));
        } catch (error) {
            toast.error(t('deleteFailed'));
        }
    };

    const getStatusBadge = (status?: string) => {
        switch (status) {
            case 'approved':
                return <Badge className="bg-green-500">{tCommon('approved')}</Badge>;
            case 'rejected':
                return <Badge variant="destructive">{tCommon('rejected')}</Badge>;
            default:
                return <Badge variant="secondary">{tCommon('pending')}</Badge>;
        }
    };

    if (isLoading) {
        return (
            <div className="flex items-center justify-center min-h-screen">
                <Loader2 className="h-8 w-8 animate-spin" />
            </div>
        );
    }

    return (
        <>
            <Header title={t('title')}>
                <Dialog open={isDialogOpen} onOpenChange={setIsDialogOpen}>
                    <DialogTrigger asChild>
                        <Button size="sm" className="md:size-default">
                            <Plus className="h-4 w-4 mr-2"/>
                            <span className="hidden sm:inline">{t('addNew')}</span>
                        </Button>
                    </DialogTrigger>
                    <DialogContent className="sm:max-w-[425px]">
                        <form onSubmit={handleSubmit}>
                            <DialogHeader>
                                <DialogTitle>{t('newRequest')}</DialogTitle>
                                <DialogDescription>
                                    {t('fillDatesAndReason')}
                                </DialogDescription>
                            </DialogHeader>

                            <div className="grid gap-4 py-4">
                                <div className="grid gap-2">
                                    <Label htmlFor="startDate">{t('startDate')} *</Label>
                                    <Input
                                        id="startDate"
                                        type="date"
                                        value={startDate}
                                        onChange={(e) => setStartDate(e.target.value)}
                                        required
                                    />
                                </div>

                                <div className="grid gap-2">
                                    <Label htmlFor="endDate">{t('endDate')} *</Label>
                                    <Input
                                        id="endDate"
                                        type="date"
                                        value={endDate}
                                        onChange={(e) => setEndDate(e.target.value)}
                                        min={startDate}
                                        required
                                    />
                                </div>

                                <div className="grid gap-2">
                                    <Label htmlFor="reason">{t('reasonOptional')}</Label>
                                    <Textarea
                                        id="reason"
                                        placeholder={t('specifyReason')}
                                        value={reason}
                                        onChange={(e) => setReason(e.target.value)}
                                        rows={3}
                                    />
                                </div>
                            </div>

                            <DialogFooter>
                                <Button type="submit" disabled={addVacationRequest.isPending}>
                                    {addVacationRequest.isPending && (
                                        <Loader2 className="mr-2 h-4 w-4 animate-spin"/>
                                    )}
                                    {t('submitRequest')}
                                </Button>
                            </DialogFooter>
                        </form>
                    </DialogContent>
                </Dialog>
            </Header>
            <Main>
                <div className="container max-w-4xl mx-auto p-4 pb-20 md:p-6">
                    <div className="flex flex-col gap-4 mb-6">
                        <div className="flex items-center justify-between">
                            <div>
                                <p className="text-muted-foreground text-sm md:text-base">
                                    {t('manageRequests')}
                                </p>
                            </div>
                        </div>

                        <Alert>
                            <AlertCircle className="h-4 w-4"/>
                            <AlertDescription>
                                {t('advanceNotice')}
                            </AlertDescription>
                        </Alert>
                    </div>

                    <Tabs defaultValue="requests" className="w-full">
                        <TabsList className="grid w-full grid-cols-2">
                            <TabsTrigger value="requests">{t('pendingRequests')}</TabsTrigger>
                            <TabsTrigger value="approved">{tCommon('approved')}</TabsTrigger>
                        </TabsList>

                        <TabsContent value="requests" className="space-y-4 mt-6">
                            {vacationRequests && vacationRequests.length === 0 ? (
                                <Card>
                                    <CardContent className="flex flex-col items-center justify-center py-10">
                                        <Calendar className="h-12 w-12 text-muted-foreground mb-4"/>
                                        <p className="text-muted-foreground text-center">
                                            {t('noPendingRequests')}
                                        </p>
                                    </CardContent>
                                </Card>
                            ) : (
                                vacationRequests?.map((vacation) => (
                                    <Card key={vacation.id} className="hover:shadow-md transition-shadow">
                                        <CardHeader className="pb-3">
                                            <div className="flex items-start justify-between gap-2">
                                                <div className="flex-1 min-w-0">
                                                    <CardTitle className="text-lg md:text-xl flex items-center gap-2 flex-wrap">
                                                        <Calendar className="h-5 w-5 flex-shrink-0"/>
                                                        <span className="truncate">
                                                            {format(new Date(vacation.startDate), 'MMM d', {locale: dateLocale})}
                                                            {' — '}
                                                            {format(new Date(vacation.endDate), 'MMM d, yyyy', {locale: dateLocale})}
                                                        </span>
                                                    </CardTitle>
                                                    <CardDescription className="mt-1">
                                                        {Math.ceil(
                                                            (new Date(vacation.endDate).getTime() -
                                                                new Date(vacation.startDate).getTime()) /
                                                            (1000 * 60 * 60 * 24)
                                                        )}{' '}
                                                        {t('days')}
                                                    </CardDescription>
                                                </div>

                                                <Button
                                                    variant="ghost"
                                                    size="icon"
                                                    onClick={() => vacation.id && handleDeleteRequest(vacation.id)}
                                                    disabled={deleteVacationRequest.isPending}
                                                    className="flex-shrink-0"
                                                >
                                                    {deleteVacationRequest.isPending ? (
                                                        <Loader2 className="h-4 w-4 animate-spin"/>
                                                    ) : (
                                                        <Trash2 className="h-4 w-4"/>
                                                    )}
                                                </Button>
                                            </div>
                                        </CardHeader>

                                        <CardContent className="space-y-2">
                                            <div className="flex items-center gap-2">
                                                {getStatusBadge(vacation.status)}
                                            </div>

                                            {vacation.reason && (
                                                <p className="text-sm text-muted-foreground">
                                                    {vacation.reason}
                                                </p>
                                            )}
                                        </CardContent>
                                    </Card>
                                ))
                            )}
                        </TabsContent>

                        <TabsContent value="approved" className="space-y-4 mt-6">
                            {approvedVacations && approvedVacations.length === 0 ? (
                                <Card>
                                    <CardContent className="flex flex-col items-center justify-center py-10">
                                        <Calendar className="h-12 w-12 text-muted-foreground mb-4"/>
                                        <p className="text-muted-foreground text-center">
                                            {t('noApprovedVacations')}
                                        </p>
                                    </CardContent>
                                </Card>
                            ) : (
                                approvedVacations?.map((vacation) => (
                                    <Card key={vacation.id} className="hover:shadow-md transition-shadow">
                                        <CardHeader className="pb-3">
                                            <div className="flex-1 min-w-0">
                                                <CardTitle className="text-lg md:text-xl flex items-center gap-2 flex-wrap">
                                                    <Calendar className="h-5 w-5 flex-shrink-0"/>
                                                    <span className="truncate">
                                                        {format(new Date(vacation.startDate), 'MMM d', {locale: dateLocale})}
                                                        {' — '}
                                                        {format(new Date(vacation.endDate), 'MMM d, yyyy', {locale: dateLocale})}
                                                    </span>
                                                </CardTitle>
                                                <CardDescription className="mt-1">
                                                    {Math.ceil(
                                                        (new Date(vacation.endDate).getTime() -
                                                            new Date(vacation.startDate).getTime()) /
                                                        (1000 * 60 * 60 * 24)
                                                    )}{' '}
                                                    {t('days')}
                                                </CardDescription>
                                            </div>
                                        </CardHeader>

                                        <CardContent className="space-y-2">
                                            <Badge className="bg-green-500">{tCommon('approved')}</Badge>

                                            {vacation.reason && (
                                                <p className="text-sm text-muted-foreground">
                                                    {vacation.reason}
                                                </p>
                                            )}
                                        </CardContent>
                                    </Card>
                                ))
                            )}
                        </TabsContent>
                    </Tabs>
                </div>
            </Main>
        </>
    );
}
