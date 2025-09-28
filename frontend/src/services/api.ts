// frontend/src/services/api.ts
import axios from 'axios';

export const api = axios.create({
  // Endereço base da nossa API .NET
  baseURL: 'https://localhost:7086/api', 
});