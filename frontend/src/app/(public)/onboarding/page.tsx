'use client';

import React, { Suspense, useState, useMemo } from 'react';
import { useSearchParams, useRouter } from 'next/navigation';
import { useTranslations } from 'next-intl';
import { toast } from 'sonner';
import { useQueryClient } from '@tanstack/react-query';
import {
    UserPlus, Building2, CalendarDays, Layers, Clock4,
    Users, CheckCircle2, Check, Plus, Trash2, Info, ArrowRight,
} from 'lucide-react';

import { Button } from '@/components/ui/shadcn/button';
import { Input } from '@/components/ui/shadcn/input';
import { Label } from '@/components/ui/shadcn/label';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/shadcn/select';
import { PasswordInput } from '@/components/ui/inputs/PasswordInput';
import { Badge } from '@/components/ui/shadcn/badge';
import { cn } from '@/lib/utils';

import WorkScheduleSelector from '@/components/features/organization/WorkScheduleSelector';
import HolidaySelector from '@/components/features/organization/HolidaySelector';
import { DepartmentAsideForm } from '@/components/features/departments';
import { ShiftTemplateAsideForm } from '@/components/features/shift-types';
import CSVImporter, { EmployeeData } from '@/components/features/employees/bulk-import/CSVImporter';
import DepartmentCard from '@/components/ui/cards/DepartmentCard';

import { useEmployerRegister, useLogin } from '@/hooks/api/auth';
import {
    useAddOrganization,
    useGetDepartments, useCreateDepartment, useUpdateDepartment, useDeleteDepartment,
    useGetShiftTemplates, useCreateShiftTemplate, useUpdateShiftTemplate, useDeleteShiftTemplate,
    useBulkCreateEmployees,
} from '@/hooks/api';

import {
    Holiday, WorkDay, Department, ShiftTemplate,
    DepartmentFormValues, ShiftTemplateFormValues,
    ApiError, dayNameToEnum,
} from '@/types';
import { useAuthStore } from '@/stores/authStore';
import { useOrgStore } from '@/stores/orgStore';
import Logo from '@/components/ui/Logo';

type ScheduleMode = 'manual' | 'fiveTwo' | 'fullTime';
type LocalWorkDay = { active: boolean; startTime: string; endTime: string };

const INITIAL_WORK_DAYS: Record<string, LocalWorkDay> = {
    monday: { active: true, startTime: '08:00', endTime: '17:00' },
    tuesday: { active: true, startTime: '08:00', endTime: '17:00' },
    wednesday: { active: true, startTime: '08:00', endTime: '17:00' },
    thursday: { active: true, startTime: '08:00', endTime: '17:00' },
    friday: { active: true, startTime: '08:00', endTime: '17:00' },
    saturday: { active: false, startTime: '08:00', endTime: '17:00' },
    sunday: { active: false, startTime: '08:00', endTime: '17:00' },
};

const ORG_TYPE_KEYS = ['retail', 'healthcare', 'manufacturing', 'cafe', 'nonprofit', 'other'] as const;

function transformWorkDays(wd: Record<string, LocalWorkDay>): WorkDay[] {
    return Object.entries(wd)
        .filter(([, d]) => d.active)
        .map(([dayName, d]) => ({
            dayOfWeek: dayNameToEnum[dayName],
            startTime: d.startTime,
            endTime: d.endTime,
        }));
}

type EmpRow = {
    firstName: string; lastName: string; email: string; phone: string;
    position: string; departmentIds: number[];
    primaryDepartmentId: number | null;
    hourlyRate: number; priority: 'low' | 'medium' | 'high';
};

const EMPTY_EMP: EmpRow = {
    firstName: '', lastName: '', email: '', phone: '',
    position: '', departmentIds: [],
    primaryDepartmentId: null,
    hourlyRate: 0, priority: 'medium',
};

const STEP_ICONS = [UserPlus, Building2, CalendarDays, Layers, Clock4, Users, CheckCircle2] as const;
const STEP_KEYS = ['register', 'orgDetails', 'schedule', 'departments', 'shiftTemplates', 'employees', 'done'] as const;

