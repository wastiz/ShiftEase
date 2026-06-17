'use client';

import React from 'react';
import { JellyTriangle } from 'ldrs/react';
import 'ldrs/react/JellyTriangle.css';

export default function Loader() {
    return (
        <div className="flex min-h-screen items-center justify-center">
            <JellyTriangle size={30} speed={1.75} color="var(--primary)" />
        </div>
    );
}
