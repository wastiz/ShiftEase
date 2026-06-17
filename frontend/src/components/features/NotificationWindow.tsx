import { Bell, Check, CheckCheck, ThumbsUp, ThumbsDown, ExternalLink, AlertTriangle, AlertCircle, Calendar, CalendarCheck } from "lucide-react";
import { cn } from "@/lib/utils";
import { useTranslations } from "next-intl";
import { useRouter } from "next/navigation";
import {
  useNotifications,
  useMarkNotificationAsRead,
  useMarkAllNotificationsAsRead,
} from "@/hooks/api/notification";
import { useApproveVacationRequest, useRejectVacationRequest } from "@/hooks/api/employee/vacations";
import { useApproveSickLeaveRequest, useRejectSickLeaveRequest } from "@/hooks/api/employee/sick-leaves";
import { useApprovePersonalDayRequest, useRejectPersonalDayRequest } from "@/hooks/api/employee/personal-days";
import { NotificationDto } from "@/types/notification";

const NotificationTypes = {
  ScheduleUpdate: "schedule_updated",
  VacationRequest: "vacation_request",
  VacationApproved: "vacation_approved",
  VacationRejected: "vacation_rejected",
  SickLeaveRequest: "sick_leave_request",
  SickLeaveApproved: "sick_leave_approved",
  SickLeaveRejected: "sick_leave_rejected",
  PersonalDayRequest: "personal_day_request",
  PersonalDayApproved: "personal_day_approved",
  PersonalDayRejected: "personal_day_rejected",
  Warning: "warning",
  Error: "error",
} as const;

function isRequestType(type: string) {
  return [
    NotificationTypes.VacationRequest,
    NotificationTypes.SickLeaveRequest,
    NotificationTypes.PersonalDayRequest,
  ].includes(type as any);
}

function getNotificationIcon(type: string) {
  switch (type) {
    case NotificationTypes.VacationApproved:
    case NotificationTypes.SickLeaveApproved:
    case NotificationTypes.PersonalDayApproved:
      return <ThumbsUp className="w-4 h-4 text-green-400" />;
    case NotificationTypes.VacationRejected:
    case NotificationTypes.SickLeaveRejected:
    case NotificationTypes.PersonalDayRejected:
      return <ThumbsDown className="w-4 h-4 text-red-400" />;
    case NotificationTypes.VacationRequest:
    case NotificationTypes.SickLeaveRequest:
    case NotificationTypes.PersonalDayRequest:
      return <CalendarCheck className="w-4 h-4 text-blue-400" />;
    case NotificationTypes.ScheduleUpdate:
      return <Calendar className="w-4 h-4 text-orange-400" />;
    case NotificationTypes.Warning:
      return <AlertTriangle className="w-4 h-4 text-yellow-400" />;
    case NotificationTypes.Error:
      return <AlertCircle className="w-4 h-4 text-red-400" />;
    default:
      return <Bell className="w-4 h-4 text-white/60" />;
  }
}

function getNavigationPath(type: string): string | null {
  switch (type) {
    case NotificationTypes.ScheduleUpdate:
      return "/overview";
    case NotificationTypes.VacationApproved:
    case NotificationTypes.VacationRejected:
      return "/vacation";
    case NotificationTypes.SickLeaveApproved:
    case NotificationTypes.SickLeaveRejected:
      return "/sick-leave";
    case NotificationTypes.PersonalDayApproved:
    case NotificationTypes.PersonalDayRejected:
      return "/personal-day";
    case NotificationTypes.VacationRequest:
      return "/dashboard";
    case NotificationTypes.SickLeaveRequest:
      return "/dashboard";
    case NotificationTypes.PersonalDayRequest:
      return "/dashboard";
    default:
      return null;
  }
}

function NotificationActions({
  notification,
  onAction,
  translations,
}: {
  notification: NotificationDto;
  onAction?: () => void;
  translations: { approve: string; reject: string; view: string };
}) {
  const router = useRouter();
  const approveVacation = useApproveVacationRequest();
  const rejectVacation = useRejectVacationRequest();
  const approveSickLeave = useApproveSickLeaveRequest();
  const rejectSickLeave = useRejectSickLeaveRequest();
  const approvePersonalDay = useApprovePersonalDayRequest();
  const rejectPersonalDay = useRejectPersonalDayRequest();

  const parsed = notification.data ? parseInt(notification.data, 10) : NaN;
  const requestId = Number.isNaN(parsed) ? null : parsed;

  const handleApprove = () => {
    if (!requestId) return;
    switch (notification.type) {
      case NotificationTypes.VacationRequest:
        approveVacation.mutate(requestId, { onSuccess: onAction });
        break;
      case NotificationTypes.SickLeaveRequest:
        approveSickLeave.mutate(requestId, { onSuccess: onAction });
        break;
      case NotificationTypes.PersonalDayRequest:
        approvePersonalDay.mutate(requestId, { onSuccess: onAction });
        break;
    }
  };

  const handleReject = () => {
    if (!requestId) return;
    switch (notification.type) {
      case NotificationTypes.VacationRequest:
        rejectVacation.mutate(requestId, { onSuccess: onAction });
        break;
      case NotificationTypes.SickLeaveRequest:
        rejectSickLeave.mutate(requestId, { onSuccess: onAction });
        break;
      case NotificationTypes.PersonalDayRequest:
        rejectPersonalDay.mutate(requestId, { onSuccess: onAction });
        break;
    }
  };

  const isLoading =
    approveVacation.isPending || rejectVacation.isPending ||
    approveSickLeave.isPending || rejectSickLeave.isPending ||
    approvePersonalDay.isPending || rejectPersonalDay.isPending;

  const navPath = getNavigationPath(notification.type);

  if (isRequestType(notification.type)) {
    return (
      <div className="flex items-center gap-1.5 mt-2">
        {requestId != null && (
          <>
            <button
              onClick={handleApprove}
              disabled={isLoading}
              className="flex items-center gap-1 px-2 py-1 text-xs font-medium rounded-md bg-green-500/20 text-green-400 hover:bg-green-500/30 transition-colors disabled:opacity-50"
            >
              <ThumbsUp className="w-3 h-3" />
              {translations.approve}
            </button>
            <button
              onClick={handleReject}
              disabled={isLoading}
              className="flex items-center gap-1 px-2 py-1 text-xs font-medium rounded-md bg-red-500/20 text-red-400 hover:bg-red-500/30 transition-colors disabled:opacity-50"
            >
              <ThumbsDown className="w-3 h-3" />
              {translations.reject}
            </button>
          </>
        )}
        {navPath && (
          <button
            onClick={() => { router.push(navPath); onAction?.(); }}
            className="flex items-center gap-1 px-2 py-1 text-xs font-medium rounded-md bg-white/10 text-white/60 hover:bg-white/20 hover:text-white transition-colors"
          >
            <ExternalLink className="w-3 h-3" />
            {translations.view}
          </button>
        )}
      </div>
    );
  }

  if (navPath) {
    return (
      <div className="mt-2">
        <button
          onClick={() => { router.push(navPath); onAction?.(); }}
          className="flex items-center gap-1 px-2 py-1 text-xs font-medium rounded-md bg-white/10 text-white/60 hover:bg-white/20 hover:text-white transition-colors"
        >
          <ExternalLink className="w-3 h-3" />
          {translations.view}
        </button>
      </div>
    );
  }

  return null;
}

