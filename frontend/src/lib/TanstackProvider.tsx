"use client";
import {QueryClient, QueryClientProvider, QueryErrorResetBoundary} from "@tanstack/react-query";
import { ReactQueryDevtools } from "@tanstack/react-query-devtools";
import type { ReactNode } from "react";

export const queryClient = new QueryClient({
    defaultOptions: {
        queries: {
            retry: false,
            staleTime: 60000,
        },
    },
});

export default function TanstackProvider({ children }: { children: ReactNode }) {
    return (
        <QueryClientProvider client={queryClient}>
            <QueryErrorResetBoundary>
                {children}
            </QueryErrorResetBoundary>
            {process.env.NODE_ENV === "development" && (
                <ReactQueryDevtools initialIsOpen={false} />
            )}
        </QueryClientProvider>
    );
}
