import Link from 'next/link';
import {motion} from 'framer-motion';

interface QuickActionCardProps {
    path: string;
    title: string;
    subtitle: string;
    color: string; // tailwind class
    icon: React.ComponentType<{ className?: string }>;
}

export default function QuickActionCard({
    path,
    title,
    subtitle,
    color,
    icon: Icon,
}: QuickActionCardProps) {
    return (
        <Link href={path}>
            <motion.div
                whileHover={{scale: 1.05, boxShadow: "0 10px 20px rgba(0,0,0,0.2)"}}
                transition={{type: "spring", stiffness: 300, damping: 20}}
                className="glass bg-bgSecondary glassBadge p-2 flex gap-4 items-center cursor-pointer rounded-2xl"
            >
                <div className={`p-1.5 bg-orange-500/15 rounded-md border border-orange-500/25`}>
                    <Icon className={color}/>
                </div>
                <div>
                    <h4 className="font-semibold text-base">{title}</h4>
                    <p className="text-textSecondary text-sm">{subtitle}</p>
                </div>
            </motion.div>
        </Link>
    );
}
