"use client";

export const dynamic = "force-dynamic";

import { useState } from "react";
import { RequestPasswordReset, SetNewPassword, ResetPasswordRequest, SetNewPasswordRequest } from "@/app/services/UserAuthenticationService";
import { useRouter, useSearchParams } from "next/navigation";
import { ForgotPasswordForm } from "@/app/components/ForgotPasswordFormComponent/ForgotPasswordForm";

export default function ResetPasswordPage() {
    const router = useRouter();
    const searchParams = useSearchParams();
    const token = searchParams.get("Token");
    const isTokenMode = !!token;

    const handleRequestReset = async (request: ResetPasswordRequest): Promise<void> => {
        try {
            await RequestPasswordReset(request);
        } catch (error: any) {
            throw error;
        }
    };

    const handleSetNewPassword = async (request: SetNewPasswordRequest): Promise<void> => {
        if (!token) throw new Error("Token is missing");
        
        try {
            await SetNewPassword(request);
            setTimeout(() => {
                router.push("/users/login?authenticationType=login");
            });
        } catch (error: any) {
            throw error;
        }
    };

    return (
        <div style={{ display: "flex", justifyContent: "center", alignContent: "center", height: "100%" }}>
            <ForgotPasswordForm
                isTokenMode={isTokenMode}
                token={token || undefined}
                handleRequestReset={handleRequestReset}
                handleSetNewPassword={handleSetNewPassword}
                loginLink="/users/login?authenticationType=login"
            />
        </div>
    );
}