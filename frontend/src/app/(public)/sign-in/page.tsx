"use client"

import { useState } from "react"
import Image from "next/image"
import { AnimatePresence, motion } from "framer-motion"
import { Card } from "@/components/ui/shadcn/card"
import LoginForm from "@/components/features/sign-in/LoginForm";
import RegisterForm from "@/components/features/sign-in/RegisterForm";
import ForgotPasswordForm from "@/components/features/sign-in/ForgotPasswordForm";
import {Tabs, TabsContent, TabsList, TabsTrigger} from "@/components/ui/shadcn/tabs";
import {Mode} from "@/types";
import { useTranslations } from 'next-intl';
import LanguageSwitcher from "@/components/ui/inputs/LanguageSwitcher";

const variants = {
    initial: { x: 100, opacity: 0 },
    animate: { x: 0, opacity: 1 },
    exit: { x: -100, opacity: 0 },
}

export default function SignIn() {
    const [mode, setMode] = useState<Mode>("login")
    const t = useTranslations('auth');
    const tCommon = useTranslations('common');

    return (
        <div className="flex flex-col gap-4 p-6 md:p-10 w-full min-h-screen overflow-y-auto">
            <div className="flex justify-between items-center gap-2">
                <a href="#" className="flex items-center gap-2 font-medium">
                    <Image src="/images/logo.svg" alt="ShiftEase logo" width={20} height={20} />
                    {tCommon('appName')}
                </a>
                <div>
                    <LanguageSwitcher/>
                </div>
            </div>
            <div className="flex flex-1 items-center justify-center py-8">
                <AnimatePresence mode="wait">
                    <motion.div
                        key={mode}
                        variants={variants}
                        initial="initial"
                        animate="animate"
                        exit="exit"
                        transition={{duration: 0.35}}
                        className="w-full max-w-[400px]"
                    >
                        <Tabs defaultValue="employer" className="w-full">
                            <TabsList>
                                <TabsTrigger value="employee">{t('employee')}</TabsTrigger>
                                <TabsTrigger value="employer">{t('employer')}</TabsTrigger>
                            </TabsList>
                            <TabsContent value="employee">
                                <Card className="bg-glassBorder">
                                    {mode === "login" && (
                                        <LoginForm setMode={setMode} role={"Employee"}></LoginForm>
                                    )}

                                    {mode === "register" && (
                                        <div className={"p-4"}>
                                            <p>{t('employeeCannotRegister')}</p>
                                        </div>
                                    )}

                                    {mode === "forgot" && (
                                        <ForgotPasswordForm setMode={setMode} role={"Employee"}></ForgotPasswordForm>
                                    )}
                                </Card>
                            </TabsContent>
                            <TabsContent value="employer">
                                <Card className="bg-glassBorder">
                                    {mode === "login" && (
                                        <LoginForm setMode={setMode} role={"Employer"}></LoginForm>
                                    )}

                                    {mode === "register" && (
                                        <RegisterForm setMode={setMode}></RegisterForm>
                                    )}

                                    {mode === "forgot" && (
                                        <ForgotPasswordForm setMode={setMode} role={"Employer"}></ForgotPasswordForm>
                                    )}
                                </Card>
                            </TabsContent>
                        </Tabs>
                    </motion.div>
                </AnimatePresence>
            </div>
        </div>
    )
}
