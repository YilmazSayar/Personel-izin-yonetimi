export const useApi = (endpoint, options = {}) => {
  const config = useRuntimeConfig();
  const token = localStorage.getItem('token');

  return $fetch(endpoint, {
    baseURL: config.public.apiUrl,
    headers: {
      Authorization: token ? `Bearer ${token}` : '',
      ...options.headers
    },
    ...options
  });
};