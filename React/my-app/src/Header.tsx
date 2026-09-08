import { useContext, useEffect } from "react";
import { Outlet, useNavigate } from "react-router-dom";
import { UserContext, type User } from "./UserContext";
import { QueryClient, useMutation, useQueryClient } from "@tanstack/react-query";
import { apiFetch } from "./api/apiFetch";


export function Header({setUser} : {
  setUser: (user: User | null) => void;
}){
    const queryClient = useQueryClient();
    const user = useContext(UserContext);
    const navigate = useNavigate();

    const logoutMutation = useMutation({
        mutationFn: async () => {
            const accessToken = localStorage.getItem("accessToken");
            const response = await fetch("http://localhost:5071/auth/logout", { 
                method: "POST", 
                credentials: "include", 
                headers:{
                Authorization: `Bearer ${accessToken}`
            }});
   
            if(!response.ok && response.status !== 401){
                const data = await response.json();
                throw new Error(data.message);
            }
            return response;
        },
        onSuccess: () => {
            localStorage.removeItem("accessToken");
            setUser(null);
            queryClient.clear();
            navigate("/login", { replace: true });
        },
        onError: (error) => alert(error.message)
    });

    return (<>
        <header>
            <div>
                {user ? <>Добро пожаловать, <span className="user name">{user.name}</span>
                <button onClick={() => logoutMutation.mutate()} disabled={logoutMutation.isPending}>Logout</button></> 
                : (<>
                    <button onClick={() => navigate("/register")}>Register</button>
                    <button onClick={() => navigate("/login")}>Login</button>
                </>)}
            </div>
        </header>
        <main>
            <Outlet />
        </main>
    </>
    )
}