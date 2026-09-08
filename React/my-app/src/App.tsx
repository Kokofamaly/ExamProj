import { useState, useContext } from 'react'
import { BrowserRouter, Routes, Route, createContext } from "react-router-dom";
import { UserContext, type User} from './UserContext';
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { Header } from './Header';
import './App.css'
import { Login } from './Login';
import { Register } from './Register';
import { ProtectedRoute } from './ProtectedRoute';
import { WordCardsApp } from './WordCardsApp';


const queryClient = new QueryClient();

function App() {
  const [user, setUser] = useState<User | null>(null);

  return (
    <QueryClientProvider client={queryClient}>
      <UserContext value={user}>
        <BrowserRouter>
          <Routes>
            <Route element={<Header setUser={setUser}/>}>
              <Route path="/login" element={<Login setUser={setUser}/>} />
              <Route path="/register" element={<Register setUser={setUser}/>} />
              <Route element={<ProtectedRoute setUser={setUser}/>}>
                <Route path="/" element={<WordCardsApp />} />
              </Route>
            </Route>
          </Routes>
        </BrowserRouter>
      </UserContext>
    </QueryClientProvider>
  )
}

export default App
