"use client";

import { useLocale, useTranslations } from 'next-intl';
import { useRouter } from 'next/navigation';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from '@/components/ui/shadcn/dropdown-menu';
import { ChevronDown } from 'lucide-react';
import { useTransition } from 'react';
import Image from 'next/image';

export default function LanguageSwitcher() {
  const t = useTranslations('language');
  const locale = useLocale();
  const router = useRouter();
  const [isPending, startTransition] = useTransition();

  const changeLanguage = (newLocale: string) => {
    startTransition(() => {
      document.cookie = `NEXT_LOCALE=${newLocale}; path=/; max-age=31536000`;
      router.refresh();
    });
  };

  const languages = [
    { code: 'en', name: 'English' },
    { code: 'ru', name: 'Русский' },
    { code: 'et', name: 'Eesti' },
  ];

  const currentLanguage = languages.find(lang => lang.code === locale);

  return (
      <DropdownMenu>
        <DropdownMenuTrigger className="flex items-center gap-2 w-full px-2 py-1.5 text-sm hover:bg-accent rounded-sm" disabled={isPending}>
          <Image src={`/${currentLanguage?.code}.png`} alt={currentLanguage?.name || ''} width={20} height={20} className="rounded-sm" />
          <span className="flex-1 text-left">{currentLanguage?.name}</span>
          <ChevronDown className="h-4 w-4 opacity-50" />
        </DropdownMenuTrigger>
        <DropdownMenuContent align="end" className="w-40">
          {languages.map((lang) => (
              <DropdownMenuItem
                  key={lang.code}
                  onClick={() => changeLanguage(lang.code)}
                  className={locale === lang.code ? 'bg-accent' : ''}
              >
                <Image src={`/${lang.code}.png`} alt={lang.name} width={20} height={20} className="mr-2 rounded-sm" />
                {lang.name}
              </DropdownMenuItem>
          ))}
        </DropdownMenuContent>
      </DropdownMenu>
  );
}
