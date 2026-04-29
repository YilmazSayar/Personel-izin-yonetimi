<template>
  <div class="min-h-screen bg-gray-50">
    <nav class="bg-blue-700 text-white p-4 flex justify-between items-center shadow-lg">
      <div class="flex items-center gap-3">
        <h1 class="font-bold">Seneka Personel İzin</h1>
        <span v-if="accountTypeLabel" class="text-xs bg-white/20 px-2 py-1 rounded font-medium">
          {{ accountTypeLabel }}
        </span>
      </div>
      <div v-if="showNavLinks" class="flex items-center gap-4">
        <NuxtLink v-if="isSystemAdmin" to="/admin-panel" class="hover:underline">Sistem Yönetimi</NuxtLink>
        <NuxtLink to="/dashboard" class="hover:underline">Panel</NuxtLink>
        <NuxtLink to="/permissions" class="hover:underline">İzinlerim</NuxtLink>
        <div class="relative">
          <button
            type="button"
            class="flex items-center gap-2 rounded-lg px-3 py-1.5 bg-blue-600 hover:bg-blue-500 transition"
            @click="profileMenuOpen = !profileMenuOpen"
          >
            <span class="w-7 h-7 rounded-full bg-white/20 inline-flex items-center justify-center font-bold uppercase">
              {{ displayName.charAt(0) }}
            </span>
            <span class="text-sm hidden sm:inline">{{ displayName }}</span>
          </button>
          <div
            v-if="profileMenuOpen"
            class="absolute right-0 top-full mt-2 min-w-[190px] rounded-xl bg-white text-gray-800 shadow-xl border border-gray-100 overflow-hidden z-50"
          >
            <button
              type="button"
              class="w-full text-left px-4 py-3 text-sm hover:bg-blue-50 transition flex items-center gap-2"
              @click="openEditNameModal"
            >
              <span>✏️</span> Ad soyad değiştir
            </button>
            <button
              type="button"
              class="w-full text-left px-4 py-3 text-sm hover:bg-red-50 text-red-600 transition flex items-center gap-2"
              @click="logout"
            >
              <span>🚪</span> Oturumu kapat
            </button>
          </div>
          <div v-if="profileMenuOpen" class="fixed inset-0 z-40" aria-hidden="true" @click="profileMenuOpen = false" />
        </div>
      </div>
    </nav>
    <main class="p-6">
      <slot />
    </main>

    <Teleport to="body">
      <div
        v-if="editNameModalOpen"
        class="fixed inset-0 z-[110] flex items-center justify-center p-4 bg-black/50"
        role="dialog"
        aria-modal="true"
        aria-labelledby="edit-name-title"
      >
        <div class="bg-white rounded-2xl shadow-xl border border-gray-100 w-full max-w-md" @click.stop>
          <div class="p-6 border-b border-gray-100">
            <h2 id="edit-name-title" class="text-xl font-bold text-gray-800">Ad Soyad Değiştir</h2>
            <p class="text-sm text-gray-500 mt-1">Profilde görünecek ad soyadı güncelleyin.</p>
          </div>
          <form class="p-6 space-y-4" @submit.prevent="submitEditName">
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Ad Soyad *</label>
              <input
                v-model="editFullName"
                type="text"
                maxlength="100"
                required
                class="w-full px-4 py-2.5 border border-gray-200 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent outline-none"
                placeholder="Ad Soyad"
              />
            </div>
            <div class="flex gap-3 pt-2">
              <button
                type="button"
                class="flex-1 px-4 py-2.5 border border-gray-200 rounded-xl font-medium text-gray-700 hover:bg-gray-50 transition"
                @click="closeEditNameModal"
              >
                İptal
              </button>
              <button
                type="submit"
                class="flex-1 px-4 py-2.5 bg-blue-600 text-white rounded-xl font-medium hover:bg-blue-700 transition disabled:opacity-50"
                :disabled="editNameSubmitting"
              >
                {{ editNameSubmitting ? 'Kaydediliyor...' : 'Kaydet' }}
              </button>
            </div>
          </form>
        </div>
        <div class="absolute inset-0 -z-10" aria-hidden="true" @click="closeEditNameModal" />
      </div>
    </Teleport>
  </div>
</template>

<script setup>
const route = useRoute();
const { clearUser, setUser, token, userName, userRole, isAdmin, isSystemAdmin, isLoggedIn } = useAuth();
const toast = useToast();
const profileMenuOpen = ref(false);
const editNameModalOpen = ref(false);
const editNameSubmitting = ref(false);
const editFullName = ref('');
const displayName = computed(() => userName.value || 'Kullanıcı');
const accountTypeLabel = computed(() => {
  if (!userRole.value) return '';
  if (isSystemAdmin.value) return 'Sistem Yöneticisi';
  return isAdmin.value ? 'Yönetici Hesabı' : 'Kullanıcı Hesabı';
});
const showNavLinks = computed(() => {
  const path = route.path || '';
  if (path === '/login' || path === '/register') return false;
  return !!isLoggedIn.value;
});
const logout = () => {
  profileMenuOpen.value = false;
  clearUser();
  navigateTo('/login');
};

function openEditNameModal() {
  profileMenuOpen.value = false;
  editFullName.value = (displayName.value || '').trim();
  editNameModalOpen.value = true;
}

function closeEditNameModal() {
  editNameModalOpen.value = false;
}

async function submitEditName() {
  if (editNameSubmitting.value) return;
  const nextName = (editFullName.value || '').trim();
  if (!nextName) {
    toast.showError('Ad soyad boş olamaz.');
    return;
  }
  editNameSubmitting.value = true;
  try {
    const res = await $fetch('https://localhost:44365/api/Auth/me/full-name', {
      method: 'PATCH',
      headers: {
        Authorization: `Bearer ${token.value}`,
        'Content-Type': 'application/json',
      },
      body: { fullName: nextName },
    });
    setUser({ userName: (res?.fullName ?? nextName) });
    closeEditNameModal();
    toast.showSuccess('Ad soyad güncellendi.');
  } catch (e) {
    const msg = e?.data?.message ?? e?.data?.Message ?? (typeof e?.data === 'string' ? e.data : null) ?? 'Ad soyad güncellenemedi.';
    toast.showError(msg);
  } finally {
    editNameSubmitting.value = false;
  }
}
</script>