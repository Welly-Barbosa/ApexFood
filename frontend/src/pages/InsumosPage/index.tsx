// frontend/src/pages/InsumosPage/index.tsx
import React, { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { getInsumos, createInsumo } from '../../services/insumoService';
import InsumosList from '../../components/Insumos/InsumosList';
import Modal from '../../components/Modal';
import InsumoForm from '../../components/Insumos/InsumoForm';
// --- CORREÇÃO 1: Removido o InsumoCreateDto que não era usado aqui ---
import type { InsumoResponseDto, InsumoCreateDto } from '../../types/insumo';

const InsumosPage: React.FC = () => {
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [incluirInativos, setIncluirInativos] = useState(false);

    const queryClient = useQueryClient();
    const queryKey = ['insumos', incluirInativos];

    const { data: insumos = [], isLoading, error } = useQuery({
        queryKey: queryKey,
        queryFn: () => getInsumos(incluirInativos),
    });

    const createInsumoMutation = useMutation({
        mutationFn: createInsumo,
        onSuccess: (novoInsumo) => {
            console.log('Mutação bem-sucedida! Insumo criado:', novoInsumo);
            queryClient.invalidateQueries({ queryKey: ['insumos'] });
            setIsModalOpen(false);
        },
        onError: (err) => {
            console.error('Erro na mutação:', err);
            alert(`Erro ao criar insumo: ${err.message}`);
        }
    });

    // --- CORREÇÃO 2: A função handleSaveInsumo foi recriada ---
    const handleSaveInsumo = (insumoData: InsumoCreateDto) => {
        createInsumoMutation.mutate(insumoData);
    };

    const handleEdit = (insumo: InsumoResponseDto) => console.log('Editar:', insumo);
    const handleViewPrices = (insumo: InsumoResponseDto) => console.log('Ver Preços:', insumo);
    const handleToggleStatus = (insumo: InsumoResponseDto) => console.log('Mudar Status:', insumo);

    return (
        <div style={{ padding: '20px', fontFamily: 'sans-serif' }}>
            <h1>Gestão de Insumos</h1>

            <div style={{ margin: '20px 0' }}>
                <label>
                    <input
                        type="checkbox"
                        checked={incluirInativos}
                        onChange={(e) => setIncluirInativos(e.target.checked)}
                    />
                    Exibir insumos inativos
                </label>
            </div>

            <button onClick={() => setIsModalOpen(true)} style={{ marginBottom: '20px' }}>
                Novo Insumo
            </button>

            {isLoading && <p>Carregando insumos...</p>}
            {error && <p>Ocorreu um erro ao buscar os insumos: {error.message}</p>}

            <InsumosList
                insumos={insumos}
                onEdit={handleEdit}
                onViewPrices={handleViewPrices}
                onToggleStatus={handleToggleStatus}
            />

            <Modal isOpen={isModalOpen} onClose={() => setIsModalOpen(false)} title="Novo Insumo">
                <InsumoForm
                    onSave={handleSaveInsumo} // --- CORREÇÃO 3: Agora a função existe e o erro desaparece ---
                    isLoading={createInsumoMutation.isPending}
                />
            </Modal>
        </div>
    );
};

export default InsumosPage;