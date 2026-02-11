"use client"

import {
    Box,
    Button,
    Group,
    Title,
    Text,
} from '@mantine/core';
import classes from './HeaderMenu.module.css';
import { useEffect, useMemo, useState} from "react";
import Link from "next/link";
import Cookies from 'js-cookie';
import {usePathname, useRouter} from "next/navigation";
import { checkAuth, getRole, LogoutUser } from '@/app/services/UserAuthenticationService';

interface LinkElement {
    key: string;
    value: string;
}

type MenuProps = {
    mainTab: LinkElement;
    catalogTab: LinkElement;
    loginLink: LinkElement;
    signupLink: LinkElement;
    logoutLink: LinkElement;
    usersTab?: LinkElement;
}


export function HeaderMenu({mainTab, catalogTab, signupLink, loginLink, logoutLink, usersTab}: MenuProps) {


    const router = useRouter();
    const [isAuthenticated, setIsAuthenticated] = useState<boolean>(false);
    const [isAdmin, setIsAdmin] = useState<boolean>(false);
    const pathname = usePathname();

    const active = useMemo(() => {
        if (pathname === mainTab.value) return mainTab.value;
        if (pathname === catalogTab.value || (catalogTab.value !== '/' && pathname.startsWith(catalogTab.value))) return catalogTab.value;
        if (usersTab && (pathname === usersTab.value || pathname.startsWith(usersTab.value + '/'))) return usersTab.value;
        return null;
    }, [pathname, mainTab.value, catalogTab.value, usersTab]);

    useEffect(() => {
        checkAuth().then((isAuth) => setIsAuthenticated(isAuth));
    }, [pathname]);

    useEffect(() => {
        if (!isAuthenticated) {
            setIsAdmin(false);
            return;
        }
        getRole().then((role) => setIsAdmin(role === "Admin"));
    }, [isAuthenticated]);



    const handleLogout = async () => {
        await LogoutUser()
        setIsAuthenticated(false);

        router.push(logoutLink.value);
    }



    return (
        <Box >
            <header className={classes.header}>
                <Group justify="space-between" h="100%">

                    <Group h="100%" gap={5} visibleFrom="sm">
                        <Link className={classes.tabLink}
                              key={mainTab.key}
                              data-active={active === mainTab.value || undefined} href={mainTab.value} style={{color: "white", textDecoration: "none"}}>{mainTab.key}</Link>
                        <Link className={classes.tabLink}
                              key={catalogTab.key}
                              data-active={active === catalogTab.value || undefined} href={catalogTab.value} style={{color: "white", textDecoration: "none"}}>{catalogTab.key}</Link>
                        {isAdmin && usersTab && (
                            <Link className={classes.tabLink}
                                  key={usersTab.key}
                                  data-active={active === usersTab.value || undefined} href={usersTab.value} style={{color: "white", textDecoration: "none"}}>{usersTab.key}</Link>
                        )}
                    </Group>

                    <Group gap="xs" align="baseline">
                        <Title> InnoShop
                        </Title>
                        {isAdmin && <Text size="xs" c="blue" className={classes.adminCaption}>Admin</Text>}
                    </Group>

                    <Group visibleFrom="sm">
                        {isAuthenticated ? (
                            <Button variant="light" onClick={handleLogout}>Log out</Button>
                        ) : (
                            <>
                                <Button variant="default" onClick={() => {
                                    console.log(isAuthenticated);
                                    console.log(Cookies.get("token"));
                                    router.push(loginLink.value);
                                }}>Log in</Button>
                                <Button onClick={() => {
                                    router.push(signupLink.value);
                                }}>Sign up</Button>
                            </>
                        )}
                    </Group>

                </Group>
            </header>
        </Box>
    );
}