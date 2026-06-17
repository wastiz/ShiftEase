import { ApiError } from "@/lib/api";

export function isApiError(err: unknown): err is ApiError {
    return err instanceof ApiError;
}
