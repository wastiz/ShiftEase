"use client";

import { useState, useMemo } from "react";
import {
    CardContent,
    CardDescription,
    CardHeader,
    CardTitle,
} from "@/components/ui/shadcn/card";
import { Label } from "@/components/ui/shadcn/label";
import { Input } from "@/components/ui/shadcn/input";
import { PasswordInput } from "@/components/ui/inputs/PasswordInput";
import { Button } from "@/components/ui/shadcn/button";
import { Mode, RegisterPayload } from "@/types";
import { Check } from "lucide-react";
import { useEmployerRegister } from "@/hooks/api/auth";
import { useTranslations } from 'next-intl';
import { isApiError } from "@/hooks/useApiError";

interface RegisterFormProps {
    setMode: (mode: Mode) => void;
}

export default function RegisterForm({ setMode }: RegisterFormProps) {
    const t = useTranslations('auth');
    const tCommon = useTranslations('common');
    const { mutate, isPending, isError, error } = useEmployerRegister();

    const [form, setForm] = useState({
        fullName: "",
        email: "",
        password: "",
    });

    const [errors, setErrors] = useState<{
        fullName?: string;
        email?: string;
        password?: string;
        server?: string;
    }>({});

    const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const { id, value } = e.target;
        setForm((prev) => ({ ...prev, [id]: value }));
        setErrors((prev) => ({ ...prev, [id]: "" }));
    };

    const passwordCriteria = useMemo(() => {
        const p = form.password || "";
        return {
            length: p.length >= 8,
            uppercase: /[A-ZА-ЯЁ]/.test(p),
            digit: /\d/.test(p),
            special: /[!@#$%^&*()_+\-=\[\]{};':"\\|,.<>\/?`~]/.test(p),
        };
    }, [form.password]);

    const validate = () => {
        const newErrors: typeof errors = {};

        const parts = form.fullName.trim().split(/\s+/);
        if (parts.length < 2) {
            newErrors.fullName = t('validation.enterFullName');
        }

        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        if (!emailRegex.test(form.email)) {
            newErrors.email = t('validation.enterValidEmail');
        }

        if (
            !passwordCriteria.length ||
            !passwordCriteria.uppercase ||
            !passwordCriteria.digit ||
            !passwordCriteria.special
        ) {
            newErrors.password = t('validation.passwordNotMeetRequirements');
        }

        setErrors(newErrors);
        return Object.keys(newErrors).length === 0;
    };

    const handleSubmit = (e: React.FormEvent) => {
        e.preventDefault();
        if (!validate()) return;

        const [firstName, ...rest] = form.fullName.trim().split(/\s+/);
        const lastName = rest.join(" ");

        const payload: RegisterPayload = {
            firstName,
            lastName,
            email: form.email.trim(),
            password: form.password,
        };

        mutate(payload, {
            onSuccess: () => setMode("login"),
            onError: (err) => {
                if (!isApiError(err)) {
                    setErrors(prev => ({ ...prev, server: tCommon('unknownError') }));
                    return;
                }
                if (err.errorCode === 'VALIDATION_ERROR' && err.errors) {
                    const fieldMap: Record<string, keyof typeof errors> = {
                        email: 'email',
                        password: 'password',
                        firstName: 'fullName',
                        lastName: 'fullName',
                    };
                    const newErrors: typeof errors = {};
                    Object.entries(err.errors).forEach(([field, messages]) => {
                        const key = fieldMap[field];
                        if (key) newErrors[key] = messages[0];
                        else newErrors.server = messages[0];
                    });
                    setErrors(prev => ({ ...prev, ...newErrors }));
                } else {
                    setErrors(prev => ({ ...prev, server: err.message }));
                }
            },
        });
    };

    const Criterion = ({
                           ok,
                           children,
                       }: {
        ok: boolean;
        children: React.ReactNode;
    }) => (
        <div className="flex items-center gap-2 text-sm">
      <span
          className={`inline-flex items-center justify-center w-5 h-5 rounded-full shrink-0 ${
              ok ? "bg-green-100 text-green-600" : "bg-gray-200 text-gray-400"
          }`}
          aria-hidden
      >
        {ok ? <Check width={13} strokeWidth={3} /> : "•"}
      </span>
            <span className="text-sm text-muted-foreground">{children}</span>
        </div>
    );

    return (
        <>
            <CardHeader>
                <CardTitle>{t('createAnAccount')}</CardTitle>
                <CardDescription>{t('fillOutDetails')}</CardDescription>
            </CardHeader>
            <CardContent>
                <form onSubmit={handleSubmit}>
                    <div className="flex flex-col gap-6">
                        {/* Full Name */}
                        <div className="grid gap-1">
                            <Label htmlFor="fullName">{t('fullName')}</Label>
                            <Input
                                id="fullName"
                                type="text"
                                value={form.fullName}
                                onChange={handleChange}
                                required
                            />
                            {errors.fullName && (
                                <p className="text-sm text-red-500">{errors.fullName}</p>
                            )}
                        </div>

                        {/* Email */}
                        <div className="grid gap-1">
                            <Label htmlFor="email">{tCommon('email')}</Label>
                            <Input
                                id="email"
                                type="email"
                                value={form.email}
                                onChange={handleChange}
                                required
                            />
                            {errors.email && (
                                <p className="text-sm text-red-500">{errors.email}</p>
                            )}
                        </div>

                        {/* Password */}
                        <div className="grid gap-1">
                            <Label htmlFor="password">{tCommon('password')}</Label>
                            <PasswordInput
                                id="password"
                                value={form.password}
                                onChange={handleChange}
                                required
                            />
                            <div className="mt-2 flex flex-col gap-1">
                                <Criterion ok={passwordCriteria.length}>
                                    {t('passwordRequirements.length')}
                                </Criterion>
                                <Criterion ok={passwordCriteria.uppercase}>
                                    {t('passwordRequirements.uppercase')}
                                </Criterion>
                                <Criterion ok={passwordCriteria.digit}>
                                    {t('passwordRequirements.digit')}
                                </Criterion>
                                <Criterion ok={passwordCriteria.special}>
                                    {t('passwordRequirements.special')}
                                </Criterion>
                            </div>
                            {errors.password && (
                                <p className="text-sm text-red-500 mt-1">{errors.password}</p>
                            )}
                        </div>

                        <Button type="submit" className="w-full" disabled={isPending}>
                            {isPending ? t('signingUp') : tCommon('signUp')}
                        </Button>

                        {isError && (
                            <p className="text-red-500">
                                {(error instanceof Error ? error : { message: tCommon('unknownError') })?.message || t('registrationFailed')}
                            </p>
                        )}
                        {errors.server && (
                            <p className="text-red-500">{errors.server}</p>
                        )}
                    </div>

                    <div className="mt-4 text-center text-sm">
                        {t('alreadyHaveAccount')}{" "}
                        <button
                            type="button"
                            onClick={() => setMode("login")}
                            className="underline underline-offset-4"
                        >
                            {tCommon('login')}
                        </button>
                    </div>
                </form>
            </CardContent>
        </>
    );
}
