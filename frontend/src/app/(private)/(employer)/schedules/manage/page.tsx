'use client'

import { useEffect, useState, useMemo, useRef, useCallback } from 'react'
import { useSearchParams } from 'next/navigation'
import { Button } from '@/components/ui/shadcn/button'
import {
    useAutoSaveSchedule,
    useExportSchedule,
    useGenerateAcoGaSchedule,
    useGenerateAcoSchedule,
    useGenerateGaSchedule,
    useGenerateSchedule,
    useSaveSchedule,
    useScheduleManagementData,
    useUnconfirmSchedule
} from "@/hooks/api"
import Header from "@/components/ui/headers/Header"
import {
    Shift,
    EmployeeTimeOff,
    GenerateStatus,
    GenerateErrorCode,
    GenerateWarningCode
} from "@/types/schedule"
import ScheduleCalendar from "@/components/features/schedules/ScheduleCalendar/ScheduleCalendar"
import { getDaysInMonth } from "@/helpers/dateHelper"
import Loader from "@/components/ui/Loader"
import { Holiday, WorkDay } from "@/types"
import { ToggleGroup, ToggleGroupItem } from "@/components/ui/shadcn/toggle-group"
import { Calendar, Download, LayoutGrid, List, Loader2 } from "lucide-react"
import SimpleView from "@/components/features/schedules/ScheduleSimple/ScheduleSimple"
import ScheduleMatrix from "@/components/features/schedules/ScheduleMatrix/ScheduleMatrix"
import { toast } from "sonner"
import { useTranslations } from 'next-intl'
import SchedulePresetDialog, { SchedulePreset } from "@/components/features/schedules/SchedulePresetDialog";
import GenerateResultDialog from "@/components/ui/GenerateResultDialog";

const today = new Date()
const AUTOSAVE_DELAY = 60_000 // 60 seconds

function parseParamInt(value: string | null, fallback: number): number {
    if (!value) return fallback;
    const parsed = parseInt(value);
    return isNaN(parsed) ? fallback : parsed;
}

function lsKey(year: number, month: number) {
    return `schedule_draft_${year}_${month + 1}`
}

export type MessageType = "warning" | "error";
export type WarningMessage = {
    message: string;
    messageType: MessageType
}

type SaveStatus = 'synced' | 'unsaved' | 'saving' | 'error'

