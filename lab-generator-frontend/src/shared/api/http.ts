import axios from 'axios'

export const http = axios.create({
  baseURL: '/api',
  headers: {
    'Content-Type': 'application/json',
  },
});

http.interceptors.response.use(
  (r) => r,
  (error) => {
    return Promise.reject(error);
  }
);