"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { toast } from "sonner";
import { Calendar } from "lucide-react";

import { Employee } from "@/types";
import Header from "@/components/ui/headers/Header";
import { Button } from "@/components/ui/shadcn/button";
import Loader from "@/components/ui/Loader";
import {
    Dialog,
    DialogContent,
    DialogFooter,
    DialogHeader,
    DialogTitle,
} from "@/components/ui/shadcn/dialog";
import EmployeeCard from "@/components/ui/cards/EmployeeCard";
import {
    TimeOffDialog,
    EmployeeAsideForm,
    EmployeeFormValues,
    EmployeeFilters,
    useEmployeeFilters,
} from "@/components/features/employees";
import {
    useCreateEmployee,
    useDeleteEmployee,
    useGetEmployees,
    useGetDepartments,
    useUpdateEmployee,
} from "@/hooks/api";
import {useTranslations} from "next-intl";
import Main from "@/components/ui/Main";

export default function Employees() {
    const router = useRouter();
    const t = useTranslations("employer.employees")
    const tCommon = useTranslations("common")

    const [selectedEmployee, setSelectedEmployee] = useState<Employee | null>(null);
    const [drawerOpen, setDrawerOpen] = useState(false);
    const [confirmDeleteId, setConfirmDeleteId] = useState<number | null>(null);
    const [timeOffDialogOpen, setTimeOffDialogOpen] = useState(false);
    const [timeOffEmployee, setTimeOffEmployee] = useState<Employee | null>(null);


    const { data: departments = [] } = useGetDepartments();
    const { data: employees = [], isLoading: isEmployeeLoading } = useGetEmployees();

    const {
        filters,
        setFilters,
        filteredEmployees,
        totalCount,
        filteredCount,
    } = useEmployeeFilters(employees);

    const createMutation = useCreateEmployee();
    const updateMutation = useUpdateEmployee(selectedEmployee?.id ?? 0);
    const deleteMutation = useDeleteEmployee(confirmDeleteId ?? 0);

    const openDrawer = (employee?: Employee) => {
        setSelectedEmployee(employee ?? null);
        setDrawerOpen(true);
    };

    const closeDrawer = () => {
        setSelectedEmployee(null);
        setDrawerOpen(false);
    };

    const handleCreate = (data: EmployeeFormValues) => {
        createMutation.mutate(data, {
            onSuccess: () => {
                toast.success("Employee created!");
                closeDrawer();
            },
            onError: () => toast.error("Failed to create employee"),
        });
    };

    const handleUpdate = (_id: number, data: EmployeeFormValues) => {
        updateMutation.mutate(data, {
            onSuccess: () => {
                toast.success("Employee updated!");
                closeDrawer();
            },
            onError: () => toast.error("Failed to update employee"),
        });
    };

    const handleDeleteClick = (id: number) => {
        setConfirmDeleteId(id);
    };

    const handleDeleteConfirm = () => {
        if (!confirmDeleteId) return;
        deleteMutation.mutate(undefined, {
            onSuccess: () => {
                toast.success("Employee deleted!");
                setConfirmDeleteId(null);
                closeDrawer();
            },
            onError: () => toast.error("Failed to delete employee"),
        });
    };

    const openTimeOffDialog = (employee: Employee) => {
        setTimeOffEmployee(employee);
        setTimeOffDialogOpen(true);
    };

    return (
        <>
            <Header title={t("title")}>
                <Button variant="outline" onClick={() => router.push("employees/bulk-create")}>
                    {tCommon("bulkAdd")}
                </Button>
                <Button onClick={() => setDrawerOpen(true)}>
                    + {t('addEmployee')}
                </Button>
            </Header>

            <Main className="p-4 space-y-4 flex flex-col flex-1 min-h-0">
                {isEmployeeLoading ? (
                    <Loader />
                ) : employees && employees.length > 0 ? (
                    <>
                        <EmployeeFilters
                            filters={filters}
                            onFiltersChange={setFilters}
                            departments={departments}
                            totalCount={totalCount}
                            filteredCount={filteredCount}
                        />

                        {filteredEmployees.length > 0 ? (
                            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
                                {filteredEmployees.map((emp) => {
                                    const empDepartments = departments.filter((g) =>
                                        emp.departmentIds?.includes(g.id)
                                    );
                                    return (
                                        <EmployeeCard
                                            key={emp.id}
                                            firstName={emp.firstName}
                                            lastName={emp.lastName}
                                            email={emp.email}
                                            position={emp.position}
                                            avatar={"/images/avatar_placeholder.png"}
                                            departments={empDepartments}
                                            actions={[
                                                { label: tCommon("edit"), onClick: () => openDrawer(emp) },
                                                {
                                                    label: t("timeOff"),
                                                    onClick: () => openTimeOffDialog(emp),
                                                    variant: "secondary",
                                                    icon: <Calendar className="h-4 w-4" />,
                                                },
                                            ]}
                                        />
                                    );
                                })}
                            </div>
                        ) : (
                            <div className="mt-20 flex flex-col justify-center items-center space-y-2">
                                <p className="text-muted-foreground">No employees match your filters</p>
                                <Button variant="ghost" onClick={() => setFilters({ search: "", departmentIds: [] })}>
                                    Clear filters
                                </Button>
                            </div>
                        )}
                    </>
                ) : (
                    <div className="flex-1 flex items-center justify-center mb-20">
                        <div className="flex flex-col gap-2 items-center">
                            <h4>No employees yet</h4>
                            <Button onClick={() => openDrawer()}>
                                Create your first employee
                            </Button>
                        </div>
                    </div>
                )}

                {/* Delete confirmation dialog */}
                <Dialog
                    open={!!confirmDeleteId}
                    onOpenChange={() => setConfirmDeleteId(null)}
                >
                    <DialogContent>
                        <DialogHeader>
                            <DialogTitle>Delete Employee</DialogTitle>
                        </DialogHeader>
                        <p>Are you sure you want to delete this employee?</p>
                        <DialogFooter className="flex justify-end gap-2">
                            <Button
                                variant="secondary"
                                onClick={() => setConfirmDeleteId(null)}
                            >
                                Cancel
                            </Button>
                            <Button
                                variant="destructive"
                                onClick={handleDeleteConfirm}
                                disabled={deleteMutation.isPending}
                            >
                                {deleteMutation.isPending ? "Deleting..." : "Delete"}
                            </Button>
                        </DialogFooter>
                    </DialogContent>
                </Dialog>

                {/* Time off dialog */}
                {timeOffEmployee && (
                    <TimeOffDialog
                        open={timeOffDialogOpen}
                        onOpenChange={setTimeOffDialogOpen}
                        employeeId={timeOffEmployee.id}
                        employeeName={`${timeOffEmployee.firstName} ${timeOffEmployee.lastName}`}
                    />
                )}
            </Main>
            
            <EmployeeAsideForm
                open={drawerOpen}
                onOpenChange={(open) => {
                    if (!open) closeDrawer();
                    else setDrawerOpen(true);
                }}
                selectedEmployee={selectedEmployee}
                departments={departments}
                onCreate={handleCreate}
                onUpdate={handleUpdate}
                onDelete={handleDeleteClick}
                isCreating={createMutation.isPending}
                isUpdating={updateMutation.isPending}
                isDeleting={deleteMutation.isPending}
            />
        </>
    );
}
