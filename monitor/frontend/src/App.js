import React from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import Login from './pages/Login';
import Layout from './components/Layout';
import { estaAutenticado } from './services/auth';
import Dashboard from './pages/Dashboard';
const Usuarios = () => <h1>Usuarios</h1>;
const Dominios = () => <h1>Dominios</h1>;
const Paises = () => <h1>Países</h1>;
const Ubicaciones = () => <h1>Ubicaciones</h1>;

const App = () => {
  return (
    <BrowserRouter>
      <Routes>
        <Route 
          path="/login" 
          element={estaAutenticado() ? <Navigate to="/" replace /> : <Login />} 
        />
        
        <Route path="/" element={<Layout><Dashboard /></Layout>} />
        <Route path="/usuarios" element={<Layout><Usuarios /></Layout>} />
        <Route path="/dominios" element={<Layout><Dominios /></Layout>} />
        <Route path="/paises" element={<Layout><Paises /></Layout>} />
        <Route path="/ubicaciones" element={<Layout><Ubicaciones /></Layout>} />
        
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </BrowserRouter>
  );
};

export default App;
