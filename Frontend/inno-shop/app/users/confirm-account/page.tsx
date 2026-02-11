"use client";


import { useEffect, useState } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import { ConfirmAccount, ConfirmAccountRequest } from "@/app/services/UserAuthenticationService";
import { Center, Title, Text, Loader } from "@mantine/core";

export default function AccountConfirmedPage() {
    const router = useRouter();
    const searchParams = useSearchParams();
    const token = searchParams.get("Token");

    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [countdown, setCountdown] = useState(2);

    useEffect(() => {
        const confirmAccount = async () => {
            if (!token) {
                setError("Confirmation token is missing");
                setIsLoading(false);
                return;
            }

            try {
                const request: ConfirmAccountRequest = { token };
                await ConfirmAccount(request);

                setIsLoading(false);
                const timer = setInterval(() => {
                    setCountdown((prev) => {
                        if (prev <= 1) {
                            clearInterval(timer);
                            router.push("/products");
                            return 0;
                        }
                        return prev - 1;
                    });
                }, 1000);

                return () => clearInterval(timer);
            } catch (error: any) {
                console.error("Account confirmation error:", error);
                setError(error.message || "Failed to confirm account");
                setIsLoading(false);
            }
        };

        confirmAccount();
    }, [token, router]);

    return (
        <div style={{
            display: "flex",
            justifyContent: "center",
            alignItems: "center",
            height: "100vh"
        }}>
            <Center style={{ flexDirection: "column", gap: 20 }}>
                {isLoading ? (
                    <>
                        <Loader size="lg" />
                        <Text>Confirming your account...</Text>
                    </>
                ) : error ? (
                    <>
                        <Title order={3} style={{ marginTop: 20 }}>
                            Account Confirmation Failed
                        </Title>
                        <Text>Your account could not be confirmed. Please contact support.</Text>
                    </>
                ) : (
                    <>
                        <Title order={2}>Account Confirmed</Title>
                        <Text size="lg">Your account has been successfully confirmed.</Text>
                        <Text size="sm" c="dimmed">
                            Redirecting to products page in {countdown} second{countdown !== 1 ? 's' : ''}...
                        </Text>
                    </>
                )}
            </Center>
        </div>
    );
}