import React, { useState, useEffect } from 'react';
import { getBalanzas, createBalanza, updateBalanza, deleteBalanza } from '../services/api';
import AddBalanzaDialog from '../components/AddBalanzaDialog';
import BalanzaCard from '../components/BalanzaCard';

const Dashboard = () => {
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
    handleCloseDialog();
  };

  const handleUpdate = async (id, data) => {
    await updateBalanza(id, data);
    await fetchBalanzas();
    handleCloseDialog();
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

  const handleEdit = (balanza) => {
    setEditingBalanza(balanza);
    setShowDialog(true);
  };

  const handleCloseDialog = () => {
    setShowDialog(false);
    setEditingBalanza(null);
  };

  const okCount = balanzas.filter((b) => b.estado === 'ok').length;
  const errorCount = balanzas.length - okCount;

  return (
    <div style={styles.container}>
      <div style={styles.header}>
        <h1 style={styles.title}>Dashboard</h1>
        <button style={styles.addBtn} onClick={() => setShowDialog(true)} title="Agregar balanza">
          +
        </button>
      </div>

      {balanzas.length > 0 && (
        <div style={styles.stats}>
          Total: <strong>{balanzas.length}</strong> | 
          <span style={{ color: '#4caf50', margin: '0 5px' }}>Conectadas: <strong>{okCount}</strong></span> | 
          <span style={{ color: '#f44336', margin: '0 5px' }}>Desconectadas: <strong>{errorCount}</strong></span>
        </div>
      )}

      <div style={styles.grid}>
        {balanzas.map((balanza) => (
          <BalanzaCard
            key={balanza.id || balanza._id}
            balanza={balanza}
            onDelete={handleDelete}
            onEdit={handleEdit}
          />
        ))}
      </div>

      {showDialog && (
        <AddBalanzaDialog
          onClose={handleCloseDialog}
          onSave={editingBalanza ? handleUpdate : handleSave}
          balanza={editingBalanza}
        />
      )}
    </div>
  );
};

const styles = {
  container: { fontFamily: "'Segoe UI', Roboto, Helvetica, Arial, sans-serif" },
  header: { display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '20px', paddingBottom: '15px', borderBottom: '1px solid #ddd', backgroundColor: 'white', padding: '15px 20px', borderRadius: '8px' },
  title: { margin: 0, fontSize: '24px', fontWeight: 'bold' },
  addBtn: { width: '40px', height: '40px', borderRadius: '50%', border: 'none', backgroundColor: '#E4002B', color: 'white', fontSize: '24px', cursor: 'pointer', display: 'flex', alignItems: 'center', justifyContent: 'center', boxShadow: '0 2px 5px rgba(0,0,0,0.2)' },
  stats: { marginBottom: '20px', fontSize: '14px', color: '#666', backgroundColor: 'white', padding: '10px 20px', borderRadius: '8px', display: 'inline-block' },
  grid: { display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(280px, 1fr))', gap: '20px' },
};

export default Dashboard;