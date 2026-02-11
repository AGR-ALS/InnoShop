import { UserInfo } from "@/app/models/User";
import { Button, Group, Modal, Stack, Text, Badge } from "@mantine/core";

interface Props {
    isOpen: boolean;
    handleClose: () => void;
    values: UserInfo;
    handleOpenEditModal: (user: UserInfo) => void;
    handleDelete: (id: string) => void;
}

export default function ReadModal(props: Props) {
    return (
        <Modal.Root opened={props.isOpen} onClose={props.handleClose} trapFocus={false}>
            <Modal.Overlay />
            <Modal.Content>
                <Modal.Header
                    style={{
                        position: "relative",
                        padding: "4%",
                        paddingBottom: "2%",
                        borderBottom: "1px solid var(--mantine-color-gray-3)",
                    }}
                >
                    <Stack>
                        <div style={{ display: "flex", gap: "8px" }}>
                            <Group gap="xs">
                                <Button variant="light" onClick={() => props.handleOpenEditModal(props.values)}>
                                    Edit
                                </Button>
                                <Button
                                    variant="light"
                                    onClick={() => props.handleDelete(props.values.id)}
                                    color="red"
                                >
                                    Delete
                                </Button>
                            </Group>
                            <Modal.CloseButton
                                variant="transparent"
                                style={{
                                    position: "absolute",
                                    top: "auto",
                                    right: "4%",
                                }}
                            />
                        </div>
                        <div style={{ marginBottom: "8px" }}>
                            <Modal.Title style={{ whiteSpace: "normal", wordBreak: "break-word" }}>
                                {props.values.name}
                            </Modal.Title>
                        </div>
                    </Stack>
                </Modal.Header>

                <Modal.Body style={{ padding: "4%", paddingTop: "2%" }}>
                    <Stack gap="md">
                        <div>
                            <Text fw={500} size="sm" c="dimmed">
                                Email
                            </Text>
                            <Text style={{ wordBreak: "break-word" }}>{props.values.email}</Text>
                        </div>
                        <div>
                            <Text fw={500} size="sm" c="dimmed">
                                Role
                            </Text>
                            <Badge color={props.values.roleName === "Admin" ? "blue" : "gray"}>
                                {props.values.roleName}
                            </Badge>
                        </div>
                        <div>
                            <Text fw={500} size="sm" c="dimmed">
                                Confirmed
                            </Text>
                            <Badge color={props.values.isConfirmed ? "green" : "yellow"}>
                                {props.values.isConfirmed ? "Yes" : "No"}
                            </Badge>
                        </div>
                        <div>
                            <Text fw={500} size="sm" c="dimmed">
                                Active
                            </Text>
                            <Badge color={props.values.isActive ? "green" : "red"}>
                                {props.values.isActive ? "Yes" : "No"}
                            </Badge>
                        </div>
                    </Stack>
                </Modal.Body>
            </Modal.Content>
        </Modal.Root>
    );
}
