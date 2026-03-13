import axios from 'axios';

const getApiUrl = () => {
  const hostname = window.location.hostname;
  return `http://${hostname}:5000/api`;
};

const api = axios.create({
  baseURL: getApiUrl(),
});

export const getBalanzas = () => api.get('/balanzas');
export const createBalanza = (data) => api.post('/balanzas', data);
export const updateBalanza = (id, data) => api.put(`/balanzas/${id}`, data);
export const deleteBalanza = (id) => api.delete(`/balanzas/${id}`);

export default api;
