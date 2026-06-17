"use client";

import { useState } from "react";
import { useTranslations } from "next-intl";
import { Mail } from "lucide-react";
import { toast } from "sonner";
import {
    Dialog,
    DialogContent,
    DialogHeader,
    DialogTitle,
    DialogDescription,
} from "@/components/ui/shadcn/dialog";
import { Button } from "@/components/ui/shadcn/button";
import { Label } from "@/components/ui/shadcn/label";
import { Textarea } from "@/components/ui/shadcn/textarea";
import {
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue,
} from "@/components/ui/shadcn/select";
import { Slider } from "@/components/ui/shadcn/slider";
import { useSubmitFeedback } from "@/hooks/api";

interface Props {
    open: boolean;
    onClose: () => void;
}

const REFERRAL_SOURCES = [
    "search_engine",
    "social_media",
    "friend_colleague",
    "advertisement",
    "other",
];

const TASK_COMPLETED_OPTIONS = [
    "yes_fully",
    "partially",
    "no",
    "not_sure",
];

export default function FeedbackModal({ open, onClose }: Props) {
    const t = useTranslations("feedback");
    const submitFeedback = useSubmitFeedback();

    const [schedulingMethodBefore, setSchedulingMethodBefore] = useState("");
    const [referralSource, setReferralSource] = useState("");
    const [todaysPurpose, setTodaysPurpose] = useState("");
    const [taskCompleted, setTaskCompleted] = useState("");
    const [obstacle, setObstacle] = useState("");
    const [biggestChallenge, setBiggestChallenge] = useState("");
    const [rating, setRating] = useState(7);
    const [ratingReason, setRatingReason] = useState("");
    const [additionalComments, setAdditionalComments] = useState("");

    const handleSubmit = (e: React.FormEvent) => {
        e.preventDefault();
        submitFeedback.mutate(
            {
                schedulingMethodBefore,
                referralSource,
                todaysPurpose,
                taskCompleted,
                obstacle: obstacle || undefined,
                biggestChallenge,
                rating,
                ratingReason,
                additionalComments: additionalComments || undefined,
            },
            {
                onSuccess: () => {
                    toast.success(t("feedbackSent"));
                    setSchedulingMethodBefore("");
                    setReferralSource("");
                    setTodaysPurpose("");
                    setTaskCompleted("");
                    setObstacle("");
                    setBiggestChallenge("");
                    setRating(7);
                    setRatingReason("");
                    setAdditionalComments("");
                    onClose();
                },
                onError: () => toast.error(t("feedbackFailed")),
            }
        );
    };

    return (
        <Dialog open={open} onOpenChange={onClose}>
            <DialogContent className="max-w-lg max-h-[90vh] overflow-y-auto">
                <DialogHeader>
                    <DialogTitle>{t("title")}</DialogTitle>
                    <DialogDescription>{t("desc")}</DialogDescription>
                </DialogHeader>

                <div className="flex items-center gap-2 rounded-md border border-primary/20 bg-primary/5 px-3 py-2 text-sm">
                    <Mail className="h-4 w-4 shrink-0 text-primary" />
                    <span className="text-muted-foreground">
                        {t("orEmail")}{" "}
                        <a
                            href="mailto:valnos04@gmail.com"
                            className="font-medium text-primary underline-offset-4 hover:underline"
                        >
                            valnos04@gmail.com
                        </a>
                    </span>
                </div>

                <form onSubmit={handleSubmit} className="space-y-4">
                    <div className="space-y-1.5">
                        <Label htmlFor="fb-before">{t("schedulingMethodBefore")}</Label>
                        <Textarea
                            id="fb-before"
                            required
                            rows={2}
                            value={schedulingMethodBefore}
                            onChange={(e) => setSchedulingMethodBefore(e.target.value)}
                            placeholder={t("schedulingMethodBeforePlaceholder")}
                        />
                    </div>

                    <div className="space-y-1.5">
                        <Label>{t("referralSource")}</Label>
                        <Select value={referralSource} onValueChange={setReferralSource} required>
                            <SelectTrigger>
                                <SelectValue placeholder={t("selectOption")} />
                            </SelectTrigger>
                            <SelectContent>
                                {REFERRAL_SOURCES.map((s) => (
                                    <SelectItem key={s} value={s}>
                                        {t(`referral_${s}`)}
                                    </SelectItem>
                                ))}
                            </SelectContent>
                        </Select>
                    </div>

                    <div className="space-y-1.5">
                        <Label htmlFor="fb-purpose">{t("todaysPurpose")}</Label>
                        <Textarea
                            id="fb-purpose"
                            required
                            rows={2}
                            value={todaysPurpose}
                            onChange={(e) => setTodaysPurpose(e.target.value)}
                            placeholder={t("todaysPurposePlaceholder")}
                        />
                    </div>

                    <div className="space-y-1.5">
                        <Label>{t("taskCompleted")}</Label>
                        <Select value={taskCompleted} onValueChange={setTaskCompleted} required>
                            <SelectTrigger>
                                <SelectValue placeholder={t("selectOption")} />
                            </SelectTrigger>
                            <SelectContent>
                                {TASK_COMPLETED_OPTIONS.map((o) => (
                                    <SelectItem key={o} value={o}>
                                        {t(`task_${o}`)}
                                    </SelectItem>
                                ))}
                            </SelectContent>
                        </Select>
                    </div>

                    <div className="space-y-1.5">
                        <Label htmlFor="fb-obstacle">
                            {t("obstacle")}{" "}
                            <span className="text-muted-foreground text-xs">({t("optional")})</span>
                        </Label>
                        <Textarea
                            id="fb-obstacle"
                            rows={2}
                            value={obstacle}
                            onChange={(e) => setObstacle(e.target.value)}
                            placeholder={t("obstaclePlaceholder")}
                        />
                    </div>

                    <div className="space-y-1.5">
                        <Label htmlFor="fb-challenge">{t("biggestChallenge")}</Label>
                        <Textarea
                            id="fb-challenge"
                            required
                            rows={2}
                            value={biggestChallenge}
                            onChange={(e) => setBiggestChallenge(e.target.value)}
                            placeholder={t("biggestChallengePlaceholder")}
                        />
                    </div>

                    <div className="space-y-2">
                        <Label>
                            {t("rating")}: <span className="font-semibold text-primary">{rating}/10</span>
                        </Label>
                        <Slider
                            min={1}
                            max={10}
                            step={1}
                            value={[rating]}
                            onValueChange={([v]) => setRating(v)}
                            className="w-full"
                        />
                        <div className="flex justify-between text-xs text-muted-foreground">
                            <span>{t("ratingLow")}</span>
                            <span>{t("ratingHigh")}</span>
                        </div>
                    </div>

                    <div className="space-y-1.5">
                        <Label htmlFor="fb-rating-reason">{t("ratingReason")}</Label>
                        <Textarea
                            id="fb-rating-reason"
                            required
                            rows={2}
                            value={ratingReason}
                            onChange={(e) => setRatingReason(e.target.value)}
                            placeholder={t("ratingReasonPlaceholder")}
                        />
                    </div>

                    <div className="space-y-1.5">
                        <Label htmlFor="fb-comments">
                            {t("additionalComments")}{" "}
                            <span className="text-muted-foreground text-xs">({t("optional")})</span>
                        </Label>
                        <Textarea
                            id="fb-comments"
                            rows={2}
                            value={additionalComments}
                            onChange={(e) => setAdditionalComments(e.target.value)}
                            placeholder={t("additionalCommentsPlaceholder")}
                        />
                    </div>

                    <div className="flex justify-end gap-2 pt-1">
                        <Button type="button" variant="outline" onClick={onClose}>
                            {t("cancel")}
                        </Button>
                        <Button type="submit" disabled={submitFeedback.isPending}>
                            {submitFeedback.isPending ? t("sending") : t("send")}
                        </Button>
                    </div>
                </form>
            </DialogContent>
        </Dialog>
    );
}