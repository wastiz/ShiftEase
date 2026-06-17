'use client'

import { useState, useEffect } from 'react'
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from '@/components/ui/shadcn/dialog'
import { Button } from '@/components/ui/shadcn/button'
import { Input } from '@/components/ui/shadcn/input'
import { Label } from '@/components/ui/shadcn/label'
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/shadcn/tabs'
import { Checkbox } from '@/components/ui/shadcn/checkbox'
import { useTranslations } from 'next-intl'

type TotalHoursFieldProps = {
    value: number | null
    hardValue: boolean
    onChangeHours: (v: number | null) => void
    onChangeHard: (v: boolean) => void
    t: ReturnType<typeof useTranslations<'schedule'>>
}

function TotalHoursField({ value, hardValue, onChangeHours, onChangeHard, t }: TotalHoursFieldProps) {
    return (
        <div className="space-y-4">
            <div className="space-y-2">
                <Label>{t('totalHours')}</Label>
                <p className="text-sm text-muted-foreground">{t('totalHoursHint')}</p>
                <Input
                    type="number"
                    min="1"
                    placeholder={t('totalHoursPlaceholder')}
                    value={value ?? ''}
                    onChange={(e) => {
                        const raw = e.target.value
                        onChangeHours(raw === '' ? null : parseFloat(raw) || null)
                    }}
                />
            </div>
            {value !== null && (
                <div className="flex items-center space-x-2">
                    <Checkbox
                        id="hard-total-hours"
                        checked={hardValue}
                        onCheckedChange={(checked) => onChangeHard(checked === true)}
                    />
                    <div>
                        <label htmlFor="hard-total-hours" className="text-sm font-medium leading-none cursor-pointer">
                            {t('hardTotalHours')}
                        </label>
                        <p className="text-xs text-muted-foreground mt-1">{t('hardTotalHoursHint')}</p>
                    </div>
                </div>
            )}
        </div>
    )
}

export type StandardPreset = {
    mode: 'standard'
    TotalHours: number | null
    HardTotalHours: boolean
}

export type AcoPreset = {
    mode: 'aco'
    TotalHours: number | null
    HardTotalHours: boolean
    NumAnts: number
    NumIterations: number
}

export type GaPreset = {
    mode: 'ga'
    TotalHours: number | null
    HardTotalHours: boolean
    PopulationSize: number
    NumGenerations: number
}

export type AcoGaPreset = {
    mode: 'aco-ga'
    TotalHours: number | null
    HardTotalHours: boolean
    NumAnts: number
    NumAcoIterations: number
    PopulationSize: number
    NumGaGenerations: number
}

export type SchedulePreset = StandardPreset | AcoPreset | GaPreset | AcoGaPreset

type SchedulePresetDialogProps = {
    open: boolean
    onOpenChange: (open: boolean) => void
    onGenerate: (preset: SchedulePreset) => void
    isGenerating?: boolean
}

const STORAGE_KEY_STANDARD = 'schedule_preset_standard'
const STORAGE_KEY_ACO = 'schedule_preset_aco'
const STORAGE_KEY_GA = 'schedule_preset_ga'
const STORAGE_KEY_ACO_GA = 'schedule_preset_aco_ga'

const defaultStandardPreset: Omit<StandardPreset, 'mode'> = {
    TotalHours: null,
    HardTotalHours: true,
}

const defaultAcoPreset: Omit<AcoPreset, 'mode'> = {
    TotalHours: null,
    HardTotalHours: true,
    NumAnts: 20,
    NumIterations: 50,
}

const defaultGaPreset: Omit<GaPreset, 'mode'> = {
    TotalHours: null,
    HardTotalHours: true,
    PopulationSize: 50,
    NumGenerations: 100,
}

const defaultAcoGaPreset: Omit<AcoGaPreset, 'mode'> = {
    TotalHours: null,
    HardTotalHours: true,
    NumAnts: 20,
    NumAcoIterations: 30,
    PopulationSize: 50,
    NumGaGenerations: 80,
}

