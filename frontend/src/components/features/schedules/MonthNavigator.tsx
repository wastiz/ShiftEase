import { Button } from "@/components/ui/shadcn/button";
import { Badge } from "@/components/ui/shadcn/badge";
import { ChevronLeft, ChevronRight } from "lucide-react";
import { useTranslations, useLocale } from "next-intl";

interface MonthNavigatorProps {
    currentMonth: number; // 0–11
    currentYear: number;
    isConfirmed: boolean;
    onChange: (month: number, year: number) => void;
}

export function MonthNavigator({
                                   currentMonth,
                                   currentYear,
                                   isConfirmed,
                                   onChange,
                               }: MonthNavigatorProps) {
    const t = useTranslations('monthNav');
    const locale = useLocale();

    const dateLocale = locale === 'ru' ? 'ru-RU' : locale === 'et' ? 'et-EE' : 'en-US';

    const handlePrev = () => {
        if (currentMonth === 0) {
            onChange(11, currentYear - 1);
        } else {
            onChange(currentMonth - 1, currentYear);
        }
    };

    const handleNext = () => {
        if (currentMonth === 11) {
            onChange(0, currentYear + 1);
        } else {
            onChange(currentMonth + 1, currentYear);
        }
    };

    return (
        <div className="flex items-center gap-3">
            <Button variant="outline" size="icon" onClick={handlePrev}>
                <ChevronLeft />
            </Button>

            <div className="flex items-center gap-2">
                <span className="font-medium whitespace-nowrap">
                    {new Date(currentYear, currentMonth).toLocaleString(dateLocale, {
                        month: "long",
                        year: "numeric",
                    })}
                </span>

                {isConfirmed ? (
                    <Badge variant="secondary">{t('confirmed')}</Badge>
                ) : (
                    <Badge variant="outline">{t('unconfirmed')}</Badge>
                )}
            </div>

            <Button variant="outline" size="icon" onClick={handleNext}>
                <ChevronRight />
            </Button>
        </div>
    );
}
