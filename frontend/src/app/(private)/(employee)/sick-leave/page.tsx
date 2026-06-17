'use client';

import { useState } from 'react';
import { useTranslations, useLocale } from 'next-intl';
import { FileText, Plus, Trash2, Loader2, AlertCircle } from 'lucide-react';
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
    useSickLeaves,
    useSickLeaveRequests,
    useAddSickLeaveRequest,
    useDeleteSickLeaveRequest,
} from '@/hooks/api';
import { EmployeeMeData } from '@/types';
import { useAuthStore } from '@/stores/authStore';
import { toast } from 'sonner';
import { format } from 'date-fns';
import { enUS, ru, et } from 'date-fns/locale';
import Header from "@/components/ui/headers/Header";
import Main from "@/components/ui/Main";

export default function SickLeavePage() {
    const t = useTranslations('employee.sickLeave');
    const tCommon = useTranslations('common');
    const locale = useLocale();
    const dateLocale = locale === 'ru' ? ru : locale === 'et' ? et : enUS;

    const [isDialogOpen, setIsDialogOpen] = useState(false);
    const [startDate, setStartDate] = useState('');
    const [endDate, setEndDate] = useState('');
    const [diagnosis, setDiagnosis] = useState('');
    const [documentNumber, setDocumentNumber] = useState('');

    const { user } = useAuthStore();
    const employeeUser = user as EmployeeMeData;
    const employeeId = employeeUser.id;

    const { data: sickLeaves, isLoading: isLoadingSickLeaves } = useSickLeaves(employeeId);
    const { data: sickLeaveRequests, isLoading: isLoadingRequests } = useSickLeaveRequests(employeeId);
    const addSickLeaveRequest = useAddSickLeaveRequest(employeeId);
    const deleteSickLeaveRequest = useDeleteSickLeaveRequest(employeeId);

    const isLoading = isLoadingSickLeaves || isLoadingRequests;

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();

        if (!startDate || !endDate) {
            toast.error(t('fillRequiredFields'));
            return;
        }

        try {
            await addSickLeaveRequest.mutateAsync({
                startDate,
                endDate,
                diagnosis: diagnosis || undefined,
                documentNumber: documentNumber || undefined,
            });

            toast.success(t('requestSubmitted'));

            setIsDialogOpen(false);
            setStartDate('');
            setEndDate('');
            setDiagnosis('');
            setDocumentNumber('');
        } catch (error) {
            toast.error(t('submitFailed'));
        }
    };

    const handleDeleteRequest = async (id: number) => {
        if (!confirm(t('confirmDelete'))) return;

        try {
            await deleteSickLeaveRequest.mutateAsync(id);
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
                            <Plus className="h-4 w-4 mr-2" />
                            <span className="hidden sm:inline">{t('newRequest')}</span>
                        </Button>
                    </DialogTrigger>
                    <DialogContent className="sm:max-w-[425px]">
                        <form onSubmit={handleSubmit}>
                            <DialogHeader>
                                <DialogTitle>{t('newSickLeaveRequest')}</DialogTitle>
                                <DialogDescription>
                                    {t('submitForApproval')}
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
                                    <Label htmlFor="documentNumber">{t('documentNumber')}</Label>
                                    <Input
                                        id="documentNumber"
                                        placeholder={t('documentNumberPlaceholder')}
                                        value={documentNumber}
                                        onChange={(e) => setDocumentNumber(e.target.value)}
                                    />
                                </div>

                                <div className="grid gap-2">
                                    <Label htmlFor="diagnosis">{t('diagnosisOptional')}</Label>
                                    <Textarea
                                        id="diagnosis"
                                        placeholder={t('briefDescription')}
                                        value={diagnosis}
                                        onChange={(e) => setDiagnosis(e.target.value)}
                                        rows={3}
                                    />
                                </div>
                            </div>

                            <DialogFooter>
                                <Button type="submit" disabled={addSickLeaveRequest.isPending}>
                                    {addSickLeaveRequest.isPending && (
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
                        <div>
                            <p className="text-muted-foreground text-sm md:text-base">
                                {t('viewAndSubmit')}
                            </p>
                        </div>

                        <Alert>
                            <AlertCircle className="h-4 w-4"/>
                            <AlertDescription>
                                {t('documentNotice')}
                            </AlertDescription>
                        </Alert>
                    </div>

                    <Tabs defaultValue="requests" className="w-full">
                        <TabsList className="grid w-full grid-cols-2">
                            <TabsTrigger value="requests">{t('pendingRequests')}</TabsTrigger>
                            <TabsTrigger value="approved">{tCommon('approved')}</TabsTrigger>
                        </TabsList>

                        <TabsContent value="requests" className="space-y-4 mt-6">
                            {sickLeaveRequests && sickLeaveRequests.length === 0 ? (
                                <Card>
                                    <CardContent className="flex flex-col items-center justify-center py-10">
                                        <FileText className="h-12 w-12 text-muted-foreground mb-4"/>
                                        <p className="text-muted-foreground text-center">
                                            {t('noPendingRequests')}
                                        </p>
                                    </CardContent>
                                </Card>
                            ) : (
                                sickLeaveRequests?.map((request) => (
                                    <Card key={request.id} className="hover:shadow-md transition-shadow">
                                        <CardHeader className="pb-3">
                                            <div className="flex items-start justify-between gap-2">
                                                <div className="flex-1 min-w-0">
                                                    <CardTitle className="text-lg md:text-xl flex items-center gap-2 flex-wrap">
                                                        <FileText className="h-5 w-5 flex-shrink-0"/>
                                                        <span className="truncate">
                                                            {format(new Date(request.startDate), 'MMM d', {locale: dateLocale})}
                                                            {' — '}
                                                            {format(new Date(request.endDate), 'MMM d, yyyy', {locale: dateLocale})}
                                                        </span>
                                                    </CardTitle>
                                                    <CardDescription className="mt-1">
                                                        {Math.ceil(
                                                            (new Date(request.endDate).getTime() -
                                                                new Date(request.startDate).getTime()) /
                                                            (1000 * 60 * 60 * 24)
                                                        )}{' '}
                                                        {t('days')}
                                                    </CardDescription>
                                                </div>

                                                <Button
                                                    variant="ghost"
                                                    size="icon"
                                                    onClick={() => request.id && handleDeleteRequest(request.id)}
                                                    disabled={deleteSickLeaveRequest.isPending}
                                                    className="flex-shrink-0"
                                                >
                                                    {deleteSickLeaveRequest.isPending ? (
                                                        <Loader2 className="h-4 w-4 animate-spin"/>
                                                    ) : (
                                                        <Trash2 className="h-4 w-4"/>
                                                    )}
                                                </Button>
                                            </div>
                                        </CardHeader>

                                        <CardContent className="space-y-2">
                                            <div className="flex items-center gap-2">
                                                {getStatusBadge(request.status)}
                                            </div>

                                            {request.documentNumber && (
                                                <div className="flex items-center gap-2 text-sm">
                                                    <span className="text-muted-foreground">{t('document')}:</span>
                                                    <span className="font-mono">{request.documentNumber}</span>
                                                </div>
                                            )}

                                            {request.diagnosis && (
                                                <p className="text-sm text-muted-foreground">
                                                    {request.diagnosis}
                                                </p>
                                            )}
                                        </CardContent>
                                    </Card>
                                ))
                            )}
                        </TabsContent>

                        <TabsContent value="approved" className="space-y-4 mt-6">
                            {sickLeaves && sickLeaves.length === 0 ? (
                                <Card>
                                    <CardContent className="flex flex-col items-center justify-center py-10">
                                        <FileText className="h-12 w-12 text-muted-foreground mb-4"/>
                                        <p className="text-muted-foreground text-center">
                                            {t('noApprovedSickLeaves')}
                                        </p>
                                    </CardContent>
                                </Card>
                            ) : (
                                sickLeaves?.map((sickLeave) => (
                                    <Card key={sickLeave.id} className="hover:shadow-md transition-shadow">
                                        <CardHeader className="pb-3">
                                            <div className="flex-1 min-w-0">
                                                <CardTitle className="text-lg md:text-xl flex items-center gap-2 flex-wrap">
                                                    <FileText className="h-5 w-5 flex-shrink-0"/>
                                                    <span className="truncate">
                                                        {format(new Date(sickLeave.startDate), 'MMM d', {locale: dateLocale})}
                                                        {' — '}
                                                        {format(new Date(sickLeave.endDate), 'MMM d, yyyy', {locale: dateLocale})}
                                                    </span>
                                                </CardTitle>
                                                <CardDescription className="mt-1">
                                                    {Math.ceil(
                                                        (new Date(sickLeave.endDate).getTime() -
                                                            new Date(sickLeave.startDate).getTime()) /
                                                        (1000 * 60 * 60 * 24)
                                                    )}{' '}
                                                    {t('days')}
                                                </CardDescription>
                                            </div>
                                        </CardHeader>

                                        <CardContent className="space-y-2">
                                            <Badge className="bg-green-500">{tCommon('approved')}</Badge>

                                            {sickLeave.documentNumber && (
                                                <div className="flex items-center gap-2 text-sm">
                                                    <span className="text-muted-foreground">{t('document')}:</span>
                                                    <span className="font-mono">{sickLeave.documentNumber}</span>
                                                </div>
                                            )}

                                            {sickLeave.diagnosis && (
                                                <p className="text-sm text-muted-foreground">
                                                    {sickLeave.diagnosis}
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
