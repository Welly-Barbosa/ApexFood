import { api } from './api';
import type{ 
  InsumoResponseDto, 
  InsumoCreateDto, 
  //InsumoUpdateDto
} from '../types/insumo';

/**
 * Busca a lista de insumos da API.
 * @param incluirInativos Flag para incluir insumos desativados na busca.
 * @returns Uma promessa com a lista de insumos.
 */
export const getInsumos = (incluirInativos: boolean = false): Promise<InsumoResponseDto[]> => {
  // Lógica para chamar GET /api/insumos?incluirInativos={incluirInativos}
  // Exemplo: const { data } = await api.get(`/insumos?incluirInativos=${incluirInativos}`);
  // return data;
  
  // Por enquanto, retornamos dados mockados para desenvolvimento:
  console.log(`Buscando insumos, incluirInativos: ${incluirInativos}`);
  const mockData: InsumoResponseDto[] = [
    { id: '1', nome: 'Farinha de Trigo', unidadeMedida: 'Kg', origem: 'Rede', isAtivo: true },
    { id: '2', nome: 'Tomate Italiano', unidadeMedida: 'Kg', origem: 'Rede', isAtivo: true },
    { id: '3', nome: 'Manjericão Fresco', unidadeMedida: 'Maço', origem: 'Local', isAtivo: true },
    { id: '4', nome: 'Queijo Parmesão (Antigo)', unidadeMedida: 'Kg', origem: 'Rede', isAtivo: false },
  ];
  return Promise.resolve(mockData.filter(insumo => incluirInativos || insumo.isAtivo));
};

/**
 * Cria um novo insumo.
 * @param insumoData Os dados do insumo a ser criado.
 * @returns Uma promessa com o insumo criado.
 */
export const createInsumo = async (insumoData: InsumoCreateDto): Promise<InsumoResponseDto> => {
    const response = await api.post('/insumos', insumoData);
    return response.data;
};

// ...outras funções como updateInsumo, deleteInsumo, getHistoricoPrecos, etc.