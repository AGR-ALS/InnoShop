"use client";

import { Title } from "@mantine/core";
import { Products } from "../components/Products";
import { CreateProduct, DeleteProduct, GetProducts, ProductImagesRequest, ProductRequest, UpdateProduct, UploadProductImages } from "../services/ProductService";
import type { Product } from "../models/Product";
import {FloatButton} from "antd";
import {PlusCircleOutlined} from '@ant-design/icons';
import {useEffect, useState} from "react";
import {useRouter} from "next/navigation";
import { CreateUpdateModal, Mode } from "@/app/components/ProductsModals/CreateUpdateModal";
import ReadModal from "@/app/components/ProductsModals/ReadModal";


export default function ProductsPage() {
    
    const defaultValues = 
    {
        name: "",
        description: "",
        price: 0,
        isAvailable: false,
        createdAt: "",
        productImages: [] as string[],
    } as Product;
    
    const router = useRouter();
    const [isLoading, setIsLoading] = useState(true);
    const [values, setValues] = useState<Product>(defaultValues);
    const [products, setProducts] = useState<Product[]>([]);
    const [isReadModalOpen, setIsReadModalOpen] = useState(false);
    const [isModalFormOpen, setIsModalFormOpen] = useState(false);
    const [mode, setMode] = useState(Mode.Create);

    useEffect(() => {
        const getProducts = async () => {
            const products = await GetProducts();
            setIsLoading(false);
            setProducts(products);
        };
        getProducts();
    }, []);

    const handleCreateProduct = async (product: ProductRequest, imagesRequest : ProductImagesRequest) => {
    const productId = await CreateProduct(product, () => { router.push("/users/login"); });
    if (imagesRequest.productImages && imagesRequest.productImages.length > 0) {
        await UploadProductImages(productId, imagesRequest, () => { router.push("/users/login"); });
    }
    closeFormModal();
    const Products = await GetProducts();
    setProducts(Products);
    };


    const handleUpdateProduct = async (productId: string, Product: ProductRequest, imagesRequest : ProductImagesRequest) => {
        await UpdateProduct(productId, Product, () => { router.push("/users/login"); });
        if (imagesRequest.productImages && imagesRequest.productImages.length > 0) {
            await UploadProductImages(productId, imagesRequest, () => { router.push("/users/login"); });
    }
        closeFormModal();
        closeReadModal();
        const Products = await GetProducts();
        setProducts(Products);
    }

    const handleDeleteProduct = async (id: string) => {
    await DeleteProduct(id, () => { router.push("/users/login"); });
    closeReadModal();
    const Products = await GetProducts();
    setProducts(Products);
    }

    const openReadModal = (Product: Product) => {
        setIsReadModalOpen(true);
        setValues(Product);
    }

        const closeReadModal = () => {
        setValues(defaultValues);
        setIsModalFormOpen(false);
        setIsReadModalOpen(false);
    }

        const openModal = () => {
        setMode(Mode.Create);
        setIsModalFormOpen(true);
    }
    const closeFormModal = () => {
        if (mode === Mode.Create) {
            setValues(defaultValues);
        }
        setIsModalFormOpen(false);
    }

        const openEditModal = (Product: Product) => {
        setMode(Mode.Edit);
        setIsModalFormOpen(true);
        setValues(Product);
    }

    return (
        <div>
                            <ReadModal values={values} isOpen={isReadModalOpen} handleClose={closeReadModal}
                           handleOpenEditModal={openEditModal} handleDelete={handleDeleteProduct}></ReadModal>
                            <CreateUpdateModal isOpen={isModalFormOpen} values={values} mode={mode}
                                         handleCreate={handleCreateProduct} handleUpdate={handleUpdateProduct}
                                         handleCancel={closeFormModal}/>
            {isLoading ? (<Title>Loading...</Title>) :
                (<Products products={products} handleOpenModal={openReadModal}/>)}
                <FloatButton style={{insetInlineEnd: 50}} type={"primary"} icon={<PlusCircleOutlined/>}
                             onClick={openModal}></FloatButton>
        </div>
    );
}