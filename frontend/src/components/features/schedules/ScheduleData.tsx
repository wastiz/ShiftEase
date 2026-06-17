import ShiftTemplateSmallCard from "@/components/ui/cards/ShiftTypeSmallCard";
import EmployeeSmallCard from "@/components/ui/cards/EmployeeSmallCard";
import { EmployeeMinData, ShiftTemplate } from "@/types";
import { ReactNode } from "react";

interface ScheduleDataProps {
    isEditable: boolean;
    layoutPosition: "left" | "top";
    shiftTypes: ShiftTemplate[];
    filteredEmployees: EmployeeMinData[];
    children: ReactNode;
    sidebarChildren?: ReactNode;
}

export default function ScheduleData({
    isEditable,
    layoutPosition,
    shiftTypes,
    filteredEmployees,
    children,
    sidebarChildren,
}: ScheduleDataProps) {
    const showTop = isEditable && layoutPosition === "top";
    const showLeft = isEditable && layoutPosition === "left";

    const defaultLeftContent = (
        <>
            <div>
                <h4 className="font-semibold mb-2">Shift Types</h4>
                <div className="space-y-2">
                    {shiftTypes.map((st) => (
                        <ShiftTemplateSmallCard
                            key={st.id}
                            id={st.id}
                            name={st.name}
                            startTime={st.startTime}
                            endTime={st.endTime}
                            color={st.color}
                        />
                    ))}
                </div>
            </div>
            <div>
                <h4 className="font-semibold mb-2">Employees</h4>
                <div className="space-y-2">
                    {filteredEmployees.map((emp) => (
                        <EmployeeSmallCard
                            key={emp.id}
                            id={emp.id}
                            name={emp.name}
                            position={emp.position}
                        />
                    ))}
                </div>
            </div>
        </>
    );

    const defaultTopContent = (
        <>
            <div>
                <h4 className="font-semibold mb-2">Shift Types</h4>
                <div className="flex gap-2 overflow-x-auto">
                    {shiftTypes.map((st) => (
                        <ShiftTemplateSmallCard
                            key={st.id}
                            id={st.id}
                            name={st.name}
                            startTime={st.startTime}
                            endTime={st.endTime}
                            color={st.color}
                        />
                    ))}
                </div>
            </div>
            <div>
                <h4 className="font-semibold mb-2">Employees</h4>
                <div className="flex gap-2 overflow-x-auto">
                    {filteredEmployees.map((emp) => (
                        <EmployeeSmallCard
                            key={emp.id}
                            id={emp.id}
                            name={emp.name}
                            position={emp.position}
                        />
                    ))}
                </div>
            </div>
        </>
    );

    return (
        <>
            {showTop && (
                <div className="border rounded-lg bg-muted/50 p-4 space-y-3 shrink-0">
                    {sidebarChildren ?? defaultTopContent}
                </div>
            )}

            <div className="flex flex-1 min-h-0 gap-4">
                {showLeft && (
                    <aside className="border rounded-lg bg-muted/50 w-64 shrink-0 p-4 space-y-6 overflow-y-auto">
                        {sidebarChildren ?? defaultLeftContent}
                    </aside>
                )}
                {children}
            </div>
        </>
    );
}