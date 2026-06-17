import {motion} from "framer-motion";
import {Check, Headset, Phone, Target, Zap} from "lucide-react";

export default function SignInRightBlock () {
    return (
        <motion.div
            initial={{opacity: 0}}
            animate={{opacity: 1}}
            transition={{duration: 0.6}}
            className="flex flex-col h-full justify-center py-8"
        >
            <div
                className="px-8 xl:px-16 2xl:px-20 flex-1 flex flex-col justify-center max-w-2xl mx-auto w-full">
                <motion.div
                    initial={{opacity: 0, y: -10}}
                    animate={{opacity: 1, y: 0}}
                    transition={{delay: 0.2, duration: 0.5}}
                    className="mb-12"
                >
                    <h1 className="text-3xl xl:text-4xl font-bold text-textPrimary leading-tight mb-4">
                        Welcome back to <span className="text-primary">ShiftEase</span>
                    </h1>
                    <p className="text-lg text-gray-400 leading-relaxed">Continue your journey to better phone
                        deliver ability</p>
                </motion.div>

                <div className="space-y-6 mb-12">
                    {[
                        {
                            icon: Target,
                            title: "99% Accuracy",
                            description: "Industry-leading precision in email validation results",
                            iconBg: "bg-red-50",
                            iconColor: "text-red-600"
                        },
                        {
                            icon: Zap,
                            title: "Instant Processing",
                            description: "Real-time email validation with immediate results",
                            iconBg: "bg-yellow-50",
                            iconColor: "text-yellow-600"
                        }
                    ].map((feature, index) => (
                        <motion.div
                            key={index}
                            initial={{opacity: 0, x: -20}}
                            animate={{opacity: 1, x: 0}}
                            transition={{delay: 0.4 + index * 0.1, duration: 0.5}}
                            className="flex items-start space-x-4 department"
                        >
                            <div
                                className={`flex-shrink-0 w-12 h-12 ${feature.iconBg} rounded-xl flex items-center justify-center shadow-sm border border-textSecondary/20`}>
                                <feature.icon className={`w-6 h-6 ${feature.iconColor}`}/>
                            </div>
                            <div className="flex-1">
                                <h3 className="text-textPrimary font-semibold text-gray-900 mb-1">
                                    {feature.title}
                                </h3>
                                <p className="text-sm text-gray-400 leading-relaxed">
                                    {feature.description}
                                </p>
                            </div>
                        </motion.div>
                    ))}
                </div>

                <motion.div
                    initial={{opacity: 0, y: 20}}
                    animate={{opacity: 1, y: 0}}
                    transition={{delay: 0.8, duration: 0.5}}
                    className="bg-card backdrop-blur-sm rounded-xl shadow-sm border border-textSecondary/50 p-8"
                >
                    <div className="text-center mb-8">
                        <h3 className="text-lg font-semibold text-textPrimary mb-2">
                            Trusted by professionals
                        </h3>
                        <p className="text-sm text-muted-foreground">
                            Join thousands who rely on our platform
                        </p>
                    </div>

                    <div className="grid grid-cols-3 gap-8 text-center">
                        {[
                            {
                                icon: Phone,
                                number: "50M+",
                                label: "Phones Verified",
                                iconBg: "bg-orange-50",
                                iconColor: "text-orange-600"
                            },
                            {
                                icon: Check,
                                number: "99%",
                                label: "Accuracy Rate",
                                iconBg: "bg-red-50",
                                iconColor: "text-red-600"
                            },
                            {
                                icon: Headset,
                                number: "24/7",
                                label: "Support",
                                iconBg: "bg-yellow-50",
                                iconColor: "text-yellow-600"
                            }
                        ].map((stat, index) => (
                            <motion.div
                                key={index}
                                initial={{opacity: 0, y: 20}}
                                animate={{opacity: 1, y: 0}}
                                transition={{delay: 1 + index * 0.1, duration: 0.5}}
                                className="flex flex-col items-center"
                            >
                                <div
                                    className={`w-14 h-14 ${stat.iconBg} rounded-xl flex items-center justify-center mb-3 shadow-sm border border-white`}>
                                    <stat.icon className={`w-7 h-7 ${stat.iconColor}`}/>
                                </div>
                                <div className="text-2xl font-bold text-textPrimary mb-1">
                                    {stat.number}
                                </div>
                                <div className="text-sm text-muted-foreground font-medium">
                                    {stat.label}
                                </div>
                            </motion.div>
                        ))}
                    </div>
                </motion.div>

                <motion.div
                    initial={{opacity: 0}}
                    animate={{opacity: 1}}
                    transition={{delay: 1.4, duration: 0.5}}
                    className="mt-8 text-center"
                >
                    <p className="text-xs text-gray-400">
                        Secure authentication powered by industry standards
                    </p>
                </motion.div>
            </div>
        </motion.div>
    )
}
