import { ReactNode } from "react";

type FormFieldProps = {
    children: ReactNode;
    description?: string;
    className?: string;
};

export default function FormField({ children, description, className }: FormFieldProps) {
    return (
        <div className={`flex flex-col gap-2 ${className || ""}`}>
            {children}
            {description && <p className="text-xs text-gray-500">{description}</p>}
        </div>
    );
}

