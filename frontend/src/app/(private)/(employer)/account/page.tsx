'use client';

import { useState } from 'react';
import { useTranslations } from 'next-intl';
import { toast } from 'sonner';
import Header from '@/components/ui/headers/Header';
import Main from '@/components/ui/Main';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/shadcn/card';
import { Button } from '@/components/ui/shadcn/button';
import { Input } from '@/components/ui/shadcn/input';
import { Label } from '@/components/ui/shadcn/label';
import { PasswordInput } from '@/components/ui/inputs/PasswordInput';
import {
    AlertDialog,
    AlertDialogContent,
    AlertDialogDescription,
    AlertDialogFooter,
    AlertDialogHeader,
    AlertDialogTitle,
} from '@/components/ui/shadcn/alert-dialog';
import { useAuthStore } from '@/stores/authStore';
import { useChangePassword, useUpdatePhone, useDeleteAccount } from '@/hooks/api/auth';
import { isApiError } from '@/hooks/useApiError';

export default function AccountPage() {
    const t = useTranslations('account');
    const tCommon = useTranslations('common');
    const { user } = useAuthStore();

    // Change password
    const [pwForm, setPwForm] = useState({ currentPassword: '', newPassword: '', confirmNewPassword: '' });
    const [pwError, setPwError] = useState<string | null>(null);
    const changePassword = useChangePassword();

    const handleChangePassword = (e: React.FormEvent) => {
        e.preventDefault();
        setPwError(null);
        if (pwForm.newPassword !== pwForm.confirmNewPassword) {
            setPwError(t('passwordMismatch'));
            return;
        }
        changePassword.mutate(
            { currentPassword: pwForm.currentPassword, newPassword: pwForm.newPassword },
            {
                onSuccess: () => {
                    toast.success(t('passwordUpdated'));
                    setPwForm({ currentPassword: '', newPassword: '', confirmNewPassword: '' });
                },
                onError: (err) => {
                    setPwError(isApiError(err) ? err.message : tCommon('unknownError'));
                },
            }
        );
    };

    // Phone
    const [phone, setPhone] = useState(user?.phone ?? '');
    const updatePhone = useUpdatePhone();

    const handleSavePhone = (e: React.FormEvent) => {
        e.preventDefault();
        updatePhone.mutate(
            { phone },
            {
                onSuccess: () => toast.success(t('phoneSaved')),
                onError: (err) => toast.error(isApiError(err) ? err.message : tCommon('unknownError')),
            }
        );
    };

    // Delete account
    const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
    const [deletePassword, setDeletePassword] = useState('');
    const deleteAccount = useDeleteAccount();

    const handleDeleteAccount = () => {
        deleteAccount.mutate(
            { password: deletePassword },
            {
                onError: (err) => {
                    toast.error(isApiError(err) ? err.message : tCommon('unknownError'));
                },
            }
        );
    };

    return (
        <>
            <Header title={t('title')} />
            <Main>
                <div className="max-w-2xl mx-auto space-y-6 p-4 md:p-6">

                    {/* Change Password */}
                    <Card>
                        <CardHeader>
                            <CardTitle>{t('changePassword')}</CardTitle>
                        </CardHeader>
                        <CardContent>
                            <form onSubmit={handleChangePassword} className="space-y-4">
                                <div className="grid gap-1.5">
                                    <Label htmlFor="currentPassword">{t('currentPassword')}</Label>
                                    <PasswordInput
                                        id="currentPassword"
                                        value={pwForm.currentPassword}
                                        onChange={(e) => setPwForm(p => ({ ...p, currentPassword: e.target.value }))}
                                        required
                                    />
                                </div>
                                <div className="grid gap-1.5">
                                    <Label htmlFor="newPassword">{t('newPassword')}</Label>
                                    <PasswordInput
                                        id="newPassword"
                                        value={pwForm.newPassword}
                                        onChange={(e) => setPwForm(p => ({ ...p, newPassword: e.target.value }))}
                                        required
                                    />
                                </div>
                                <div className="grid gap-1.5">
                                    <Label htmlFor="confirmNewPassword">{t('confirmNewPassword')}</Label>
                                    <PasswordInput
                                        id="confirmNewPassword"
                                        value={pwForm.confirmNewPassword}
                                        onChange={(e) => setPwForm(p => ({ ...p, confirmNewPassword: e.target.value }))}
                                        required
                                    />
                                </div>
                                {pwError && <p className="text-sm text-destructive">{pwError}</p>}
                                <Button type="submit" disabled={changePassword.isPending}>
                                    {changePassword.isPending ? tCommon('saving') : tCommon('save')}
                                </Button>
                            </form>
                        </CardContent>
                    </Card>

                    {/* Contact Phone */}
                    <Card>
                        <CardHeader>
                            <CardTitle>{t('contactPhone')}</CardTitle>
                            <CardDescription>{t('phoneDescription')}</CardDescription>
                        </CardHeader>
                        <CardContent>
                            <form onSubmit={handleSavePhone} className="flex gap-3 items-end">
                                <div className="grid gap-1.5 flex-1">
                                    <Label htmlFor="phone">{t('contactPhone')}</Label>
                                    <Input
                                        id="phone"
                                        type="tel"
                                        placeholder="+1 234 567 8900"
                                        value={phone}
                                        onChange={(e) => setPhone(e.target.value)}
                                    />
                                </div>
                                <Button type="submit" disabled={updatePhone.isPending}>
                                    {updatePhone.isPending ? tCommon('saving') : tCommon('save')}
                                </Button>
                            </form>
                        </CardContent>
                    </Card>

                    {/* Danger Zone */}
                    <Card className="border-destructive/40">
                        <CardHeader>
                            <CardTitle className="text-destructive">{t('dangerZone')}</CardTitle>
                        </CardHeader>
                        <CardContent className="flex items-center justify-between gap-4">
                            <div>
                                <p className="font-medium text-sm">{t('deleteAccount')}</p>
                                <p className="text-sm text-muted-foreground">{t('deleteAccountDescription')}</p>
                            </div>
                            <Button
                                variant="destructive"
                                onClick={() => setDeleteDialogOpen(true)}
                                className="shrink-0"
                            >
                                {t('deleteAccountButton')}
                            </Button>
                        </CardContent>
                    </Card>
                </div>

                {/* Delete account confirmation */}
                <AlertDialog open={deleteDialogOpen} onOpenChange={(open) => {
                    setDeleteDialogOpen(open);
                    if (!open) setDeletePassword('');
                }}>
                    <AlertDialogContent>
                        <AlertDialogHeader>
                            <AlertDialogTitle>{t('deleteAccountConfirmTitle')}</AlertDialogTitle>
                            <AlertDialogDescription>{t('deleteAccountConfirmDescription')}</AlertDialogDescription>
                        </AlertDialogHeader>
                        <div className="grid gap-1.5 py-2">
                            <Label htmlFor="deletePassword">{t('confirmWithPassword')}</Label>
                            <PasswordInput
                                id="deletePassword"
                                value={deletePassword}
                                onChange={(e) => setDeletePassword(e.target.value)}
                            />
                        </div>
                        <AlertDialogFooter>
                            <Button variant="secondary" onClick={() => setDeleteDialogOpen(false)}>
                                {tCommon('cancel')}
                            </Button>
                            <Button
                                variant="destructive"
                                onClick={handleDeleteAccount}
                                disabled={!deletePassword || deleteAccount.isPending}
                            >
                                {deleteAccount.isPending ? tCommon('deleting') : t('deleteAccountButton')}
                            </Button>
                        </AlertDialogFooter>
                    </AlertDialogContent>
                </AlertDialog>
            </Main>
        </>
    );
}
