import {getRequestConfig} from 'next-intl/server';
import {cookies, headers} from 'next/headers';

const locales = ['en', 'ru', 'et'];
const defaultLocale = 'en';

export default getRequestConfig(async () => {
  // Try to get locale from cookie first
  const cookieStore = await cookies();
  let locale = cookieStore.get('NEXT_LOCALE')?.value;

  // If no cookie, try to get from Accept-Language header
  if (!locale) {
    const headersList = await headers();
    const acceptLanguage = headersList.get('accept-language');
    if (acceptLanguage) {
      const preferredLocale = acceptLanguage.split(',')[0].split('-')[0];
      locale = locales.includes(preferredLocale) ? preferredLocale : defaultLocale;
    }
  }

  // Fallback to default locale
  if (!locale || !locales.includes(locale)) {
    locale = defaultLocale;
  }

  return {
    locale,
    messages: (await import(`../../messages/${locale}.json`)).default
  };
});
