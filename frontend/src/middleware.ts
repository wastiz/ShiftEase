// middleware.ts
import { NextRequest, NextResponse } from "next/server";

const PUBLIC_ROUTES = ["/sign-in", "/reset-password", "/onboarding"];
const EMPLOYER_ROUTES = ["/dashboard", "/employees", "/schedules", "/organizations"];
const EMPLOYEE_ROUTES = ["/my-shifts", "/my-requests", "/profile"];

export async function middleware(req: NextRequest) {
  const { pathname } = req.nextUrl;

  if (pathname === "/") {
    return NextResponse.redirect(new URL("/sign-in", req.url));
  }

  // --- locale ---
  const response = NextResponse.next();
  if (!req.cookies.has("NEXT_LOCALE")) {
    const locale = req.cookies.get("NEXT_LOCALE")?.value || "en";
    response.cookies.set("NEXT_LOCALE", locale, { path: "/", maxAge: 31536000 });
  }

  // --- auth ---
  if (PUBLIC_ROUTES.some(r => pathname.startsWith(r))) {
    return response;
  }

  const meRes = await fetch(`${process.env.INTERNAL_API_URL}/auth/me`, {
    headers: { cookie: req.headers.get("cookie") ?? "" },
  });

  if (!meRes.ok) {
    const loginUrl = new URL("/login", req.url);
    loginUrl.searchParams.set("from", pathname);
    return NextResponse.redirect(loginUrl);
  }

  const user = await meRes.json();
  const role: string = user.role;

  if (EMPLOYEE_ROUTES.some(r => pathname.startsWith(r)) && role !== "Employee") {
    return NextResponse.redirect(new URL("/dashboard", req.url));
  }

  if (EMPLOYER_ROUTES.some(r => pathname.startsWith(r)) && role !== "Employer") {
    return NextResponse.redirect(new URL("/my-shifts", req.url));
  }

  return response;
}

export const config = {
  matcher: ["/((?!_next/static|_next/image|favicon.ico|api|.*\\..*).*)" ],
};