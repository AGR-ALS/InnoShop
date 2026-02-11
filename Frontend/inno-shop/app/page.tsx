import { Button, Center, Title } from "@mantine/core";
import Image from "next/image";
import Link from "next/link";

export default function Home() {
  return (
    <div
            style={{
                height: "93.5vh",
                verticalAlign: "top",
                width: '100%',
                backgroundSize: 'cover',
                backgroundPosition: 'center',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                backgroundImage: `url("/futuristic-galaxy-border-background-gray-minimal-style.jpg")`,
                marginTop: '0'
            }}
        >
            <Center style={{ flexDirection: 'column', gap: 20}}>
                <Title order={1} size="h1" style={{ color: '#fff', fontWeight: 800 }}>
                    InnoShop
                </Title>
                <Link href="/products" passHref>
                    <Button size="lg" radius="xl">
                        Explore Products
                    </Button>
                </Link>
            </Center>
        </div>
  );
}