interface NotificationWindowProps {
  className?: string;
  onClose?: () => void;
}

export default function NotificationWindow({
  className,
  onClose,
}: NotificationWindowProps) {
  const t = useTranslations("notifications");
  const { data: notifications, isLoading } = useNotifications();
  const markAsRead = useMarkNotificationAsRead();
  const markAllAsRead = useMarkAllNotificationsAsRead();

  const unreadCount = notifications?.filter((n) => !n.isRead).length ?? 0;

  const actionTranslations = {
    approve: t('approve'),
    reject: t('reject'),
    view: t('view'),
  };

  return (
    <div
      className={cn(
        "w-80 max-h-[28rem] bg-sidebar border border-white/10 rounded-lg shadow-2xl flex flex-col animate-in fade-in-0 zoom-in-95 duration-150",
        className
      )}
    >
      {/* Header */}
      <div className="border-b border-white/10 px-3 py-2.5 flex items-center justify-between shrink-0">
        <div className="flex items-center gap-2">
          <h3 className="text-sm font-semibold text-white">{t("title")}</h3>
          {unreadCount > 0 && (
            <span className="text-[10px] bg-orange-500 text-white rounded-full px-1.5 py-0.5 min-w-[18px] text-center leading-none">
              {unreadCount}
            </span>
          )}
        </div>
        {unreadCount > 0 && (
          <button
            onClick={() => markAllAsRead.mutate()}
            className="p-1 rounded-md hover:bg-white/10 transition-colors text-white/60 hover:text-white"
            title={t("markAllRead")}
          >
            <CheckCheck className="w-3.5 h-3.5" />
          </button>
        )}
      </div>

      {/* Content */}
      <div className="flex-1 overflow-y-auto">
        {isLoading ? (
          <div className="flex items-center justify-center py-8">
            <div className="w-5 h-5 border-2 border-white/20 border-t-orange-400 rounded-full animate-spin" />
          </div>
        ) : !notifications || notifications.length === 0 ? (
          <div className="text-center py-8">
            <div className="w-10 h-10 bg-white/5 rounded-full flex items-center justify-center mx-auto mb-2 border border-white/10">
              <Bell className="w-5 h-5 text-orange-400" />
            </div>
            <p className="text-white/80 text-xs font-medium mb-0.5">
              {t("noNew")}
            </p>
            <p className="text-white/50 text-[10px]">{t("allCaughtUp")}</p>
          </div>
        ) : (
          <div className="flex flex-col">
            {notifications.map((notification) => (
              <div
                key={notification.id}
                className={cn(
                  "px-3 py-2.5 border-b border-white/5 hover:bg-white/5 transition-colors",
                  !notification.isRead && "bg-white/[0.03]"
                )}
              >
                <div className="flex items-start gap-2">
                  <div className="shrink-0 mt-0.5">
                    {getNotificationIcon(notification.type)}
                  </div>
                  <div className="flex-1 min-w-0">
                    {notification.title && (
                      <p className="text-xs font-medium text-white truncate">
                        {notification.title}
                      </p>
                    )}
                    <p className="text-[11px] text-white/70 mt-0.5 line-clamp-2 leading-relaxed">
                      {notification.message}
                    </p>
                    <p className="text-[10px] text-white/40 mt-0.5">
                      {new Date(notification.createdAt).toLocaleString()}
                    </p>
                    <NotificationActions
                      notification={notification}
                      onAction={onClose}
                      translations={actionTranslations}
                    />
                  </div>
                  <div className="shrink-0 flex flex-col items-center gap-1">
                    {!notification.isRead && (
                      <button
                        onClick={() => markAsRead.mutate(notification.id)}
                        className="p-0.5 rounded hover:bg-white/10 transition-colors text-white/40 hover:text-white"
                        title={t("markRead")}
                      >
                        <Check className="w-3 h-3" />
                      </button>
                    )}
                    {!notification.isRead && (
                      <div className="w-1.5 h-1.5 bg-orange-400 rounded-full" />
                    )}
                  </div>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}
