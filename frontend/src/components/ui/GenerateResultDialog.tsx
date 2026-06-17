"use client";

import {
    Dialog,
    DialogContent,
    DialogDescription,
    DialogHeader,
    DialogTitle,
} from "@/components/ui/shadcn/dialog";
import { Button } from "@/components/ui/shadcn/button";
import { AlertCircle, AlertTriangle, CheckCircle2 } from "lucide-react";
import { useTranslations } from "next-intl";
import { GenerateErrorCode, GenerateWarningCode, GenerateStatus } from "@/types/schedule";

interface GenerateResultDialogProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    status: GenerateStatus;
    error?: GenerateErrorCode;
    warnings?: GenerateWarningCode[];
}

export default function GenerateResultDialog({
    open,
    onOpenChange,
    status,
    error,
    warnings = [],
}: GenerateResultDialogProps) {
    const t = useTranslations("schedule");

    const getTitle = () => {
        switch (status) {
            case GenerateStatus.Success:
                return t("generateSuccess");
            case GenerateStatus.Warning:
                return t("generateWarning");
            case GenerateStatus.Error:
                return t("generateError");
        }
    };

    const getIcon = () => {
        switch (status) {
            case GenerateStatus.Success:
                return <CheckCircle2 className="h-6 w-6 text-green-500" />;
            case GenerateStatus.Warning:
                return <AlertTriangle className="h-6 w-6 text-yellow-500" />;
            case GenerateStatus.Error:
                return <AlertCircle className="h-6 w-6 text-red-500" />;
        }
    };

    const getBgColor = () => {
        switch (status) {
            case GenerateStatus.Success:
                return "bg-green-50 border-green-200";
            case GenerateStatus.Warning:
                return "bg-yellow-50 border-yellow-200";
            case GenerateStatus.Error:
                return "bg-red-50 border-red-200";
        }
    };

    return (
        <Dialog open={open} onOpenChange={onOpenChange}>
            <DialogContent className="sm:max-w-md">
                <DialogHeader>
                    <DialogTitle className="flex items-center gap-2">
                        {getIcon()}
                        {getTitle()}
                    </DialogTitle>
                </DialogHeader>

                <div className="mt-4 space-y-3">
                    {status === GenerateStatus.Error && error && (
                        <div className={`p-3 rounded-md border ${getBgColor()}`}>
                            <p className="text-sm text-red-700">
                                {t(`errors.${error}`)}
                            </p>
                        </div>
                    )}

                    {status === GenerateStatus.Warning && warnings.length > 0 && (
                        <div className={`p-3 rounded-md border ${getBgColor()}`}>
                            <ul className="space-y-2">
                                {warnings.map((warning, index) => (
                                    <li
                                        key={index}
                                        className="text-sm text-yellow-700 flex items-start gap-2"
                                    >
                                        <AlertTriangle className="h-4 w-4 mt-0.5 shrink-0" />
                                        {t(`warnings.${warning}`)}
                                    </li>
                                ))}
                            </ul>
                        </div>
                    )}

                    {status === GenerateStatus.Success && (
                        <div className={`p-3 rounded-md border ${getBgColor()}`}>
                            <p className="text-sm text-green-700 flex items-center gap-2">
                                <CheckCircle2 className="h-4 w-4" />
                                {t("generateSuccess")}
                            </p>
                        </div>
                    )}
                </div>

                <div className="flex justify-end mt-4">
                    <Button onClick={() => onOpenChange(false)}>
                        OK
                    </Button>
                </div>
            </DialogContent>
        </Dialog>
    );
}
