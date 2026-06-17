"use client";

import { useState } from "react";
import { useTranslations } from "next-intl";
import { toast } from "sonner";
import { AsideDrawer } from "@/components/ui/AsideDrawer";
import { Button } from "@/components/ui/shadcn/button";
import { Input } from "@/components/ui/shadcn/input";
import { VacationForm } from "./VacationForm";
import { useGetEmployees, useAddApprovedVacation } from "@/hooks/api";
import { Employee, VacationDto } from "@/types";

type Props = {
    open: boolean;
    onOpenChange: (open: boolean) => void;
};

const FORM_ID = "add-vacation-form";

export function AddVacationAsideForm({ open, onOpenChange }: Props) {
    const t = useTranslations("timeOff");
    const [search, setSearch] = useState("");
    const [selectedEmployee, setSelectedEmployee] = useState<Employee | null>(null);

    const { data: employees = [] } = useGetEmployees();
    const addVacationMutation = useAddApprovedVacation(selectedEmployee?.id ?? 0);

    const filteredEmployees = employees.filter((e) =>
        search.trim() === ""
            ? true
            : `${e.firstName} ${e.lastName}`.toLowerCase().includes(search.toLowerCase())
    );

    const handleSubmit = (data: VacationDto) => {
        if (!selectedEmployee) return;
        addVacationMutation.mutate(data, {
            onSuccess: () => {
                toast.success(t("vacationAddedSuccess"));
                handleClose();
            },
            onError: () => toast.error(t("vacationAddFailed")),
        });
    };

    const handleClose = () => {
        setSelectedEmployee(null);
        setSearch("");
        onOpenChange(false);
    };

    return (
        <AsideDrawer
            open={open}
            onOpenChange={handleClose}
            title="Добавить отпуск"
            description="Выберите сотрудника и укажите даты отпуска"
            footer={
                selectedEmployee ? (
                    <Button
                        type="submit"
                        form={FORM_ID}
                        disabled={addVacationMutation.isPending}
                    >
                        {addVacationMutation.isPending ? t("adding") : t("addVacation")}
                    </Button>
                ) : undefined
            }
        >
            {!selectedEmployee ? (
                <div className="space-y-3">
                    <Input
                        placeholder="Поиск сотрудника..."
                        value={search}
                        onChange={(e) => setSearch(e.target.value)}
                    />
                    <div className="space-y-2">
                        {filteredEmployees.map((emp) => (
                            <button
                                key={emp.id}
                                className="w-full text-left p-3 border rounded-lg hover:bg-accent transition-colors"
                                onClick={() => setSelectedEmployee(emp)}
                            >
                                <p className="font-medium">{emp.firstName} {emp.lastName}</p>
                                {emp.position && (
                                    <p className="text-sm text-muted-foreground">{emp.position}</p>
                                )}
                            </button>
                        ))}
                    </div>
                </div>
            ) : (
                <div className="space-y-4">
                    <div className="flex items-center justify-between p-3 border rounded-lg bg-muted/50">
                        <div>
                            <p className="font-medium">{selectedEmployee.firstName} {selectedEmployee.lastName}</p>
                            {selectedEmployee.position && (
                                <p className="text-sm text-muted-foreground">{selectedEmployee.position}</p>
                            )}
                        </div>
                        <Button variant="ghost" size="sm" onClick={() => setSelectedEmployee(null)}>
                            Изменить
                        </Button>
                    </div>
                    <VacationForm key={selectedEmployee.id} formId={FORM_ID} onSubmit={handleSubmit} />
                </div>
            )}
        </AsideDrawer>
    );
}
