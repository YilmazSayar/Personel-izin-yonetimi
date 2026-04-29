<template>
  <div class="p-4">
    <div class="flex flex-wrap items-center justify-between gap-4 mb-6">
      <div class="flex items-center gap-4">
        <NuxtLink
          to="/admin/units"
          class="flex items-center gap-2 text-gray-600 hover:text-gray-900 font-medium"
        >
          ← Birim listesine dön
        </NuxtLink>
        <h1 class="text-3xl font-bold text-gray-800 tracking-tight">
          {{ unitName || 'Birimdeki çalışanlar' }}
        </h1>
      </div>
      <button
        @click="fetchMembers"
        class="flex items-center gap-2 bg-blue-50 text-blue-600 px-4 py-2 rounded-xl font-bold hover:bg-blue-100 transition active:scale-95"
      >
        <span class="text-lg">🔄</span> Listeyi Yenile
      </button>
    </div>

    <div v-if="isLoading" class="flex flex-col items-center justify-center py-20 text-gray-500">
      <div class="animate-spin text-4xl mb-4 text-blue-600">🌀</div>
      <p class="font-medium">Yükleniyor...</p>
    </div>

    <template v-else>
      <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-4 mb-4">
        <label class="block text-sm font-medium text-gray-700 mb-2">Arama</label>
        <div class="relative">
          <span class="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400">🔍</span>
          <input
            v-model="searchQuery"
            type="text"
            placeholder="Ad soyad veya e-posta ile ara..."
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
          {{ filteredMembers.length }} çalışan listeleniyor
        </p>
      </div>

      <div class="bg-white rounded-3xl shadow-sm border border-gray-100 overflow-hidden">
        <table class="w-full text-left">
          <thead class="bg-gray-50/50 border-b border-gray-100">
            <tr class="text-gray-400 text-[11px] uppercase tracking-widest font-black">
              <th class="px-6 py-5">ID</th>
              <th class="px-6 py-5">Ad Soyad</th>
              <th class="px-6 py-5">E-posta</th>
              <th class="px-6 py-5">Kalan İzin</th>
              <th class="px-6 py-5">Rol</th>
              <th class="px-6 py-5 text-right">İşlem</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-gray-50">
            <tr v-for="m in filteredMembers" :key="m.userId || m.UserId" class="hover:bg-blue-50/30 transition-colors group">
              <td class="px-6 py-4 text-sm font-mono text-gray-400">#{{ m.userId ?? m.UserId }}</td>
              <td class="px-6 py-4 font-bold text-gray-800">{{ m.fullName || m.FullName || '—' }}</td>
              <td class="px-6 py-4 text-gray-600 text-sm">{{ m.email || m.Email }}</td>
              <td class="px-6 py-4">
                <div class="flex items-center gap-2">
                  <input
                    type="number"
                    min="0"
                    :value="editingLeaveDays[m.userId ?? m.UserId] ?? (m.remainingLeaveDays ?? m.RemainingLeaveDays ?? 0)"
                    class="w-14 rounded-lg border border-gray-200 px-2 py-1.5 text-sm font-semibold text-blue-600"
                    @input="editingLeaveDays[m.userId ?? m.UserId] = parseInt($event.target.value, 10) || 0"
                  />
                  <span class="text-gray-500 text-sm">gün</span>
                  <button
                    v-if="hasLeaveDaysChanged(m)"
                    type="button"
                    class="text-xs bg-blue-600 text-white px-2.5 py-1.5 rounded-lg hover:bg-blue-700 disabled:opacity-50"
                    :disabled="leaveDaysSaving[m.userId ?? m.UserId]"
                    @click="saveLeaveDays(m)"
                  >
                    {{ leaveDaysSaving[m.userId ?? m.UserId] ? '...' : 'Kaydet' }}
                  </button>
                </div>
              </td>
              <td class="px-6 py-4">
                <span
                  v-if="m.isCreator || m.IsCreator"
                  class="px-2.5 py-1 rounded-lg text-[10px] font-black uppercase tracking-tighter bg-purple-100 text-purple-700"
                >
                  Yönetici
                </span>
                <span v-else class="px-2.5 py-1 rounded-lg text-[10px] font-black uppercase tracking-tighter bg-blue-100 text-blue-700">Çalışan</span>
              </td>
              <td class="px-6 py-4 text-right">
                <button
                  v-if="!(m.isCreator || m.IsCreator)"
                  type="button"
                  class="text-xs text-amber-700 hover:text-amber-900 font-bold hover:underline disabled:opacity-50"
                  :disabled="removeLoading[m.userId ?? m.UserId]"
                  @click="removeFromUnit(m)"
                >
                  {{ removeLoading[m.userId ?? m.UserId] ? '...' : 'Birimden çıkar' }}
                </button>
                <span v-else class="text-gray-400 text-xs">—</span>
              </td>
            </tr>
            <tr v-if="filteredMembers.length === 0">
              <td colspan="6" class="px-6 py-16 text-center text-gray-400 italic bg-gray-50/20">
                <div class="text-4xl mb-2">📭</div>
                {{ searchQuery.trim() ? 'Arama kriterine uygun çalışan bulunamadı.' : 'Bu birimde henüz kayıtlı çalışan yok.' }}
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </template>
  </div>
