import React, { useContext, useState } from 'react';
import {UserContext, type User} from './UserContext'
import { useMutation } from '@tanstack/react-query';
import { data, useNavigate } from 'react-router-dom';
import { ErrorContext } from './ErrorProvider';
import { API_URL } from './api/apiConfig';

interface LoginProps {
  setUser: (user: User | null) => void;
}

export function Register({setUser} : LoginProps){
    const [name, setName] = useState("");
    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");
    const navigate = useNavigate();
    const setErrorMessage = useContext(ErrorContext);

    const mutation = useMutation({
        mutationFn: registerUser,
        onSuccess: (data) =>{
            setUser(data.user);
            localStorage.setItem("accessToken", data.accessToken);
            navigate("/");
        },
        onError: (error) => setErrorMessage(error.message)
    });
    
    async function registerUser(){
        const response = await fetch(`${API_URL}/auth/register`, {
            method: 'POST',
            headers:{
                "Content-type": "application/json"
            },
            credentials: "include",
            body: JSON.stringify({name, email, password})
        })
        const data = await response.json();

        if(!response.ok){
            throw new Error(data.message)
        }

        return data;
    }

    function handleRegister(e : React.SubmitEvent){
        e.preventDefault();
        mutation.mutate();
    }

    return(<>
        <form onSubmit={handleRegister}>
            <label>Name</label>
            <input 
                type="text" 
                placeholder="Enter your name" 
                value={name} 
                onChange={e => setName(e.target.value)} 
                disabled={mutation.isPending}
                required/>

            <label>Email</label>
            <input 
                type="email" 
                placeholder="Enter your email" 
                value={email} 
                onChange={e => setEmail(e.target.value)} 
                disabled={mutation.isPending}
                required/>

            <label>Password</label>
            <input 
                type="password" 
                placeholder="Enter your password" 
                value={password} 
                onChange={e => setPassword(e.target.value)} 
                disabled={mutation.isPending}
                required/>

            <button type="submit" disabled={mutation.isPending}>Register</button>
        </form>
    </>)
}