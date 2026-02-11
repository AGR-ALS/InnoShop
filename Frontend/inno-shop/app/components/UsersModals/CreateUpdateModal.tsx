"use client";

import { UserInfo } from "@/app/models/User";
import { UserRegisterRequest } from "@/app/services/UserAuthenticationService";
import { UserUpdateRequest } from "@/app/services/UserService";
import { Button, Modal, Stack, TextInput, Select, PasswordInput } from "@mantine/core";
import { useEffect, useState } from "react";

interface Props {
    isOpen: boolean;
    values: UserInfo;
    mode: Mode;
    handleCreate?: (request: UserRegisterRequest) => void | Promise<void>;
    handleUpdate: (id: string, request: UserUpdateRequest) => void;
    handleCancel: () => void;
}

export enum Mode {
    Create,
    Edit,
}

export function CreateUpdateModal({
    isOpen,
    values,
    mode,
    handleCreate,
    handleUpdate,
    handleCancel,
}: Props) {
    const [name, setName] = useState(values?.name ?? "");
    const [email, setEmail] = useState(values?.email ?? "");
    const [password, setPassword] = useState("");
    const [role, setRole] = useState(values?.roleName ?? "Regular");
    const [isConfirmed, setIsConfirmed] = useState(values?.isConfirmed ?? false);
    const [isActive, setIsActive] = useState(values?.isActive ?? true);
    const [nameError, setNameError] = useState(false);
    const [emailError, setEmailError] = useState<string | null>(null);
    const [passwordError, setPasswordError] = useState(false);

    useEffect(() => {
        setName(values.name);
        setEmail(values.email);
        setRole(values.roleName);
        setIsConfirmed(values.isConfirmed);
        setIsActive(values.isActive);
        if (mode === Mode.Create) {
            setPassword("");
        }
    }, [values, mode]);

    const handleSubmit = async () => {
        let hasError = false;
        setEmailError(null);
        if (!name.trim()) {
            setNameError(true);
            hasError = true;
        }
        if (!email.trim()) {
            setEmailError("Email is required");
            hasError = true;
        }
        if (mode === Mode.Create) {
            if (!password || password.length < 1) {
                setPasswordError(true);
                hasError = true;
            }
        }
        if (hasError) return;

        if (mode === Mode.Create && handleCreate) {
            try {
                await Promise.resolve(
                    handleCreate({
                        username: name.trim(),
                        email: email.trim(),
                        password,
                    })
                );
                setPassword("");
            } catch (e) {
                const message = e instanceof Error ? e.message : "Failed to create user";
                setEmailError(message);
            }
            return;
        }

        const request: UserUpdateRequest = {
            name: name.trim(),
            email: email.trim(),
            role,
            isConfirmed,
            isActive,
        };
        handleUpdate(values.id, request);
    };

    return (
        <Modal
            opened={isOpen}
            title={mode === Mode.Create ? "Create User" : "Edit User"}
            onClose={handleCancel}
        >
            <Stack>
                <TextInput
                    label={mode === Mode.Create ? "Username" : "Name"}
                    value={name}
                    onChange={(e) => {
                        setName(e.target.value);
                        setNameError(false);
                    }}
                    placeholder={mode === Mode.Create ? "Enter username" : "Enter name"}
                    error={nameError && (mode === Mode.Create ? "Username is required" : "Name is required")}
                />
                <TextInput
                    label="Email"
                    type="email"
                    value={email}
                    onChange={(e) => {
                        setEmail(e.target.value);
                        setEmailError(null);
                    }}
                    placeholder="Enter email"
                    error={emailError}
                />
                {mode === Mode.Create && (
                    <PasswordInput
                        label="Password"
                        value={password}
                        onChange={(e) => {
                            setPassword(e.target.value);
                            setPasswordError(false);
                        }}
                        placeholder="Enter password"
                        error={passwordError && "Password is required"}
                    />
                )}
                {mode === Mode.Edit && (
                    <>
                        <Select
                            label="Role"
                            value={role}
                            onChange={(v) => v && setRole(v)}
                            data={[
                                { value: "Regular", label: "Regular" },
                                { value: "Admin", label: "Admin" },
                            ]}
                        />
                        <Select
                            label="Confirmed"
                            value={isConfirmed ? "yes" : "no"}
                            onChange={(v) => setIsConfirmed(v === "yes")}
                            data={[
                                { value: "yes", label: "Yes" },
                                { value: "no", label: "No" },
                            ]}
                        />
                        <Select
                            label="Active"
                            value={isActive ? "active" : "inactive"}
                            onChange={(v) => setIsActive(v === "active")}
                            data={[
                                { value: "active", label: "Active" },
                                { value: "inactive", label: "Inactive" },
                            ]}
                        />
                    </>
                )}
                <Button onClick={handleSubmit}>
                    {mode === Mode.Create ? "Create User" : "Update User"}
                </Button>
            </Stack>
        </Modal>
    );
}
