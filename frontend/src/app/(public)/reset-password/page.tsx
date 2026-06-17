"use client";

import { useState } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/shadcn/card";
import { Label } from "@/components/ui/shadcn/label";
import { Input } from "@/components/ui/shadcn/input";
import { PasswordInput } from "@/components/ui/inputs/PasswordInput";
import { Button } from "@/components/ui/shadcn/button";
import { toast } from "sonner";
import { useResetPassword } from "@/hooks/api/auth";
import { isApiError } from "@/hooks/useApiError";

export default function ResetPasswordPage() {
    const router = useRouter();
    const searchParams = useSearchParams();
    const token = searchParams.get("token");

    const [newPassword, setNewPassword] = useState("");
    const [confirmPassword, setConfirmPassword] = useState("");
    const [error, setError] = useState<string | null>(null);

    const { mutate, isPending } = useResetPassword();

    const handleSubmit = async (event: React.FormEvent<HTMLFormElement>) => {
        event.preventDefault();
        setError(null);

        if (!token) {
            setError("Invalid reset link");
            return;
        }

        if (newPassword !== confirmPassword) {
            setError("Passwords do not match");
            return;
        }

        if (newPassword.length < 8) {
            setError("Password must be at least 8 characters");
            return;
        }

        mutate(
            { token, newPassword },
            {
                onSuccess: () => {
                    toast.success("Password reset successfully!");
                    router.push("/sign-in");
                },
                onError: (err: unknown) => {
                    if (isApiError(err)) {
                        setError(err.errorCode === 'PASSWORD_RESET_FAILED'
                            ? "This reset link is invalid or has expired."
                            : err.message);
                    } else {
                        setError("Failed to reset password. Please try again.");
                    }
                },
            }
        );
    };

    if (!token) {
        return (
            <div className="flex min-h-screen items-center justify-center">
                <Card className="w-full max-w-md">
                    <CardHeader>
                        <CardTitle>Invalid Link</CardTitle>
                        <CardDescription>This password reset link is invalid or has expired</CardDescription>
                    </CardHeader>
                    <CardContent>
                        <Button onClick={() => router.push("/sign-in")} className="w-full">
                            Back to login
                        </Button>
                    </CardContent>
                </Card>
            </div>
        );
    }

    return (
        <div className="flex min-h-screen items-center justify-center">
            <Card className="w-full max-w-md">
                <CardHeader>
                    <CardTitle>Reset Password</CardTitle>
                    <CardDescription>Enter your new password</CardDescription>
                </CardHeader>
                <CardContent>
                    <form onSubmit={handleSubmit}>
                        <div className="flex flex-col gap-6">
                            <div className="grid gap-3">
                                <Label htmlFor="newPassword">
                                    New Password
                                    <span className="text-red-500 ml-1">*</span>
                                </Label>
                                <PasswordInput
                                    id="newPassword"
                                    placeholder="Enter new password"
                                    value={newPassword}
                                    onChange={(e) => {
                                        setNewPassword(e.target.value);
                                        setError(null);
                                    }}
                                    required
                                    minLength={8}
                                />
                            </div>

                            <div className="grid gap-3">
                                <Label htmlFor="confirmPassword">
                                    Confirm Password
                                    <span className="text-red-500 ml-1">*</span>
                                </Label>
                                <PasswordInput
                                    id="confirmPassword"
                                    placeholder="Confirm new password"
                                    value={confirmPassword}
                                    onChange={(e) => {
                                        setConfirmPassword(e.target.value);
                                        setError(null);
                                    }}
                                    required
                                    minLength={8}
                                />
                            </div>

                            {error && <p className="text-red-500 text-sm">{error}</p>}

                            <Button type="submit" className="w-full" disabled={isPending}>
                                {isPending ? "Resetting..." : "Reset Password"}
                            </Button>

                            <Button
                                type="button"
                                variant="ghost"
                                className="w-full"
                                onClick={() => router.push("/sign-in")}
                            >
                                Back to login
                            </Button>
                        </div>
                    </form>
                </CardContent>
            </Card>
        </div>
    );
}
