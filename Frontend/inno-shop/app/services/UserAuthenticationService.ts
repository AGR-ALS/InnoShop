export interface UserLoginRequest {
    email: string;
    password: string;
}

export interface UserRegisterRequest {
    username: string;
    email: string;
    password: string;
}

export const LoginUser = async (user: UserLoginRequest) => {
    const response = await fetch("http://localhost:5266/users-api/users/login", {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
        },
        body: JSON.stringify(user),
        credentials: "include"
    });

    if(response.status === 401 || response.status === 404) {
        throw new Error("Incorrect login or password");
    }

    else if(!response.ok){
        throw new Error("Login error");
    }

}

export const sendRegisterRequest = async (user: UserRegisterRequest): Promise<Response> => {
    return fetch("http://localhost:5266/users-api/users/register", {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
        },
        body: JSON.stringify(user),
        credentials: "include",
    });
};

export const RegisterUser = async (user: UserRegisterRequest) => {
    const registerResponse = await sendRegisterRequest(user);

    if (registerResponse.status === 422) {
        throw new Error("Such email is already registered");
    }

    await LoginUser(user);
}

export const checkAuth = async (): Promise<boolean> => {
    var response = await sendAuthenticationCheckRequest();
    if (!response) {
        try {
            await validateRefershToken();
            response = await sendAuthenticationCheckRequest();
        } catch (error) {
            return false;
        }
    }
    return response;
};


export const sendAuthenticationCheckRequest = async (): Promise<boolean> => {
    try {
        const response = await fetch("http://localhost:5266/users-api/users/isAuth", {
            method: "GET",
            credentials: "include",
        });

        if (!response.ok) {
            return false;
        }

        const result = await response.json();
        return result;

    } catch (error) {
        console.error("Auth check failed", error);
        return false;
    }
};


export const validateRefershToken = async () => {
        const response = await fetch("http://localhost:5266/users-api/users/validate-refresh-token", {
            method: "POST",
            credentials: "include",
        });

        if (response.status === 401)
            throw new Error("Refresh token is invalid or has expired");

};

export const LogoutUser = async () => {
    try {
        const res = await fetch("http://localhost:5266/users-api/users/logout", {
            method: "POST",
            credentials: "include",
        });

        if (!res.ok) {
            console.error("Logout failed", res.statusText);
            return;
        }

    } catch (error) {
        console.error("Logout error", error);
    }
};

export const getRole = async (): Promise<string | null> => {
    try {
        const response = await fetch("http://localhost:5266/users-api/users/role", {
            method: "GET",
            credentials: "include",
        });

        if (!response.ok || response.status === 401) {
            return null;
        }

        const contentType = response.headers.get("content-type");
        if (contentType?.includes("application/json")) {
            const data = await response.json();
            return typeof data === "string" ? data : data?.role ?? null;
        }
        const text = await response.text();
        return text || null;
    } catch (error) {
        console.error("Role check failed", error);
        return null;
    }
};

export interface ResetPasswordRequest {
    email: string;
}

export interface SetNewPasswordRequest {
    token: string;
    newPassword: string;
}

export const RequestPasswordReset = async (request: ResetPasswordRequest): Promise<void> => {
    const response = await fetch("http://localhost:5266/users-api/users/send-reset-password-mail", {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
        },
        body: JSON.stringify(request),
        credentials: "include",
    });

    if (!response.ok) {
        if (response.status === 404) {
            throw new Error("Email not found");
        }
        throw new Error("Failed to send reset password email");
    }
};

export const SetNewPassword = async (request: SetNewPasswordRequest): Promise<void> => {
    const response = await fetch("http://localhost:5266/users-api/users/set-new-password", {
        method: "PUT",
        headers: {
            "Content-Type": "application/json",
        },
        body: JSON.stringify(request),
        credentials: "include",
    });

    if (!response.ok) {
        if (response.status === 400 || response.status === 404) {
            throw new Error("Invalid or expired token");
        }
        throw new Error("Failed to set new password");
    }
};

export interface SendConfirmationRequest {
    email: string;
}

export interface ConfirmAccountRequest {
    token: string;
}

export const SendConfirmationEmail = async (request: SendConfirmationRequest): Promise<void> => {
    const response = await fetch("http://localhost:5266/users-api/users/send-account-confirmation-mail", {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
        },
        body: JSON.stringify(request),
        credentials: "include",
    });

    if (!response.ok) {
        throw new Error("Failed to send confirmation email");
    }
};

export const ConfirmAccount = async (request: ConfirmAccountRequest): Promise<void> => {
    const response = await fetch("http://localhost:5266/users-api/users/confirm-account", {
        method: "PUT",
        headers: {
            "Content-Type": "application/json",
        },
        body: JSON.stringify(request),
        credentials: "include",
    });

    if (!response.ok) {
        if (response.status === 400 || response.status === 404) {
            throw new Error("Invalid or expired confirmation token");
        }
        throw new Error("Failed to confirm account");
    }
};
