import { create } from "zustand";
import { persist } from "zustand/middleware";

interface OrgStore {
    currentOrgId: number | null;
    _hasHydrated: boolean;
    setCurrentOrg: (orgId: number) => void;
    clearCurrentOrg: () => void;
}

export const useOrgStore = create<OrgStore>()(
    persist(
        (set) => ({
            currentOrgId: null,
            _hasHydrated: false,
            setCurrentOrg: (orgId) => set({ currentOrgId: orgId }),
            clearCurrentOrg: () => set({ currentOrgId: null }),
        }),
        {
            name: "current-org",
            onRehydrateStorage: () => (state) => {
                if (state) state._hasHydrated = true;
            },
        }
    )
);