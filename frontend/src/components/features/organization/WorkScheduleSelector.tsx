"use client";

import { Label } from "@/components/ui/shadcn/label";
import { Switch } from "@/components/ui/shadcn/switch";
import {
    ToggleGroup,
    ToggleGroupItem,
} from "@/components/ui/shadcn/toggle-group";
import {
    Tooltip,
    TooltipContent,
    TooltipProvider,
    TooltipTrigger,
} from "@/components/ui/shadcn/tooltip";
import { TimePicker } from "@/components/ui/inputs/TimePicker";
import { Info } from "lucide-react";
import { useTranslations } from "next-intl";

export type LocalWorkDay = {
    active: boolean;
    startTime: string;
    endTime: string;
};

export type ScheduleMode = "manual" | "fiveTwo" | "fullTime";

interface WorkScheduleSelectorProps {
    scheduleMode: ScheduleMode;
    setScheduleMode: (mode: ScheduleMode) => void;

    workDays: Record<string, LocalWorkDay>;
    setWorkDays: React.Dispatch<
        React.SetStateAction<Record<string, LocalWorkDay>>
    >;

    fullTimeStartTime: string;
    fullTimeEndTime: string;

    setFullTimeStartTime: (value: string) => void;
    setFullTimeEndTime: (value: string) => void;
}

export default function WorkScheduleSelector({
                                                 scheduleMode,
                                                 setScheduleMode,
                                                 workDays,
                                                 setWorkDays,
                                                 fullTimeStartTime,
                                                 fullTimeEndTime,
                                                 setFullTimeStartTime,
                                                 setFullTimeEndTime,
                                             }: WorkScheduleSelectorProps) {
    const t = useTranslations("organization");

    const handleDayToggle = (day: string) => {
        setWorkDays((prev) => ({
            ...prev,
            [day]: {
                ...prev[day],
                active: !prev[day].active,
            },
        }));
    };

    const handleTimeChange = (
        day: string,
        field: "startTime" | "endTime",
        value: string
    ) => {
        setWorkDays((prev) => ({
            ...prev,
            [day]: {
                ...prev[day],
                [field]: value,
            },
        }));
    };

    const handleFullTimeTimeChange = (
        field: "startTime" | "endTime",
        value: string
    ) => {
        if (field === "startTime") {
            setFullTimeStartTime(value);
        } else {
            setFullTimeEndTime(value);
        }

        setWorkDays((prev) => {
            const updated = { ...prev };

            Object.keys(updated).forEach((day) => {
                if (updated[day].active) {
                    updated[day][field] = value;
                }
            });

            return updated;
        });
    };

    return (
        <div className="space-y-4">
            <Label className="text-sm font-medium flex items-center gap-2">
                {t("workSchedule")}
                <span className="text-red-500">*</span>

                <TooltipProvider>
                    <Tooltip>
                        <TooltipTrigger asChild>
                            <Info className="h-4 w-4 text-muted-foreground cursor-help" />
                        </TooltipTrigger>

                        <TooltipContent className="max-w-xs">
                            <p>{t("workScheduleTooltip")}</p>
                        </TooltipContent>
                    </Tooltip>
                </TooltipProvider>
            </Label>

            <ToggleGroup
                type="single"
                value={scheduleMode}
                onValueChange={(value) =>
                    value && setScheduleMode(value as ScheduleMode)
                }
                className="flex gap-2 justify-start"
            >
                <ToggleGroupItem value="manual">
                    {t("manual")}
                </ToggleGroupItem>

                <ToggleGroupItem value="fiveTwo">
                    {t("fiveTwoWorkWeek")}
                </ToggleGroupItem>

                <ToggleGroupItem value="fullTime">
                    {t("fullTimeOperation")}
                </ToggleGroupItem>
            </ToggleGroup>

            {scheduleMode === "fullTime" && (
                <div className="flex items-center gap-2 p-4 bg-muted rounded-lg">
          <span className="font-medium">
            {t("allWeekDaysFrom")}
          </span>

                    <TimePicker
                        value={fullTimeStartTime}
                        onChange={(time) =>
                            handleFullTimeTimeChange("startTime", time)
                        }
                    />

                    <span>{t("to")}</span>

                    <TimePicker
                        value={fullTimeEndTime}
                        onChange={(time) =>
                            handleFullTimeTimeChange("endTime", time)
                        }
                    />

                    <span className="text-sm text-muted-foreground ml-2">
            {t("timeAppliedToAllDays")}
          </span>
                </div>
            )}

            {scheduleMode !== "fullTime" && (
                <div className="flex flex-col gap-4">
                    {Object.entries(workDays).map(([day, conf]) => (
                        <div key={day} className="flex items-center gap-2">
                            <Switch
                                checked={conf.active}
                                onCheckedChange={() => handleDayToggle(day)}
                            />

                            <span className="capitalize w-24">
                {t(`days.${day}`)}
              </span>

                            {conf.active && (
                                <>
                                    <TimePicker
                                        value={conf.startTime}
                                        onChange={(time) =>
                                            handleTimeChange(day, "startTime", time)
                                        }
                                    />

                                    <span>{t("to")}</span>

                                    <TimePicker
                                        value={conf.endTime}
                                        onChange={(time) =>
                                            handleTimeChange(day, "endTime", time)
                                        }
                                    />
                                </>
                            )}
                        </div>
                    ))}
                </div>
            )}
        </div>
    );
}