export type NotificationDto = {
    id: number;
    type: string;
    title?: string | null;
    message: string;
    data?: string | null;
    isRead: boolean;
    createdAt: string; // ISO string from backend
};
