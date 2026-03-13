import React from 'react';

const styles = {
  card: {
    padding: '20px',
    borderRadius: '8px',
    color: 'white',
    boxShadow: '0 2px 8px rgba(0, 0, 0, 0.1)',
    transition: 'transform 0.2s',
  },
  success: {
    backgroundColor: '#4caf50',
  },
  danger: {
    backgroundColor: '#f44336',
  },
  nombre: {
    fontSize: '18px',
    fontWeight: '600',
    marginBottom: '8px',
  },
  ip: {
    fontSize: '14px',
    opacity: 0.9,
    marginBottom: '8px',
  },
  medicion: {
    fontSize: '12px',
    opacity: 0.9,
    fontWeight: '500',
  },
  badgeWarning: {
    backgroundColor: '#ffc107',
    color: '#212529',
    fontWeight: 'bold',
    padding: '2px 8px',
    borderRadius: '4px',
    fontSize: '12px',
  },
  badgeDanger: {
    backgroundColor: '#dc3545',
    color: 'white',
    fontWeight: 'bold',
    padding: '2px 8px',
    borderRadius: '4px',
    fontSize: '12px',
  },
  estado: {
    fontSize: '11px',
    textTransform: 'uppercase',
    marginTop: '10px',
    fontWeight: '600',
  },
  actionBtn: {
    background: 'rgba(255,255,255,0.3)',
    border: 'none',
    borderRadius: '50%',
    width: '24px',
    height: '24px',
    cursor: 'pointer',
    color: 'white',
    fontSize: '14px',
  },
  buttonsContainer: {
    position: 'absolute',
    top: '8px',
    right: '8px',
    display: 'flex',
    gap: '6px',
  },
  cardWrapper: {
    position: 'relative',
  },
};

function BalanzaCard({ balanza, onDelete, onEdit }) {
  const isOk = balanza.estado === 'ok';
  const cardStyle = {
    ...styles.card,
    ...(isOk ? styles.success : styles.danger),
  };

  const formatDate = (dateStr) => {
    if (!dateStr) return 'Nunca';
    const date = new Date(dateStr);
    return date.toLocaleString('es-ES', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
      second: '2-digit',
    });
  };

  const getAntiguedadBadgeStyle = (dateStr) => {
    if (!dateStr) return styles.badgeDanger;
    const date = new Date(dateStr);
    const now = new Date();
    const diffMinutes = (now - date) / (1000 * 60);

    const tiempoWarning = balanza.tiempoWarning || 30;
    const tiempoDanger = balanza.tiempoDanger || 60;

    if (diffMinutes >= tiempoDanger) {
      return styles.badgeDanger;
    } else if (diffMinutes >= tiempoWarning) {
      return styles.badgeWarning;
    }
    return null;
  };

  return (
    <div style={styles.cardWrapper}>
      <div style={cardStyle}>
        <div style={styles.buttonsContainer}>
          <button
            style={styles.actionBtn}
            onClick={() => onEdit(balanza)}
            title="Editar"
          >
            ✎
          </button>
          <button
            style={styles.actionBtn}
            onClick={() => onDelete(balanza.id)}
            title="Eliminar"
          >
            ×
          </button>
        </div>
        <div style={styles.nombre}>{balanza.nombre}</div>
        <div style={styles.ip}>IP: {balanza.ip}</div>
        <div style={styles.medicion}>
          Peso: {balanza.ultimoPeso != null ? `${balanza.ultimoPeso} kg` : 'Sin datos'}
        </div>
        <div style={styles.medicion}>
          Última medición:{' '}
          {getAntiguedadBadgeStyle(balanza.ultimaMedicion) ? (
            <span style={getAntiguedadBadgeStyle(balanza.ultimaMedicion)}>
              {formatDate(balanza.ultimaMedicion)}
            </span>
          ) : (
            formatDate(balanza.ultimaMedicion)
          )}
        </div>
        <div style={styles.estado}>
          Estado: {isOk ? '● Conectado' : '● Desconectado'}
        </div>
      </div>
    </div>
  );
}

export default BalanzaCard;
