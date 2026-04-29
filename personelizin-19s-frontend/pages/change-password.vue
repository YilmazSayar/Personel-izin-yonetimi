<template>
  <div class="auth-wrapper">
    <div class="auth-card">
      <h2>Şifrenizi değiştirin</h2>
      <p class="subtitle">Hesabınız yönetici tarafından oluşturuldu. Devam etmek için yeni bir şifre belirleyin.</p>

      <form @submit.prevent="handleSubmit">
        <div class="input-group">
          <label>Yeni şifre</label>
          <input
            v-model="form.newPassword"
            type="password"
            placeholder="••••••••"
            required
            minlength="1"
            autocomplete="new-password"
          />
        </div>
        <div class="input-group">
          <label>Yeni şifre (tekrar)</label>
          <input
            v-model="form.newPasswordConfirm"
            type="password"
            placeholder="••••••••"
            required
            minlength="1"
            autocomplete="new-password"
          />
        </div>
        <p v-if="form.newPassword && form.newPasswordConfirm && form.newPassword !== form.newPasswordConfirm" class="text-red-600 text-sm mt-1">
          Şifreler eşleşmiyor.
        </p>
        <button
          type="submit"
          class="btn-primary"
          :disabled="loading || (form.newPassword !== form.newPasswordConfirm)"
        >
          {{ loading ? 'Güncelleniyor...' : 'Şifreyi güncelle' }}
        </button>
      </form>
    </div>
  </div>
</template>

<script setup>
definePageMeta({
  layout: 'default',
  middleware: 'auth'
})

const { token, userRole, clearMustChangePassword } = useAuth()
const form = ref({ newPassword: '', newPasswordConfirm: '' })
const toast = useToast()
const loading = ref(false)

const handleSubmit = async () => {
  if (form.value.newPassword !== form.value.newPasswordConfirm) {
    toast.showError('Şifreler eşleşmiyor.')
    return
  }
  if (!form.value.newPassword?.trim()) {
    toast.showError('Yeni şifre boş olamaz.')
    return
  }
  loading.value = true
  try {
    await $fetch('https://localhost:44365/api/Auth/change-password', {
      method: 'POST',
      headers: {
        Authorization: `Bearer ${token.value}`,
        'Content-Type': 'application/json'
      },
      body: {
        NewPassword: form.value.newPassword,
        NewPasswordConfirm: form.value.newPasswordConfirm
      }
    })
    clearMustChangePassword()
    toast.showSuccess('Şifreniz güncellendi.')
    const role = (userRole.value || '').toLowerCase()
    navigateTo(role === 'admin' ? '/admin-panel' : '/dashboard')
  } catch (error) {
    const msg = error?.data?.message ?? error?.data?.Message ?? (typeof error?.data === 'string' ? error.data : null) ?? 'Şifre güncellenemedi.'
    toast.showError(msg)
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
.auth-wrapper {
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 100vh;
  background-color: #f5f7fb;
}
.auth-card {
  background: white;
  padding: 2.5rem;
  border-radius: 12px;
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.1);
  width: 100%;
  max-width: 400px;
}
h2 {
  color: #2d3748;
  margin-bottom: 0.5rem;
  text-align: center;
}
.subtitle {
  color: #718096;
  text-align: center;
  margin-bottom: 2rem;
  font-size: 0.9rem;
}
.input-group {
  margin-bottom: 1.5rem;
}
label {
  display: block;
  margin-bottom: 0.5rem;
  color: #4a5568;
  font-weight: 500;
}
input {
  width: 100%;
  padding: 0.75rem;
  border: 1px solid #e2e8f0;
  border-radius: 6px;
  box-sizing: border-box;
}
.btn-primary {
  width: 100%;
  padding: 0.75rem;
  background-color: #3182ce;
  color: white;
  border: none;
  border-radius: 6px;
  font-weight: 600;
  cursor: pointer;
  margin-top: 10px;
}
.btn-primary:hover:not(:disabled) {
  background-color: #2b6cb0;
}
.btn-primary:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}
</style>
