'use client'

import { useRouter } from 'next/navigation'
import EntityCheckResult from '@/components/features/schedules/EntityCheckResult'
import { useCheckEntities, useScheduleSummaries } from "@/hooks/api";
import Loader from "@/components/ui/Loader";
import { Card, CardContent } from "@/components/ui/shadcn/card";
import { Button } from "@/components/ui/shadcn/button";
import Header from "@/components/ui/headers/Header";
import { ScheduleItem } from "@/types";
import {
    CheckCircle2,
    X,
    AlertTriangle,
} from "lucide-react";
import { useState, useEffect } from "react";
import { useTranslations } from "next-intl";
import {MONTH_NAMES} from "@/helpers/dateHelper";
import {ScheduleCard} from "@/components/ui/cards/ScheduleCard";

function parseMonthToNumber(month: string): number {
    const num = parseInt(month);
    if (!isNaN(num) && num >= 1 && num <= 12) return num;
    return MONTH_NAMES[month.toLowerCase()] ?? new Date().getMonth() + 1;
}

export default function Schedules() {
    const router = useRouter()
    const t = useTranslations('employer.schedules')
    const [showSuccessCard, setShowSuccessCard] = useState(false)

    const { data: entities, isLoading: loadingEntities, error: entitiesError } = useCheckEntities()

    const hasAllEntities = !!(entities?.departments && entities?.employees && entities?.shiftTypes)

    const {
        data: scheduleSummary,
        isLoading: loadingScheduleSummaries,
        error: scheduleSummariesError,
    } = useScheduleSummaries(hasAllEntities)

    useEffect(() => {
        if (!hasAllEntities) return
        const timer = setTimeout(() => {
            setShowSuccessCard(false)
        }, 5000)
        return () => clearTimeout(timer)
    }, [hasAllEntities])

    const handleEditClick = (item: ScheduleItem) => {
        const month = parseMonthToNumber(item.month);
        const year = parseInt(item.year) || new Date().getFullYear();
        router.push(`/schedules/manage?month=${month}&year=${year}`)
    }

    const handleGoToSchedule = () => {
        const now = new Date();
        router.push(`/schedules/manage?month=${now.getMonth() + 1}&year=${now.getFullYear()}`)
    }

    if (loadingEntities) return <Loader />
    if (entitiesError) return <div className="text-red-500">{`${entitiesError}`}</div>

    if (!hasAllEntities) {
        return <EntityCheckResult entities={entities} />
    }

    if (loadingScheduleSummaries) return <Loader />
    if (scheduleSummariesError) return <div className="text-red-500">{`${scheduleSummariesError}`}</div>

    const confirmed = scheduleSummary?.confirmedSchedules ?? [];
    const unconfirmed = scheduleSummary?.unconfirmedSchedules ?? [];
    const hasAny = confirmed.length > 0 || unconfirmed.length > 0;

    return (
        <>
            <Header title={t('title')}>
                <Button onClick={handleGoToSchedule}>
                    {t("manageSchedules")}
                </Button>
            </Header>

            <main className="p-4 space-y-6">
                {showSuccessCard && (
                    <div className="transition-all duration-500 ease-in-out animate-in slide-in-from-top">
                        <Card className="border-green-200 relative">
                            <CardContent className="pt-6 pr-12">
                                <div className="flex items-center gap-2 text-green-700">
                                    <CheckCircle2 className="h-5 w-5" />
                                    <p className="font-medium">{t('allDataAvailable')}</p>
                                </div>
                                <Button
                                    variant="ghost"
                                    size="icon"
                                    className="absolute top-2 right-2 h-8 w-8 text-green-700 hover:bg-green-100"
                                    onClick={() => setShowSuccessCard(false)}
                                >
                                    <X className="h-4 w-4" />
                                </Button>
                            </CardContent>
                        </Card>
                    </div>
                )}

                {!hasAny ? (
                    <p className="text-muted-foreground">{t('noSchedulesYet')}</p>
                ) : (
                    <>
                        {unconfirmed.length > 0 && (
                            <section className="space-y-3">
                                <h2 className="text-base font-semibold text-amber-700 flex items-center gap-2">
                                    <AlertTriangle className="h-4 w-4" />
                                    {t('unconfirmedSchedules')} ({unconfirmed.length})
                                </h2>
                                <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
                                    {unconfirmed.map((item) => (
                                        <ScheduleCard
                                            key={item.id}
                                            item={item}
                                            confirmed={false}
                                            onEdit={handleEditClick}
                                        />
                                    ))}
                                </div>
                            </section>
                        )}

                        {confirmed.length > 0 && (
                            <section className="space-y-3">
                                <h2 className="text-base font-semibold text-green-700 flex items-center gap-2">
                                    <CheckCircle2 className="h-4 w-4" />
                                    {t('confirmedSchedules')} ({confirmed.length})
                                </h2>
                                <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
                                    {confirmed.map((item) => (
                                        <ScheduleCard
                                            key={item.id}
                                            item={item}
                                            confirmed={true}
                                            onEdit={handleEditClick}
                                        />
                                    ))}
                                </div>
                            </section>
                        )}
                    </>
                )}
            </main>
        </>
    )
}
