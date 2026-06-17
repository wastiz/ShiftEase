import Image from "next/image";

type LogoProps = {
    width?: number
    height?: number
    variant?: "small" | "big"
    className?: string
}

export default function Logo({
                                 width = 20,
                                 height = 20,
                                 variant = "small",
                                 className = "",
                             }: LogoProps) {
    return (
        <div className={`flex items-center gap-2 ${className}`}>
            <Image
                src="/images/logo.svg"
                alt="ShiftEase logo"
                width={width}
                height={height}
                className="h-5 w-auto object-contain"
            />
            {variant === "big" && (
                <h1 className="text-xl font-semibold">ShiftEase</h1>
            )}
        </div>
    )
}