export default function ManageSchedule() {
    const t = useTranslations('schedule');
    const searchParams = useSearchParams();

    const initialMonth = useMemo(() => {
        const m = parseParamInt(searchParams.get('month'), today.getMonth() + 1);
        return m - 1;
    }, []);
    const initialYear = useMemo(() => parseParamInt(searchParams.get('year'), today.getFullYear()), []);

    const [viewMode, setViewMode] = useState<'calendar' | 'simple' | 'matrix'>('simple');
    const [currentMonth, setCurrentMonth] = useState(initialMonth)
    const [currentYear, setCurrentYear] = useState(initialYear)
    const [daysOfMonth, setDaysOfMonth] = useState(getDaysInMonth(initialYear, initialMonth))

    const [isConfirmed, setIsConfirmed] = useState(false)
    const [scheduleId, setScheduleId] = useState<number | null>(null)
    const [shiftsData, setShiftsData] = useState<Shift[]>([])
    const [orgHolidays, setOrgHolidays] = useState<Holiday[]>([])
    const [orgSchedule, setOrgSchedule] = useState<WorkDay[]>([])
    const [employeeTimeOffs, setEmployeeTimeOffs] = useState<EmployeeTimeOff[]>([])

    const [generateDialogOpen, setGenerateDialogOpen] = useState(false)
    const [warningMessage, setWarningMessage] = useState<WarningMessage | null>(null)
    const [resultDialogOpen, setResultDialogOpen] = useState(false)
    const [generateResult, setGenerateResult] = useState<{
        status: GenerateStatus;
        error?: GenerateErrorCode;
        warnings?: GenerateWarningCode[];
    } | null>(null)

    // Autosave state
    const [saveStatus, setSaveStatus] = useState<SaveStatus>('synced')
    const serverShiftsRef = useRef<string>('[]')
    const autoSaveTimerRef = useRef<ReturnType<typeof setTimeout> | null>(null)
    const autoSave = useAutoSaveSchedule()

    useEffect(() => {
        setDaysOfMonth(getDaysInMonth(currentYear, currentMonth))
    }, [currentMonth, currentYear])

    const { data, isLoading, error } = useScheduleManagementData({
        month: currentMonth + 1,
        year: currentYear,
    })

    useEffect(() => {
        if (!data) return

        const serverShifts = data.schedule?.shifts ?? []
        serverShiftsRef.current = JSON.stringify(serverShifts)

        // Check for a local draft and restore automatically
        let shiftsToSet = serverShifts
        try {
            const stored = localStorage.getItem(lsKey(currentYear, currentMonth))
            if (stored) {
                const draft = JSON.parse(stored) as { shifts: Shift[] }
                if (draft.shifts) shiftsToSet = draft.shifts
            }
        } catch {}

        setShiftsData(shiftsToSet)
        setIsConfirmed(data.schedule?.isConfirmed ?? false)
        setScheduleId(data.schedule?.id ?? null)
        setOrgHolidays(data.organizationHolidays ?? [])
        setOrgSchedule(data.organizationSchedule ?? [])
        setEmployeeTimeOffs(data.employeeTimeOffs ?? [])
    }, [data])

    // Autosave effect: runs on every shiftsData change
    useEffect(() => {
        const key = lsKey(currentYear, currentMonth)

        // If data matches server — nothing to save
        if (JSON.stringify(shiftsData) === serverShiftsRef.current) {
            if (autoSaveTimerRef.current) {
                clearTimeout(autoSaveTimerRef.current)
                autoSaveTimerRef.current = null
            }
            return
        }

        // Immediately persist to localStorage
        localStorage.setItem(key, JSON.stringify({ shifts: shiftsData }))
        setSaveStatus('unsaved')

        // Debounce the API call
        if (autoSaveTimerRef.current) clearTimeout(autoSaveTimerRef.current)
        autoSaveTimerRef.current = setTimeout(() => {
            const startDate = daysOfMonth[0].isoDate
            const endDate = daysOfMonth[daysOfMonth.length - 1].isoDate
            const currentShifts = shiftsData

            setSaveStatus('saving')
            autoSave.mutate(
                { startDate, endDate, shifts: currentShifts, isConfirmed },
                {
                    onSuccess: () => {
                        serverShiftsRef.current = JSON.stringify(currentShifts)
                        localStorage.removeItem(key)
                        setSaveStatus('synced')
                    },
                    onError: () => setSaveStatus('error'),
                }
            )
        }, AUTOSAVE_DELAY)

        return () => {
            if (autoSaveTimerRef.current) {
                clearTimeout(autoSaveTimerRef.current)
                autoSaveTimerRef.current = null
            }
        }
    }, [shiftsData, currentMonth, currentYear])

    const clearLocalDraft = useCallback(() => {
        localStorage.removeItem(lsKey(currentYear, currentMonth))
        serverShiftsRef.current = JSON.stringify(shiftsData)
        setSaveStatus('synced')
        if (autoSaveTimerRef.current) {
            clearTimeout(autoSaveTimerRef.current)
            autoSaveTimerRef.current = null
        }
    }, [currentYear, currentMonth, shiftsData])

    const saveMutation = useSaveSchedule({
        startDate: daysOfMonth[0].isoDate,
        endDate: daysOfMonth[daysOfMonth.length - 1]?.isoDate,
        shiftsData,
    })

    const unconfirmMutation = useUnconfirmSchedule()
    const exportSchedule = useExportSchedule()
    const generateSchedule = useGenerateSchedule()
    const generateAcoSchedule = useGenerateAcoSchedule()
    const generateGaSchedule = useGenerateGaSchedule()
    const generateAcoGaSchedule = useGenerateAcoGaSchedule()

    const onGenerateResult = (result: { status: GenerateStatus; error?: GenerateErrorCode; warnings?: GenerateWarningCode[]; shifts?: Shift[] } | undefined) => {
        if (!result) return;

        setGenerateResult({
            status: result.status,
            error: result.error,
            warnings: result.warnings,
        });

        if (result.status === GenerateStatus.Error) {
            setResultDialogOpen(true);
            setWarningMessage({ message: t(`errors.${result.error}`), messageType: "error" });
        } else if (result.status === GenerateStatus.Warning) {
            setShiftsData(result.shifts ?? []);
            setIsConfirmed(false);
            setResultDialogOpen(true);
            const warningMessages = (result.warnings ?? []).map(w => t(`warnings.${w}`)).join("; ");
            setWarningMessage({ message: warningMessages, messageType: "warning" });
        } else {
            setShiftsData(result.shifts ?? []);
            setIsConfirmed(false);
            setWarningMessage(null);
            toast.success(t('generateSuccess'));
        }
    }

    const handleGenerate = (preset: SchedulePreset) => {
        if (preset.mode === 'aco') {
            generateAcoSchedule.mutate(
                {
                    startDate: daysOfMonth[0].isoDate,
                    endDate: daysOfMonth[daysOfMonth.length - 1]?.isoDate,
                    NumAnts: preset.NumAnts,
                    NumIterations: preset.NumIterations,
                    TotalHours: preset.TotalHours,
                    HardTotalHours: preset.HardTotalHours,
                },
                {
                    onSuccess: onGenerateResult,
                    onError: () => toast.error(t('generateError')),
                }
            );
            return;
        }

        if (preset.mode === 'ga') {
            generateGaSchedule.mutate(
                {
                    startDate: daysOfMonth[0].isoDate,
                    endDate: daysOfMonth[daysOfMonth.length - 1]?.isoDate,
                    PopulationSize: preset.PopulationSize,
                    NumGenerations: preset.NumGenerations,
                    TotalHours: preset.TotalHours,
                    HardTotalHours: preset.HardTotalHours,
                },
                {
                    onSuccess: onGenerateResult,
                    onError: () => toast.error(t('generateError')),
                }
            );
            return;
        }

        if (preset.mode === 'aco-ga') {
            generateAcoGaSchedule.mutate(
                {
                    startDate: daysOfMonth[0].isoDate,
                    endDate: daysOfMonth[daysOfMonth.length - 1]?.isoDate,
                    NumAnts: preset.NumAnts,
                    NumAcoIterations: preset.NumAcoIterations,
                    PopulationSize: preset.PopulationSize,
                    NumGaGenerations: preset.NumGaGenerations,
                    TotalHours: preset.TotalHours,
                    HardTotalHours: preset.HardTotalHours,
                },
                {
                    onSuccess: onGenerateResult,
                    onError: () => toast.error(t('generateError')),
                }
            );
            return;
        }

        generateSchedule.mutate(
            {
                startDate: daysOfMonth[0].isoDate,
                endDate: daysOfMonth[daysOfMonth.length - 1]?.isoDate,
                TotalHours: preset.TotalHours,
                HardTotalHours: preset.HardTotalHours,
            },
            {
                onSuccess: onGenerateResult,
                onError: () => toast.error(t('generateError')),
            }
        )
    }

    const handleConfirmToggle = () => {
        if (isConfirmed && scheduleId) {
            unconfirmMutation.mutate(scheduleId, { onSuccess: clearLocalDraft })
        } else {
            saveMutation.mutate(true, { onSuccess: clearLocalDraft })
        }
    }

    const handleExport = () => {
        if (!scheduleId) return
        exportSchedule.mutate(scheduleId, {
            onSuccess: () => toast.success(t('exportSuccess')),
            onError: () => toast.error(t('exportFailed')),
        })
    }

    if (isLoading) return <Loader />
    if (error) return <p className="p-4 text-red-500">{t('failedToLoad')}</p>

    return (
        <div className="flex flex-col h-screen overflow-hidden">
            <Header title={t('manageSchedule')}>
                <ToggleGroup
                    type="single"
                    value={viewMode}
                    onValueChange={(value) => value && setViewMode(value as "calendar" | "simple" | "matrix")}
                    className="ml-auto"
                >
                    <ToggleGroupItem value="simple" aria-label={t('simpleView')}>
                        <List className="h-4 w-4" />
                    </ToggleGroupItem>
                    <ToggleGroupItem value="calendar" aria-label={t('calendarView')}>
                        <Calendar className="h-4 w-4" />
                    </ToggleGroupItem>
                    <ToggleGroupItem value="matrix" aria-label="Matrix view">
                        <LayoutGrid className="h-4 w-4" />
                    </ToggleGroupItem>
                </ToggleGroup>

                <div className="flex gap-2">
                    <Button onClick={() => setGenerateDialogOpen(true)} disabled={generateSchedule.isPending || generateAcoSchedule.isPending || generateGaSchedule.isPending || generateAcoGaSchedule.isPending}>
                        {(generateSchedule.isPending || generateAcoSchedule.isPending || generateGaSchedule.isPending || generateAcoGaSchedule.isPending) ? t('generating') : t('generateSchedule')}
                    </Button>
                </div>

                <div className="relative">
                    <Button
                        onClick={() => saveMutation.mutate(false, { onSuccess: clearLocalDraft })}
                        disabled={saveMutation.isPending}
                    >
                        {t('save')}
                    </Button>
                    {saveStatus === 'unsaved' && (
                        <span className="absolute -top-1 -right-1 w-2.5 h-2.5 bg-orange-500 rounded-full" />
                    )}
                    {saveStatus === 'saving' && (
                        <span className="absolute -top-1 -right-1 w-2.5 h-2.5 bg-blue-500 rounded-full animate-pulse" />
                    )}
                    {saveStatus === 'error' && (
                        <span className="absolute -top-1 -right-1 w-2.5 h-2.5 bg-red-500 rounded-full" />
                    )}
                </div>

                <Button
                    onClick={handleConfirmToggle}
                    disabled={saveMutation.isPending || unconfirmMutation.isPending}
                    variant={isConfirmed ? "outline" : "default"}
                >
                    {isConfirmed ? t('unconfirm') : t('confirm')}
                </Button>

                <Button onClick={handleExport} disabled={exportSchedule.isPending || !scheduleId}>
                    {exportSchedule.isPending ? (
                        <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                    ) : (
                        <Download className="mr-2 h-4 w-4" />
                    )}
                    {t('exportToExcel')}
                </Button>
            </Header>

            <div className="flex-1 overflow-hidden">
                {viewMode === 'calendar' ? (
                    <ScheduleCalendar
                        shiftsData={shiftsData}
                        setShiftsData={setShiftsData}
                        shiftTypes={data?.shiftTypes ?? []}
                        employees={data?.employees ?? []}
                        departments={data?.departments ?? []}
                        daysOfMonth={daysOfMonth}
                        currentMonth={currentMonth}
                        currentYear={currentYear}
                        setCurrentMonth={setCurrentMonth}
                        setCurrentYear={setCurrentYear}
                        isConfirmed={isConfirmed}
                        isEditable={true}
                        orgHolidays={orgHolidays}
                        orgSchedule={orgSchedule}
                        employeeTimeOffs={employeeTimeOffs}
                        warningMessage={null}
                    />
                ) : viewMode === 'matrix' ? (
                    <ScheduleMatrix
                        employees={data?.employees ?? []}
                        shiftTypes={data?.shiftTypes ?? []}
                        departments={data?.departments ?? []}
                        shiftsData={shiftsData}
                        setShiftsData={setShiftsData}
                        daysOfMonth={daysOfMonth}
                        currentMonth={currentMonth}
                        currentYear={currentYear}
                        setCurrentMonth={setCurrentMonth}
                        setCurrentYear={setCurrentYear}
                        isConfirmed={isConfirmed}
                        orgHolidays={orgHolidays}
                        orgSchedule={orgSchedule}
                        employeeTimeOffs={employeeTimeOffs}
                    />
                ) : (
                    <SimpleView
                        employees={data?.employees ?? []}
                        shiftTypes={data?.shiftTypes ?? []}
                        departments={data?.departments ?? []}
                        shiftsData={shiftsData}
                        setShiftsData={setShiftsData}
                        daysOfMonth={daysOfMonth}
                        currentMonth={currentMonth}
                        currentYear={currentYear}
                        setCurrentMonth={setCurrentMonth}
                        setCurrentYear={setCurrentYear}
                        isConfirmed={isConfirmed}
                        orgHolidays={orgHolidays}
                        orgSchedule={orgSchedule}
                        employeeTimeOffs={employeeTimeOffs}
                        warningMessage={null}
                    />
                )}
            </div>

            <SchedulePresetDialog
                open={generateDialogOpen}
                onOpenChange={setGenerateDialogOpen}
                onGenerate={handleGenerate}
                isGenerating={generateSchedule.isPending || generateAcoSchedule.isPending || generateGaSchedule.isPending || generateAcoGaSchedule.isPending}
            />

            {generateResult && (
                <GenerateResultDialog
                    open={resultDialogOpen}
                    onOpenChange={setResultDialogOpen}
                    status={generateResult.status}
                    error={generateResult.error}
                    warnings={generateResult.warnings}
                />
            )}
        </div>
    )
}
