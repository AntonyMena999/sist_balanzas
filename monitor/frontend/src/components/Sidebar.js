import React from 'react';
import { NavLink } from 'react-router-dom';
import { 
  FaBalanceScale, 
  FaTachometerAlt, 
  FaUsers, 
  FaGlobe, 
  FaFlag, 
  FaMapMarkerAlt, 
  FaSignOutAlt 
} from 'react-icons/fa';

const Sidebar = ({ usuario, onCerrarSesion }) => {
  const rol = usuario?.rol || '';

  // Función para estilos de NavLink
  const getLinkStyle = ({ isActive }) => {
    return isActive 
      ? { ...styles.link, ...styles.activeLink } 
      : styles.link;
  };

  return (
    <div style={styles.sidebar}>
      {/* Header */}
      <div style={styles.header}>
        <div style={styles.logoIcon}>
          <FaBalanceScale />
        </div>
        <span style={styles.title}>Monitor Balanzas</span>
      </div>

      {/* Menú de Navegación */}
      <div style={styles.menu}>
        
        {/* Sección MONITOR - Visible para todos */}
        <div style={styles.sectionLabel}>MONITOR</div>
        <NavLink to="/" style={getLinkStyle}>
          <FaTachometerAlt style={styles.icon} />
          <span>Dashboard</span>
        </NavLink>

        {/* Secciones Admin */}
        {rol === 'admin' && (
          <>
            <div style={styles.sectionLabel}>SEGURIDAD</div>
            <NavLink to="/usuarios" style={getLinkStyle}>
              <FaUsers style={styles.icon} />
              <span>Usuarios</span>
            </NavLink>
            <NavLink to="/dominios" style={getLinkStyle}>
              <FaGlobe style={styles.icon} />
              <span>Dominios</span>
            </NavLink>

            <div style={styles.sectionLabel}>ESTRUCTURA</div>
            <NavLink to="/paises" style={getLinkStyle}>
              <FaFlag style={styles.icon} />
              <span>Países</span>
            </NavLink>
            <NavLink to="/ubicaciones" style={getLinkStyle}>
              <FaMapMarkerAlt style={styles.icon} />
              <span>Ubicaciones</span>
            </NavLink>
          </>
        )}
      </div>

      {/* Footer Fijo */}
      <div style={styles.footer}>
        <div style={styles.userInfo}>
          <div style={styles.userName}>{usuario?.nombre || 'Usuario'}</div>
          <div style={styles.userEmail}>{usuario?.email}</div>
        </div>
        <button onClick={onCerrarSesion} style={styles.logoutBtn}>
          <FaSignOutAlt style={styles.logoutIcon} />
          Cerrar Sesión
        </button>
      </div>
    </div>
  );
};

const styles = {
  sidebar: {
    width: '220px',
    height: '100vh',
    backgroundColor: '#1a1a1a',
    display: 'flex',
    flexDirection: 'column',
    fontFamily: "'Segoe UI', Roboto, Helvetica, Arial, sans-serif",
    borderRight: '1px solid #2a2a2a',
    position: 'fixed',
    left: 0,
    top: 0,
    zIndex: 1000
  },
  header: {
    height: '70px',
    display: 'flex',
    alignItems: 'center',
    padding: '0 20px',
    gap: '12px',
    borderBottom: '1px solid #333'
  },
  logoIcon: {
    color: '#E4002B',
    fontSize: '24px',
    display: 'flex',
    alignItems: 'center'
  },
  title: {
    color: 'white',
    fontWeight: 'bold',
    fontSize: '16px'
  },
  menu: {
    flex: 1,
    padding: '20px 15px',
    overflowY: 'auto'
  },
  sectionLabel: {
    color: '#666',
    fontSize: '11px',
    fontWeight: 'bold',
    marginBottom: '10px',
    marginTop: '20px',
    paddingLeft: '10px'
  },
  link: {
    display: 'flex',
    alignItems: 'center',
    gap: '12px',
    padding: '12px 15px',
    color: '#aaa',
    textDecoration: 'none',
    fontSize: '14px',
    borderRadius: '8px',
    marginBottom: '5px',
    transition: 'background 0.2s, color 0.2s',
  },
  activeLink: {
    backgroundColor: '#E4002B',
    color: 'white',
    fontWeight: '600'
  },
  icon: { fontSize: '16px' },
  
  footer: {
    padding: '20px',
    backgroundColor: '#151515',
    borderTop: '1px solid #333'
  },
  userInfo: { marginBottom: '15px' },
  userName: { color: 'white', fontSize: '14px', fontWeight: 'bold' },
  userEmail: { color: '#888', fontSize: '12px', whiteSpace: 'nowrap', overflow: 'hidden', textOverflow: 'ellipsis' },
  
  logoutBtn: {
    width: '100%',
    padding: '10px',
    backgroundColor: 'transparent',
    border: '1px solid #E4002B',
    color: '#E4002B',
    borderRadius: '6px',
    cursor: 'pointer',
    display: 'flex',
    alignItems: 'center',
    justifyContent: 'center',
    gap: '8px',
    fontSize: '14px',
    fontWeight: '600',
    transition: 'background 0.2s'
  },
  logoutIcon: { fontSize: '14px' }
};

export default Sidebar;