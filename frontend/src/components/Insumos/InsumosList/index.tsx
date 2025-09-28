import React from 'react';
import type { InsumoResponseDto } from '../../../types/insumo';
import styled from 'styled-components';

// --- Estilos Auxiliares ---
// Usando styled-components que já está no projeto para criar os badges.
const Badge = styled.span<{ variant: 'rede' | 'local' }>`
  padding: 4px 8px;
  border-radius: 12px;
  font-size: 0.8rem;
  font-weight: bold;
  color: white;
  background-color: ${props => props.variant === 'rede' ? '#007bff' : '#28a745'};
`;

const StatusText = styled.span<{ $isAtivo: boolean }>`
  color: ${props => props.$isAtivo ? '#28a745' : '#dc3545'};
  font-weight: bold;
`;

// --- Propriedades do Componente ---
interface InsumosListProps {
  insumos: InsumoResponseDto[];
  // Funções para lidar com as ações (serão implementadas a seguir)
  onEdit: (insumo: InsumoResponseDto) => void;
  onViewPrices: (insumo: InsumoResponseDto) => void;
  onToggleStatus: (insumo: InsumoResponseDto) => void;
}

/**
 * Componente responsável por renderizar a tabela de insumos.
 */
const InsumosList: React.FC<InsumosListProps> = ({ 
  insumos,
  onEdit,
  onViewPrices,
  onToggleStatus
}) => {
  if (insumos.length === 0) {
    return <p>Nenhum insumo encontrado.</p>;
  }

  return (
    <table style={{ width: '100%', borderCollapse: 'collapse' }}>
      <thead>
        <tr style={{ borderBottom: '2px solid #ddd', textAlign: 'left' }}>
          <th style={{ padding: '8px' }}>Nome</th>
          <th style={{ padding: '8px' }}>Origem</th>
          <th style={{ padding: '8px' }}>Status</th>
          <th style={{ padding: '8px' }}>Ações</th>
        </tr>
      </thead>
      <tbody>
        {insumos.map((insumo) => (
          <tr key={insumo.id} style={{ borderBottom: '1px solid #eee' }}>
            <td style={{ padding: '8px' }}>{insumo.nome}</td>
            <td style={{ padding: '8px' }}>
              <Badge variant={insumo.origem === 'Rede' ? 'rede' : 'local'}>
                {insumo.origem}
              </Badge>
            </td>
            <td style={{ padding: '8px' }}>
              <StatusText $isAtivo={insumo.isAtivo}>
                {insumo.isAtivo ? 'Ativo' : 'Inativo'}
              </StatusText>
            </td>
            <td style={{ padding: '8px' }}>
              <button onClick={() => onEdit(insumo)} style={{ marginRight: '4px' }}>Editar</button>
              <button onClick={() => onViewPrices(insumo)} style={{ marginRight: '4px' }}>Preços</button>
              <button onClick={() => onToggleStatus(insumo)}>
                {insumo.isAtivo ? 'Desativar' : 'Ativar'}
              </button>
            </td>
          </tr>
        ))}
      </tbody>
    </table>
  );
};

export default InsumosList;