export default function HeaderPlain({ title }: { title: string }) {
    return (
        <header className="w-full h-1/15 flex items-center justify-between shrink-0 border-b px-4 py-4">
            <div className="flex gap-2 items-center">
                <h1 className="text-xl font-bold">{title}</h1>
            </div>
        </header>
    );
}