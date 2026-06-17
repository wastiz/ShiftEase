"use client";

import { Button } from "@/components/ui/shadcn/button";
import {
    DropdownMenu,
    DropdownMenuContent,
    DropdownMenuItem,
    DropdownMenuTrigger,
} from "@/components/ui/shadcn/dropdown-menu";
import { Label } from "@/components/ui/shadcn/label";
import {useTranslations} from "next-intl";

type ColorPickerProps = {
    value: string;
    onChange: (color: string) => void;
    label?: string;
};

const DEFAULT_COLORS = [
    "#ef4444", // red
    "#f97316", // orange
    "#f59e0b", // amber
    "#eab308", // yellow
    "#84cc16", // lime
    "#22c55e", // green
    "#10b981", // emerald
    "#14b8a6", // teal
    "#06b6d4", // cyan
    "#0ea5e9", // sky
    "#3b82f6", // blue
    "#6366f1", // indigo
    "#8b5cf6", // violet
    "#a855f7", // purple
    "#d946ef", // fuchsia
    "#ec4899", // pink
    "#f43f5e", // rose
    "#64748b", // slate
    "#dc2626", // red-600
    "#ea580c", // orange-600
    "#ca8a04", // yellow-600
    "#16a34a", // green-600
    "#0891b2", // cyan-600
    "#2563eb", // blue-600
    "#7c3aed", // violet-600
    "#c026d3", // fuchsia-600
    "#db2777", // pink-600
    "#475569", // slate-600
];

export default function ColorPicker({ value, onChange }: ColorPickerProps) {
    return (
        <div className="space-y-2">
            <DropdownMenu>
                <DropdownMenuTrigger asChild>
                    <Button
                        type="button"
                        variant="outline"
                        className="w-full justify-start gap-2"
                    >
                        <div
                            className="w-6 h-6 rounded-full border-2"
                            style={{ backgroundColor: value }}
                        />
                        <span className="font-mono text-sm">{value}</span>
                    </Button>
                </DropdownMenuTrigger>
                <DropdownMenuContent className="w-64">
                    <div className="grid grid-cols-7 gap-2 p-2">
                        {DEFAULT_COLORS.map((color) => (
                            <button
                                key={color}
                                type="button"
                                className="w-8 h-8 rounded-full border-2 transition-transform hover:scale-110"
                                style={{
                                    backgroundColor: color,
                                    borderColor: value === color ? "#000" : "transparent",
                                }}
                                onClick={() => onChange(color)}
                            />
                        ))}
                    </div>
                </DropdownMenuContent>
            </DropdownMenu>
        </div>
    );
}
