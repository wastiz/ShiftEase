type MainProps = {
    className?: string;
    children?: React.ReactNode;
}

export default function Main ({className, children}: MainProps) {
    return (
        <main className={`flex-1 p-4 h-14/15 ${className}`}>
            {children}
        </main>
    )
}
