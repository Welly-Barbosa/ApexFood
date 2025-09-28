// frontend/src/components/Insumos/InsumoForm/index.tsx
import React, { useState } from 'react';
import type { InsumoCreateDto } from '../../../types/insumo';

interface InsumoFormProps {
    onSave: (insumo: InsumoCreateDto) => void;
    isLoading: boolean;
}

const InsumoForm: React.FC<InsumoFormProps> = ({ onSave, isLoading }) => {
    const [nome, setNome] = useState('');
    const [unidadeMedida, setUnidadeMedida] = useState('');

    const handleSubmit = (event: React.FormEvent) => {
        event.preventDefault();
        if (!nome || !unidadeMedida) {
            alert('Por favor, preencha todos os campos.');
            return;
        }
        onSave({ nome, unidadeMedida });
    };

    return (
        <form onSubmit={handleSubmit}>
            <div style={{ marginBottom: '15px' }}>
                <label>
                    Nome do Insumo
                    <input
                        type="text"
                        value={nome}
                        onChange={(e) => setNome(e.target.value)}
                        style={{ width: '100%', padding: '8px', marginTop: '5px' }}
                        required
                    />
                </label>
            </div>
            <div style={{ marginBottom: '15px' }}>
                <label>
                    Unidade de Medida (ex: Kg, Lt, Un)
                    <input
                        type="text"
                        value={unidadeMedida}
                        onChange={(e) => setUnidadeMedida(e.target.value)}
                        style={{ width: '100%', padding: '8px', marginTop: '5px' }}
                        required
                    />
                </label>
            </div>
            <button type="submit" disabled={isLoading}>
                {isLoading ? 'Salvando...' : 'Salvar Insumo'}
            </button>
        </form>
    );
};

export default InsumoForm;