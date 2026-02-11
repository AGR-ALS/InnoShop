import {
    Anchor,
    Button,
    Group,
    Paper,
    PaperProps,
    PasswordInput,
    Stack,
    Text,
    TextInput,
} from '@mantine/core';
import { useForm } from '@mantine/form';
import Link from 'next/link';
import { useState } from 'react';
import ErrorNotification from "@/app/components/Notifications/ErrorNotification";
import { ResetPasswordRequest, SetNewPasswordRequest } from "@/app/services/UserAuthenticationService";

export interface ForgotPasswordFormProps extends PaperProps {
    isTokenMode: boolean;
    token?: string;
    handleRequestReset: (request: ResetPasswordRequest) => Promise<void>;
    handleSetNewPassword: (request: SetNewPasswordRequest) => Promise<void>;
    handleCancel?: () => void;
    loginLink: string;
}

export function ForgotPasswordForm({
    isTokenMode,
    token,
    handleRequestReset,
    handleSetNewPassword,
    handleCancel,
    loginLink,
    ...props
}: ForgotPasswordFormProps) {
    const [emailError, setEmailError] = useState(false);
    const [passwordError, setPasswordError] = useState(false);
    const [successMessage, setSuccessMessage] = useState<string | null>(null);
    const [isLoading, setIsLoading] = useState(false);

    const emailForm = useForm({
        initialValues: {
            email: '',
        },
        validate: {
            email: (val) => (/^\S+@\S+$/.test(val) ? null : 'Invalid email'),
        },
    });

    const passwordForm = useForm({
        initialValues: {
            password: '',
            confirmPassword: '',
        },
        validate: {
            password: (val) => (val.length < 6 ? 'Password should include at least 6 characters' : null),
            confirmPassword: (val, values) =>
                val !== values.password ? 'Passwords do not match' : null,
        },
    });

    const onEmailSubmit = async () => {
        setEmailError(false);
        setSuccessMessage(null);
        setIsLoading(true);

        try {
            const request: ResetPasswordRequest = {
                email: emailForm.values.email,
            };
            await handleRequestReset(request);
            setSuccessMessage('Password reset email has been sent. Please check your inbox.');
            emailForm.reset();
        } catch (error: any) {
            if (error?.message === 'Email not found') {
                setEmailError(true);
            } else {
                console.error('Reset password error', error);
                setEmailError(true);
            }
        } finally {
            setIsLoading(false);
        }
    };

    const onPasswordSubmit = async () => {
        if (!token) return;

        setPasswordError(false);
        setSuccessMessage(null);
        setIsLoading(true);

        try {
            const request: SetNewPasswordRequest = {
                token: token,
                newPassword: passwordForm.values.password,
            };
            await handleSetNewPassword(request);
            setSuccessMessage('Password has been reset successfully. You can now log in.');
            passwordForm.reset();
        } catch (error: any) {
            if (error?.message?.includes('Invalid') || error?.message?.includes('expired')) {
                setPasswordError(true);
            } else {
                console.error('Set new password error', error);
                setPasswordError(true);
            }
        } finally {
            setIsLoading(false);
        }
    };

    return (
        <Paper
            radius="md"
            p="lg"
            withBorder={true}
            style={{
                width: '20%',
                justifySelf: 'center',
                alignSelf: 'flex-start',
                minWidth: '370px',
                position: 'relative',
                top: '17vh',
            }}
            {...props}
        >
            {isTokenMode ? (
                <>
                    <Text size="lg" fw={500} mb="md">
                        Set New Password
                    </Text>

                    {passwordError && (
                        <ErrorNotification
                            message="Invalid or expired token. Please request a new password reset."
                            onClose={() => setPasswordError(false)}
                        />
                    )}

                    {successMessage && (
                        <Text c="green" size="sm" mb="md">
                            {successMessage}
                        </Text>
                    )}

                    <form onSubmit={passwordForm.onSubmit(onPasswordSubmit)}>
                        <Stack>
                            <PasswordInput
                                required
                                label="New Password"
                                placeholder="Enter new password"
                                value={passwordForm.values.password}
                                onChange={(event) =>
                                    passwordForm.setFieldValue('password', event.currentTarget.value)
                                }
                                error={passwordForm.errors.password}
                                radius="md"
                            />

                            <PasswordInput
                                required
                                label="Confirm Password"
                                placeholder="Confirm new password"
                                value={passwordForm.values.confirmPassword}
                                onChange={(event) =>
                                    passwordForm.setFieldValue('confirmPassword', event.currentTarget.value)
                                }
                                error={passwordForm.errors.confirmPassword}
                                radius="md"
                            />
                        </Stack>

                        <Group justify="space-between" mt="xl">
                            <Anchor component={Link} href={loginLink} c="dimmed" size="xs">
                                Back to Login
                            </Anchor>
                            <Button type="submit" radius="xl" loading={isLoading}>
                                Reset Password
                            </Button>
                        </Group>
                    </form>
                </>
            ) : (
                <>
                    <Text size="lg" fw={500} mb="md">
                        Reset Password
                    </Text>

                    <Text size="sm" c="dimmed" mb="md">
                        Enter your email address and we'll send you a link to reset your password.
                    </Text>

                    {emailError && (
                        <ErrorNotification
                            message="Email not found or failed to send reset email."
                            onClose={() => setEmailError(false)}
                        />
                    )}

                    {successMessage && (
                        <Text c="green" size="sm" mb="md">
                            {successMessage}
                        </Text>
                    )}

                    <form onSubmit={emailForm.onSubmit(onEmailSubmit)}>
                        <Stack>
                            <TextInput
                                required
                                label="Email"
                                placeholder="your.email@example.com"
                                value={emailForm.values.email}
                                onChange={(event) => emailForm.setFieldValue('email', event.currentTarget.value)}
                                error={emailForm.errors.email}
                                radius="md"
                            />
                        </Stack>

                        <Group justify="space-between" mt="xl">
                            <Anchor component={Link} href={loginLink} c="dimmed" size="xs">
                                Back to Login
                            </Anchor>
                            <Button type="submit" radius="xl" loading={isLoading}>
                                Send Reset Link
                            </Button>
                        </Group>
                    </form>
                </>
            )}
        </Paper>
    );
}