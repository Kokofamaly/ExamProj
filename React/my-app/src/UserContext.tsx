import {createContext} from 'react';

export interface User{
    email: string,
    name: string
}

export const UserContext = createContext<User | null>(null);