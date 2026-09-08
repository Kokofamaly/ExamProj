import { useContext, useEffect } from "react";
import { UserContext, type User } from "./UserContext";
import { Navigate, Outlet } from "react-router-dom";
import { apiFetch } from "./api/apiFetch";
import { useQuery } from "@tanstack/react-query";


export function ProtectedRoute({ setUser } : {
  setUser: (user: User | null) => void;
}){
    const user = useContext(UserContext);

    const authQuery = useQuery({
        queryKey: ["authDefault"],
        queryFn: async () =>{
            const response = await apiFetch("/auth/me");
            let data;
            if(!response.ok){
                data = await response.json();
                throw new Error(data.message);
            }

            data = await response.json();

            return data;
        },
    });

    useEffect(() => {
        if(authQuery.data){
            setUser(authQuery.data);
        }
    }, [authQuery.data]);

    if(authQuery.isPending){
        return <div>Loading...</div>
    }
    
    if(authQuery.isError){
        return <Navigate to="login" replace/>;
    }


    return <Outlet />;
}