function InfoBox({ children }: { children: React.ReactNode }) {
    return (
        <div className="flex gap-3 p-4 rounded-lg bg-primary/5 border border-primary/20 text-sm text-muted-foreground mb-6">
            <Info className="w-4 h-4 text-primary shrink-0 mt-0.5" />
            <p>{children}</p>
        </div>
    );
}

function OnboardingContent() {
    const searchParams = useSearchParams();
    const router = useRouter();
    const t = useTranslations('onboarding');
    const tAuth = useTranslations('auth');
    const tCommon = useTranslations('common');
    const queryClient = useQueryClient();

    const { user } = useAuthStore();
    const { setCurrentOrg } = useOrgStore();
    const fromOrg = searchParams.get('from') === 'organizations';
    const startStep = (fromOrg || !!user) ? 1 : 0;

    const [step, setStep] = useState(startStep);
    const [maxStep, setMaxStep] = useState(startStep);
    const [orgCreated, setOrgCreated] = useState(false);

    const goToStep = (target: number) => {
        setStep(target);
        setMaxStep(s => Math.max(s, target));
    };

    // Step 0 state
    const [authMode, setAuthMode] = useState<'register' | 'login'>('register');
    const [regForm, setRegForm] = useState({ fullName: '', email: '', phone: '', password: '' });
    const [loginForm, setLoginForm] = useState({ email: '', password: '' });
    const [authErrors, setAuthErrors] = useState<Record<string, string>>({});

    // Step 1 state
    const [orgName, setOrgName] = useState('');
    const [orgType, setOrgType] = useState('');
    const [orgCustomType, setOrgCustomType] = useState('');

    // Step 2 state
    const [scheduleMode, setScheduleMode] = useState<ScheduleMode>('fiveTwo');
    const [workDays, setWorkDays] = useState(INITIAL_WORK_DAYS);
    const [holidays, setHolidays] = useState<Holiday[]>([]);
    const [ftStart, setFtStart] = useState('00:00');
    const [ftEnd, setFtEnd] = useState('23:59');

    // Step 3 state
    const [deptOpen, setDeptOpen] = useState(false);
    const [selDept, setSelDept] = useState<Department | null>(null);

    // Step 4 state
    const [shiftOpen, setShiftOpen] = useState(false);
    const [selShift, setSelShift] = useState<ShiftTemplate | null>(null);

    // Step 5 state
    const [empRows, setEmpRows] = useState<EmpRow[]>([{ ...EMPTY_EMP }]);

    // API hooks
    const registerMutation = useEmployerRegister();
    const loginMutation = useLogin('Employer');
    const addOrg = useAddOrganization();
    const { data: depts = [] } = useGetDepartments();
    const createDept = useCreateDepartment();
    const updateDept = useUpdateDepartment(selDept?.id ?? 0);
    const deleteDept = useDeleteDepartment();
    const { data: shiftTemplates = [] } = useGetShiftTemplates();
    const createShift = useCreateShiftTemplate();
    const updateShift = useUpdateShiftTemplate(selShift?.id ?? 0);
    const deleteShift = useDeleteShiftTemplate();
    const bulkCreate = useBulkCreateEmployees();

    // Password criteria
    const pwCriteria = useMemo(() => {
        const p = regForm.password;
        return {
            length: p.length >= 8,
            uppercase: /[A-ZА-ЯЁ]/.test(p),
            digit: /\d/.test(p),
            special: /[!@#$%^&*()\-_=+[\]{};':"\\|,.<>/?`~]/.test(p),
        };
    }, [regForm.password]);

    // Handlers: auth
    const handleRegister = (e: React.FormEvent) => {
        e.preventDefault();
        const errs: Record<string, string> = {};
        const parts = regForm.fullName.trim().split(/\s+/);
        if (parts.length < 2) errs.fullName = tAuth('validation.enterFullName');
        if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(regForm.email)) errs.email = tAuth('validation.enterValidEmail');
        if (!/^\+?\d{7,15}$/.test(regForm.phone)) errs.phone = tAuth('validation.enterValidPhone');
        if (!pwCriteria.length || !pwCriteria.uppercase || !pwCriteria.digit || !pwCriteria.special)
            errs.password = tAuth('validation.passwordNotMeetRequirements');
        if (Object.keys(errs).length) { setAuthErrors(errs); return; }
        setAuthErrors({});
        const [firstName, ...rest] = regForm.fullName.trim().split(/\s+/);
        registerMutation.mutate(
            { firstName, lastName: rest.join(' '), email: regForm.email.trim(), phone: regForm.phone.trim(), password: regForm.password },
            {
                onSuccess: () => goToStep(1),
                onError: (err) => setAuthErrors({ server: err.message }),
            }
        );
    };

    const handleLogin = (e: React.FormEvent) => {
        e.preventDefault();
        setAuthErrors({});
        loginMutation.mutate(loginForm, {
            onSuccess: () => goToStep(1),
            onError: (err) => setAuthErrors({ server: err.message }),
        });
    };

    // Handler: create org (end of step 2)
    const handleCreateOrg = () => {
        const resolvedType = orgType === 'other'
            ? (orgCustomType.trim() || 'Other')
            : t(`steps.orgDetails.types.${orgType}`);
        addOrg.mutate(
            {
                name: orgName.trim(),
                organizationType: resolvedType,
                description: '',
                isOpen24_7: false,
                workDays: transformWorkDays(workDays),
                holidays,
                nightShiftBonus: 0,
                holidayBonus: 0,
                employeeCount: 0,
            },
            {
                onSuccess: (data: any) => {
                    console.log(data);
                    if (data?.id) setCurrentOrg(data.id);
                    setOrgCreated(true);
                    queryClient.invalidateQueries();
                    toast.success(t('orgCreated'));
                    goToStep(3);
                },
                onError: () => toast.error(tCommon('somethingWentWrong')),
            }
        );
    };

    // Handlers: departments
    const handleCreateDept = (data: DepartmentFormValues) => {
        createDept.mutate(data, {
            onSuccess: () => { toast.success(t('deptCreated')); setDeptOpen(false); setSelDept(null); },
            onError: () => toast.error(tCommon('somethingWentWrong')),
        });
    };
    const handleUpdateDept = (_id: number, data: DepartmentFormValues) => {
        updateDept.mutate(data, {
            onSuccess: () => { toast.success(t('deptUpdated')); setDeptOpen(false); setSelDept(null); },
            onError: () => toast.error(tCommon('somethingWentWrong')),
        });
    };
    const handleDeleteDept = (id: number) => {
        deleteDept.mutate(id, {
            onSuccess: () => { toast.success(t('deptDeleted')); setDeptOpen(false); setSelDept(null); },
            onError: () => toast.error(tCommon('somethingWentWrong')),
        });
    };

    // Handlers: shifts
    const handleCreateShift = (data: ShiftTemplateFormValues) => {
        createShift.mutate(data, {
            onSuccess: () => { toast.success(t('shiftCreated')); setShiftOpen(false); setSelShift(null); },
            onError: () => toast.error(tCommon('somethingWentWrong')),
        });
    };
    const handleUpdateShift = (_id: number, data: ShiftTemplateFormValues) => {
        updateShift.mutate(data, {
            onSuccess: () => { toast.success(t('shiftUpdated')); setShiftOpen(false); setSelShift(null); },
            onError: () => toast.error(tCommon('somethingWentWrong')),
        });
    };
    const handleDeleteShift = (id: number) => {
        deleteShift.mutate(id, {
            onSuccess: () => { toast.success(t('shiftDeleted')); setShiftOpen(false); setSelShift(null); },
            onError: () => toast.error(tCommon('somethingWentWrong')),
        });
    };

    // Handler: employees
    const handleCSVImport = (employees: EmployeeData[]) => setEmpRows(employees);

    const handleBulkSubmit = () => {
        const valid = empRows.filter(r => r.firstName.trim() && r.email.trim());
        if (!valid.length) { goToStep(6); return; }
        bulkCreate.mutate(valid, {
            onSuccess: (res: any) => {
                if (res?.data?.successCount > 0)
                    toast.success(t('steps.employees.importedCount', { count: res.data.successCount }));
                goToStep(6);
            },
            onError: () => goToStep(6),
        });
    };

    // Navigation
    const canNext1 = orgName.trim().length > 0 && orgType.length > 0 && (orgType !== 'other' || orgCustomType.trim().length > 0);

    const handleNext = () => {
        if (step === 1 && !canNext1) return;
        if (step === 2) { if (!orgCreated) handleCreateOrg(); else goToStep(3); return; }
        if (step === 5) { handleBulkSubmit(); return; }
        goToStep(Math.min(step + 1, 6));
    };

    const handleSkip = () => goToStep(Math.min(step + 1, 6));
    const handleBack = () => setStep(s => Math.max(s - 1, startStep));

    const isLoading = registerMutation.isPending || loginMutation.isPending || addOrg.isPending || bulkCreate.isPending;

    // ─── Step renders ─────────────────────────────────────────────────────────

    function renderStep0() {
        return (
            <div className="max-w-md">
                <InfoBox>{t('steps.register.why')}</InfoBox>
                <div className="flex gap-1 mb-6 p-1 bg-muted rounded-lg">
                    <button
                        type="button"
                        onClick={() => setAuthMode('register')}
                        className={cn('flex-1 py-2 rounded-md text-sm font-medium transition-colors',
                            authMode === 'register' ? 'bg-background shadow-sm text-foreground' : 'text-muted-foreground hover:text-foreground')}
                    >
                        {tCommon('register')}
                    </button>
                    <button
                        type="button"
                        onClick={() => setAuthMode('login')}
                        className={cn('flex-1 py-2 rounded-md text-sm font-medium transition-colors',
                            authMode === 'login' ? 'bg-background shadow-sm text-foreground' : 'text-muted-foreground hover:text-foreground')}
                    >
                        {tCommon('logIn')}
                    </button>
                </div>

                {authMode === 'register' ? (
                    <form onSubmit={handleRegister} className="space-y-4">
                        <div className="space-y-1">
                            <Label htmlFor="fullName">{tAuth('fullName')}</Label>
                            <Input id="fullName" value={regForm.fullName} onChange={e => setRegForm(p => ({ ...p, fullName: e.target.value }))} placeholder="John Smith" />
                            {authErrors.fullName && <p className="text-xs text-destructive">{authErrors.fullName}</p>}
                        </div>
                        <div className="space-y-1">
                            <Label htmlFor="email">{tCommon('email')}</Label>
                            <Input id="email" type="email" value={regForm.email} onChange={e => setRegForm(p => ({ ...p, email: e.target.value }))} placeholder={tAuth('emailPlaceholder')} />
                            {authErrors.email && <p className="text-xs text-destructive">{authErrors.email}</p>}
                        </div>
                        <div className="space-y-1">
                            <Label htmlFor="phone">{tCommon('phone')}</Label>
                            <Input id="phone" type="tel" value={regForm.phone} onChange={e => setRegForm(p => ({ ...p, phone: e.target.value }))} placeholder="+1234567890" />
                            {authErrors.phone && <p className="text-xs text-destructive">{authErrors.phone}</p>}
                        </div>
                        <div className="space-y-1">
                            <Label htmlFor="password">{tCommon('password')}</Label>
                            <PasswordInput id="password" value={regForm.password} onChange={e => setRegForm(p => ({ ...p, password: (e.target as HTMLInputElement).value }))} />
                            <div className="mt-2 grid grid-cols-2 gap-1">
                                {([
                                    [pwCriteria.length, tAuth('passwordRequirements.length')],
                                    [pwCriteria.uppercase, tAuth('passwordRequirements.uppercase')],
                                    [pwCriteria.digit, tAuth('passwordRequirements.digit')],
                                    [pwCriteria.special, tAuth('passwordRequirements.special')],
                                ] as [boolean, string][]).map(([ok, label]) => (
                                    <div key={label} className="flex items-center gap-1.5 text-xs">
                                        <span className={cn('w-4 h-4 rounded-full flex items-center justify-center shrink-0', ok ? 'bg-emerald-500/20 text-emerald-500' : 'bg-muted text-muted-foreground')}>
                                            {ok ? <Check className="w-2.5 h-2.5" /> : '·'}
                                        </span>
                                        <span className="text-muted-foreground">{label}</span>
                                    </div>
                                ))}
                            </div>
                            {authErrors.password && <p className="text-xs text-destructive mt-1">{authErrors.password}</p>}
                        </div>
                        {authErrors.server && <p className="text-sm text-destructive">{authErrors.server}</p>}
                        <Button type="submit" className="w-full" disabled={registerMutation.isPending}>
                            {registerMutation.isPending ? tAuth('signingUp') : tCommon('signUp')}
                        </Button>
                    </form>
                ) : (
                    <form onSubmit={handleLogin} className="space-y-4">
                        <div className="space-y-1">
                            <Label htmlFor="login-email">{tCommon('email')}</Label>
                            <Input id="login-email" type="email" value={loginForm.email} onChange={e => setLoginForm(p => ({ ...p, email: e.target.value }))} placeholder={tAuth('emailPlaceholder')} />
                        </div>
                        <div className="space-y-1">
                            <Label htmlFor="login-password">{tCommon('password')}</Label>
                            <PasswordInput id="login-password" value={loginForm.password} onChange={e => setLoginForm(p => ({ ...p, password: (e.target as HTMLInputElement).value }))} />
                        </div>
                        {authErrors.server && <p className="text-sm text-destructive">{authErrors.server}</p>}
                        <Button type="submit" className="w-full" disabled={loginMutation.isPending}>
                            {loginMutation.isPending ? tAuth('loggingIn') : tCommon('logIn')}
                        </Button>
                    </form>
                )}
            </div>
        );
    }

    function renderStep1() {
        return (
            <div className="max-w-lg space-y-5">
                <InfoBox>{t('steps.orgDetails.why')}</InfoBox>
                <div className="space-y-1">
                    <Label htmlFor="orgName">{t('steps.orgDetails.name')} <span className="text-destructive">*</span></Label>
                    <Input
                        id="orgName"
                        value={orgName}
                        onChange={e => setOrgName(e.target.value)}
                        placeholder={t('steps.orgDetails.namePlaceholder')}
                        autoFocus
                    />
                </div>
                <div className="space-y-1">
                    <Label>{t('steps.orgDetails.type')} <span className="text-destructive">*</span></Label>
                    <Select value={orgType} onValueChange={setOrgType}>
                        <SelectTrigger>
                            <SelectValue placeholder={t('steps.orgDetails.typePlaceholder')} />
                        </SelectTrigger>
                        <SelectContent>
                            {ORG_TYPE_KEYS.map(key => (
                                <SelectItem key={key} value={key}>{t(`steps.orgDetails.types.${key}`)}</SelectItem>
                            ))}
                        </SelectContent>
                    </Select>
                </div>
                {orgType === 'other' && (
                    <div className="space-y-1">
                        <Label htmlFor="orgCustomType">{t('steps.orgDetails.customType')} <span className="text-destructive">*</span></Label>
                        <Input
                            id="orgCustomType"
                            value={orgCustomType}
                            onChange={e => setOrgCustomType(e.target.value)}
                            placeholder={t('steps.orgDetails.customTypePlaceholder')}
                        />
                    </div>
                )}
            </div>
        );
    }

    function renderStep2() {
        return (
            <div className="max-w-2xl space-y-6">
                <InfoBox>{t('steps.schedule.why')}</InfoBox>
                <WorkScheduleSelector
                    scheduleMode={scheduleMode}
                    setScheduleMode={setScheduleMode}
                    workDays={workDays}
                    setWorkDays={setWorkDays}
                    fullTimeStartTime={ftStart}
                    fullTimeEndTime={ftEnd}
                    setFullTimeStartTime={setFtStart}
                    setFullTimeEndTime={setFtEnd}
                />
                <HolidaySelector selectedHolidays={holidays} setSelectedHolidays={setHolidays} />
                <p className="text-xs text-muted-foreground">{t('steps.schedule.canSkip')}</p>
            </div>
        );
    }

    function renderStep3() {
        return (
            <div className="max-w-2xl space-y-4">
                <InfoBox>{t('steps.departments.why')}</InfoBox>
                <Button
                    type="button"
                    onClick={() => { setSelDept(null); setDeptOpen(true); }}
                    variant="outline"
                >
                    <Plus className="w-4 h-4 mr-2" />
                    {t('steps.departments.addDepartment')}
                </Button>

                {depts.length === 0 ? (
                    <p className="text-sm text-muted-foreground py-4">{t('steps.departments.noDepartments')}</p>
                ) : (
                    <>
                        <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
                            {depts.map(d => (
                                <DepartmentCard
                                    key={d.id}
                                    name={d.name}
                                    description={d.description}
                                    color={d.color}
                                    workingHours={d.startTime && d.endTime ? `${d.startTime} – ${d.endTime}` : undefined}
                                    actions={[{ label: tCommon('edit'), onClick: () => { setSelDept(d); setDeptOpen(true); } }]}
                                />
                            ))}
                        </div>
                        <Badge variant="secondary">{t('steps.departments.deptCount', { count: depts.length })}</Badge>
                    </>
                )}
                <p className="text-xs text-muted-foreground">{t('steps.departments.skipHint')}</p>
            </div>
        );
    }

    function renderStep4() {
        return (
            <div className="max-w-2xl space-y-4">
                <InfoBox>{t('steps.shiftTemplates.why')}</InfoBox>
                <Button
                    type="button"
                    onClick={() => { setSelShift(null); setShiftOpen(true); }}
                    variant="outline"
                >
                    <Plus className="w-4 h-4 mr-2" />
                    {t('steps.shiftTemplates.addShift')}
                </Button>

                {shiftTemplates.length === 0 ? (
                    <p className="text-sm text-muted-foreground py-4">{t('steps.shiftTemplates.noShifts')}</p>
                ) : (
                    <>
                        <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
                            {shiftTemplates.map(s => (
                                <div
                                    key={s.id}
                                    onClick={() => { setSelShift(s); setShiftOpen(true); }}
                                    className="flex items-center gap-3 p-3 rounded-lg border cursor-pointer hover:border-primary/50 transition-colors"
                                >
                                    <div className="w-3 h-3 rounded-full shrink-0" style={{ backgroundColor: s.color }} />
                                    <div className="min-w-0 flex-1">
                                        <p className="font-medium text-sm truncate">{s.name}</p>
                                        <p className="text-xs text-muted-foreground">
                                            {s.startTime?.slice(0, 5)} – {s.endTime?.slice(0, 5)} · {s.minEmployees}–{s.maxEmployees} {tCommon('employees')}
                                        </p>
                                    </div>
                                </div>
                            ))}
                        </div>
                        <Badge variant="secondary">{t('steps.shiftTemplates.shiftCount', { count: shiftTemplates.length })}</Badge>
                    </>
                )}
                <p className="text-xs text-muted-foreground">{t('steps.shiftTemplates.skipHint')}</p>
            </div>
        );
    }

    function renderStep5() {
        return (
            <div className="max-w-3xl space-y-4">
                <InfoBox>{t('steps.employees.why')}</InfoBox>
                <div className="flex gap-2 flex-wrap">
                    <CSVImporter onImport={handleCSVImport} />
                </div>

                <div className="overflow-x-auto border rounded-lg">
                    <table className="w-full border-collapse text-sm">
                        <thead className="bg-muted/50">
                            <tr>
                                {['firstName', 'lastName', 'email', 'phone', 'position'].map(col => (
                                    <th key={col} className="p-3 text-left font-medium text-xs whitespace-nowrap border-b">
                                        {t(`steps.employees.${col}`)}
                                        {['firstName', 'lastName', 'email'].includes(col) && <span className="text-destructive ml-1">*</span>}
                                    </th>
                                ))}
                                <th className="p-3 border-b w-10" />
                            </tr>
                        </thead>
                        <tbody>
                            {empRows.map((row, i) => (
                                <tr key={i} className="border-b hover:bg-muted/20">
                                    {(['firstName', 'lastName', 'email', 'phone', 'position'] as const).map(col => (
                                        <td key={col} className="p-2">
                                            <Input
                                                value={row[col] as string}
                                                onChange={e => setEmpRows(rows => rows.map((r, j) => j === i ? { ...r, [col]: e.target.value } : r))}
                                                type={col === 'email' ? 'email' : 'text'}
                                                className="h-8 text-sm"
                                                placeholder={col === 'email' ? 'john@example.com' : ''}
                                            />
                                        </td>
                                    ))}
                                    <td className="p-2">
                                        {empRows.length > 1 && (
                                            <Button type="button" variant="ghost" size="sm" className="h-8 w-8 p-0"
                                                onClick={() => setEmpRows(rows => rows.filter((_, j) => j !== i))}>
                                                <Trash2 className="w-3.5 h-3.5 text-destructive" />
                                            </Button>
                                        )}
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                </div>

                <Button type="button" variant="outline" size="sm" onClick={() => setEmpRows(rows => [...rows, { ...EMPTY_EMP }])}>
                    <Plus className="w-4 h-4 mr-2" />
                    {t('steps.employees.addRow')}
                </Button>

                <p className="text-xs text-muted-foreground">{t('steps.employees.skipHint')}</p>
            </div>
        );
    }

    function renderStep6() {
        return (
            <div className="max-w-lg mx-auto text-center space-y-6 py-8">
                <div className="w-20 h-20 rounded-full bg-emerald-500/10 flex items-center justify-center mx-auto">
                    <CheckCircle2 className="w-10 h-10 text-emerald-500" />
                </div>
                <div>
                    <h2 className="text-2xl font-bold">{t('steps.done.title')}</h2>
                    <p className="text-muted-foreground mt-2">{t('steps.done.description')}</p>
                </div>
                <InfoBox>{t('steps.done.why')}</InfoBox>
                <div className="flex flex-col sm:flex-row gap-3 justify-center">
                    <Button size="lg" onClick={() => router.push('/schedules/manage')}>
                        <ArrowRight className="w-4 h-4 mr-2" />
                        {t('steps.done.goToSchedules')}
                    </Button>
                    <Button size="lg" variant="secondary" onClick={() => router.push('/dashboard')}>
                        {t('steps.done.goToDashboard')}
                    </Button>
                </div>
                <p className="text-xs text-muted-foreground">{t('steps.done.tip')}</p>
            </div>
        );
    }

    function renderCurrentStep() {
        switch (step) {
            case 0: return renderStep0();
            case 1: return renderStep1();
            case 2: return renderStep2();
            case 3: return renderStep3();
            case 4: return renderStep4();
            case 5: return renderStep5();
            case 6: return renderStep6();
            default: return null;
        }
    }

    const TOTAL = 7;
    const showNav = step > 0 && step < 6;
    const showSkip = [3, 4, 5].includes(step);

    return (
        <div className="min-h-screen bg-background flex flex-col">
            {/* Header */}
            <header className="border-b px-6 py-4 flex items-center justify-between shrink-0">
                <Logo />
                <span className="text-sm text-muted-foreground hidden sm:block">
                    {step + 1} / {TOTAL}
                </span>
                {/* Mobile progress */}
                <div className="sm:hidden flex items-center gap-3">
                    <div className="w-24 h-1.5 rounded-full bg-muted overflow-hidden">
                        <div
                            className="h-full bg-primary rounded-full transition-all duration-300"
                            style={{ width: `${((step + 1) / TOTAL) * 100}%` }}
                        />
                    </div>
                    <span className="text-xs text-muted-foreground">{step + 1}/{TOTAL}</span>
                </div>
            </header>

            <div className="flex flex-1 overflow-hidden">
                {/* Sidebar */}
                <aside className="w-56 border-r p-4 hidden md:flex flex-col shrink-0 overflow-y-auto">
                    <div className="space-y-1 mt-2">
                        {STEP_KEYS.map((key, i) => {
                            const StepIcon = STEP_ICONS[i];
                            const isCompleted = i < step;
                            const isActive = i === step;
                            const isSkipped = i < startStep;
                            const isClickable = i <= maxStep && i !== step && i < 6;
                            if (isSkipped) return null;
                            return (
                                <div
                                    key={key}
                                    onClick={isClickable ? () => setStep(i) : undefined}
                                    className={cn(
                                        'flex items-center gap-3 px-3 py-2.5 rounded-lg transition-colors text-sm',
                                        isActive && 'bg-primary/10 text-primary',
                                        isCompleted && 'text-muted-foreground hover:bg-muted/50',
                                        isClickable && 'cursor-pointer',
                                        !isActive && !isCompleted && 'text-muted-foreground/60'
                                    )}
                                >
                                    <div className={cn(
                                        'w-6 h-6 rounded-full flex items-center justify-center shrink-0 text-xs font-semibold',
                                        isCompleted && 'bg-emerald-500/20 text-emerald-600',
                                        isActive && 'bg-primary text-primary-foreground',
                                        !isCompleted && !isActive && 'bg-muted text-muted-foreground'
                                    )}>
                                        {isCompleted ? <Check className="w-3 h-3" /> : <StepIcon className="w-3 h-3" />}
                                    </div>
                                    <span className={cn('font-medium truncate', isActive && 'text-primary font-semibold')}>
                                        {t(`steps.${key}.title`)}
                                    </span>
                                </div>
                            );
                        })}
                    </div>
                </aside>

                {/* Main content */}
                <main className="flex-1 overflow-y-auto">
                    <div className="max-w-3xl mx-auto px-6 py-8">
                        {/* Step header */}
                        <div className="mb-8">
                            <div className="flex items-center gap-3 mb-2">
                                {React.createElement(STEP_ICONS[step], { className: 'w-6 h-6 text-primary' })}
                                <h1 className="text-2xl font-bold">{t(`steps.${STEP_KEYS[step]}.title`)}</h1>
                            </div>
                            <p className="text-muted-foreground">{t(`steps.${STEP_KEYS[step]}.description`)}</p>
                        </div>

                        {/* Step content */}
                        {renderCurrentStep()}

                        {/* Navigation */}
                        {showNav && (
                            <div className="flex items-center justify-between mt-10 pt-6 border-t">
                                <Button variant="ghost" onClick={handleBack} disabled={step <= startStep}>
                                    {tCommon('back')}
                                </Button>
                                <div className="flex gap-2">
                                    {showSkip && (
                                        <Button variant="outline" onClick={handleSkip} disabled={isLoading}>
                                            {t('skip')}
                                        </Button>
                                    )}
                                    <Button
                                        onClick={handleNext}
                                        disabled={isLoading || (step === 1 && !canNext1)}
                                    >
                                        {isLoading
                                            ? tCommon('loading')
                                            : step === 2
                                                ? t('createOrg')
                                                : step === 5
                                                    ? tCommon('next')
                                                    : tCommon('next')}
                                        <ArrowRight className="w-4 h-4 ml-2" />
                                    </Button>
                                </div>
                            </div>
                        )}
                    </div>
                </main>
            </div>

            {/* Drawers (rendered outside main content so they overlay correctly) */}
            <DepartmentAsideForm
                open={deptOpen}
                onOpenChange={open => { if (!open) { setDeptOpen(false); setSelDept(null); } else setDeptOpen(true); }}
                selectedDepartment={selDept}
                onCreate={handleCreateDept}
                onUpdate={handleUpdateDept}
                onDelete={handleDeleteDept}
                isCreating={createDept.isPending}
                isUpdating={updateDept.isPending}
                isDeleting={deleteDept.isPending}
            />
            <ShiftTemplateAsideForm
                open={shiftOpen}
                onOpenChange={open => { if (!open) { setShiftOpen(false); setSelShift(null); } else setShiftOpen(true); }}
                selectedShift={selShift}
                departments={depts}
                onCreate={handleCreateShift}
                onUpdate={handleUpdateShift}
                onDelete={handleDeleteShift}
                isCreating={createShift.isPending}
                isUpdating={updateShift.isPending}
                isDeleting={deleteShift.isPending}
            />
        </div>
    );
}

export default function OnboardingPage() {
    return (
        <Suspense>
            <OnboardingContent />
        </Suspense>
    );
}