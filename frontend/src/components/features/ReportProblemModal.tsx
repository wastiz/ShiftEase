"use client";

import { useState } from "react";
import { useTranslations } from "next-intl";
import { Mail } from "lucide-react";
import toast from "react-hot-toast";
import {
    Dialog,
    DialogContent,
    DialogHeader,
    DialogTitle,
    DialogDescription,
} from "@/components/ui/shadcn/dialog";
import { Button } from "@/components/ui/shadcn/button";
import { Input } from "@/components/ui/shadcn/input";
import { Label } from "@/components/ui/shadcn/label";
import { Textarea } from "@/components/ui/shadcn/textarea";
import { useSendSupportMessage } from "@/hooks/api";

interface Props {
    open: boolean;
    onClose: () => void;
}

export default function ReportProblemModal({ open, onClose }: Props) {
    const t = useTranslations("support");
    const sendMessage = useSendSupportMessage();

    const [senderEmail, setSenderEmail] = useState("");
    const [subject, setSubject] = useState("");
    const [message, setMessage] = useState("");

    const handleSubmit = (e: React.FormEvent) => {
        e.preventDefault();
        sendMessage.mutate(
            { senderEmail, subject, message },
            {
                onSuccess: () => {
                    toast.success(t("reportSent"));
                    setSenderEmail("");
                    setSubject("");
                    setMessage("");
                    onClose();
                },
                onError: () => toast.error(t("reportFailed")),
            }
        );
    };

    return (
        <Dialog open={open} onOpenChange={onClose}>
            <DialogContent className="max-w-md">
                <DialogHeader>
                    <DialogTitle>{t("reportTitle")}</DialogTitle>
                    <DialogDescription>{t("reportDesc")}</DialogDescription>
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
                        <Label htmlFor="rp-email">{t("yourEmail")}</Label>
                        <Input
                            id="rp-email"
                            type="email"
                            required
                            value={senderEmail}
                            onChange={(e) => setSenderEmail(e.target.value)}
                            placeholder="you@example.com"
                        />
                    </div>
                    <div className="space-y-1.5">
                        <Label htmlFor="rp-subject">{t("subject")}</Label>
                        <Input
                            id="rp-subject"
                            required
                            value={subject}
                            onChange={(e) => setSubject(e.target.value)}
                            placeholder={t("subjectPlaceholder")}
                        />
                    </div>
                    <div className="space-y-1.5">
                        <Label htmlFor="rp-message">{t("message")}</Label>
                        <Textarea
                            id="rp-message"
                            required
                            rows={4}
                            value={message}
                            onChange={(e) => setMessage(e.target.value)}
                            placeholder={t("messagePlaceholder")}
                        />
                    </div>
                    <div className="flex justify-end gap-2 pt-1">
                        <Button type="button" variant="outline" onClick={onClose}>
                            {t("cancel")}
                        </Button>
                        <Button type="submit" disabled={sendMessage.isPending}>
                            {sendMessage.isPending ? t("sending") : t("send")}
                        </Button>
                    </div>
                </form>
            </DialogContent>
        </Dialog>
    );
}