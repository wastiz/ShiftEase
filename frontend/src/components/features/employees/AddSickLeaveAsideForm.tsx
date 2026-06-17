"use client";

import { useState } from "react";
import { useTranslations } from "next-intl";
import { toast } from "sonner";
import { AsideDrawer } from "@/components/ui/AsideDrawer";
import { Button } from "@/components/ui/shadcn/button";
import { Input } from "@/components/ui/shadcn/input";
import { SickLeaveForm } from "./SickLeaveForm";
import { useGetEmployees, useAddApprovedSickLeave } from "@/hooks/api";
import { Employee, SickLeaveDto } from "@/types";

type Props = {
    open: boolean;
    onOpenChange: (open: boolean) => void;
};

const FORM_ID = "add-sick-leave-form";

export function AddSickLeaveAsideForm({ open, onOpenChange }: Props) {
    const t = useTranslations("timeOff");
    const [search, setSearch] = useState("");
    const [selectedEmployee, setSelectedEmployee] = useState<Employee | null>(null);

    const { data: employees = [] } = useGetEmployees();
    const addSickLeaveMutation = useAddApprovedSickLeave(selectedEmployee?.id ?? 0);

    const filteredEmployees = employees.filter((e) =>
        search.trim() === ""
            ? true
            : `${e.firstName} ${e.lastName}`.toLowerCase().includes(search.toLowerCase())
    );

    const handleSubmit = (data: SickLeaveDto) => {
        if (!selectedEmployee) return;
        addSickLeaveMutation.mutate(data, {
            onSuccess: () => {
                toast.success(t("sickLeaveAddedSuccess"));
                handleClose();
            },
            onError: () => toast.error(t("sickLeaveAddFailed")),
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
            title="Добавить больничный"
            description="Выберите сотрудника и укажите данные больничного"
            footer={
                selectedEmployee ? (
                    <Button
                        type="submit"
                        form={FORM_ID}
                        disabled={addSickLeaveMutation.isPending}
                    >
                        {addSickLeaveMutation.isPending ? t("adding") : t("addSickLeave")}
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
                    <SickLeaveForm key={selectedEmployee.id} formId={FORM_ID} onSubmit={handleSubmit} />
                </div>
            )}
        </AsideDrawer>
    );
}
