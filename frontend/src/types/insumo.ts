// frontend/src/types/insumo.ts

/**
 * Contrato de dados para um insumo retornado pela API.
 */
export interface InsumoResponseDto {
  id: string;
  nome: string;
  unidadeMedida: string;
  origem: 'Rede' | 'Local';
  isAtivo: boolean;
}

/**
 * Contrato para criação de um novo insumo.
 */
export interface InsumoCreateDto {
  nome: string;
  unidadeMedida: string;
}

/**
 * Contrato para atualização de um insumo existente.
 */
export interface InsumoUpdateDto {
  nome: string;
  unidadeMedida: string;
}

/**
 * Contrato para criação de um novo registro no histórico de preços.
 */
export interface HistoricoPrecoCreateDto {
  insumoId: string;
  preco: number;
  dataVigencia: string; // Formato 'YYYY-MM-DD'
}

/**
 * Contrato para a resposta do histórico de preços de um insumo.
 */
export interface HistoricoPrecoResponseDto {
  id: string;
  preco: number;
  dataVigencia: string; // Formato 'YYYY-MM-DD'
}