export default function SchedulePresetDialog({
    open,
    onOpenChange,
    onGenerate,
    isGenerating = false,
}: SchedulePresetDialogProps) {
    const t = useTranslations('schedule')

    const [activeTab, setActiveTab] = useState<'standard' | 'aco' | 'ga' | 'aco-ga'>('standard')
    const [standard, setStandard] = useState<Omit<StandardPreset, 'mode'>>(defaultStandardPreset)
    const [aco, setAco] = useState<Omit<AcoPreset, 'mode'>>(defaultAcoPreset)
    const [ga, setGa] = useState<Omit<GaPreset, 'mode'>>(defaultGaPreset)
    const [acoGa, setAcoGa] = useState<Omit<AcoGaPreset, 'mode'>>(defaultAcoGaPreset)

    useEffect(() => {
        if (!open) return

        try {
            const stored = localStorage.getItem(STORAGE_KEY_STANDARD)
            setStandard(stored ? JSON.parse(stored) : defaultStandardPreset)
        } catch {
            setStandard(defaultStandardPreset)
        }

        try {
            const stored = localStorage.getItem(STORAGE_KEY_ACO)
            setAco(stored ? JSON.parse(stored) : defaultAcoPreset)
        } catch {
            setAco(defaultAcoPreset)
        }

        try {
            const stored = localStorage.getItem(STORAGE_KEY_GA)
            setGa(stored ? JSON.parse(stored) : defaultGaPreset)
        } catch {
            setGa(defaultGaPreset)
        }

        try {
            const stored = localStorage.getItem(STORAGE_KEY_ACO_GA)
            setAcoGa(stored ? JSON.parse(stored) : defaultAcoGaPreset)
        } catch {
            setAcoGa(defaultAcoGaPreset)
        }
    }, [open])

    const handleSave = () => {
        if (activeTab === 'standard') {
            localStorage.setItem(STORAGE_KEY_STANDARD, JSON.stringify(standard))
            onGenerate({ mode: 'standard', ...standard })
        } else if (activeTab === 'aco') {
            localStorage.setItem(STORAGE_KEY_ACO, JSON.stringify(aco))
            onGenerate({ mode: 'aco', ...aco })
        } else if (activeTab === 'ga') {
            localStorage.setItem(STORAGE_KEY_GA, JSON.stringify(ga))
            onGenerate({ mode: 'ga', ...ga })
        } else {
            localStorage.setItem(STORAGE_KEY_ACO_GA, JSON.stringify(acoGa))
            onGenerate({ mode: 'aco-ga', ...acoGa })
        }
        onOpenChange(false)
    }

    return (
        <Dialog open={open} onOpenChange={onOpenChange}>
            <DialogContent className="max-w-2xl max-h-[90vh] overflow-y-auto">
                <DialogHeader>
                    <DialogTitle>{t('generateSchedule')}</DialogTitle>
                    <DialogDescription>{t('schedulePresetDescription')}</DialogDescription>
                </DialogHeader>

                <Tabs value={activeTab} onValueChange={(v) => setActiveTab(v as 'standard' | 'aco' | 'ga' | 'aco-ga')}>
                    <TabsList className="w-full">
                        <TabsTrigger value="standard" className="flex-1">{t('tabGreedy')}</TabsTrigger>
                        <TabsTrigger value="aco" className="flex-1">{t('tabAco')}</TabsTrigger>
                        <TabsTrigger value="ga" className="flex-1">{t('tabGa')}</TabsTrigger>
                        <TabsTrigger value="aco-ga" className="flex-1">{t('tabAcoGa')}</TabsTrigger>
                    </TabsList>

                    {/* Greedy tab */}
                    <TabsContent value="standard">
                        <div className="space-y-6 py-4">
                            <TotalHoursField
                                value={standard.TotalHours}
                                hardValue={standard.HardTotalHours}
                                onChangeHours={(v) => setStandard(prev => ({ ...prev, TotalHours: v }))}
                                onChangeHard={(v) => setStandard(prev => ({ ...prev, HardTotalHours: v }))}
                                t={t}
                            />
                        </div>
                    </TabsContent>

                    {/* ACO tab */}
                    <TabsContent value="aco">
                        <div className="space-y-6 py-4">
                            <div className="space-y-2">
                                <Label>{t('numAnts')}</Label>
                                <p className="text-sm text-muted-foreground">{t('numAntsHint')}</p>
                                <Input
                                    type="number"
                                    min="1"
                                    value={aco.NumAnts}
                                    onChange={(e) =>
                                        setAco(prev => ({ ...prev, NumAnts: parseInt(e.target.value) || 1 }))
                                    }
                                />
                            </div>

                            <div className="space-y-2">
                                <Label>{t('numIterations')}</Label>
                                <p className="text-sm text-muted-foreground">{t('numIterationsHint')}</p>
                                <Input
                                    type="number"
                                    min="1"
                                    value={aco.NumIterations}
                                    onChange={(e) =>
                                        setAco(prev => ({ ...prev, NumIterations: parseInt(e.target.value) || 1 }))
                                    }
                                />
                            </div>

                            <TotalHoursField
                                value={aco.TotalHours}
                                hardValue={aco.HardTotalHours}
                                onChangeHours={(v) => setAco(prev => ({ ...prev, TotalHours: v }))}
                                onChangeHard={(v) => setAco(prev => ({ ...prev, HardTotalHours: v }))}
                                t={t}
                            />
                        </div>
                    </TabsContent>

                    {/* GA tab */}
                    <TabsContent value="ga">
                        <div className="space-y-6 py-4">
                            <div className="space-y-2">
                                <Label>{t('populationSize')}</Label>
                                <p className="text-sm text-muted-foreground">{t('populationSizeHint')}</p>
                                <Input
                                    type="number"
                                    min="1"
                                    value={ga.PopulationSize}
                                    onChange={(e) =>
                                        setGa(prev => ({ ...prev, PopulationSize: parseInt(e.target.value) || 1 }))
                                    }
                                />
                            </div>

                            <div className="space-y-2">
                                <Label>{t('numGenerations')}</Label>
                                <p className="text-sm text-muted-foreground">{t('numGenerationsHint')}</p>
                                <Input
                                    type="number"
                                    min="1"
                                    value={ga.NumGenerations}
                                    onChange={(e) =>
                                        setGa(prev => ({ ...prev, NumGenerations: parseInt(e.target.value) || 1 }))
                                    }
                                />
                            </div>

                            <TotalHoursField
                                value={ga.TotalHours}
                                hardValue={ga.HardTotalHours}
                                onChangeHours={(v) => setGa(prev => ({ ...prev, TotalHours: v }))}
                                t={t}
                                onChangeHard={(v) => setGa(prev => ({ ...prev, HardTotalHours: v }))}
                            />
                        </div>
                    </TabsContent>

                    {/* ACO+GA tab */}
                    <TabsContent value="aco-ga">
                        <div className="space-y-6 py-4">
                            <div className="space-y-2">
                                <Label>{t('numAnts')}</Label>
                                <p className="text-sm text-muted-foreground">{t('numAntsHint')}</p>
                                <Input
                                    type="number"
                                    min="1"
                                    value={acoGa.NumAnts}
                                    onChange={(e) =>
                                        setAcoGa(prev => ({ ...prev, NumAnts: parseInt(e.target.value) || 1 }))
                                    }
                                />
                            </div>

                            <div className="space-y-2">
                                <Label>{t('numAcoIterations')}</Label>
                                <p className="text-sm text-muted-foreground">{t('numAcoIterationsHint')}</p>
                                <Input
                                    type="number"
                                    min="1"
                                    value={acoGa.NumAcoIterations}
                                    onChange={(e) =>
                                        setAcoGa(prev => ({ ...prev, NumAcoIterations: parseInt(e.target.value) || 1 }))
                                    }
                                />
                            </div>

                            <div className="space-y-2">
                                <Label>{t('populationSize')}</Label>
                                <p className="text-sm text-muted-foreground">{t('populationSizeHint')}</p>
                                <Input
                                    type="number"
                                    min="1"
                                    value={acoGa.PopulationSize}
                                    onChange={(e) =>
                                        setAcoGa(prev => ({ ...prev, PopulationSize: parseInt(e.target.value) || 1 }))
                                    }
                                />
                            </div>

                            <div className="space-y-2">
                                <Label>{t('numGaGenerations')}</Label>
                                <p className="text-sm text-muted-foreground">{t('numGaGenerationsHint')}</p>
                                <Input
                                    type="number"
                                    min="1"
                                    value={acoGa.NumGaGenerations}
                                    onChange={(e) =>
                                        setAcoGa(prev => ({ ...prev, NumGaGenerations: parseInt(e.target.value) || 1 }))
                                    }
                                />
                            </div>

                            <TotalHoursField
                                value={acoGa.TotalHours}
                                hardValue={acoGa.HardTotalHours}
                                onChangeHours={(v) => setAcoGa(prev => ({ ...prev, TotalHours: v }))}
                                onChangeHard={(v) => setAcoGa(prev => ({ ...prev, HardTotalHours: v }))}
                                t={t}
                            />
                        </div>
                    </TabsContent>
                </Tabs>

                <DialogFooter>
                    <Button variant="outline" onClick={() => onOpenChange(false)}>
                        {t('cancel')}
                    </Button>
                    <Button onClick={handleSave} disabled={isGenerating}>
                        {isGenerating ? t('generating') : t('generateSchedule')}
                    </Button>
                </DialogFooter>
            </DialogContent>
        </Dialog>
    )
}
