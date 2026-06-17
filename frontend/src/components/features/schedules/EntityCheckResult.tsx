'use client';

import { Button } from "@/components/ui/shadcn/button";
import { useRouter } from "next/navigation";
import { Check, X } from "lucide-react";
import { CheckEntitiesResult } from "@/types";
import { useTranslations } from "next-intl";

interface EntityCheckResultProps {
    entities?: CheckEntitiesResult;
}

export default function EntityCheckResult({ entities }: EntityCheckResultProps) {
    const router = useRouter();
    const t = useTranslations('entityCheck');

    if (!entities) return null;

    const requiredEntities: Array<{
        key: keyof Pick<CheckEntitiesResult, 'departments' | 'employees' | 'shiftTypes'>;
        label: string;
        createLabel: string;
        path: string;
    }> = [
        { key: 'departments', label: t('departments'), createLabel: t('createDepartments'), path: '/departments' },
        { key: 'employees', label: t('employees'), createLabel: t('createEmployees'), path: '/employees' },
        { key: 'shiftTypes', label: t('shiftTypes'), createLabel: t('createShiftTypes'), path: '/shift-templates' },
    ];

    return (
        <div className="space-y-6 p-6 border rounded-lg">
            <h4 className="text-lg font-semibold">{t('progress')}</h4>
            <p className="text-sm text-muted-foreground">
                {t('description')}
            </p>

            <div className="space-y-3">
                {requiredEntities.map(item => {
                    const exists = entities[item.key];

                    return (
                        <div
                            key={item.key}
                            className="flex items-center justify-between border rounded-md p-3"
                        >
                            <div className="flex items-center gap-2">
                                {exists ? (
                                    <Check className="text-green-600 w-5 h-5" />
                                ) : (
                                    <X className="text-red-500 w-5 h-5" />
                                )}
                                <span className="font-medium">{item.label}</span>
                            </div>

                            {!exists && (
                                <Button size="sm" onClick={() => router.push(item.path)}>
                                    {item.createLabel}
                                </Button>
                            )}
                        </div>
                    );
                })}
            </div>
        </div>
    );
}
