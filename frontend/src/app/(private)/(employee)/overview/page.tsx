'use client'

import {useGetConfirmedSchedule} from "@/hooks/api";
import {useEffect, useState, useMemo} from "react";
import {useTranslations, useLocale} from "next-intl";
import {EmployeeMeData, Shift} from "@/types";
import {useAuthStore} from "@/stores/authStore";
import {Card, CardContent, CardDescription, CardHeader, CardTitle} from "@/components/ui/shadcn/card";
import {Badge} from "@/components/ui/shadcn/badge";
import {Calendar as CalendarIcon, Clock, Users, StickyNote, Loader2} from "lucide-react";
import {Separator} from "@/components/ui/shadcn/separator";
import {Calendar} from "@/components/ui/shadcn/calendar";

const today = new Date();

export default function EmployeePersonalPage() {
    const t = useTranslations('employee.overview');
    const locale = useLocale();
    const dateLocale = locale === 'ru' ? 'ru-RU' : locale === 'et' ? 'et-EE' : 'en-US';
    const {user} = useAuthStore();
    const employeeUser = user as EmployeeMeData;

    const [currentMonth, setCurrentMonth] = useState(today);
    const [selectedDay, setSelectedDay] = useState<Date | undefined>(undefined);
    const [shiftsData, setShiftsData] = useState<Shift[]>([]);

    const calendarMonth = currentMonth.getMonth() + 1;
    const calendarYear = currentMonth.getFullYear();

    const primaryDepartmentId = employeeUser.departmentIds?.[0];
    const {data: scheduleData, isLoading} = useGetConfirmedSchedule(calendarMonth, calendarYear, primaryDepartmentId);

    useEffect(() => {
        if (scheduleData) {
            const myShifts = scheduleData.shifts.filter(shift =>
                shift.employees.some(emp => emp.id === employeeUser.id)
            );
            setShiftsData(myShifts);
        }
    }, [scheduleData, employeeUser.id]);

    const upcomingShifts = useMemo(() => {
        const now = new Date();
        now.setHours(0, 0, 0, 0);

        return shiftsData
            .filter(s => {
                const shiftDate = new Date(s.date);
                shiftDate.setHours(0, 0, 0, 0);
                return shiftDate >= now;
            })
            .sort((a, b) => {
                const aTime = new Date(`${a.date}T${a.startTime}`).getTime();
                const bTime = new Date(`${b.date}T${b.startTime}`).getTime();
                return aTime - bTime;
            })
            .slice(0, 5);
    }, [shiftsData]);

    const workDays = useMemo(() => {
        return shiftsData.map(s => {
            const d = new Date(s.date);
            d.setHours(0, 0, 0, 0);
            return d;
        });
    }, [shiftsData]);

    const selectedDayShifts = useMemo(() => {
        if (!selectedDay) return [];
        const sel = selectedDay.toISOString().split('T')[0];
        return shiftsData.filter(s => s.date === sel);
    }, [selectedDay, shiftsData]);

    const getMyNote = (shift: Shift) => {
        const myAssignment = shift.employees.find(emp => emp.id === employeeUser.id);
        return myAssignment?.note;
    };

    const getCoworkers = (shift: Shift) => {
        return shift.employees.filter(emp => emp.id !== employeeUser.id);
    };

    if (isLoading) {
        return (
            <div className="flex items-center justify-center h-screen">
                <Loader2 className="h-8 w-8 animate-spin" />
            </div>
        );
    }

    return (
        <div className="flex flex-col gap-6 p-6">
            <div>
                <h1 className="text-3xl font-bold tracking-tight">{t('title')}</h1>
                <p className="text-muted-foreground">
                    {t('subtitle')}
                </p>
            </div>

            {/* Upcoming Shifts Section */}
            <Card>
                <CardHeader>
                    <CardTitle className="flex items-center gap-2">
                        <CalendarIcon className="h-5 w-5" />
                        {t('upcomingShifts')}
                    </CardTitle>
                    <CardDescription>
                        {t('yourNextShifts', { count: upcomingShifts.length })}
                    </CardDescription>
                </CardHeader>
                <CardContent>
                    {upcomingShifts.length === 0 ? (
                        <p className="text-muted-foreground text-center py-8">
                            {t('noUpcomingShifts')}
                        </p>
                    ) : (
                        <div className="space-y-4">
                            {upcomingShifts.map((shift, index) => {
                                const myNote = getMyNote(shift);
                                const coworkers = getCoworkers(shift);
                                const shiftDate = new Date(shift.date);

                                return (
                                    <div key={shift.id} className="border rounded-lg p-4 hover:bg-accent/50 transition-colors">
                                        <div className="flex items-start justify-between mb-3">
                                            <div className="flex items-center gap-3">
                                                <div
                                                    className="w-3 h-3 rounded-full"
                                                    style={{ backgroundColor: shift.color }}
                                                />
                                                <div>
                                                    <h3 className="font-semibold text-lg">
                                                        {shift.shiftTypeName}
                                                    </h3>
                                                    <p className="text-sm text-muted-foreground">
                                                        {shiftDate.toLocaleDateString(dateLocale, {
                                                            weekday: 'long',
                                                            year: 'numeric',
                                                            month: 'long',
                                                            day: 'numeric'
                                                        })}
                                                    </p>
                                                </div>
                                            </div>
                                            {index === 0 && (
                                                <Badge variant="default">{t('next')}</Badge>
                                            )}
                                        </div>

                                        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                                            <div className="flex items-center gap-2 text-sm">
                                                <Clock className="h-4 w-4 text-muted-foreground" />
                                                <span className="font-medium">
                                                    {shift.startTime} - {shift.endTime}
                                                </span>
                                            </div>

                                            {coworkers.length > 0 && (
                                                <div className="flex items-center gap-2 text-sm">
                                                    <Users className="h-4 w-4 text-muted-foreground" />
                                                    <span className="text-muted-foreground">
                                                        {t('coworkers')}: {coworkers.map(c => c.name).join(', ')}
                                                    </span>
                                                </div>
                                            )}
                                        </div>

                                        {myNote && (
                                            <>
                                                <Separator className="my-3" />
                                                <div className="flex items-start gap-2 text-sm bg-muted/50 p-3 rounded">
                                                    <StickyNote className="h-4 w-4 text-muted-foreground mt-0.5 flex-shrink-0" />
                                                    <div>
                                                        <p className="font-medium text-xs text-muted-foreground mb-1">
                                                            {t('note')}:
                                                        </p>
                                                        <p>{myNote}</p>
                                                    </div>
                                                </div>
                                            </>
                                        )}
                                    </div>
                                );
                            })}
                        </div>
                    )}
                </CardContent>
            </Card>

            {/* Calendar Section */}
            <Card>
                <CardHeader>
                    <CardTitle>{t('shiftCalendar')}</CardTitle>
                    <CardDescription>
                        {t('clickDayForDetails')}
                    </CardDescription>
                </CardHeader>
                <CardContent>
                    <div className="flex flex-col lg:flex-row gap-6">
                        <Calendar
                            mode="single"
                            selected={selectedDay}
                            onSelect={setSelectedDay}
                            month={currentMonth}
                            onMonthChange={setCurrentMonth}
                            modifiers={{
                                workDay: workDays,
                            }}
                            modifiersClassNames={{
                                workDay: "bg-primary/20 text-primary font-semibold",
                            }}
                            className="flex-1 rounded-md border"
                        />

                        {/* Selected day details */}
                        <div className="flex-1">
                            {selectedDay ? (
                                selectedDayShifts.length > 0 ? (
                                    <div className="space-y-3">
                                        <h4 className="font-medium text-sm text-muted-foreground">
                                            {selectedDay.toLocaleDateString(dateLocale, {
                                                weekday: 'long',
                                                day: 'numeric',
                                                month: 'long'
                                            })}
                                        </h4>
                                        {selectedDayShifts.map(shift => {
                                            const myNote = getMyNote(shift);
                                            const coworkers = getCoworkers(shift);

                                            return (
                                                <div key={shift.id} className="border rounded-lg p-3 space-y-2">
                                                    <div className="flex items-center gap-2">
                                                        <div
                                                            className="w-2.5 h-2.5 rounded-full"
                                                            style={{ backgroundColor: shift.color }}
                                                        />
                                                        <span className="font-medium">{shift.shiftTypeName}</span>
                                                    </div>

                                                    <div className="flex items-center gap-2 text-sm">
                                                        <Clock className="h-3.5 w-3.5 text-muted-foreground" />
                                                        <span>{shift.startTime} — {shift.endTime}</span>
                                                    </div>

                                                    {coworkers.length > 0 && (
                                                        <div className="flex items-start gap-2 text-sm">
                                                            <Users className="h-3.5 w-3.5 text-muted-foreground mt-0.5" />
                                                            <span className="text-muted-foreground">
                                                                {coworkers.map(c => c.name).join(', ')}
                                                            </span>
                                                        </div>
                                                    )}

                                                    {myNote && (
                                                        <div className="flex items-start gap-2 text-sm bg-muted/50 p-2 rounded">
                                                            <StickyNote className="h-3.5 w-3.5 text-muted-foreground mt-0.5" />
                                                            <span>{myNote}</span>
                                                        </div>
                                                    )}
                                                </div>
                                            );
                                        })}
                                    </div>
                                ) : (
                                    <div className="flex items-center justify-center h-full text-muted-foreground text-sm py-8">
                                        {t('noShiftsThisDay')}
                                    </div>
                                )
                            ) : (
                                <div className="flex items-center justify-center h-full text-muted-foreground text-sm py-8">
                                    {t('selectDayInCalendar')}
                                </div>
                            )}
                        </div>
                    </div>
                </CardContent>
            </Card>
        </div>
    );
}
