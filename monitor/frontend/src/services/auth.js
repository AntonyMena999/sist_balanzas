import jwtDecode from 'jwt-decode';

const TOKEN_KEY = 'balanzas_token';

export const guardarToken = (token) => {
  localStorage.setItem(TOKEN_KEY, token);
};

export const obtenerToken = () => {
  return localStorage.getItem(TOKEN_KEY);
};

export const cerrarSesion = () => {
  localStorage.removeItem(TOKEN_KEY);
};

export const obtenerUsuario = () => {
  const token = obtenerToken();
  if (!token) return null;

  try {
    const decoded = jwtDecode(token);
    return {
      userId: decoded.userId,
      email: decoded.email,
      rol: decoded.rol,
      pais: decoded.pais
    };
  } catch (error) {
    return null;
  }
};

export const estaAutenticado = () => {
  const token = obtenerToken();
  if (!token) return false;

  try {
    const decoded = jwtDecode(token);
    const currentTime = Date.now() / 1000;

    if (decoded.exp < currentTime) {
      cerrarSesion();
      return false;
    }
    return true;
  } catch (error) {
    cerrarSesion();
    return false;
  }
};
