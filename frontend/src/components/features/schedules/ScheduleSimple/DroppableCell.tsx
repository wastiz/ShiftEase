import { useEffect, useRef, useState } from "react";
import { draggable } from "@atlaskit/pragmatic-drag-and-drop/element/adapter";
import { dropTargetForElements } from "@atlaskit/pragmatic-drag-and-drop/element/adapter";
import NoteEditor from "@/components/features/schedules/NoteEditor";
import { Button } from "@/components/ui/shadcn/button";
import { X } from "lucide-react";
import { DayAssignment } from "@/helpers/scheduleHelper";

type DroppableCellProps = {
    employeeId: number;
    date: string;
    assignment?: DayAssignment;
    onAssign: (employeeId: number, date: string, shiftTypeId: number) => void;
    onRemove: (employeeId: number, date: string) => void;
    onUpdateNote: (employeeId: number, date: string, note: string) => void;
    isNonWorkingDay: boolean;
    isCompact?: boolean;
}

export default function DroppableCell({
    employeeId,
    date,
    assignment,
    onAssign,
    onRemove,
    onUpdateNote,
    isNonWorkingDay,
    isCompact = false,
}: DroppableCellProps) {
    const ref = useRef<HTMLDivElement>(null);
    const [isDraggingOver, setIsDraggingOver] = useState(false);

    useEffect(() => {
        if (!ref.current) return;
        return dropTargetForElements({
            element: ref.current,
            onDrop: ({ source }) => {
                if (source.data.type === "shiftType") {
                    const shiftTypeId = source.data.id as number;
                    onAssign(employeeId, date, shiftTypeId);
                }
                setIsDraggingOver(false);
            },
            onDragEnter: () => setIsDraggingOver(true),
            onDragLeave: () => setIsDraggingOver(false),
        });
    }, [employeeId, date, onAssign]);

    if (isNonWorkingDay) {
        return (
            <div className={`${isCompact ? "h-6" : "min-h-[60px] p-2"} rounded bg-red-50 dark:bg-red-950/20 flex items-center justify-center`}>
                {!isCompact && (
                    <span className="text-xs text-red-600 dark:text-red-400 font-medium">
                        Off
                    </span>
                )}
            </div>
        );
    }

    return (
        <div
            ref={ref}
            className={`${isCompact ? "h-6" : "min-h-[60px] p-2"} rounded transition-colors ${isDraggingOver ? "bg-accent border-2 border-dashed border-primary" : ""
                }`}
        >
            {assignment ? (
                <div
                    className={`${isCompact ? "h-full w-full border-l-2 p-0" : "relative p-2 border-l-4"} rounded text-xs bg-card`}
                    style={{ borderLeftColor: assignment.color }}
                >
                    {!isCompact ? (
                        <>
                            <div className="flex items-center justify-between gap-1">
                                <span className="font-medium">{assignment.shiftTypeName}</span>
                                <div className="flex items-center gap-1">
                                    <NoteEditor
                                        note={assignment.note}
                                        onSave={(note) => onUpdateNote(employeeId, date, note)}
                                        buttonClassName="h-4 w-4 p-0"
                                    />
                                    <Button
                                        variant="ghost"
                                        size="sm"
                                        className="h-4 w-4 p-0"
                                        onClick={() => onRemove(employeeId, date)}
                                    >
                                        <X className="h-3 w-3" />
                                    </Button>
                                </div>
                            </div>
                            <div className="text-muted-foreground text-[10px]">
                                {(() => {
                                    const startMinutes = parseInt(assignment.startTime.split(':')[0]) * 60 + parseInt(assignment.startTime.split(':')[1]);
                                    const endMinutes = parseInt(assignment.endTime.split(':')[0]) * 60 + parseInt(assignment.endTime.split(':')[1]);
                                    const isOvernight = endMinutes <= startMinutes;

                                    return (
                                        <>
                                            {assignment.startTime.slice(0, 5)} - {assignment.endTime.slice(0, 5)}
                                            {isOvernight && <span className="ml-1 text-orange-500">(+1)</span>}
                                        </>
                                    );
                                })()}
                            </div>
                        </>
                    ) : (
                        <div
                            className="h-full w-full opacity-60"
                            style={{ backgroundColor: assignment.color }}
                            title={`${assignment.shiftTypeName}: ${assignment.startTime.slice(0, 5)} - ${assignment.endTime.slice(0, 5)}`}
                        />
                    )}
                </div>
            ) : (
                <div className="h-full flex items-center justify-center text-[10px] text-muted-foreground">
                    {!isCompact && "Drop shift here"}
                </div>
            )}
        </div>
    );
}
