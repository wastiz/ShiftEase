export const PUBLIC_ROUTES  = ["/", "/sign-in", "/reset-password", "/onboarding"];
export const EMPLOYER_ROUTES = ["/dashboard", "/account", "/departments", "/employees", "/shift-templates", "/schedules", "/organizations"];
const EMPLOYEE_ROUTES = ["/my-shifts", "/my-requests", "/profile"];

export const isPublicRoute = (path: string) => PUBLIC_ROUTES.some(r => r === "/" ? path === "/" : path.startsWith(r));
export const isEmployerRoute = (path: string) => EMPLOYER_ROUTES.some(r => path.startsWith(r));
export const isEmployeeRoute = (path: string) => EMPLOYEE_ROUTES.some(r => path.startsWith(r));

export const ROLE_HOME = {
    Employer: "/dashboard",
    Employee: "/my-shifts",
} as const;