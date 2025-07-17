import { useAuth } from '../components/assets/AuthContext';

export const useAuthData = () => {
    const { employer } = useAuth();
    return {
      token: employer?.token || null,
      orgId: employer?.orgId || null,
    };
  };

export const useAuthToken = () => {
  const { employer } = useAuth();
  return employer?.token || null;
};