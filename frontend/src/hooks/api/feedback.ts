import {useMutation} from "@tanstack/react-query";
import api from "@/lib/api";

interface SupportMessagePayload {
    senderEmail: string;
    subject: string;
    message: string;
}

interface FeedbackPayload {
    schedulingMethodBefore: string;
    referralSource: string;
    todaysPurpose: string;
    taskCompleted: string;
    obstacle?: string;
    biggestChallenge: string;
    rating: number;
    ratingReason: string;
    additionalComments?: string;
}

export function useSendSupportMessage() {
    return useMutation({
        mutationFn: async (dto: SupportMessagePayload) => {
            return await api.post("/feedback/support/send-message", dto)
        },
    });
}

export function useSubmitFeedback() {
    return useMutation({
        mutationFn: async (dto: FeedbackPayload) => {
            return await api.post("/feedback/submit", dto)
        }
    });
}