</template>

<script setup>
definePageMeta({ layout: 'admin', middleware: 'auth' })

const route = useRoute();
const { token } = useAuth();
const unitId = computed(() => route.params.id);
const unitName = ref('');
const members = ref([]);
const searchQuery = ref('');
const isLoading = ref(true);
const editingLeaveDays = ref({});
const leaveDaysSaving = ref({});
const removeLoading = ref({});

const filteredMembers = computed(() => {
  const q = (searchQuery.value || '').trim().toLowerCase();
  if (!q) return members.value;
  return members.value.filter((m) => {
    const name = (m.fullName ?? m.FullName ?? '').toString().toLowerCase();
    const email = (m.email ?? m.Email ?? '').toString().toLowerCase();
    return name.includes(q) || email.includes(q);
  });
});

function hasLeaveDaysChanged(m) {
  const id = m.userId ?? m.UserId;
  const current = (m.remainingLeaveDays ?? m.RemainingLeaveDays) ?? 0;
  const edited = editingLeaveDays.value[id];
  return edited !== undefined && edited !== current;
}

async function saveLeaveDays(m) {
  const id = m.userId ?? m.UserId;
  const uId = unitId.value;
  if (!id || !uId) return;
  const days = editingLeaveDays.value[id] ?? (m.remainingLeaveDays ?? m.RemainingLeaveDays) ?? 0;
  leaveDaysSaving.value = { ...leaveDaysSaving.value, [id]: true };
  try {
    await $fetch(`https://localhost:44365/api/Units/${uId}/users/${id}/remaining-leave-days`, {
      method: 'PATCH',
      headers: { Authorization: `Bearer ${token.value}`, 'Content-Type': 'application/json' },
      body: { RemainingLeaveDays: days }
    });
    const idx = members.value.findIndex((x) => (x.userId ?? x.UserId) === id);
    if (idx !== -1) {
      members.value[idx] = { ...members.value[idx], remainingLeaveDays: days, RemainingLeaveDays: days };
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

async function removeFromUnit(m) {
  const id = m.userId ?? m.UserId;
  const uId = unitId.value;
  if (!id || !uId) return;
  const ok = await useConfirm().showConfirm({
    title: 'Çalışanı birimden çıkar',
    message: `"${m.fullName || m.FullName || m.email || m.Email}" birimden çıkarılsın mı? Hesabı silinmez, sadece birim ataması kaldırılır.`,
    confirmText: 'Çıkar',
    danger: true
  });
  if (!ok) return;
  removeLoading.value = { ...removeLoading.value, [id]: true };
  try {
    await $fetch(`https://localhost:44365/api/Units/${uId}/users/${id}`, {
      method: 'DELETE',
      headers: { Authorization: `Bearer ${token.value}` }
    });
    members.value = members.value.filter((x) => (x.userId ?? x.UserId) !== id);
    useToast().showSuccess('Çalışan birimden çıkarıldı.');
  } catch (e) {
    const msg = e?.data?.message ?? e?.data?.Message ?? (typeof e?.data === 'string' ? e.data : null) ?? 'İşlem başarısız.';
    useToast().showError(msg);
  } finally {
    removeLoading.value = { ...removeLoading.value, [id]: false };
  }
}

const fetchMembers = async () => {
  const id = unitId.value;
  if (!id || !token.value) return;
  isLoading.value = true;
  try {
    const data = await $fetch(`https://localhost:44365/api/Units/${id}/members`, {
      headers: { Authorization: `Bearer ${token.value}` }
    });
    const list = data?.members ?? data?.Members ?? (Array.isArray(data) ? data : []);
    members.value = Array.isArray(list) ? list : [];
    const units = await $fetch('https://localhost:44365/api/Units', { headers: { Authorization: `Bearer ${token.value}` } });
    const arr = Array.isArray(units) ? units : [];
    const unit = arr.find((u) => (u.id ?? u.Id) === Number(id));
    unitName.value = unit ? (unit.name ?? unit.Name) : '';
  } catch (e) {
    members.value = [];
    unitName.value = '';
  } finally {
    isLoading.value = false;
  }
};

onMounted(() => { fetchMembers(); });
watch(unitId, () => { fetchMembers(); });
</script>
