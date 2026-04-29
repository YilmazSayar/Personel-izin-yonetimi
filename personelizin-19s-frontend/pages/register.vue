<template>
  <div class="register-page">
    <div class="auth-wrapper">
      <div class="auth-card">
        <h2>Üye Ol</h2>
        <p class="subtitle">İzinlerim sistemine hızlıca kayıt olun.</p>

        <form @submit.prevent="handleRegister" class="user-form">
          <div class="input-group">
            <label for="fullname">Ad Soyad</label>
            <input
              id="fullname"
              v-model="form.fullName"
              type="text"
              placeholder="Adınız Soyadınız"
            />
          </div>
          <div class="input-group">
            <label>E-posta <span class="required">*</span></label>
            <input v-model="form.email" type="email" placeholder="ornek@sirket.com" required />
          </div>
          <div class="input-group">
            <label>Şifre <span class="required">*</span></label>
            <input v-model="form.password" type="password" placeholder="En az 4 karakter" required />
          </div>
          <button type="submit" class="btn-primary">Kayıt Ol</button>
        </form>

        <div class="auth-footer">
          Zaten hesabınız var mı? <NuxtLink to="/login">Giriş Yap</NuxtLink>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
const form = ref({ email: '', password: '', fullName: '' })
const toast = useToast()

const handleRegister = async () => {
  try {
    const body = {
      email: form.value.email,
      password: form.value.password,
      fullName: (form.value.fullName || '').trim() || undefined
    }
    const response = await $fetch('https://localhost:44365/api/Auth/register', {
      method: 'POST',
      body
    })
    toast.showSuccess(response.message || 'Kayıt başarılı.')
    setTimeout(() => navigateTo('/login'), 2000)
  } catch (error) {
    const data = error?.data ?? error?.response?._data
    toast.showError((typeof data === 'string' ? data : data?.message) || 'Kayıt sırasında bir hata oluştu.')
  }
}
</script>

<style scoped>
.register-page {
  min-height: 100vh;
  background-color: #f0f2f5;
}
.auth-wrapper {
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 100vh;
  padding: 2rem 1rem;
}
.auth-card {
  background: white;
  padding: 2.5rem;
  border-radius: 12px;
  box-shadow: 0 8px 24px rgba(0,0,0,0.08);
  width: 100%;
  max-width: 400px;
}
h2 { color: #2d3748; margin-bottom: 0.5rem; text-align: center; font-size: 1.5rem; }
.subtitle { color: #718096; text-align: center; margin-bottom: 1.5rem; font-size: 0.9rem; }
.user-form { margin-bottom: 1rem; }
.input-group { margin-bottom: 1.25rem; }
label { display: block; margin-bottom: 0.5rem; color: #4a5568; font-weight: 500; }
.required { color: #e53e3e; }
input {
  width: 100%;
  padding: 0.75rem 1rem;
  border: 1px solid #e2e8f0;
  border-radius: 6px;
  font-size: 1rem;
  box-sizing: border-box;
}
input:focus { border-color: #3182ce; outline: none; }
.btn-primary {
  width: 100%;
  padding: 0.75rem;
  background-color: #3182ce;
  color: white;
  border: none;
  border-radius: 6px;
  font-weight: 600;
  cursor: pointer;
  font-size: 1rem;
  margin-top: 0.25rem;
}
.btn-primary:hover { background-color: #2b6cb0; }
.auth-footer { margin-top: 1rem; text-align: center; font-size: 0.9rem; color: #718096; }
.auth-footer a { color: #3182ce; font-weight: 600; text-decoration: none; }
.auth-footer a:hover { text-decoration: underline; }
</style>
