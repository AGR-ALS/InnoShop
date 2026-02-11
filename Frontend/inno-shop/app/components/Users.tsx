"use client";

import { UserCard } from "@/app/components/UserCard/UserCard";
import styles from "./Users.module.css";
import type { UserInfo } from "../models/User";

interface UsersProps {
    users: UserInfo[];
    handleOpenModal: (user: UserInfo) => void;
}

export function Users(props: UsersProps) {
    return (
        <div className={styles.grid}>
            {props.users.map((user) => (
                <div key={user.id} className={styles.item}>
                    <UserCard user={user} handleOpenModal={props.handleOpenModal} />
                </div>
            ))}
        </div>
    );
}
