import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import axios from 'axios';
import { FaBalanceScale, FaChartBar, FaExchangeAlt, FaCog, FaLock } from 'react-icons/fa';
import { guardarToken } from '../services/auth';

const getApiUrl = () => {
  const hostname = window.location.hostname;
  return `http://${hostname}:5000/api`;
};

const Login = () => {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const navigate = useNavigate();

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setLoading(true);

    try {
      const response = await axios.post(`${getApiUrl()}/auth/login`, {
        email,
        password
      });

      if (response.status === 200) {
        // Asumimos que el token viene en response.data.token o directamente en response.data
        const token = response.data.token || response.data;
        guardarToken(token);
        navigate('/');
      }
    } catch (err) {
      console.error(err);
      setError('Credenciales incorrectas');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div style={styles.container}>
      {/* Lado Izquierdo */}
      <div style={styles.leftPanel}>
        <div style={styles.logoContainer}>
          <FaBalanceScale />
        </div>
        <h1 style={styles.appTitle}>Balanzas</h1>
        <h2 style={styles.appSubtitle}>MONITOR</h2>

        <div style={styles.featureList}>
          <div style={styles.featureItem}>
            <span style={styles.featureIcon}><FaChartBar /></span>
            <span style={styles.featureText}>Monitoreo en tiempo real de balanzas</span>
          </div>
          <div style={styles.featureItem}>
            <span style={styles.featureIcon}><FaExchangeAlt /></span>
            <span style={styles.featureText}>Integración con SAP</span>
          </div>
          <div style={styles.featureItem}>
            <span style={styles.featureIcon}><FaCog /></span>
            <span style={styles.featureText}>Gestión de plantas y ubicaciones</span>
          </div>
        </div>
      </div>

      {/* Lado Derecho */}
      <div style={styles.rightPanel}>
        <div style={styles.formContainer}>
          <h2 style={styles.welcomeTitle}>Bienvenido</h2>
          <p style={styles.welcomeSubtitle}>Inicia sesión con tu cuenta corporativa</p>

          <form onSubmit={handleSubmit} style={styles.form}>
            <div style={styles.inputGroup}>
              <label style={styles.label}>Email corporativo</label>
              <input
                type="email"
                placeholder="usuario@kfc.com.ec"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                style={styles.input}
                required
              />
            </div>
            
            <div style={styles.inputGroup}>
              <label style={styles.label}>Contraseña de red</label>
              <input
                type="password"
                placeholder="••••••••"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                style={styles.input}
                required
              />
            </div>

            {error && <div style={styles.error}>{error}</div>}

            <button type="submit" style={loading ? styles.buttonDisabled : styles.button} disabled={loading}>
              {loading ? 'Iniciando sesión...' : 'Iniciar Sesión'}
            </button>
          </form>

          <p style={styles.footerText}>
            <FaLock style={{marginRight: '6px'}}/>Autenticación via Active Directory
          </p>
        </div>
      </div>
    </div>
  );
};

const styles = {
  container: {
    display: 'flex',
    height: '100vh',
    width: '100%',
    fontFamily: "'Segoe UI', Roboto, Helvetica, Arial, sans-serif",
  },
  leftPanel: {
    flex: 1,
    backgroundColor: '#1a1a1a',
    display: 'flex',
    flexDirection: 'column',
    justifyContent: 'center',
    padding: '0 80px',
    color: 'white',
  },
  rightPanel: {
    flex: 1,
    backgroundColor: 'white',
    display: 'flex',
    flexDirection: 'column',
    justifyContent: 'center',
    alignItems: 'center',
  },
  logoContainer: {
    width: '60px',
    height: '60px',
    backgroundColor: '#E4002B',
    borderRadius: '12px',
    display: 'flex',
    alignItems: 'center',
    justifyContent: 'center',
    fontSize: '32px',
    marginBottom: '20px',
  },
  appTitle: { fontSize: '36px', fontWeight: 'bold', margin: '0 0 5px 0' },
  appSubtitle: { fontSize: '14px', color: '#888', letterSpacing: '4px', marginBottom: '40px', fontWeight: '500' },
  featureList: { display: 'flex', flexDirection: 'column', gap: '20px' },
  featureItem: { display: 'flex', alignItems: 'center', gap: '15px' },
  featureIcon: { color: '#E4002B', fontSize: '18px', fontWeight: 'bold' },
  featureText: { color: '#aaa', fontSize: '16px' },
  
  formContainer: { width: '100%', maxWidth: '400px', padding: '20px' },
  welcomeTitle: { fontSize: '32px', fontWeight: 'bold', color: '#000', marginBottom: '10px', textAlign: 'center' },
  welcomeSubtitle: { fontSize: '16px', color: '#666', marginBottom: '40px', textAlign: 'center' },
  form: { display: 'flex', flexDirection: 'column', gap: '20px' },
  inputGroup: { display: 'flex', flexDirection: 'column', gap: '8px' },
  label: { fontSize: '14px', fontWeight: '600', color: '#333' },
  input: {
    padding: '12px 16px',
    borderRadius: '6px',
    border: '1px solid #ddd',
    fontSize: '16px',
    outline: 'none',
    transition: 'border-color 0.2s',
  },
  button: {
    backgroundColor: '#E4002B',
    color: 'white',
    border: 'none',
    borderRadius: '6px',
    padding: '14px',
    fontSize: '16px',
    fontWeight: 'bold',
    cursor: 'pointer',
    marginTop: '10px',
  },
  buttonDisabled: {
    backgroundColor: '#f5a3b5',
    color: 'white',
    border: 'none',
    borderRadius: '6px',
    padding: '14px',
    fontSize: '16px',
    fontWeight: 'bold',
    cursor: 'not-allowed',
    marginTop: '10px',
  },
  error: { color: '#E4002B', fontSize: '14px', textAlign: 'center' },
  footerText: { marginTop: '30px', fontSize: '12px', color: '#999', textAlign: 'center' },
};

export default Login;