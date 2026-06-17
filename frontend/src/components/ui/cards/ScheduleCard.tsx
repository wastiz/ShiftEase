import {ScheduleItem} from "@/types";
import {Card, CardContent, CardFooter, CardHeader, CardTitle} from "@/components/ui/shadcn/card";
import {Badge} from "@/components/ui/shadcn/badge";
import {AlertTriangle, ArrowRight, BarChart2, CalendarDays, CheckCircle2, Clock} from "lucide-react";
import {SeverityBadge} from "@/components/ui/badges/SeverityBadge";
import {Button} from "@/components/ui/shadcn/button";
import {useTranslations} from "next-intl";

export function ScheduleCard({
                              item,
                              confirmed,
                              onEdit,
                          }: {
    item: ScheduleItem;
    confirmed: boolean;
    onEdit: (item: ScheduleItem) => void;
}) {
    const hasWarnings = item.warnings && item.warnings.length > 0;
    const tCommon = useTranslations("common")

    return (
        <Card className={`w-full border`}>
            <CardHeader className="pb-2">
                <div className="flex items-center justify-between">
                    <CardTitle className="text-base font-semibold">
                        {item.month} {item.year}
                    </CardTitle>
                    <Badge
                        className={
                            confirmed
                                ? 'bg-green-100 text-green-800 border-green-200'
                                : 'bg-amber-100 text-amber-800 border-amber-200'
                        }
                    >
                        {confirmed ? (
                            <><CheckCircle2 className="h-3 w-3 mr-1" /> Confirmed</>
                        ) : (
                            'Unconfirmed'
                        )}
                    </Badge>
                </div>
            </CardHeader>

            <CardContent className="space-y-3">
                {/* Stats row */}
                <div className="grid grid-cols-3 gap-3">
                    <div className="flex flex-col items-center p-2 rounded-lg bg-muted/50">
                        <BarChart2 className="h-4 w-4 text-muted-foreground mb-1" />
                        <span className="text-sm font-semibold">{item.coverage}</span>
                        <span className="text-xs text-muted-foreground">Coverage</span>
                    </div>
                    <div className="flex flex-col items-center p-2 rounded-lg bg-muted/50">
                        <Clock className="h-4 w-4 text-muted-foreground mb-1" />
                        <span className="text-sm font-semibold">{item.totalHours}h</span>
                        <span className="text-xs text-muted-foreground">Hours</span>
                    </div>
                    <div className="flex flex-col items-center p-2 rounded-lg bg-muted/50">
                        <CalendarDays className="h-4 w-4 text-muted-foreground mb-1" />
                        <span className="text-sm font-semibold">{item.totalShifts}</span>
                        <span className="text-xs text-muted-foreground">Shifts</span>
                    </div>
                </div>

                {/* Warnings */}
                {hasWarnings && (
                    <div className="space-y-1.5">
                        <p className="text-xs font-medium text-muted-foreground flex items-center gap-1">
                            <AlertTriangle className="h-3 w-3" />
                            {item.warnings.length} warning{item.warnings.length > 1 ? 's' : ''}
                        </p>
                        <div className="flex flex-wrap gap-1.5">
                            {item.warnings.slice(0, 3).map((w, i) => (
                                <div
                                    key={i}
                                    className="flex items-center gap-1.5 text-xs border rounded-md px-2 py-1"
                                >
                                    <SeverityBadge severity={w.severity} />
                                    <span className="text-muted-foreground">{w.label}</span>
                                    <span className="font-medium">{w.assigned}/{w.required}</span>
                                </div>
                            ))}
                            {item.warnings.length > 3 && (
                                <span className="text-xs text-muted-foreground self-center">
                                    +{item.warnings.length - 3} more
                                </span>
                            )}
                        </div>
                    </div>
                )}
            </CardContent>

            <CardFooter>
                <Button
                    variant="outline"
                    size="sm"
                    className="ml-auto"
                    onClick={() => onEdit(item)}
                >
                    {tCommon("edit")}<ArrowRight className="h-3.5 w-3.5 ml-1" />
                </Button>
            </CardFooter>
        </Card>
    );
}