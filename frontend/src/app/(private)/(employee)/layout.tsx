import React from "react";
import EmployeeLayout from "@/components/features/layouts/EmployeeLayout";

export default function RootLayout({
    children,
}: Readonly<{
    children: React.ReactNode;
}>) {
    return <EmployeeLayout>{children}</EmployeeLayout>;
}
