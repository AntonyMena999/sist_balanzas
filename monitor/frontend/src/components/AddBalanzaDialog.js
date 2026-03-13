import React, { useState } from 'react';

const styles = {
  overlay: {
    position: 'fixed',
    top: 0,
    left: 0,
    right: 0,
    bottom: 0,
    backgroundColor: 'rgba(0, 0, 0, 0.5)',
    display: 'flex',
    alignItems: 'center',
    justifyContent: 'center',
    zIndex: 1000,
  },
  dialog: {
    backgroundColor: 'white',
    padding: '24px',
    borderRadius: '8px',
    minWidth: '350px',
    boxShadow: '0 4px 20px rgba(0, 0, 0, 0.15)',
  },
  title: {
    fontSize: '20px',
    fontWeight: '600',
    marginBottom: '20px',
    color: '#333',
  },
  field: {
    marginBottom: '16px',
  },
  label: {
    display: 'block',
    marginBottom: '6px',
    fontWeight: '500',
    color: '#555',
  },
  input: {
    width: '100%',
    padding: '10px 12px',
    border: '1px solid #ddd',
    borderRadius: '4px',
    fontSize: '14px',
  },
  buttons: {
    display: 'flex',
    justifyContent: 'flex-end',
    gap: '10px',
    marginTop: '24px',
  },
  btnCancel: {
    padding: '10px 20px',
    border: '1px solid #ddd',
    borderRadius: '4px',
    backgroundColor: 'white',
    cursor: 'pointer',
    fontSize: '14px',
  },
  btnSave: {
    padding: '10px 20px',
    border: 'none',
    borderRadius: '4px',
    backgroundColor: '#2196F3',
    color: 'white',
    cursor: 'pointer',
    fontSize: '14px',
  },
  error: {
    color: '#d32f2f',
    fontSize: '13px',
    marginTop: '8px',
  },
};

function AddBalanzaDialog({ onClose, onSave, balanza }) {
  const isEditing = !!balanza;
  const [nombre, setNombre] = useState(balanza?.nombre || '');
  const [ip, setIp] = useState(balanza?.ip || '');
  const [tiempoWarning, setTiempoWarning] = useState(balanza?.tiempoWarning || 30);
  const [tiempoDanger, setTiempoDanger] = useState(balanza?.tiempoDanger || 60);
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');

    if (!nombre.trim() || !ip.trim()) {
      setError('Nombre e IP son requeridos');
      return;
    }

    if (tiempoWarning <= 0 || tiempoDanger <= 0) {
      setError('Los tiempos deben ser mayores a 0');
      return;
    }

    if (tiempoWarning >= tiempoDanger) {
      setError('Tiempo Warning debe ser menor que Tiempo Danger');
      return;
    }

    setLoading(true);
    try {
      const data = {
        nombre: nombre.trim(),
        ip: ip.trim(),
        tiempoWarning: parseInt(tiempoWarning),
        tiempoDanger: parseInt(tiempoDanger)
      };
      if (isEditing) {
        await onSave(balanza.id, data);
      } else {
        await onSave(data);
      }
      onClose();
    } catch (err) {
      setError(err.response?.data || 'Error al guardar la balanza');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div style={styles.overlay} onClick={onClose}>
      <div style={styles.dialog} onClick={(e) => e.stopPropagation()}>
        <h2 style={styles.title}>{isEditing ? 'Editar Balanza' : 'Agregar Balanza'}</h2>
        <form onSubmit={handleSubmit}>
          <div style={styles.field}>
            <label style={styles.label}>Nombre</label>
            <input
              style={styles.input}
              type="text"
              value={nombre}
              onChange={(e) => setNombre(e.target.value)}
              placeholder="Ej: Fileteado"
              autoFocus
            />
          </div>
          <div style={styles.field}>
            <label style={styles.label}>IP</label>
            <input
              style={styles.input}
              type="text"
              value={ip}
              onChange={(e) => setIp(e.target.value)}
              placeholder="Ej: 172.28.3.250"
            />
          </div>
          <div style={styles.field}>
            <label style={styles.label}>Tiempo Warning (minutos)</label>
            <input
              style={styles.input}
              type="number"
              value={tiempoWarning}
              onChange={(e) => setTiempoWarning(e.target.value)}
              min="1"
              placeholder="30"
            />
          </div>
          <div style={styles.field}>
            <label style={styles.label}>Tiempo Danger (minutos)</label>
            <input
              style={styles.input}
              type="number"
              value={tiempoDanger}
              onChange={(e) => setTiempoDanger(e.target.value)}
              min="1"
              placeholder="60"
            />
          </div>
          {error && <div style={styles.error}>{error}</div>}
          <div style={styles.buttons}>
            <button type="button" style={styles.btnCancel} onClick={onClose}>
              Cancelar
            </button>
            <button type="submit" style={styles.btnSave} disabled={loading}>
              {loading ? 'Guardando...' : 'Guardar'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}

export default AddBalanzaDialog;
