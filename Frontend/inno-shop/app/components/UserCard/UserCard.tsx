"use client";

import { Card, Group, Text, Badge } from "@mantine/core";
import classes from "./UserCard.module.scss";
import type { UserInfo } from "../../models/User";

interface UserCardProps {
    user: UserInfo;
    handleOpenModal: (user: UserInfo) => void;
}

export function UserCard(props: UserCardProps) {
    const { user } = props;
    return (
        <Card radius="md" withBorder padding="xl" onClick={() => props.handleOpenModal(props.user)} className={classes.card}>
            <Group justify="space-between" mb="xs">
                <Text fw={500} fz="lg">
                    {user.name}
                </Text>
                <Badge size="sm" variant="light" color={user.roleName === "Admin" ? "blue" : "gray"}>
                    {user.roleName}
                </Badge>
            </Group>
            <Text fz="sm" c="dimmed" mt="xs">
                {user.email}
            </Text>
            <Group gap="xs" mt="md">
                <Badge color={user.isConfirmed ? "green" : "yellow"} size="sm">
                    {user.isConfirmed ? "Confirmed" : "Unconfirmed"}
                </Badge>
                <Badge color={user.isActive ? "green" : "red"} size="sm">
                    {user.isActive ? "Active" : "Inactive"}
                </Badge>
            </Group>
        </Card>
    );
}
