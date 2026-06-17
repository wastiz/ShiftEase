import {useMutation, useQuery, useQueryClient} from "@tanstack/react-query";
import api from "@/lib/api";
import {NotificationDto} from "@/types/notification";
import {notificationKeys} from "@/lib/api-keys";

export function useNotifications() {
    return useQuery<NotificationDto[], Error>({
        queryKey: notificationKeys.all,
        queryFn: async () => {
            return await api.get<NotificationDto[]>("/notifications")
        },
    });
}

export function useUnreadNotificationsCount() {
    return useQuery<number, Error>({
        queryKey: notificationKeys.unreadCount(),
        queryFn: async () => {
            return await api.get<number>("/notifications/unread-count")
        },
    });
}


export function useMarkNotificationAsRead() {
    const queryClient = useQueryClient();

    return useMutation<void, Error, number>({
        mutationFn: async (id) => {
            await api.post(`/notifications/${id}/read`);
        },
        onSuccess: () => {
            queryClient.invalidateQueries({
                queryKey: notificationKeys.all,
            });
            queryClient.invalidateQueries({
                queryKey: notificationKeys.unreadCount(),
            });
        },
    });
}

export function useMarkAllNotificationsAsRead() {
    const queryClient = useQueryClient();

    return useMutation<void, Error>({
        mutationFn: async () => {
            await api.post("notifications/read-all");
        },
        onSuccess: () => {
            queryClient.invalidateQueries({
                queryKey: notificationKeys.all,
            });
            queryClient.invalidateQueries({
                queryKey: notificationKeys.unreadCount(),
            });
        },
    });
}
