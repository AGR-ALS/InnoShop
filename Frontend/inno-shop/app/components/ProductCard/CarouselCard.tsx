"use client";

import { Carousel } from '@mantine/carousel';
import { Button, Card, Group, Image, Text } from '@mantine/core';
import '@mantine/core/styles.css';
import '@mantine/carousel/styles.css';
import classes from './CarouselCard.module.css';
import type { Product } from '../../models/Product';

interface cardProps {
    product: Product;
    handleOpenModal: (product : Product) => void;
}

export function CarouselCard(props: cardProps) {
    const imagesCount = props.product.productImages?.length || 0;
    const hasImages = imagesCount > 0;
    const showCarouselControls = imagesCount > 1;


    const slides = hasImages
        ? props.product.productImages.map((image, index) => (
            <Carousel.Slide key={image} style={{ width: '100%' }}>
                <Image
                    src={image}
                    height={220}
                    fit="cover"
                    alt={`Slide ${index + 1}`}
                />
            </Carousel.Slide>
        ))
        : [
            <Carousel.Slide key="no-image" style={{ width: '100%' }}>
                <Image
                    src="/no-image-icon.png"
                    height={220}
                    fit="contain"
                    alt="No image available"
                    style={{ backgroundColor: 'var(--mantine-color-gray-1)' }}
                />
            </Carousel.Slide>
        ];

    return (
        <Card radius="md" withBorder padding="xl" style={{ height: '100%', display: 'flex', flexDirection: 'column' }}>
            <Card.Section>
                {hasImages ? (
                    <Carousel
                        withControls = {showCarouselControls}
                        withIndicators = {showCarouselControls}
                        emblaOptions={{ loop: true }}
                        classNames={{
                            root: classes.carousel,
                            controls: classes.carouselControls,
                            indicator: classes.carouselIndicator,
                        }}
                        previousControlProps={{ 'aria-label': 'Previous slide' }}
                        nextControlProps={{ 'aria-label': 'Next slide' }}
                    >
                        {slides}
                    </Carousel>
                ) : (
                    <div style={{ height: 220, display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
                        <Image
                            src="/no-image-icon.png"
                            height={180}
                            fit="contain"
                            alt="No image available"
                        />
                    </div>
                )}
            </Card.Section>
            <div onClick={() => props.handleOpenModal(props.product)} style={{ flex: 1, display: 'flex', flexDirection: 'column' }}>
                <Group justify="space-between" mt="lg" style={{ flexWrap: 'nowrap' }}>
                    <Text fw={500} fz="lg" className={classes.title}>
                        {props.product.name}
                    </Text>

                    <Group gap={5} style={{ flexShrink: 0 }}>
                        <Text fz="sm" fw={600}>
                            {props.product.isAvailable ? 'In Stock' : 'Out of Stock'}
                        </Text>
                    </Group>
                </Group>

                <Text fz="sm" c="dimmed" mt="sm" className={classes.description}>
                    {props.product.description?.trim() ? props.product.description : 'No description provided'}
                </Text>

                <Group justify="space-between" mt="md" style={{ marginTop: 'auto' }}>
                    <div>
                        <Text fz="xl" span fw={500} className={classes.price}>
                            {props.product.price} USD
                        </Text>
                    </div>

                    <Button radius="md" onClick={(e) => {
                        e.stopPropagation();
                    }}>Buy now</Button>
                </Group>
            </div>
        </Card>
    );
}