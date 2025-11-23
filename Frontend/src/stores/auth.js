import { ref, computed } from 'vue'

// Use relative path when running in production/docker, or env variable for development
const API_URL = import.meta.env.VITE_API_URL ?? ''

const token = ref(localStorage.getItem('token') ?? '')
const user = ref(null)

export function useAuthStore() {
  const isAuthenticated = computed(() => !!token.value)

  const setToken = (newToken) => {
    token.value = newToken
    if (newToken) {
      localStorage.setItem('token', newToken)
    } else {
      localStorage.removeItem('token')
    }
  }

  const setUser = (userData) => {
    user.value = userData
  }

  const logout = () => {
    setToken('')
    setUser(null)
  }

  const apiFetch = async (path, options = {}) => {
    const headers = { ...(options.headers ?? {}) }
    if (options.body && !headers['Content-Type']) {
      headers['Content-Type'] = 'application/json'
    }
    if (token.value) {
      headers['Authorization'] = `Bearer ${token.value}`
    }

    const response = await fetch(`${API_URL}${path}`, { ...options, headers })
    if (!response.ok) {
      if (response.status === 401) {
        logout()
        throw new Error('Unauthorized')
      }
      const text = await response.text()
      throw new Error(text || 'Request failed')
    }

    if (response.status === 204) {
      return null
    }

    const text = await response.text()
    return text ? JSON.parse(text) : undefined
  }

  return {
    token,
    user,
    isAuthenticated,
    setToken,
    setUser,
    logout,
    apiFetch
  }
}

