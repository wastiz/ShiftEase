'use client';

import React from 'react';

interface IcoTitleProps {
    icon: React.ComponentType<{ className?: string }>;
    title: string;
    className?: string;
    color?: 'orange' | 'blue' | 'green';
}

export default function IcoTitle({
    icon: Icon,
    title,
    className = '',
    color = 'orange',
}: IcoTitleProps) {
    const bgClass = {
        orange: 'bg-orange-500/15',
        blue: 'bg-blue-500/15',
        green: 'bg-green-500/15',
    }[color];

    const textClass = {
        orange: 'text-orange-400',
        blue: 'text-blue-400',
        green: 'text-green-400',
    }[color];

    return (
        <div className={`flex items-center mb-1 ${className}`}>
            <div
                className={`p-1.5 rounded-md border border-transparent ${bgClass}`}
            >
                <span className={textClass}>
                    <Icon className="w-5 h-5" />
                </span>
            </div>
            <h3 className="text-xl ml-3">{title}</h3>
        </div>
    );
}
