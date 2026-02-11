import { checkAuth } from "./UserAuthenticationService";

export interface ProductRequest {
    name: string;
    description: string;
    price: number;
    isAvailable: boolean;
};

export interface ProductImagesRequest{
    productImages: File[] | null;
}

export const GetProducts = async () => {
    const url = "http://localhost:5266/products-api/products";
    const response = await fetch(url);

    if (!response.ok) {
        throw new Error(`HTTP Error: ${response.status}`);
    }
    console.log("Fetched products successfully");
    return response.json();
}

export const CreateProduct = async (product: ProductRequest, handleRedirect: () => void) => {
    const url = "http://localhost:5266/products-api/products";
    async function sendCreateRequest() : Promise<Response> {
        return fetch(url, {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify(product),
       credentials : "include"
 });
    }
    var response = await sendCreateRequest();
     if (response.status === 401) {
        if(await checkAuth()) {
            response =  await sendCreateRequest();
        }
        else {
        handleRedirect();
        }
    }

    const createdProductId = await response.json();
    return createdProductId;
}

export const UploadProductImages = async (productId: string, images: ProductImagesRequest, handleRedirect: () => void) => {

    const url = `http://localhost:5266/products-api/products/${productId}/images`;
    async function sendUpdateImagesRequest() : Promise<Response>
    {
        return fetch(url, {
        method: "PUT",
        body: formData,
        credentials: "include"
    });
    }

    const formData = new FormData();
    
    if (images.productImages) {
        images.productImages.forEach((file) => {
            formData.append("ProductImages", file);
        });
    }

    var response = await sendUpdateImagesRequest();
     if (response.status === 401) {
        if(await checkAuth()) {
            response =  await sendUpdateImagesRequest();
        }
        else {
        handleRedirect();
        }
    }

    if (!response.ok) {
        throw new Error(`HTTP Error: ${response.status}`);
    }
}

export const UpdateProduct = async (id: string, product: ProductRequest, handleRedirect: () => void) => {
    const url = `http://localhost:5266/products-api/products/${id}`;
    async function sendUpdateRequest() : Promise<Response>
    {
return fetch(url, {
        method: "PUT",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify(product),
     credentials : "include"
 });
    }
    var response = await sendUpdateRequest();
    if (response.status === 401) {
        if(await checkAuth()) {
            response =  await sendUpdateRequest();
        }
        else {
        handleRedirect();
        }
    }
}

export const DeleteProduct = async (id: string, handleRedirect: () => void) => {
    const url = `http://localhost:5266/products-api/products/${id}`;
    async function sendDeleteRequest() : Promise<Response>
    {
return fetch(url, {
        method: "DELETE",
     credentials : "include"
 });
    }
    var response = await sendDeleteRequest();
    if (response.status === 401) {
        if(await checkAuth()) {
           response = await sendDeleteRequest();
        }
        else {
        handleRedirect();
        }
    }
}
