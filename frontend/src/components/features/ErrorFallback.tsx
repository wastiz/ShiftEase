"use client";
import { AlertCircle } from "lucide-react";
import Link from "next/link";
import { Card, CardContent } from "@/components/ui/shadcn/card";
import { Button } from "@/components/ui/shadcn/button";

interface ErrorFallbackProps {
    error: Error & { digest?: string };
    reset: () => void;
}

export function ErrorFallback({ error, reset }: ErrorFallbackProps) {
    return (
        <div className="flex min-h-[50vh] items-center justify-center p-6">
            <Card className="w-full max-w-md border-destructive/30">
                <CardContent className="flex flex-col items-center gap-4 text-center">
                    <AlertCircle className="text-destructive" size={40} />
                    <div className="space-y-1">
                        <h2 className="text-lg font-semibold">Something went wrong</h2>
                        <p className="text-muted-foreground text-sm">
                            {error.message || "An unexpected error occurred."}
                        </p>
                    </div>
                    <div className="flex gap-3">
                        <Button onClick={reset} variant="default">
                            Try again
                        </Button>
                        <Button variant="outline" asChild>
                            <Link href="/public">Go home</Link>
                        </Button>
                    </div>
                </CardContent>
            </Card>
        </div>
    );
}
