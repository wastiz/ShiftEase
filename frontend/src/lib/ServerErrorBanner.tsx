"use client";
import { useServerErrorStore } from "@/zustand/server-error-state";
import { AlertCircle } from "lucide-react";

export const ServerErrorBanner = () => {
    const isServerDown = useServerErrorStore((state) => state.isServerDown);
    if (!isServerDown) return null;
    return (
        <div className="fixed bottom-0 z-60 flex w-full items-center justify-center gap-3 bg-red-400/70 backdrop-blur-sm py-2 px-4 text-center text-white shadow-lg border-t border-red-300">
            <AlertCircle size={20} className="mr-2" />
            <span className="text-base font-medium drop-shadow">
        The server is temporarily unavailable. Some features may not work
        properly.
      </span>
            <a
                href="https://wa.me/545799459459645794657"
                target="_blank"
                rel="noopener noreferrer"
                className="relative cursor-pointer font-medium underline decoration-white underline-offset-4 transition hover:text-white/80"
            >
                Contact Support
            </a>
        </div>
    );
};
