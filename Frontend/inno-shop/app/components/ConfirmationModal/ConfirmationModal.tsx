import { Modal, Text, Button, Center } from "@mantine/core";

interface ConfirmationModalProps {
    opened: boolean;
    onClose: () => void;
    email: string;
}

export function ConfirmationModal({ opened, onClose, email }: ConfirmationModalProps) {
    return (
        <Modal
            opened={opened}
            onClose={onClose}
            title="Account Registration Successful"
            centered
            withCloseButton={false}
        >
            <Text size="sm" mb="md">
                Thank you for registering! We have sent a confirmation email to <strong>{email}</strong>.
            </Text>
            <Text size="sm" mb="lg">
                Please check your inbox and click the confirmation link to activate your account.
            </Text>
            <Center>
                <Button onClick={onClose}>
                    Got it
                </Button>
            </Center>
        </Modal>
    );
}