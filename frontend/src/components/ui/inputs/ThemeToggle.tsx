"use client";

import { useTheme } from "next-themes";
import { Moon, Sun } from "lucide-react";
import { useEffect, useState } from "react";
import { useTranslations } from "next-intl";
import { SidebarMenuButton } from "@/components/ui/shadcn/sidebar";

export default function ThemeToggle() {
    const { theme, setTheme } = useTheme();
    const [mounted, setMounted] = useState(false);
    const t = useTranslations("common");

    useEffect(() => setMounted(true), []);

    if (!mounted) return null;

    const isDark = theme === "dark";

    return (
        <SidebarMenuButton
            tooltip={isDark ? t("lightMode") : t("darkMode")}
            onClick={() => setTheme(isDark ? "light" : "dark")}
        >
            {isDark ? <Sun /> : <Moon />}
            <span>{isDark ? t("lightMode") : t("darkMode")}</span>
        </SidebarMenuButton>
    );
}