import { create } from "zustand";
import type { MeResponse } from "@/types/auth";

interface AuthStore {
    user: MeResponse | null;
    setUser: (user: MeResponse | null) => void;
    clear: () => void;
}

export const useAuthStore = create<AuthStore>((set) => ({
    user: null,
    setUser: (user) => set({ user }),
    clear: () => set({ user: null }),
}));