import './globals.css';
import type { Metadata } from 'next';
import { Prompt } from 'next/font/google';
import TanstackProvider from '@/lib/TanstackProvider';
import { Toaster } from '@/components/ui/shadcn/sonner';
import {AuthProvider} from "@/lib/AuthContext";
import { ThemeProvider } from "@/lib/ThemeProvider";
import "@fontsource-variable/inter";
import "@fontsource-variable/inter/wght.css";
import "@fontsource-variable/inter/wght-italic.css";
import {NextIntlClientProvider} from 'next-intl';
import {getLocale, getMessages} from 'next-intl/server';

export const metadata: Metadata = {
    title: {
        default: "ShiftEase",
        template: "%s | ShiftEase",
    },
    description: "Open-source shift scheduling software.",
    icons: {
        icon: "/images/logo.svg",
    },
};

const prompt = Prompt({
    subsets: ['latin'],
    weight: ['400', '500', '600', '700'],
    variable: '--font-prompt',
});

export default async function RootLayout({
    children,
}: Readonly<{
    children: React.ReactNode;
}>) {
      const locale = await getLocale();
      const messages = await getMessages();

      return (
          <html lang={locale} className={prompt.variable} suppressHydrationWarning>
              <body className="font-sans bg-bgPrimary text-textPrimary">
                    <TanstackProvider>
                        <AuthProvider>
                            <NextIntlClientProvider messages={messages}>
                                <ThemeProvider>
                                    <Toaster position="bottom-right" />
                                    {children}
                                </ThemeProvider>
                            </NextIntlClientProvider>
                        </AuthProvider>
                    </TanstackProvider>
              </body>
          </html>
      );
}
