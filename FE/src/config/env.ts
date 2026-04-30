export const env = {
  API_BASE_URL: import.meta.env.VITE_API_BASE_URL || '/api',
  isDevelopment: import.meta.env.DEV,
  isProduction: import.meta.env.PROD,
  ENABLE_DEBUG: import.meta.env.VITE_ENABLE_DEBUG === 'true',
};
