"use client";

import {ConfirmationModal} from "@/app/components/ConfirmationModal/ConfirmationModal";

export const dynamic = 'force-dynamic';


import {useState} from "react";
import {
    LoginUser,
    RegisterUser,
    SendConfirmationEmail,
    UserLoginRequest,
    UserRegisterRequest
} from "@/app/services/UserAuthenticationService";
import {useRouter, useSearchParams} from "next/navigation";
import {AuthenticationForm} from "@/app/components/AuthenticationFormComponent/AuthenticationForm";
import { User } from "@/app/models/User";

export default function UserLoginPage() {
    const router = useRouter();
    const searchParams = useSearchParams();
    const authenticationType = searchParams.get("authenticationType");

    const [showConfirmationModal, setShowConfirmationModal] = useState(false);
    const [registeredEmail, setRegisteredEmail] = useState("");

    const defaultValues = {
        nickname: "",
        password: "",
    } as User;

    const [user, setUser] = useState<User>(defaultValues);

    const handleLogin = async (userRequest: UserLoginRequest) => {
        try {
            await LoginUser(userRequest);
        }catch(err: any) {
            if (err.message == "Incorrect login or password") {
                throw new Error(err.message);
            }
        }
        router.push("/products");

        setUser(defaultValues);
    }

    const handleRegister = async (userRequest: UserRegisterRequest) => {
        try {
            await RegisterUser(userRequest);

            try {
                await SendConfirmationEmail({ email: userRequest.email });
            } catch (confirmationError) {
                console.error("Failed to send confirmation email:", confirmationError);
            }

            setRegisteredEmail(userRequest.email);
            setShowConfirmationModal(true);
        } catch (error: any) {
            throw error;
        }
        setUser(defaultValues);
    }


    const handleConfirmationModalClose = () => {
        router.push("/products");
    }

    const handleCancel = () => {
    } //TODO: erase this later


    return (
        <div style={{display: "flex", justifyContent: "center", alignContent: "center", height: '100%'}}>
            <AuthenticationForm
                key={authenticationType}
                handle_login={handleLogin}
                handle_register={handleRegister}
                handle_cancel={handleCancel}
                isLoginState={authenticationType === 'login'}
            />

            <ConfirmationModal
                opened={showConfirmationModal}
                onClose={handleConfirmationModalClose}
                email={registeredEmail}
            />
        </div>
    );
}