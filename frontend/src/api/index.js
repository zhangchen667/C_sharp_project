import axios from 'axios'

const request = axios.create({
  baseURL: 'http://localhost:5000/api',
  timeout: 10000
})

request.interceptors.request.use(
  config => {
    const token = localStorage.getItem('token')
    if (token) {
      config.headers.Authorization = `Bearer ${token}`
    }
    return config
  },
  error => Promise.reject(error)
)

request.interceptors.response.use(
  response => response.data,
  error => {
    console.error('API Error:', error)
    return Promise.reject(error)
  }
)

export const authApi = {
  login: (data) => request.post('/auth/login', data),
  register: (data) => request.post('/auth/register', data)
}

export const postsApi = {
  getList: (params) => request.get('/posts', { params }),
  getDetail: (id) => request.get(`/posts/${id}`),
  getCategories: () => request.get('/posts/categories'),
  create: (data) => request.post('/posts', data),
  delete: (id) => request.delete(`/posts/${id}`)
}

export const albumsApi = {
  getPhotos: () => request.get('/albums/photos'),
  upload: (formData) => request.post('/albums/upload', formData, {
    headers: { 'Content-Type': 'multipart/form-data' }
  }),
  delete: (id) => request.delete(`/albums/photos/${id}`)
}

export const commentsApi = {
  getList: () => request.get('/comments'),
  getAll: () => request.get('/comments/all'),
  create: (data) => request.post('/comments', data),
  reply: (id, reply) => request.put(`/comments/${id}/reply`, reply, {
    headers: { 'Content-Type': 'application/json' }
  }),
  delete: (id) => request.delete(`/comments/${id}`)
}
