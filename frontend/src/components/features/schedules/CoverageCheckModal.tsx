'use client'

import { useMemo } from 'react'
import {
    Dialog,
    DialogContent,
    DialogHeader,
    DialogTitle,
} from '@/components/ui/shadcn/dialog'
import { Badge } from '@/components/ui/shadcn/badge'
import { CheckCircle2, AlertCircle, AlertTriangle } from 'lucide-react'
import { DateData, Department, Holiday, Shift, ShiftTemplate, WorkDay } from '@/types'
import { roundToMinutes } from '@/helpers/dateHelper'
import { computeCoverage } from '@/helpers/coverageHelper'

type CoverageCheckModalProps = {
    open: boolean
    onOpenChange: (open: boolean) => void
    shiftTypes: ShiftTemplate[]
    shiftsData: Shift[]
    daysOfMonth: DateData[]
    orgHolidays: Holiday[]
    orgSchedule: WorkDay[]
    departments: Department[]
}

export function CoverageCheckModal({
    open,
    onOpenChange,
    shiftTypes,
    shiftsData,
    daysOfMonth,
    orgHolidays,
    orgSchedule,
    departments,
}: CoverageCheckModalProps) {
    const coverage = useMemo(
        () => computeCoverage(shiftTypes, shiftsData, daysOfMonth, orgHolidays, orgSchedule, departments),
        [shiftTypes, shiftsData, daysOfMonth, orgHolidays, orgSchedule, departments]
    )

    const totalWorkingDays = coverage[0]?.totalWorkingDays ?? 0
    const allOk = coverage.every(c => c.issues.filter(i => i.severity === 'understaffed').length === 0)
    const totalUnderstaffedDays = new Set(
        coverage.flatMap(c =>
            c.issues.filter(i => i.severity === 'understaffed').map(i => i.isoDate)
        )
    ).size
    const fullyCoveredDays = totalWorkingDays - totalUnderstaffedDays

    return (
        <Dialog open={open} onOpenChange={onOpenChange}>
            <DialogContent className="max-w-2xl max-h-[85vh] flex flex-col">
                <DialogHeader>
                    <DialogTitle>Coverage Check</DialogTitle>
                </DialogHeader>

                {/* Summary */}
                <div className={`flex items-center gap-3 rounded-lg p-3 ${allOk ? 'bg-green-50 dark:bg-green-950/30' : 'bg-amber-50 dark:bg-amber-950/30'}`}>
                    {allOk
                        ? <CheckCircle2 className="text-green-600 shrink-0" size={20} />
                        : <AlertTriangle className="text-amber-600 shrink-0" size={20} />
                    }
                    <span className="text-sm font-medium">
                        {allOk
                            ? `All ${totalWorkingDays} working days are fully covered`
                            : `${fullyCoveredDays} of ${totalWorkingDays} working days fully covered`
                        }
                    </span>
                </div>

                {/* Per shift type */}
                <div className="flex-1 overflow-y-auto space-y-3 pr-1">
                    {coverage.map(({ shiftType, departmentName, totalWorkingDays: total, okDays, issues }) => {
                        const understaffedIssues = issues.filter(i => i.severity === 'understaffed')
                        const overstaffedIssues = issues.filter(i => i.severity === 'overstaffed')
                        const isOk = understaffedIssues.length === 0

                        return (
                            <div key={shiftType.id} className="border rounded-lg overflow-hidden">
                                {/* Header */}
                                <div className="flex items-center gap-3 p-3 bg-muted/40">
                                    <span
                                        className="w-3 h-3 rounded-full shrink-0"
                                        style={{ backgroundColor: shiftType.color }}
                                    />
                                    <div className="flex-1 min-w-0">
                                        <div className="flex items-center gap-2 flex-wrap">
                                            <span className="font-medium text-sm">{shiftType.name}</span>
                                            <Badge variant="outline" className="text-xs">{departmentName}</Badge>
                                            <span className="text-xs text-muted-foreground">
                                                {roundToMinutes(shiftType.startTime)} – {roundToMinutes(shiftType.endTime)}
                                            </span>
                                        </div>
                                        <div className="text-xs text-muted-foreground mt-0.5">
                                            Required: {shiftType.minEmployees}–{shiftType.maxEmployees} employees
                                        </div>
                                    </div>
                                    <div className="shrink-0 flex items-center gap-1.5">
                                        {isOk
                                            ? <CheckCircle2 size={16} className="text-green-500" />
                                            : <AlertCircle size={16} className="text-red-500" />
                                        }
                                        <span className={`text-sm font-medium ${isOk ? 'text-green-600' : 'text-red-600'}`}>
                                            {okDays}/{total}
                                        </span>
                                    </div>
                                </div>

                                {/* Issues list */}
                                {issues.length > 0 && (
                                    <div className="divide-y">
                                        {understaffedIssues.length > 0 && (
                                            <div className="p-3 space-y-1">
                                                <p className="text-xs font-medium text-red-600 mb-2 flex items-center gap-1">
                                                    <AlertCircle size={12} /> Understaffed ({understaffedIssues.length} days)
                                                </p>
                                                <div className="flex flex-wrap gap-1.5">
                                                    {understaffedIssues.map(issue => (
                                                        <span
                                                            key={issue.isoDate}
                                                            className="inline-flex items-center gap-1 text-xs bg-red-50 dark:bg-red-950/30 text-red-700 dark:text-red-400 border border-red-200 dark:border-red-800 rounded px-1.5 py-0.5"
                                                        >
                                                            {issue.label}
                                                            <span className="font-medium">{issue.assigned}/{issue.required}</span>
                                                        </span>
                                                    ))}
                                                </div>
                                            </div>
                                        )}
                                        {overstaffedIssues.length > 0 && (
                                            <div className="p-3 space-y-1">
                                                <p className="text-xs font-medium text-amber-600 mb-2 flex items-center gap-1">
                                                    <AlertTriangle size={12} /> Overstaffed ({overstaffedIssues.length} days)
                                                </p>
                                                <div className="flex flex-wrap gap-1.5">
                                                    {overstaffedIssues.map(issue => (
                                                        <span
                                                            key={issue.isoDate}
                                                            className="inline-flex items-center gap-1 text-xs bg-amber-50 dark:bg-amber-950/30 text-amber-700 dark:text-amber-400 border border-amber-200 dark:border-amber-800 rounded px-1.5 py-0.5"
                                                        >
                                                            {issue.label}
                                                            <span className="font-medium">{issue.assigned}/{issue.required}</span>
                                                        </span>
                                                    ))}
                                                </div>
                                            </div>
                                        )}
                                    </div>
                                )}

                                {issues.length === 0 && (
                                    <div className="px-3 py-2 text-xs text-muted-foreground">
                                        All working days covered
                                    </div>
                                )}
                            </div>
                        )
                    })}

                    {shiftTypes.length === 0 && (
                        <p className="text-sm text-muted-foreground text-center py-6">
                            No shift types configured
                        </p>
                    )}
                </div>
            </DialogContent>
        </Dialog>
    )
}
