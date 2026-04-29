<script setup>
definePageMeta({
  layout: 'admin',
  middleware: 'auth'
})

const { token } = useAuth();
const { refreshTrigger, open: openCreateUserModal } = useCreateUserModal();
const { roleLabel } = useTurkishLabels();
const users = ref([]);
const searchQuery = ref('');
const isLoading = ref(true);
const editingLeaveDays = ref({});
const leaveDaysSaving = ref({});

// Arama: ad soyad, e-posta veya birim adı ile
const filteredUsers = computed(() => {
  const q = (searchQuery.value || '').trim().toLowerCase();
  if (!q) return users.value;
  return users.value.filter((u) => {
    const name = (u.FullName ?? u.fullName ?? '').toString().toLowerCase();
    const email = (u.Email ?? u.email ?? '').toString().toLowerCase();
    const unitName = (u.UnitName ?? u.unitName ?? '').toString().toLowerCase();
    return name.includes(q) || email.includes(q) || unitName.includes(q);
  });
});

// 1. Veritabanından Kullanıcıları Çekme Fonksiyonu
const fetchUsers = async () => {
  // Backend'e gitmeden önce elimizdeki en güncel token'ı alıyoruz
  const activeToken = token.value || localStorage.getItem('token');
  
  if (!activeToken) {
    console.error("Yetkilendirme anahtarı (token) bulunamadı!");
    return;
  }

  isLoading.value = true;
  try {
    // URL'yi AuthController içindeki yeni metoda (api/Auth/users) yönlendirdik
    const data = await $fetch('https://localhost:44365/api/Auth/users', {
      headers: {
        Authorization: `Bearer ${activeToken}`
      }
    });

    // Veriyi users listesine ata
    users.value = Array.isArray(data) ? data : [];
  } catch (error) {
    console.error("Kullanıcılar yüklenemedi:", error);
    // Hata durumunda kullanıcıya bilgi ver
  } finally {
    isLoading.value = false;
  }
};

function hasLeaveDaysChanged(u) {
  const id = u.Id ?? u.id;
  const current = (u.RemainingLeaveDays ?? u.remainingLeaveDays) ?? 0;
  const edited = editingLeaveDays.value[id];
  return edited !== undefined && edited !== current;
}

async function saveLeaveDays(u) {
  const id = u.Id ?? u.id;
  const unitId = u.UnitId ?? u.unitId;
  if (id == null || unitId == null) return;
  const days = editingLeaveDays.value[id] ?? (u.RemainingLeaveDays ?? u.remainingLeaveDays) ?? 0;
  leaveDaysSaving.value = { ...leaveDaysSaving.value, [id]: true };
  try {
    await $fetch(`https://localhost:44365/api/Units/${unitId}/users/${id}/remaining-leave-days`, {
      method: 'PATCH',
      headers: { Authorization: `Bearer ${token.value}`, 'Content-Type': 'application/json' },
      body: { RemainingLeaveDays: days }
    });
    const idx = users.value.findIndex(x => (x.Id ?? x.id) === id);
    if (idx !== -1) {
      users.value[idx] = { ...users.value[idx], RemainingLeaveDays: days, remainingLeaveDays: days };
    }
    editingLeaveDays.value = { ...editingLeaveDays.value, [id]: undefined };
    useToast().showSuccess('Kalan izin günü güncellendi.');
  } catch (e) {
    const msg = e?.data?.message ?? e?.data?.Message ?? (typeof e?.data === 'string' ? e.data : null) ?? 'Güncellenemedi.';
    useToast().showError(msg);
  } finally {
    leaveDaysSaving.value = { ...leaveDaysSaving.value, [id]: false };
  }
}

// 2. Kullanıcı Silme Fonksiyonu
const deleteUser = async (id) => {
  const ok = await useConfirm().showConfirm({
    title: 'Personeli sil',
    message: 'Bu personeli silmek istediğinize emin misiniz? Bu işlem geri alınamaz.',
    confirmText: 'Sil',
    danger: true,
  });
  if (!ok) return;

  const activeToken = token.value || localStorage.getItem('token');
  try {
    await $fetch(`https://localhost:44365/api/Auth/users/${id}`, {
      method: 'DELETE',
      headers: {
        Authorization: `Bearer ${activeToken}`
      }
    });
    users.value = users.value.filter(u => u.Id !== id && u.id !== id);
    useToast().showSuccess("Personel başarıyla silindi.");
  } catch (error) {
    console.error("Silme hatası:", error);
    useToast().showError("Personel silinemedi.");
  }
};

onMounted(() => {
  fetchUsers();
});

watch(refreshTrigger, () => {
  fetchUsers();
});
</script>

