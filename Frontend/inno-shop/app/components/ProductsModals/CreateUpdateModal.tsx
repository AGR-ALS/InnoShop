import { Product } from "@/app/models/Product";
import { ProductImagesRequest, ProductRequest } from "@/app/services/ProductService";
import { Button, Menu, Modal, Stack, Textarea, TextInput, Select, Text, Badge, Group, ActionIcon } from "@mantine/core";
import { useEffect, useState } from "react";
import { DropzoneButton } from "../DropzoneComponent/DropzoneButton";
import { IconX } from "@tabler/icons-react";

interface Props {
    isOpen: boolean;
    values: Product;
    mode: Mode;
    handleCreate: (request: ProductRequest, images: ProductImagesRequest) => void;
    handleUpdate: (id: string, request: ProductRequest, images: ProductImagesRequest) => void;
    handleCancel: () => void;
}

export enum Mode {
    Create,
    Edit,
}
export function CreateUpdateModal({isOpen, values, mode, handleCreate, handleUpdate, handleCancel}: Props) {
    const [name, setName] = useState(values?.name || "");
    const [description, setDescription] = useState(values?.description || "");
    const [price, setPrice] = useState(values?.price || 0);
    const [isAvailable, setIsAvailable] = useState(values?.isAvailable || false);
    const [images, setImages] = useState<File[]>([]);
    const [nameError, setNameError] = useState("");
    const [descriptionError, setDescriptionError] = useState("");
    const [priceError, setPriceError] = useState("");
    const [showErrors, setShowErrors] = useState(false);

    useEffect(() => {
        setName(values.name);
        setDescription(values.description);
        setPrice(values.price);
        setIsAvailable(values.isAvailable);
    }, [values]); //Clears values when modal is closed

    useEffect(() => {
        setShowErrors(false);
        setNameError("");
        setDescriptionError("");
        setPriceError("");
    }, [values, isOpen]);

    const handleAddImage = (file: File) => {
        setImages([...images, file]);
    };

    const handleRemoveImage = (index: number) => {
        setImages(images.filter((_, i) => i !== index));
    };


    const validateForm = () => {
        let isValid = true;

        if (name.trim().length === 0) {
            setNameError("Name is required");
            isValid = false;
        } else if (name.length > 100) {
            setNameError("Name must not exceed 100 characters");
            isValid = false;
        } else {
            setNameError("");
        }

        if (description.length > 500) {
            setDescriptionError("Description must not exceed 500 characters");
            isValid = false;
        } else {
            setDescriptionError("");
        }

        if (price < 0) {
            setPriceError("Price cannot be negative");
            isValid = false;
        } else if (price > 999999999999) {
            setPriceError("Price cannot be that high");
            isValid = false;
        } else {
            setPriceError("");
        }

        setShowErrors(true);
        return isValid;
    };

    const handleOnOk = async () => {
        if (!validateForm()) {
            return;
        }

        const productRequest = {name, description, price, isAvailable};
        let productImageRequest;
        if (images.length > 0) {
            productImageRequest = {productImages: images};
        } else {
            productImageRequest = {productImages: null};
        }

        if (mode == Mode.Create) {
            handleCreate(productRequest, productImageRequest);
        } else {
            handleUpdate(values.id, productRequest, productImageRequest);
        }
        setImages([]);
        setShowErrors(false);
    };

    return (
        <div>
            <Modal opened={isOpen} title={mode === Mode.Create ? "Create Product" : "Edit Product"}
                   onClose={handleCancel}>
                <Stack>
                    <TextInput
                        label="Product Name"
                        value={name}
                        onChange={e => {setName(e.target.value);}}
                        placeholder="Enter product name"
                        error={showErrors ? nameError : false}
                    />
                    <Textarea
                        label="Description"
                        value={description}
                        onChange={e => setDescription(e.target.value)}
                        placeholder="Enter product description"
                        autosize
                        error={showErrors ? descriptionError : false}
                    />
                    <TextInput
                        label="Price"
                        type="number"
                        value={price}
                        onChange={e => setPrice(parseFloat(e.target.value))}
                        placeholder="Enter price"
                        error={showErrors ? priceError : false}
                    />
                    <Select
                        label="Availability"
                        placeholder="Select status"
                        value={isAvailable ? "available" : "not-available"}
                        onChange={(value) => setIsAvailable(value === "available")}
                        data={[
                            { value: "available", label: "Available" },
                            { value: "not-available", label: "Not Available" }
                        ]}
                    />
                    <Button onClick={() => handleOnOk()}>
                        {mode === Mode.Create ? "Create Product" : "Update Product"}
                    </Button>
                    <DropzoneButton onDrop={handleAddImage}></DropzoneButton>
                    {images.length > 0 && (
                        <div>
                            <Text size="sm" fw={500}>Images to upload ({images.length}):</Text>
                            <Group>
                                {images.map((file, index) => (
                                    <Badge key={index} leftSection={
                                        <ActionIcon
                                            size="xs"
                                            color="blue"
                                            radius="xl"
                                            variant="transparent"
                                            onClick={() => handleRemoveImage(index)}
                                        >
                                            <IconX size={10} />
                                        </ActionIcon>
                                    }>
                                        {file.name}
                                    </Badge>
                                ))}
                            </Group>
                        </div>
                    )}
                </Stack>
            </Modal>
        </div>
    );
}
