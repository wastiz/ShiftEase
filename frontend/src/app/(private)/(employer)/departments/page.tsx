"use client";

import { useState } from "react";
import { toast } from "sonner";

import { Department } from "@/types";
import Header from "@/components/ui/headers/Header";
import Loader from "@/components/ui/Loader";
import DepartmentCard from "@/components/ui/cards/DepartmentCard";
import { DepartmentAsideForm } from "@/components/features/departments";
import { Button } from "@/components/ui/shadcn/button";
import {
    Dialog,
    DialogContent,
    DialogHeader,
    DialogTitle,
    DialogFooter,
} from "@/components/ui/shadcn/dialog";
import {
    useGetDepartments,
    useCreateDepartment,
    useUpdateDepartment,
    useDeleteDepartment,
} from "@/hooks/api";
import { DepartmentFormValues } from "@/types";
import {useTranslations} from "next-intl";

export default function Departments() {
    const t = useTranslations("employer.departments");
    const tCommon = useTranslations("common")
    const [selectedDepartment, setSelectedDepartment] = useState<Department | null>(null);
    const [drawerOpen, setDrawerOpen] = useState(false);
    const [confirmDeleteId, setConfirmDeleteId] = useState<number | null>(null);

    const { data: departments = [], isLoading } = useGetDepartments();

    const createMutation = useCreateDepartment();
    const updateMutation = useUpdateDepartment(selectedDepartment?.id ?? 0);
    const deleteMutation = useDeleteDepartment();

    const openDrawer = (department?: Department) => {
        setSelectedDepartment(department ?? null);
        setDrawerOpen(true);
    };

    const closeDrawer = () => {
        setSelectedDepartment(null);
        setDrawerOpen(false);
    };

    const handleCreate = (data: DepartmentFormValues) => {
        createMutation.mutate(data, {
            onSuccess: () => closeDrawer(),
            onError: () => toast.error("Failed to create department"),
        });
    };

    const handleUpdate = (_id: number, data: DepartmentFormValues) => {
        updateMutation.mutate(data, {
            onSuccess: () => {
                toast.success("Department updated!");
                closeDrawer();
            },
            onError: () => toast.error("Failed to update department"),
        });
    };

    const handleDeleteClick = (id: number) => {
        setConfirmDeleteId(id);
    };

    const handleDeleteConfirm = () => {
        if (!confirmDeleteId) return;
        deleteMutation.mutate(confirmDeleteId, {
            onSuccess: () => {
                toast.success("Department deleted!");
                setConfirmDeleteId(null);
                closeDrawer();
            },
            onError: () => toast.error("Failed to delete department"),
        });
    };

    const getWorkingHours = (department: Department) => {
        if (department.startTime && department.endTime) {
            return `${department.startTime} - ${department.endTime}`;
        }
        return t("usesOrganizationHours");
    };

    return (
        <>
            <Header title={t("title")}>
                <Button onClick={() => setDrawerOpen(true)}>
                    + {t('addDepartment')}
                </Button>
            </Header>

            <main className="p-4">
                {isLoading ? (
                    <Loader />
                ) : departments.length > 0 ? (
                    <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
                        {departments.map((department) => (
                            <DepartmentCard
                                key={department.id}
                                name={department.name}
                                description={department.description}
                                color={department.color}
                                workingHours={getWorkingHours(department)}
                                actions={[
                                    {
                                        label: tCommon("edit"),
                                        onClick: () => openDrawer(department),
                                    },
                                ]}
                            />
                        ))}
                    </div>
                ) : (
                    <div className="mt-40 flex flex-col justify-center items-center space-y-4">
                        <h4>No departments yet</h4>
                        <Button onClick={() => openDrawer()}>
                            Create your first department
                        </Button>
                    </div>
                )}

                {/* Delete confirmation dialog */}
                <Dialog
                    open={!!confirmDeleteId}
                    onOpenChange={() => setConfirmDeleteId(null)}
                >
                    <DialogContent>
                        <DialogHeader>
                            <DialogTitle>Delete Department</DialogTitle>
                        </DialogHeader>
                        <p>Are you sure you want to delete this department?</p>
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
            </main>
            <DepartmentAsideForm
                open={drawerOpen}
                onOpenChange={(open) => {
                    if (!open) closeDrawer();
                    else setDrawerOpen(true);
                }}
                selectedDepartment={selectedDepartment}
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
