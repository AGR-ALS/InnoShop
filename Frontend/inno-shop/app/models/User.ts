export interface User{
    id:string,
    nickname:string,
    email:string,
    password:string,
}

export interface UserInfo {
    id: string;
    name: string;
    email: string;
    roleName: string;
    isConfirmed: boolean;
    isActive: boolean;
}