import { Product } from "@/app/models/Product";
import { Button, Group, Modal, Stack, Text } from "@mantine/core";

interface props {
    isOpen: boolean;
    handleClose: () => void;
    values: Product;
    handleOpenEditModal: (Product: Product) => void;
    handleDelete: (id: string) => void;
}


export default function (props: props) {
    return (

        <Modal.Root opened={props.isOpen} onClose={props.handleClose} trapFocus={false}>
            <Modal.Overlay/>
            <Modal.Content>
                <Modal.Header style={{position: "relative", padding:'4%', paddingBottom: '2%', borderBottom:'1px solid var(--mantine-color-gray-3)'}}>
                    <Stack>
                    <div style={{ display: "flex",  gap: "8px" }}>
                        <Group gap={"xs"}>
                            <Button
                                variant="light"
                                onClick={() => props.handleOpenEditModal(props.values)}
                            >
                                Edit
                            </Button>
                            <Button
                                variant="light"
                                onClick={() => props.handleDelete(props.values.id)} 
                                color={"red"}
                            >
                                Delete
                            </Button>
                        </Group>
                        <Modal.CloseButton variant="transparent" style={{
                            position: "absolute",
                            top: "auto",
                            right: "4%",
                        }}/>
                    </div>
                    <div style={{ marginBottom: '8px' }}>
                        <Modal.Title style={{ whiteSpace: "normal", wordBreak: "break-word" }}>
                            {props.values.name}
                        </Modal.Title>
                    </div>
                    </Stack>

                </Modal.Header>

                <Modal.Body style={{padding:'4%', paddingTop: '2%'}}>
                    <Stack gap="md">
                        <div>
                            <Text fw={500} size="sm" c="dimmed">Description</Text>
                            <Text style={{ whiteSpace: "pre-wrap", wordBreak: "break-word" }}>
                                {props.values.description}
                            </Text>
                        </div>
                        <div>
                            <Text fw={500} size="sm" c="dimmed">Price</Text>
                            <Text fw={600}>${props.values.price}</Text>
                        </div>
                        <div>
                            <Text fw={500} size="sm" c="dimmed">Status</Text>
                            <Text fw={600} c={props.values.isAvailable ? "green" : "red"}>
                                {props.values.isAvailable ? "Available" : "Not Available"}
                            </Text>
                        </div>
                    </Stack>
                </Modal.Body>
            </Modal.Content>
        </Modal.Root>

    );
}