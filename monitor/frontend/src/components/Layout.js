import React from 'react';
import { Navigate, useNavigate } from 'react-router-dom';
import Sidebar from './Sidebar';
import { estaAutenticado, obtenerUsuario, cerrarSesion } from '../services/auth';

const Layout = ({ children }) => {
  const navigate = useNavigate();

  // Protección de ruta: si no está autenticado, redirigir al login
  if (!estaAutenticado()) {
    return <Navigate to="/login" replace />;
  }

  const usuario = obtenerUsuario();

  const handleCerrarSesion = () => {
    cerrarSesion();
    window.location.href ='/login';
  };

  return (
    <div style={styles.container}>
      <Sidebar usuario={usuario} onCerrarSesion={handleCerrarSesion} />
      <main style={styles.content}>
        {children}
      </main>
    </div>
  );
};

const styles = {
  container: {
    display: 'flex',
    minHeight: '100vh',
    backgroundColor: '#f4f6f8',
  },
  content: {
    marginLeft: '220px',
    padding: '24px',
    width: '100%',
    boxSizing: 'border-box',
  }
};

export default Layout;