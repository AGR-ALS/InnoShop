
import { MantineProvider } from "@mantine/core";
import "@mantine/core/styles.css";
import { Suspense } from "react";
import { HeaderMenu } from "./components/HeaderMenuComponent/HeaderMenu";

const items = [
    {key: "Home", value: "/"},
    {key: "Catalog", value: "/products"},
    {key: "Users", value: "/users"},
    {key: "Register", value: "/users/login?authenticationType=register"},
    {key: "Login", value: "/users/login?authenticationType=login"},
    {key: "Logout", value: "/products"},
];

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="en">
        <body>
<MantineProvider defaultColorScheme="dark">
  <HeaderMenu  mainTab={items[0]} catalogTab={items[1]} usersTab={items[2]} loginLink={items[4]} signupLink={items[3]} logoutLink={items[5]} />
  <main>
     <Suspense>
        {children}
        </Suspense>
        </main>
</MantineProvider>
</body>
        </html>
  );
}
