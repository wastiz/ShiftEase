import {Badge} from "@/components/ui/shadcn/badge";

export function SeverityBadge({ severity }: { severity: string }) {
    const lower = severity.toLowerCase();
    if (lower === 'critical' || lower === 'high') {
        return (
            <Badge className="bg-red-100 text-red-800 border-red-200">
                {severity}
            </Badge>
        );
    }
    if (lower === 'medium' || lower === 'warning') {
        return (
            <Badge className="bg-amber-100 text-amber-800 border-amber-200">
                {severity}
            </Badge>
        );
    }
    return (
        <Badge className="bg-blue-100 text-blue-800 border-blue-200">
            {severity}
        </Badge>
    );
}