import { ReactNode } from "react";
import { Card } from "@/components/ui/shadcn/card";
import { Button } from "@/components/ui/shadcn/button";

type InfoCardAction = {
    label: string;
    onClick: () => void;
    variant?: "default" | "destructive" | "secondary" | "outline" | "ghost";
    icon?: ReactNode;
};

type InfoCardProps = {
    title?: string;
    subtitle?: string;
    image?: string;
    text?: string;
    content?: ReactNode;
    actions?: InfoCardAction[];
    style?: React.CSSProperties;
    borderColor?: string;
    className?: string;
};

export default function InfoCard({
                                     title,
                                     subtitle,
                                     image,
                                     text,
                                     content,
                                     actions = [],
                                     style = {},
                                     borderColor,
                                     className = "",
                                 }: InfoCardProps) {
    return (
        <Card
            className={`overflow-hidden ${className}`}
            style={{
                ...style,
                ...(borderColor && {
                    borderColor: `${borderColor}80`,
                    borderWidth: '1px'
                })
            }}
        >
            {image && (
                <div className="w-full h-40 overflow-hidden">
                    <img src={image} alt={title} className="w-full h-full object-cover" />
                </div>
            )}

            <div className="p-4 flex flex-col gap-2">
                {title && <h3 className="text-lg font-semibold">{title}</h3>}
                {subtitle && <p className="text-sm text-muted-foreground">{subtitle}</p>}
                {text && <p className="text-sm">{text}</p>}
                {content && <div>{content}</div>}

                {actions.length > 0 && (
                    <div className="mt-2 flex flex-wrap gap-2">
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
