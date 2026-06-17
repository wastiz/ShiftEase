import { useMemo, useState } from "react";
import { X } from "lucide-react";
import {Popover, PopoverContent, PopoverTrigger} from "@/components/ui/shadcn/popover";
import {Button} from "@/components/ui/shadcn/button";
import {
    Command,
    CommandEmpty,
    CommandGroup,
    CommandInput,
    CommandItem,
    CommandList
} from "@/components/ui/shadcn/command";
import {EmployeeMinData} from "@/types";
import {useTranslations} from "next-intl";

type Props = {
    employees: EmployeeMinData[];
    selectedEmployeeId: number | null;
    onSelect: (id: number | null) => void;
};

export function EmployeeFilter({ employees, selectedEmployeeId, onSelect }: Props) {
    const t = useTranslations('employer.schedules')
    const [open, setOpen] = useState(false);
    const [searchValue, setSearchValue] = useState("");

    const suggestions = useMemo(() => {
        if (!searchValue.trim()) return [];
        const query = searchValue.toLowerCase();
        return employees.filter(emp => emp.name.toLowerCase().includes(query)).slice(0, 8);
    }, [employees, searchValue]);

    const selectedEmployee = employees.find(
        (e) => e.id === selectedEmployeeId
    );

    return (
        <div className="relative ml-auto">
            <Popover open={open} onOpenChange={setOpen}>
                <PopoverTrigger asChild>
                    <Button
                        variant="outline"
                        className={`w-44 justify-start font-normal ${
                            selectedEmployeeId ? "pr-7" : ""
                        }`}
                        role="combobox"
                    >
            <span className="truncate text-sm">
              {selectedEmployee?.name || t("searchEmployee")}
            </span>
                    </Button>
                </PopoverTrigger>

                <PopoverContent className="w-64 p-0" align="start">
                    <Command>
                        <CommandInput
                            placeholder={t("searchEmployee")}
                            value={searchValue}
                            onValueChange={setSearchValue}
                        />

                        <CommandList>
                            <CommandEmpty>{t("noEmployeesFound")}</CommandEmpty>

                            <CommandGroup>
                                {suggestions.map((emp) => (
                                    <CommandItem
                                        key={emp.id}
                                        value={emp.name}
                                        onSelect={() => {
                                            onSelect(emp.id);
                                            setSearchValue("");
                                            setOpen(false);
                                        }}
                                    >
                                        {emp.name}
                                    </CommandItem>
                                ))}
                            </CommandGroup>
                        </CommandList>
                    </Command>
                </PopoverContent>
            </Popover>

            {selectedEmployeeId && (
                <button
                    className="absolute right-2 top-1/2 -translate-y-1/2 text-muted-foreground hover:text-foreground"
                    onClick={() => {
                        onSelect(null);
                        setSearchValue("");
                    }}
                >
                    <X className="h-3.5 w-3.5" />
                </button>
            )}
        </div>
    );
}