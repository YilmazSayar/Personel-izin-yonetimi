<template>
  <div class="auth-wrapper">
    <div class="auth-card">
      <h2>Personel Girişi</h2>
      <p class="subtitle">Devam etmek için hesabınıza giriş yapın.</p>
      
      <form @submit.prevent="handleLogin">
        <div class="input-group">
          <label>E-posta</label>
          <input v-model="form.email" type="email" placeholder="admin@seneka.com.tr" required />
        </div>
        
        <div class="input-group">
          <label>Şifre</label>
          <input v-model="form.password" type="password" placeholder="••••••••" required />
        </div>
        
        <button type="submit" class="btn-primary" :disabled="loading">
          {{ loading ? 'Giriş Yapılıyor...' : 'Giriş Yap' }}
        </button>
      </form>

      <div class="auth-footer">
        Hesabın yok mu?
        <NuxtLink to="/register" class="auth-link">Kayıt Ol</NuxtLink>
      </div>

    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const { setUser } = useAuth()
const form = ref({ email: '', password: '' })
const toast = useToast()
const loading = ref(false)

const handleLogin = async () => {
  loading.value = true
  try {
    const response = await $fetch('https://localhost:44365/api/Auth/login', {
      method: 'POST',
      body: form.value
    })

    const token = response.token || response.Token
    const userId = response.userId ?? response.UserId
    const userName = response.userName ?? response.UserName
    const userRole = response.role ?? response.Role
    const mustChangePassword = !!(response.mustChangePassword ?? response.MustChangePassword)

    if (token && userId != null) {
      setUser({ token, userId, userName, userRole, mustChangePassword })
      if (mustChangePassword) {
        navigateTo('/change-password')
        return
      }
      const role = (userRole || '').toLowerCase()
      navigateTo(role === 'admin' ? '/admin-panel' : '/dashboard')
    } else {
      toast.showError("Kullanıcı bilgileri eksik geldi!")
    }
  } catch (error) {
    console.error("Giriş Hatası:", error)
    toast.showError('E-posta veya şifre hatalı!')
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
.auth-wrapper { display: flex; justify-content: center; align-items: center; min-height: 100vh; background-color: #f5f7fb; }
.auth-card { background: white; padding: 2.5rem; border-radius: 12px; box-shadow: 0 8px 24px rgba(0,0,0,0.1); width: 100%; max-width: 400px; }
h2 { color: #2d3748; margin-bottom: 0.5rem; text-align: center; }
.subtitle { color: #718096; text-align: center; margin-bottom: 2rem; font-size: 0.9rem; }
.input-group { margin-bottom: 1.5rem; }
label { display: block; margin-bottom: 0.5rem; color: #4a5568; font-weight: 500; }
input { width: 100%; padding: 0.75rem; border: 1px solid #e2e8f0; border-radius: 6px; box-sizing: border-box; }
.btn-primary { width: 100%; padding: 0.75rem; background-color: #3182ce; color: white; border: none; border-radius: 6px; font-weight: 600; cursor: pointer; margin-top: 10px; }
.btn-primary:hover { background-color: #2b6cb0; }
.status-msg { margin-top: 1rem; text-align: center; color: #c53030; background: #fed7d7; padding: 0.5rem; border-radius: 4px; }
.auth-footer { margin-top: 1.5rem; text-align: center; font-size: 0.9rem; color: #718096; }
.auth-link { color: #3182ce; font-weight: 600; margin-left: 0.25rem; text-decoration: none; }
.auth-link:hover { text-decoration: underline; }
</style>