import { ReactNode } from "react";
import { Card } from "@/components/ui/shadcn/card";
import { Button } from "@/components/ui/shadcn/button";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/shadcn/avatar";
import { Badge } from "@/components/ui/shadcn/badge";

type EmployeeCardAction = {
    label: string;
    onClick: () => void;
    variant?: "default" | "destructive" | "secondary" | "outline" | "ghost";
    icon?: ReactNode;
};

type EmployeeDepartment = {
    id: number;
    name: string;
    color: string;
};

type EmployeeCardProps = {
    firstName: string;
    lastName: string;
    email: string;
    position: string;
    avatar?: string;
    departments?: EmployeeDepartment[];
    actions?: EmployeeCardAction[];
    className?: string;
};

export default function EmployeeCard({
                                         firstName,
                                         lastName,
                                         email,
                                         position,
                                         avatar,
                                         departments = [],
                                         actions = [],
                                         className = "",
                                     }: EmployeeCardProps) {
    const fullName = `${firstName} ${lastName}`;
    const initials = `${firstName[0]}${lastName[0]}`.toUpperCase();

    return (
        <Card className={`overflow-hidden ${className}`}>
            <div className="p-4 flex flex-col gap-3">
                <div className="flex items-center gap-3">
                    <Avatar className="h-12 w-12">
                        <AvatarImage src={avatar} alt={fullName} />
                        <AvatarFallback>{initials}</AvatarFallback>
                    </Avatar>
                    <div className="flex-1 min-w-0">
                        <h3 className="text-lg font-semibold truncate">{fullName}</h3>
                        <p className="text-sm text-muted-foreground truncate">{email}</p>
                    </div>
                </div>

                <p className={"text-sm"}>{position}</p>

                {departments.length > 0 ? (
                    <div className="flex gap-1 flex-wrap">
                        {departments.map((g) => (
                            <Badge
                                key={g.id}
                                style={{
                                    backgroundColor: g.color + "40",
                                    borderColor: g.color
                                }}
                                className="border text-xs text-white"
                            >
                                {g.name}
                            </Badge>
                        ))}
                    </div>
                ) : (
                    <p className="text-xs text-muted-foreground">No departments assigned (may be flexible)</p>
                )}

                {actions.length > 0 && (
                    <div className="flex flex-wrap gap-2">
                        {actions.map(({ label, onClick, variant = "default", icon }, index) => (
                            <Button key={index} onClick={onClick} variant={variant} size="sm">
                                {icon && <span className="mr-2">{icon}</span>}
                                {label}
                            </Button>
                        ))}
                    </div>
                )}
            </div>
        </Card>
    );
}
