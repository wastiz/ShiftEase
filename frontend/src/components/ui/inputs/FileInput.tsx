'use client'

import { useRef, useState } from "react"
import { UploadCloud } from "lucide-react"
import { cn } from "@/lib/utils"

export function FileInput() {
    const inputRef = useRef<HTMLInputElement | null>(null)
    const [isDragging, setIsDragging] = useState(false)

    const handleDrop = (e: React.DragEvent<HTMLDivElement>) => {
        e.preventDefault()
        setIsDragging(false)
        const file = e.dataTransfer.files[0]
        if (file && file.type === "text/csv") {
            console.log("CSV File dropped:", file)
        }
    }

    return (
        <div
            className={cn(
                "w-full h-full border-2 border-dashed rounded-md flex flex-col items-center justify-center text-center p-6 transition",
                isDragging ? "border-primary bg-primary/10" : "border-gray-300"
            )}
            onDragOver={(e) => {
                e.preventDefault()
                setIsDragging(true)
            }}
            onDragLeave={() => setIsDragging(false)}
            onDrop={handleDrop}
            onClick={() => inputRef.current?.click()}
        >
            <UploadCloud className="w-10 h-10 text-primary mb-4" />
            <p className="text-sm font-medium text-gray-700">
                Drop your file here or <span className="underline text-primary">browse</span>
            </p>
            <p className="text-xs text-muted-foreground mt-2">
                Left space: 199.99 MB.
                <br />
                If you don’t have enough space, delete the old files below.
            </p>

            <input
                ref={inputRef}
                type="file"
                accept=".csv"
                className="hidden"
                onChange={(e) => {
                    const file = e.target.files?.[0]
                    if (file) console.log("Selected file:", file)
                }}
            />
        </div>
    )
}
