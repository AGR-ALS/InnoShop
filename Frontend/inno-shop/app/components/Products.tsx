"use client";

import { CarouselCard } from "@/app/components/ProductCard/CarouselCard";
import styles from "./Products.module.css";
import type { Product } from "../models/Product";

interface productsProps {
  products: Product[];
  handleOpenModal: (product : Product) => void;
}

export function Products(props: productsProps) {
    return (
    <div className={styles.grid}>
        {props.products.map((product) => (
            <div key={product.id} className={styles.item}>
                <CarouselCard product={product} handleOpenModal={props.handleOpenModal}></CarouselCard>
            </div>
        ))}
    </div>
    );
}