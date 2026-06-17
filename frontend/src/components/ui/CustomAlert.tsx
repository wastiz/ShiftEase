import { Alert, AlertDescription, AlertTitle } from "./shadcn/alert"

interface AlertProps {
    variant: string;
    title: string;
    description?: string;
    icon: React.ComponentType<{ className?: string }>;
    children?: React.ReactNode;
}

export default function CustomAlert ({variant, title, description, icon: Icon, children}: AlertProps) {
    return (
        <Alert>
            <Icon />
            <AlertTitle>{title}</AlertTitle>
            <AlertDescription>{description}</AlertDescription>
        </Alert>
    )
}
