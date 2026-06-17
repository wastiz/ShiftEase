export type UserRole = "Employer" | "Employee";

export interface User {
    authenticated: boolean;
    id: number;
    role: UserRole;
    fullName: string;
    email: string;
}

export interface EmployeeMeData extends User {
    departmentIds: number[];
    departmentNames: string[];
    organizationId: number;
    organizationName: string;
    position: string;
}


export interface MeResponse {
    id: number;
    email: string;
    fullName: string;
    role: UserRole;
    phone?: string | null;
}

export interface AuthState {
    user: MeResponse | null;
    isLoading: boolean;
    isAuthenticated: boolean;
}

export type RegisterPayload = {
    firstName: string;
    lastName: string;
    email: string;
    password: string;
};

export interface RegisterResponse {
    success: boolean;
    message: string;
    user: User;
}

//Login
export interface LoginPayload {
    email: string;
    password: string;
}

export interface LoginResponse {
    success: boolean;
    message: string;
    user: User;
}

//Logout
export interface LogoutResponse {
    success: boolean;
    message: string;
}

export type ForgotPasswordPayload = {
    email: string;
};

export type DeleteUserPayload = {
    password: string;
};