"use client";

import * as React from "react";
import { format } from "date-fns";
import { ru } from "date-fns/locale";
import { CalendarIcon } from "lucide-react";
import { DateRange } from "react-day-picker";

import { cn } from "@/lib/utils";
import { Button } from "@/components/ui/shadcn/button";
import { Calendar } from "@/components/ui/shadcn/calendar";
import { Popover, PopoverContent, PopoverTrigger } from "@/components/ui/shadcn/popover";

type DatePickerProps = {
    date?: Date;
    onChange?: (date: Date | undefined) => void;
};

export function DatePicker({ date, onChange }: DatePickerProps) {
    return (
        <Popover>
            <PopoverTrigger asChild>
                <Button
                    variant="outline"
                    id="date-picker-simple"
                    className="justify-start font-normal"
                >
                    {date ? format(date, "PPP") : <span>Pick a date</span>}
                </Button>
            </PopoverTrigger>
            <PopoverContent className="w-auto p-0" align="start">
                <Calendar
                    mode="single"
                    selected={date}
                    onSelect={onChange}
                    defaultMonth={date}
                />
            </PopoverContent>
        </Popover>
    );
}

type DatePickerRangeProps = {
    value?: DateRange;
    onChange?: (range: DateRange | undefined) => void;
    className?: string;
};

export function DatePickerRange({ value, onChange, className }: DatePickerRangeProps) {
    const fmt = (d: Date) => format(d, "d MMM yyyy", { locale: ru });

    const label = value?.from
        ? value.to
            ? `${fmt(value.from)} — ${fmt(value.to)}`
            : fmt(value.from)
        : "Выберите период";

    return (
        <Popover>
            <PopoverTrigger asChild>
                <Button
                    variant="outline"
                    className={cn(
                        "w-full justify-start text-left font-normal",
                        !value?.from && "text-muted-foreground",
                        className
                    )}
                >
                    <CalendarIcon className="mr-2 h-4 w-4 shrink-0" />
                    {label}
                </Button>
            </PopoverTrigger>
            <PopoverContent className="w-fit p-0" align="end">
                <Calendar
                    mode="range"
                    selected={value}
                    onSelect={onChange}
                    defaultMonth={value?.from}
                    numberOfMonths={2}
                />
            </PopoverContent>
        </Popover>
    );
}
