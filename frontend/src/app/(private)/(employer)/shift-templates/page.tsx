"use client";

import { useState } from "react";
import { toast } from "sonner";
import { Loader2 } from "lucide-react";

import { ShiftTemplate, ShiftTemplateFormValues } from "@/types";
import Header from "@/components/ui/headers/Header";
import ShiftTemplateCard from "@/components/ui/cards/ShiftTypeCard";
import { ShiftTemplateAsideForm } from "@/components/features/shift-types";
import {
    Dialog,
    DialogContent,
    DialogFooter,
    DialogHeader,
    DialogTitle,
} from "@/components/ui/shadcn/dialog";
import { Button } from "@/components/ui/shadcn/button";
import {
    useCreateShiftTemplate,
    useDeleteShiftTemplate,
    useGetDepartments,
    useGetShiftTemplates,
    useUpdateShiftTemplate,
} from "@/hooks/api";
import {roundToMinutes} from "@/helpers/dateHelper";
import {getWorkingHoursLabel} from "@/helpers/scheduleHelper";
import {useTranslations} from "next-intl";

export default function ShiftTemplates() {
    const [drawerOpen, setDrawerOpen] = useState(false);
    const [selectedShift, setSelectedShift] = useState<ShiftTemplate | null>(null);
    const [confirmDeleteId, setConfirmDeleteId] = useState<number | null>(null);
    const t = useTranslations("employer.shiftTypes")

    const { data: shifts = [], isLoading, isError } = useGetShiftTemplates();
    const { data: departments = [] } = useGetDepartments();

    const createMutation = useCreateShiftTemplate();
    const updateMutation = useUpdateShiftTemplate(selectedShift?.id ?? 0);
    const deleteMutation = useDeleteShiftTemplate();

    const openDrawer = (shift?: ShiftTemplate) => {
        setSelectedShift(shift ?? null);
        setDrawerOpen(true);
    };

    const closeDrawer = () => {
        setSelectedShift(null);
        setDrawerOpen(false);
    };

    const handleCreate = (data: ShiftTemplateFormValues) => {
        createMutation.mutate(data, {
            onSuccess: () => {
                toast.success("Shift type created!");
                closeDrawer();
            },
            onError: () => toast.error("Failed to create shift type"),
        });
    };

    const handleUpdate = (_id: number, data: ShiftTemplateFormValues) => {
        updateMutation.mutate(
            { id: selectedShift?.id ?? 0, ...data },
            {
                onSuccess: () => {
                    toast.success("Shift type updated!");
                    closeDrawer();
                },
                onError: () => toast.error("Failed to update shift type"),
            }
        );
    };

    const handleDeleteClick = (id: number) => {
        setConfirmDeleteId(id);
    };

    const handleDeleteConfirm = () => {
        if (!confirmDeleteId) return;
        deleteMutation.mutate(confirmDeleteId, {
            onSuccess: () => {
                toast.success("Shift type deleted!");
                setConfirmDeleteId(null);
                closeDrawer();
            },
            onError: () => toast.error("Failed to delete shift type"),
        });
    };

    return (
        <>
            <Header title={t("title")}>
                <Button onClick={() => setDrawerOpen(true)}>
                    + {t('addShiftTemplate')}
                </Button>
            </Header>

            <main className="p-4">
                {isLoading && (
                    <div className="flex justify-center mt-10">
                        <Loader2 className="animate-spin" size={32} />
                    </div>
                )}

                {isError && (
                    <p className="text-red-500">Failed to load shift types</p>
                )}

                {!isLoading && shifts.length === 0 && (
                    <div className="mt-40 flex flex-col justify-center items-center space-y-4">
                        <h4>No shift types yet</h4>
                        <Button onClick={() => openDrawer()}>
                            Create your first shift type
                        </Button>
                    </div>
                )}

                {shifts.length > 0 && (
                    <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
                        {shifts.map((shift) => (
                            <ShiftTemplateCard
                                key={shift.id}
                                name={shift.name}
                                employees={`${shift.minEmployees}-${shift.maxEmployees}`}
                                timeRange={`${roundToMinutes(shift.startTime)} - ${roundToMinutes(shift.endTime)}`}
                                workingHours={shift.breakDuration
                                    ? getWorkingHoursLabel(shift.startTime, shift.endTime, shift.breakDuration)
                                    : undefined}
                                color={shift.color}
                                actions={[
                                    {
                                        label: "Edit",
                                        onClick: () => openDrawer(shift),
                                    },
                                ]}
                            />
                        ))}
                    </div>
                )}

                {/* Delete confirmation dialog */}
                <Dialog
                    open={!!confirmDeleteId}
                    onOpenChange={() => setConfirmDeleteId(null)}
                >
                    <DialogContent>
                        <DialogHeader>
                            <DialogTitle>Delete Shift Type</DialogTitle>
                        </DialogHeader>
                        <p>Are you sure you want to delete this shift type?</p>
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
            <ShiftTemplateAsideForm
                open={drawerOpen}
                onOpenChange={(open) => {
                    if (!open) closeDrawer();
                    else setDrawerOpen(true);
                }}
                selectedShift={selectedShift}
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
