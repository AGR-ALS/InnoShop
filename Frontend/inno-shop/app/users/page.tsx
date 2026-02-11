"use client";

import { Title } from "@mantine/core";
import { Users as UsersList } from "../components/Users";
import { CreateUser, DeleteUser, GetUsers, UpdateUser, UserUpdateRequest } from "../services/UserService";
import type { UserInfo } from "../models/User";
import { getRole, UserRegisterRequest } from "../services/UserAuthenticationService";
import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { FloatButton } from "antd";
import { PlusCircleOutlined } from "@ant-design/icons";
import { CreateUpdateModal, Mode } from "@/app/components/UsersModals/CreateUpdateModal";
import ReadModal from "@/app/components/UsersModals/ReadModal";

const defaultValues: UserInfo = {
    id: "",
    name: "",
    email: "",
    roleName: "Regular",
    isConfirmed: false,
    isActive: true,
};

export default function UsersPage() {
    const router = useRouter();
    const [isLoading, setIsLoading] = useState(true);
    const [values, setValues] = useState<UserInfo>(defaultValues);
    const [users, setUsers] = useState<UserInfo[]>([]);
    const [isReadModalOpen, setIsReadModalOpen] = useState(false);
    const [isModalFormOpen, setIsModalFormOpen] = useState(false);
    const [mode, setMode] = useState(Mode.Create);

    useEffect(() => {
        const load = async () => {
            const role = await getRole();
            if (role !== "Admin") {
                router.push("/products");
                return;
            }
            try {
                const list = await GetUsers(() => router.push("/users/login"));
                setUsers(list);
            } catch {
                setUsers([]);
            } finally {
                setIsLoading(false);
            }
        };
        load();
    }, [router]);

    const refreshUsers = async () => {
        const list = await GetUsers(() => router.push("/users/login"));
        setUsers(list);
    };

    const handleCreateUser = async (body: UserRegisterRequest) => {
        await CreateUser(body, () => router.push("/users/login"));
        closeFormModal();
        await refreshUsers();
    };

    const handleUpdateUser = async (id: string, body: UserUpdateRequest) => {
        await UpdateUser(id, body, () => router.push("/users/login"));
        closeFormModal();
        closeReadModal();
        await refreshUsers();
    };

    const handleDeleteUser = async (id: string) => {
        await DeleteUser(id, () => router.push("/users/login"));
        closeReadModal();
        await refreshUsers();
    };

    const openReadModal = (user: UserInfo) => {
        setIsReadModalOpen(true);
        setValues(user);
    };

    const closeReadModal = () => {
        setValues(defaultValues);
        setIsModalFormOpen(false);
        setIsReadModalOpen(false);
    };

    const closeFormModal = () => {
        if (mode === Mode.Create) {
            setValues(defaultValues);
        }
        setIsModalFormOpen(false);
    };

    const openEditModal = (user: UserInfo) => {
        setMode(Mode.Edit);
        setIsModalFormOpen(true);
        setValues(user);
    };

    const openModal = () => {
        setMode(Mode.Create);
        setValues(defaultValues);
        setIsModalFormOpen(true);
    };

    return (
        <div>
            <ReadModal
                values={values}
                isOpen={isReadModalOpen}
                handleClose={closeReadModal}
                handleOpenEditModal={openEditModal}
                handleDelete={handleDeleteUser}
            />
            <CreateUpdateModal
                isOpen={isModalFormOpen}
                values={values}
                mode={mode}
                handleCreate={handleCreateUser}
                handleUpdate={handleUpdateUser}
                handleCancel={closeFormModal}
            />
            {isLoading ? (
                <Title>Loading...</Title>
            ) : (
                <UsersList users={users} handleOpenModal={openReadModal} />
            )}
            <FloatButton
                style={{ insetInlineEnd: 50 }}
                type="primary"
                icon={<PlusCircleOutlined />}
                onClick={openModal}
            />
        </div>
    );
}
