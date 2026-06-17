"use client";
import { ErrorFallback } from "@/components/features/ErrorFallback";

export default function EmployerPlainError({
    error,
    reset,
}: {
    error: Error & { digest?: string };
    reset: () => void;
}) {
    return <ErrorFallback error={error} reset={reset} />;
}
