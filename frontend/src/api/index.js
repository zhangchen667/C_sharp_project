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
  update: (id, data) => request.put(`/posts/${id}`, data),
  delete: (id) => request.delete(`/posts/${id}`),
  uploadImage: (formData) => request.post('/posts/upload-image', formData, {
    headers: { 'Content-Type': 'multipart/form-data' }
  }),
  getComments: (postId) => request.get(`/posts/${postId}/comments`),
  createComment: (data) => request.post('/posts/comments', data),
  replyComment: (id, reply) => request.put(`/posts/comments/${id}/reply`, reply, {
    headers: { 'Content-Type': 'application/json' }
  }),
  deleteComment: (id) => request.delete(`/posts/comments/${id}`)
}

export const profileApi = {
  getProfile: (userId) => request.get(`/profile/${userId || ''}`),
  updateProfile: (data) => request.put('/profile', data),
  getPosts: (userId, params) => request.get(`/profile/${userId}/posts`, { params }),
  getPhotos: (userId) => request.get(`/profile/${userId}/photos`),
  uploadPhoto: (formData) => request.post('/profile/photos', formData, {
    headers: { 'Content-Type': 'multipart/form-data' }
  }),
  deletePhoto: (id) => request.delete(`/profile/photos/${id}`)
}
