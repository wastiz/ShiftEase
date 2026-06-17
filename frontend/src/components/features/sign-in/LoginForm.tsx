import { CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/shadcn/card";
import { Label } from "@/components/ui/shadcn/label";
import { Input } from "@/components/ui/shadcn/input";
import { PasswordInput } from "@/components/ui/inputs/PasswordInput";
import { Button } from "@/components/ui/shadcn/button";
import { Mode, UserRole } from "@/types";
import { useLogin } from "@/hooks/api/auth";
import { LoginPayload } from "@/types";
import { useState } from "react";
import { useRouter } from "next/navigation";
import { useTranslations } from 'next-intl';
import { isApiError } from "@/hooks/useApiError";

interface LoginFormProps {
    setMode: (mode: Mode) => void;
    role: UserRole
}

export default function LoginForm({ setMode, role }: LoginFormProps) {
    const t = useTranslations('auth');
    const tCommon = useTranslations('common');
    const router = useRouter();
    const [form, setForm] = useState<LoginPayload>({
        email: "",
        password: "",
    });
    const [errorMessage, setErrorMessage] = useState<string | null>(null);

    const { mutate, isPending, isSuccess } = useLogin(role)

    const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const { id, value } = e.target;
        setForm((prev) => ({ ...prev, [id]: value }));
    };

    const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();
        setErrorMessage(null);

        mutate(form, {
            onError: (err) => {
                if (!isApiError(err)) return;
                setErrorMessage(err.message);
            },
        });
    };


    return (
        <>
            <CardHeader>
                <CardTitle>
                    {t('loginTitle', { role: t(role) })}
                </CardTitle>
                <CardDescription>{t('loginDescription')}</CardDescription>
            </CardHeader>
            <CardContent>
                <form onSubmit={handleSubmit}>
                    <div className="flex flex-col gap-6">
                        {/* Email */}
                        <div className="grid gap-3">
                            <Label htmlFor="email">{tCommon('email')}</Label>
                            <Input
                                id="email"
                                type="email"
                                placeholder={t('emailPlaceholder')}
                                value={form.email}
                                onChange={handleChange}
                                required
                            />
                        </div>

                        {/* Password */}
                        <div className="grid gap-3">
                            <div className="flex items-center">
                                <Label htmlFor="password">{tCommon('password')}</Label>
                                <button
                                    type="button"
                                    className="ml-auto text-sm underline-offset-4 hover:underline"
                                    onClick={() => setMode("forgot")}
                                >
                                    {t('forgotPassword')}
                                </button>
                            </div>
                            <PasswordInput
                                id="password"
                                value={form.password}
                                onChange={handleChange}
                                required
                            />
                        </div>

                        {/* Buttons */}
                        <div className="flex flex-col gap-3">
                            <Button type="submit" className="w-full" disabled={isPending}>
                                {isPending ? t('loggingIn') : tCommon('login')}
                            </Button>
                        </div>

                        {/* Error & Success */}
                        {errorMessage && <p className="text-red-500">{errorMessage}</p>}
                        {isSuccess && (
                            <p className="text-green-500">{t('loginSuccess')}</p>
                        )}
                    </div>

                    {/* Switch to register */}
                    <div className="mt-4 text-center text-sm">
                        {t('dontHaveAccount')}{" "}
                        <button
                            type="button"
                            onClick={() => setMode("register")}
                            className="underline underline-offset-4"
                        >
                            {tCommon('register')}
                        </button>
                    </div>
                </form>
            </CardContent>
        </>
    );
}