<template>
  <div class="p-4">
    <div class="flex justify-between items-center mb-6">
      <h1 class="text-3xl font-bold text-gray-800 tracking-tight">Personel Yönetimi</h1>
      <div class="flex items-center gap-2">
        <button
          type="button"
          @click="openCreateUserModal()"
          class="flex items-center gap-2 bg-blue-600 text-white px-4 py-2 rounded-xl font-bold hover:bg-blue-700 transition active:scale-95"
        >
          <span class="text-lg">➕</span> Kullanıcı oluştur
        </button>
        <button
          @click="fetchUsers"
          class="flex items-center gap-2 bg-blue-50 text-blue-600 px-4 py-2 rounded-xl font-bold hover:bg-blue-100 transition active:scale-95"
        >
          <span class="text-lg">🔄</span> Listeyi Yenile
        </button>
      </div>
    </div>

    <div v-if="isLoading" class="flex flex-col items-center justify-center py-20 text-gray-500">
      <div class="animate-spin text-4xl mb-4 text-blue-600">🌀</div>
      <p class="font-medium">Veritabanından veriler çekiliyor...</p>
    </div>

    <template v-else>
      <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-4 mb-4">
        <label class="block text-sm font-medium text-gray-700 mb-2">Arama</label>
        <div class="relative">
          <span class="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400">🔍</span>
          <input
            v-model="searchQuery"
            type="text"
            placeholder="Ad soyad, e-posta veya birim adı ile ara..."
            class="w-full pl-10 pr-4 py-2.5 border border-gray-200 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent outline-none"
          />
          <button
            v-if="searchQuery"
            type="button"
            class="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gray-600"
            aria-label="Temizle"
            @click="searchQuery = ''"
          >
            ✕
          </button>
        </div>
        <p v-if="searchQuery.trim()" class="mt-2 text-sm text-gray-500">
          {{ filteredUsers.length }} personel listeleniyor
        </p>
      </div>

      <div class="bg-white rounded-3xl shadow-sm border border-gray-100 overflow-hidden">
      <table class="w-full text-left">
        <thead class="bg-gray-50/50 border-b border-gray-100">
          <tr class="text-gray-400 text-[11px] uppercase tracking-widest font-black">
            <th class="px-6 py-5">ID</th>
            <th class="px-6 py-5">Ad Soyad</th>
            <th class="px-6 py-5">E-posta</th>
            <th class="px-6 py-5">Birim</th>
            <th class="px-6 py-5">Kalan İzin</th>
            <th class="px-6 py-5">Rol</th>
            <th class="px-6 py-5 text-right">İşlemler</th>
          </tr>
        </thead>
        <tbody class="divide-y divide-gray-50">
          <tr v-for="user in filteredUsers" :key="user.Id || user.id" class="hover:bg-blue-50/30 transition-colors">
            <td class="px-6 py-4 text-sm font-mono text-gray-400">#{{ user.Id || user.id }}</td>
            <td class="px-6 py-4 font-bold text-gray-800">{{ user.FullName || user.fullName }}</td>
            <td class="px-6 py-4 text-gray-600 text-sm">{{ user.Email || user.email }}</td>
            <td class="px-6 py-4 text-gray-600 text-sm">{{ user.UnitName || user.unitName || '—' }}</td>
            <td class="px-6 py-4">
              <div class="flex items-center gap-2">
                <input
                  type="number"
                  min="0"
                  :value="editingLeaveDays[user.Id ?? user.id] ?? (user.RemainingLeaveDays ?? user.remainingLeaveDays ?? 0)"
                  class="w-14 rounded-lg border border-gray-200 px-2 py-1.5 text-sm font-semibold text-blue-600"
                  @input="editingLeaveDays[user.Id ?? user.id] = parseInt($event.target.value, 10) || 0"
                />
                <span class="text-gray-500 text-sm">gün</span>
                <button
                  v-if="hasLeaveDaysChanged(user)"
                  type="button"
                  class="text-xs bg-blue-600 text-white px-2.5 py-1.5 rounded-lg hover:bg-blue-700 disabled:opacity-50"
                  :disabled="leaveDaysSaving[user.Id ?? user.id]"
                  @click="saveLeaveDays(user)"
                >
                  {{ leaveDaysSaving[user.Id ?? user.id] ? '...' : 'Kaydet' }}
                </button>
              </div>
            </td>
            <td class="px-6 py-4">
              <span 
                class="px-2.5 py-1 rounded-lg text-[10px] font-black uppercase tracking-tighter" 
                :class="(user.Role || user.role) === 'Manager' ? 'bg-purple-100 text-purple-700' : (user.Role || user.role) === 'Admin' ? 'bg-amber-100 text-amber-700' : 'bg-blue-100 text-blue-700'"
              >
                {{ roleLabel(user.Role || user.role) }}
              </span>
            </td>
            <td class="px-6 py-4 text-right">
              <button
                type="button"
                @click="deleteUser(user.Id || user.id)"
                class="inline-flex items-center px-3 py-2 rounded-xl text-xs font-semibold text-white bg-red-600 hover:bg-red-700 transition no-underline"
              >
                Kaydı sil
              </button>
            </td>
          </tr>
          
          <tr v-if="filteredUsers.length === 0">
            <td colspan="7" class="px-6 py-16 text-center text-gray-400 italic bg-gray-50/20">
              <div class="text-4xl mb-2">📭</div>
              {{ searchQuery.trim() ? 'Arama kriterine uygun personel bulunamadı.' : 'Biriminizde henüz kayıtlı çalışan yok. Davet kodunuzu paylaşarak çalışanların üye olmasını sağlayın.' }}
            </td>
          </tr>
        </tbody>
      </table>
      </div>
    </template>
  </div>
</template>