import React, { useState, useEffect } from 'react';
import { getBalanzas, createBalanza, updateBalanza, deleteBalanza } from './services/api';
import AddBalanzaDialog from './components/AddBalanzaDialog';
import BalanzaCard from './components/BalanzaCard';

const styles = {
  container: {
    minHeight: '100vh',
    backgroundColor: '#f5f5f5',
  },
  header: {
    backgroundColor: '#1976d2',
    color: 'white',
    padding: '16px 24px',
    display: 'flex',
    justifyContent: 'space-between',
    alignItems: 'center',
    boxShadow: '0 2px 4px rgba(0, 0, 0, 0.1)',
  },
  title: {
    fontSize: '22px',
    fontWeight: '500',
  },
  addBtn: {
    width: '40px',
    height: '40px',
    borderRadius: '50%',
    border: 'none',
    backgroundColor: 'white',
    color: '#1976d2',
    fontSize: '24px',
    cursor: 'pointer',
    display: 'flex',
    alignItems: 'center',
    justifyContent: 'center',
    boxShadow: '0 2px 4px rgba(0, 0, 0, 0.2)',
  },
  body: {
    padding: '24px',
  },
  grid: {
    display: 'grid',
    gridTemplateColumns: 'repeat(auto-fill, minmax(280px, 1fr))',
    gap: '20px',
  },
  empty: {
    textAlign: 'center',
    color: '#666',
    padding: '60px 20px',
    fontSize: '16px',
  },
  stats: {
    marginBottom: '20px',
    color: '#666',
    fontSize: '14px',
  },
};

function App() {
  const [balanzas, setBalanzas] = useState([]);
  const [showDialog, setShowDialog] = useState(false);
  const [editingBalanza, setEditingBalanza] = useState(null);
  const [loading, setLoading] = useState(true);

  const fetchBalanzas = async () => {
    try {
      const response = await getBalanzas();
      setBalanzas(response.data);
    } catch (error) {
      console.error('Error al cargar balanzas:', error);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchBalanzas();
    const interval = setInterval(fetchBalanzas, 3000);
    return () => clearInterval(interval);
  }, []);

  const handleSave = async (data) => {
    await createBalanza(data);
    await fetchBalanzas();
  };

  const handleUpdate = async (id, data) => {
    await updateBalanza(id, data);
    await fetchBalanzas();
  };

  const handleEdit = (balanza) => {
    setEditingBalanza(balanza);
    setShowDialog(true);
  };

  const handleCloseDialog = () => {
    setShowDialog(false);
    setEditingBalanza(null);
  };

  const handleDelete = async (id) => {
    if (window.confirm('¿Eliminar esta balanza?')) {
      try {
        await deleteBalanza(id);
        await fetchBalanzas();
      } catch (error) {
        console.error('Error al eliminar:', error);
      }
    }
  };

  const okCount = balanzas.filter((b) => b.estado === 'ok').length;
  const errorCount = balanzas.length - okCount;

  return (
    <div style={styles.container}>
      <header style={styles.header}>
        <h1 style={styles.title}>Monitor de Balanzas</h1>
        <button style={styles.addBtn} onClick={() => setShowDialog(true)} title="Agregar balanza">
          +
        </button>
      </header>

      <main style={styles.body}>
        {balanzas.length > 0 && (
          <div style={styles.stats}>
            Total: {balanzas.length} |
            <span style={{ color: '#4caf50' }}> Conectadas: {okCount}</span> |
            <span style={{ color: '#f44336' }}> Desconectadas: {errorCount}</span>
          </div>
        )}

        {loading ? (
          <div style={styles.empty}>Cargando...</div>
        ) : balanzas.length === 0 ? (
          <div style={styles.empty}>
            No hay balanzas registradas.<br />
            Usa el botón + para agregar una.
          </div>
        ) : (
          <div style={styles.grid}>
            {balanzas.map((balanza) => (
              <BalanzaCard
                key={balanza.id}
                balanza={balanza}
                onDelete={handleDelete}
                onEdit={handleEdit}
              />
            ))}
          </div>
        )}
      </main>

      {showDialog && (
        <AddBalanzaDialog
          onClose={handleCloseDialog}
          onSave={editingBalanza ? handleUpdate : handleSave}
          balanza={editingBalanza}
        />
      )}
    </div>
  );
}

export default App;
