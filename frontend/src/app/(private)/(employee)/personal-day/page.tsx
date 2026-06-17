'use client';

import { useTranslations } from 'next-intl';
import Header from "@/components/ui/headers/Header";
import Main from "@/components/ui/Main";

export default function PersonalDayPage() {
    const t = useTranslations('employee.personalDay');

    return (
        <>
            <Header title={t('title')} />
            <Main>
                <div className="container max-w-4xl mx-auto p-4 pb-20 md:p-6">
                    <p className="text-muted-foreground">{t('comingSoon')}</p>
                </div>
            </Main>
        </>
    );
}
