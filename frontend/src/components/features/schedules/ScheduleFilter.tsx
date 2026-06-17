import { Filter } from "lucide-react";
import { Popover, PopoverContent, PopoverTrigger } from "@/components/ui/shadcn/popover";
import { Button } from "@/components/ui/shadcn/button";
import { Checkbox } from "@/components/ui/shadcn/checkbox";
import { Label } from "@/components/ui/shadcn/label";
import { Department, ShiftTemplate } from "@/types";

type Props = {
    departments: Department[];
    shiftTypes: ShiftTemplate[];

    selectedDepartmentId: number | null;
    selectedShiftTemplates: number[];

    onToggleDepartment: (id: number) => void;
    onToggleShiftTemplate: (id: number) => void;

    onClear: () => void;
};

export function ScheduleFilter({
                                        departments,
                                        shiftTypes,
                                        selectedDepartmentId,
                                        selectedShiftTemplates,
                                        onToggleDepartment,
                                        onToggleShiftTemplate,
                                        onClear,
                                    }: Props) {
    const hasActiveFilters =
        selectedShiftTemplates.length > 0 || selectedDepartmentId !== null;

    return (
        <Popover>
            <PopoverTrigger asChild>
                <Button variant="outline" size="icon">
                    <Filter className={hasActiveFilters ? "text-primary" : ""} />
                </Button>
            </PopoverTrigger>

            <PopoverContent className="w-80">
                <div className="space-y-4">
                    {/* Departments */}
                    {departments.length > 0 && (
                        <div className="space-y-2">
                            <h4 className="font-medium text-sm">
                                Filter by Department
                            </h4>

                            <div className="space-y-2">
                                {departments.map((g) => (
                                    <div
                                        key={g.id}
                                        className="flex items-center space-x-2"
                                    >
                                        <Checkbox
                                            id={`filter-department-${g.id}`}
                                            checked={selectedDepartmentId === g.id}
                                            onCheckedChange={() =>
                                                onToggleDepartment(g.id)
                                            }
                                        />

                                        <Label
                                            htmlFor={`filter-department-${g.id}`}
                                            className="flex items-center gap-2 cursor-pointer"
                                        >
                                            {g.color && (
                                                <div
                                                    className="w-3 h-3 rounded-full"
                                                    style={{ backgroundColor: g.color }}
                                                />
                                            )}
                                            {g.name}
                                        </Label>
                                    </div>
                                ))}
                            </div>
                        </div>
                    )}

                    {/* Shift types */}
                    <div className="space-y-2">
                        <h4 className="font-medium text-sm">
                            Filter by Shift Type
                        </h4>

                        <div className="space-y-2">
                            {shiftTypes.map((st) => (
                                <div
                                    key={st.id}
                                    className="flex items-center space-x-2"
                                >
                                    <Checkbox
                                        id={`filter-shift-${st.id}`}
                                        checked={selectedShiftTemplates.includes(
                                            st.id
                                        )}
                                        onCheckedChange={() =>
                                            onToggleShiftTemplate(st.id)
                                        }
                                    />

                                    <Label
                                        htmlFor={`filter-shift-${st.id}`}
                                        className="flex items-center gap-2 cursor-pointer"
                                    >
                                        <div
                                            className="w-3 h-3 rounded"
                                            style={{ backgroundColor: st.color }}
                                        />
                                        {st.name}
                                    </Label>
                                </div>
                            ))}
                        </div>
                    </div>

                    {/* Clear */}
                    {hasActiveFilters && (
                        <Button
                            variant="outline"
                            size="sm"
                            onClick={onClear}
                            className="w-full"
                        >
                            Clear Filters
                        </Button>
                    )}
                </div>
            </PopoverContent>
        </Popover>
    );
}