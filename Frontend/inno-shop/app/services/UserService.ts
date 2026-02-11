import { checkAuth, sendRegisterRequest, UserRegisterRequest } from "./UserAuthenticationService";
import type { UserInfo } from "../models/User";

export interface UserUpdateRequest {
    name: string;
    email: string;
    role: string;
    isConfirmed: boolean;
    isActive: boolean;
}

const baseUrl = "http://localhost:5266/users-api/users";

export const GetUsers = async (handleRedirect?: () => void): Promise<UserInfo[]> => {
    async function sendRequest(): Promise<Response> {
        return fetch(baseUrl, {
            method: "GET",
            credentials: "include",
        });
    }
    let response = await sendRequest();
    if (response.status === 401) {
        if (await checkAuth()) {
            response = await sendRequest();
        } else {
            handleRedirect?.();
            return [];
        }
    }
    if (!response.ok) {
        throw new Error(`HTTP Error: ${response.status}`);
    }
    return response.json();
};

export const UpdateUser = async (id: string, body: UserUpdateRequest, handleRedirect: () => void): Promise<void> => {
    const url = `${baseUrl}/${id}`;
    async function sendRequest(): Promise<Response> {
        return fetch(url, {
            method: "PUT",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(body),
            credentials: "include",
        });
    }
    let response = await sendRequest();
    if (response.status === 401) {
        if (await checkAuth()) {
            response = await sendRequest();
        } else {
            handleRedirect();
            return;
        }
    }
    if (!response.ok) {
        throw new Error(`HTTP Error: ${response.status}`);
    }
};

export const CreateUser = async (body: UserRegisterRequest, handleRedirect: () => void): Promise<void> => {
    let response = await sendRegisterRequest(body);
    if (response.status === 401) {
        if (await checkAuth()) {
            response = await sendRegisterRequest(body);
        } else {
            handleRedirect();
            return;
        }
    }
    if (response.status === 422) {
        throw new Error("Such email is already registered");
    }
    if (!response.ok) {
        throw new Error(`HTTP Error: ${response.status}`);
    }
};

export const DeleteUser = async (id: string, handleRedirect: () => void): Promise<void> => {
    const url = `${baseUrl}/${id}`;
    async function sendRequest(): Promise<Response> {
        return fetch(url, {
            method: "DELETE",
            credentials: "include",
        });
    }
    let response = await sendRequest();
    if (response.status === 401) {
        if (await checkAuth()) {
            response = await sendRequest();
        } else {
            handleRedirect();
            return;
        }
    }
    if (!response.ok) {
        throw new Error(`HTTP Error: ${response.status}`);
    }
};
