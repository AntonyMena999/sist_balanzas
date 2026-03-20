import React, { useState, useEffect } from 'react';
import axios from 'axios';
import { FaTrash, FaEdit, FaPlus, FaUser } from 'react-icons/fa';
import { obtenerToken } from '../services/auth';

const getApiUrl = () => {
  const hostname = window.location.hostname;
  return `http://${hostname}:5000/api`;
};

const Usuarios = () => {
  const [usuarios, setUsuarios] = useState([]);
  const [roles, setRoles] = useState([]);
  const [dominios, setDominios] = useState([]);
  const [loading, setLoading] = useState(true);
  const [showModal, setShowModal] = useState(false);
  const [error, setError] = useState('');

  // Form State
  const [formData, setFormData] = useState({
    email: '',
    nombre: '',
    pais: '',
    estado: 1,
    rolId: '',
    dominioId: ''
  });

  // Configuración de Headers con Token
  const getAuthHeaders = () => {
    const token = obtenerToken();
    return { headers: { Authorization: `Bearer ${token}` } };
  };

  const fetchData = async () => {
    try {
      const [usersRes, rolesRes, dominiosRes] = await Promise.all([
        axios.get(`${getApiUrl()}/usuarios`, getAuthHeaders()),
        axios.get(`${getApiUrl()}/roles`, getAuthHeaders()),
        axios.get(`${getApiUrl()}/dominios`, getAuthHeaders())
      ]);

      setUsuarios(usersRes.data);
      setRoles(rolesRes.data);
      setDominios(dominiosRes.data);
    } catch (err) {
      console.error('Error cargando datos:', err);
      setError('Error al cargar la lista de usuarios');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchData();
  }, []);

  const handleDelete = async (id) => {
    if (window.confirm('¿Estás seguro de eliminar este usuario?')) {
      try {
        await axios.delete(`${getApiUrl()}/usuarios/${id}`, getAuthHeaders());
        fetchData();
      } catch (err) {
        console.error('Error eliminando usuario:', err);
        alert('Error al eliminar usuario');
      }
    }
  };

  const handleInputChange = (e) => {
    const { name, value } = e.target;
    setFormData({
      ...formData,
      [name]: name === 'estado' ? parseInt(value) : value
    });
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    try {
      await axios.post(`${getApiUrl()}/usuarios`, formData, getAuthHeaders());
      setShowModal(false);
      setFormData({ email: '', nombre: '', pais: '', estado: 1, rolId: '', dominioId: '' });
      fetchData();
    } catch (err) {
      console.error('Error creando usuario:', err);
      alert('Error al crear usuario');
    }
  };

  // Helpers para mapear IDs a Nombres en la tabla
  const getRolName = (id) => roles.find(r => r.id === id || r._id === id)?.nombre || id;
  const getDominioName = (id) => dominios.find(d => d.id === id || d._id === id)?.descripcion || id;

  return (
    <div style={styles.container}>
      <div style={styles.header}>
        <div style={styles.titleContainer}>
          <FaUser style={styles.titleIcon} />
          <h1 style={styles.title}>Gestión de Usuarios</h1>
        </div>
        <button style={styles.addButton} onClick={() => setShowModal(true)}>
          <FaPlus style={{ marginRight: '8px' }} /> Nuevo Usuario
        </button>
      </div>

      {loading ? (
        <div style={styles.loading}>Cargando usuarios...</div>
      ) : (
        <div style={styles.tableContainer}>
          <table style={styles.table}>
            <thead>
              <tr>
                <th style={styles.th}>EMAIL</th>
                <th style={styles.th}>NOMBRE</th>
                <th style={styles.th}>DOMINIO</th>
                <th style={styles.th}>PAÍS</th>
                <th style={styles.th}>ROL</th>
                <th style={styles.th}>ESTADO</th>
                <th style={styles.th}>ACCIONES</th>
              </tr>
            </thead>
            <tbody>
              {usuarios.map((user) => (
                <tr key={user.id || user._id} style={styles.tr}>
                  <td style={styles.td}>{user.email}</td>
                  <td style={styles.td}>{user.nombre}</td>
                  <td style={styles.td}>{getDominioName(user.dominioId)}</td>
                  <td style={styles.td}>{user.pais}</td>
                  <td style={styles.td}>{getRolName(user.rolId)}</td>
                  <td style={styles.td}>
                    <span style={user.estado === 1 ? styles.badgeActive : styles.badgeInactive}>
                      {user.estado === 1 ? 'Activo' : 'Inactivo'}
                    </span>
                  </td>
                  <td style={styles.td}>
                    <div style={styles.actions}>
                      <button style={styles.editBtn} title="Editar">
                        <FaEdit />
                      </button>
                      <button 
                        style={styles.deleteBtn} 
                        onClick={() => handleDelete(user.id || user._id)}
                        title="Eliminar"
                      >
                        <FaTrash />
                      </button>
                    </div>
                  </td>
                </tr>
              ))}
              {usuarios.length === 0 && (
                <tr>
                  <td colSpan="7" style={{...styles.td, textAlign: 'center'}}>No hay usuarios registrados</td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      )}

      {/* Modal Crear Usuario */}
      {showModal && (
        <div style={styles.modalOverlay}>
          <div style={styles.modal}>
            <div style={styles.modalHeader}>
              <h2 style={styles.modalTitle}>Nuevo Usuario</h2>
              <button style={styles.closeBtn} onClick={() => setShowModal(false)}>×</button>
            </div>
            <form onSubmit={handleSubmit} style={styles.modalBody}>
              <div style={styles.formGroup}>
                <label style={styles.label}>Email Corporativo</label>
                <input
                  type="email"
                  name="email"
                  value={formData.email}
                  onChange={handleInputChange}
                  style={styles.input}
                  required
                  placeholder="usuario@kfc.com.ec"
                />
              </div>
              <div style={styles.formGroup}>
                <label style={styles.label}>Nombre Completo</label>
                <input
                  type="text"
                  name="nombre"
                  value={formData.nombre}
                  onChange={handleInputChange}
                  style={styles.input}
                  required
                />
              </div>
              <div style={styles.row}>
                <div style={styles.col}>
                  <label style={styles.label}>País</label>
                  <select
                    name="pais"
                    value={formData.pais}
                    onChange={handleInputChange}
                    style={styles.select}
                    required
                  >
                    <option value="">Seleccione...</option>
                    <option value="EC">Ecuador</option>
                    <option value="COL">Colombia</option>
                    <option value="CH">Chile</option>
                    <option value="VEN">Venezuela</option>
                    <option value="BR">Brasil</option>
                  </select>
                </div>
                <div style={styles.col}>
                  <label style={styles.label}>Estado</label>
                  <select name="estado" value={formData.estado} onChange={handleInputChange} style={styles.select}>
                    <option value={1}>Activo</option>
                    <option value={0}>Inactivo</option>
                  </select>
                </div>
              </div>
              <div style={styles.formGroup}>
                <label style={styles.label}>Rol</label>
                <select name="rolId" value={formData.rolId} onChange={handleInputChange} style={styles.select} required>
                  <option value="">Seleccione un rol...</option>
                  {roles.map(rol => (
                    <option key={rol.id || rol._id} value={rol.id || rol._id}>{rol.nombre}</option>
                  ))}
                </select>
              </div>
              <div style={styles.formGroup}>
                <label style={styles.label}>Dominio</label>
                <select name="dominioId" value={formData.dominioId} onChange={handleInputChange} style={styles.select} required>
                  <option value="">Seleccione un dominio...</option>
                  {dominios.map(dom => (
                    <option key={dom.id || dom._id} value={dom.id || dom._id}>{dom.descripcion}</option>
                  ))}
                </select>
              </div>
              <div style={styles.modalFooter}>
                <button type="button" style={styles.cancelBtn} onClick={() => setShowModal(false)}>Cancelar</button>
                <button type="submit" style={styles.saveBtn}>Guardar Usuario</button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};

const styles = {
  container: { padding: '0', fontFamily: "'Segoe UI', Roboto, Helvetica, Arial, sans-serif" },
  header: { display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '24px' },
  titleContainer: { display: 'flex', alignItems: 'center', gap: '10px' },
  titleIcon: { fontSize: '24px', color: '#333' },
  title: { fontSize: '24px', fontWeight: 'bold', margin: 0, color: '#1a1a1a' },
  addButton: { backgroundColor: '#E4002B', color: 'white', border: 'none', borderRadius: '6px', padding: '10px 20px', fontSize: '14px', fontWeight: 'bold', cursor: 'pointer', display: 'flex', alignItems: 'center' },
  
  loading: { textAlign: 'center', padding: '40px', color: '#666' },
  
  tableContainer: { backgroundColor: 'white', borderRadius: '8px', boxShadow: '0 2px 4px rgba(0,0,0,0.05)', overflow: 'hidden' },
  table: { width: '100%', borderCollapse: 'collapse' },
  th: { backgroundColor: '#f8f9fa', padding: '16px', textAlign: 'left', fontSize: '12px', fontWeight: 'bold', color: '#666', textTransform: 'uppercase', borderBottom: '1px solid #eee' },
  tr: { borderBottom: '1px solid #eee' },
  td: { padding: '16px', fontSize: '14px', color: '#333' },
  
  badgeActive: { backgroundColor: '#d1e7dd', color: '#0f5132', padding: '4px 8px', borderRadius: '4px', fontSize: '12px', fontWeight: '600' },
  badgeInactive: { backgroundColor: '#f8d7da', color: '#842029', padding: '4px 8px', borderRadius: '4px', fontSize: '12px', fontWeight: '600' },
  
  actions: { display: 'flex', gap: '8px' },
  editBtn: { backgroundColor: '#e2e6ea', color: '#6c757d', border: 'none', borderRadius: '4px', width: '32px', height: '32px', cursor: 'pointer', display: 'flex', alignItems: 'center', justifyContent: 'center' },
  deleteBtn: { backgroundColor: '#ffe5e9', color: '#E4002B', border: 'none', borderRadius: '4px', width: '32px', height: '32px', cursor: 'pointer', display: 'flex', alignItems: 'center', justifyContent: 'center' },

  // Modal Styles
  modalOverlay: { position: 'fixed', top: 0, left: 0, right: 0, bottom: 0, backgroundColor: 'rgba(0,0,0,0.5)', display: 'flex', alignItems: 'center', justifyContent: 'center', zIndex: 1000 },
  modal: { backgroundColor: 'white', borderRadius: '8px', width: '100%', maxWidth: '500px', boxShadow: '0 4px 12px rgba(0,0,0,0.15)' },
  modalHeader: { padding: '20px', borderBottom: '1px solid #eee', display: 'flex', justifyContent: 'space-between', alignItems: 'center' },
  modalTitle: { margin: 0, fontSize: '18px', fontWeight: 'bold' },
  closeBtn: { background: 'none', border: 'none', fontSize: '24px', cursor: 'pointer', color: '#999' },
  modalBody: { padding: '20px' },
  formGroup: { marginBottom: '16px' },
  label: { display: 'block', fontSize: '13px', fontWeight: '600', marginBottom: '8px', color: '#444' },
  input: { width: '100%', padding: '10px', borderRadius: '4px', border: '1px solid #ddd', fontSize: '14px', boxSizing: 'border-box' },
  select: { width: '100%', padding: '10px', borderRadius: '4px', border: '1px solid #ddd', fontSize: '14px', boxSizing: 'border-box', backgroundColor: 'white' },
  
  row: { display: 'flex', gap: '15px', marginBottom: '16px' },
  col: { flex: 1 },
  
  modalFooter: { marginTop: '20px', display: 'flex', justifyContent: 'flex-end', gap: '10px' },
  cancelBtn: { padding: '10px 20px', border: '1px solid #ddd', borderRadius: '4px', backgroundColor: 'white', cursor: 'pointer', fontWeight: '500' },
  saveBtn: { padding: '10px 20px', border: 'none', borderRadius: '4px', backgroundColor: '#E4002B', color: 'white', cursor: 'pointer', fontWeight: 'bold' }
};

export default Usuarios;