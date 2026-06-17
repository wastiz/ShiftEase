"use client";
import { Component, type ErrorInfo, type ReactNode } from "react";

interface ErrorBoundaryProps {
    children: ReactNode;
    fallback?: ReactNode | ((props: { error: Error; reset: () => void }) => ReactNode);
}

interface ErrorBoundaryState {
    error: Error | null;
}

export class ErrorBoundary extends Component<ErrorBoundaryProps, ErrorBoundaryState> {
    constructor(props: ErrorBoundaryProps) {
        super(props);
        this.state = { error: null };
    }

    static getDerivedStateFromError(error: Error): ErrorBoundaryState {
        return { error };
    }

    componentDidCatch(error: Error, errorInfo: ErrorInfo) {
        console.error("ErrorBoundary caught an error:", error, errorInfo);
    }

    reset = () => {
        this.setState({ error: null });
    };

    render() {
        if (this.state.error) {
            const { fallback } = this.props;
            if (typeof fallback === "function") {
                return fallback({ error: this.state.error, reset: this.reset });
            }
            if (fallback) {
                return fallback;
            }
            // Import lazily to avoid circular deps — use inline default fallback
            return (
                <div className="flex min-h-[200px] items-center justify-center p-6">
                    <div className="text-center">
                        <p className="text-destructive mb-2 font-semibold">Something went wrong</p>
                        <button
                            onClick={this.reset}
                            className="text-primary text-sm underline underline-offset-4"
                        >
                            Try again
                        </button>
                    </div>
                </div>
            );
        }

        return this.props.children;
    }
}
