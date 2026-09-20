import React, { createContext, useState } from "react";
import './ErrorProvider.css'

export const ErrorContext = createContext<React.Dispatch<React.SetStateAction<string>>>(
    () => {}
);

export function ErrorProvider({children}: {children: React.ReactNode}){
    const [errorMessage, setErrorMessage] = useState("");
    
    return(
        <ErrorContext value={setErrorMessage}>
            {children}
            {errorMessage 
            && <ErrorNotification message={errorMessage} setMessage={setErrorMessage}/>}
        </ErrorContext>
    )
}

function ErrorNotification({ message, setMessage }: {message: string, setMessage: React.Dispatch<React.SetStateAction<string>>}){
    return(
        <div className="error notification">
            <div className="notification text">
                <span>Error!</span>
                <span className="error message">{message}</span>
            </div>
            <div className="notification buttons">
                <button onClick={() => setMessage("")}>Close</button>
            </div>
        </div>
    );
}