<template>
  <div class="max-w-6xl mx-auto">
    <h1 class="text-2xl font-bold text-gray-800 mb-2">Sistem Yönetimi</h1>
    <p class="text-gray-600 mb-6">Kullanıcıları listeleyebilir, rol atayabilir ve silebilirsiniz.</p>

    <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-4 mb-4">
      <label class="block text-sm font-medium text-gray-700 mb-2">Arama</label>
      <div class="flex gap-3 flex-wrap items-end">
        <div class="flex-1 min-w-[140px]">
          <label class="block text-xs text-gray-500 mb-1">Ad soyad</label>
          <input
            v-model="searchName"
            type="text"
            placeholder="Ad veya soyad..."
            class="w-full px-3 py-2.5 border border-gray-200 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent outline-none text-sm"
          />
        </div>
        <div class="flex-1 min-w-[140px]">
          <label class="block text-xs text-gray-500 mb-1">E-posta</label>
          <input
            v-model="searchEmail"
            type="text"
            placeholder="E-posta adresi veya parçası..."
            class="w-full px-3 py-2.5 border border-gray-200 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent outline-none text-sm"
          />
        </div>
        <div class="flex-1 min-w-[140px]">
          <label class="block text-xs text-gray-500 mb-1">E-posta uzantısı</label>
          <input
            v-model="searchDomain"
            type="text"
            placeholder="Örn: gmail.com, seneka.com"
            class="w-full px-3 py-2.5 border border-gray-200 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent outline-none text-sm"
          />
        </div>
        <button
          v-if="hasSearch"
          type="button"
          class="px-3 py-2.5 text-gray-500 hover:text-gray-700 text-sm font-medium"
          aria-label="Aramayı temizle"
          @click="clearSearch"
        >
          Temizle
        </button>
        <div class="relative">
          <button
            type="button"
            class="flex items-center gap-2 px-4 py-2.5 border border-gray-200 rounded-lg bg-white text-sm font-medium text-gray-700 hover:bg-gray-50 focus:ring-2 focus:ring-blue-500 outline-none transition"
            :class="{ 'ring-2 ring-blue-500 border-blue-500': filterOpen }"
            @click="filterOpen = !filterOpen"
          >
            Filtrele
            <span class="text-gray-400">▼</span>
          </button>
          <div v-if="filterOpen" class="fixed inset-0 z-10" aria-hidden="true" @click="filterOpen = false" />
          <div
            v-if="filterOpen"
            class="absolute right-0 top-full mt-1 py-1 w-48 bg-white border border-gray-200 rounded-lg shadow-lg z-20"
          >
            <button
              v-for="opt in filterOptions"
              :key="opt.value"
              type="button"
              class="w-full text-left px-4 py-2 text-sm hover:bg-gray-100 transition"
              :class="{ 'bg-blue-50 text-blue-700 font-medium': selectedFilter === opt.value }"
              @click="selectedFilter = opt.value; filterOpen = false"
            >
              {{ opt.label }}
            </button>
          </div>
        </div>
        <button
          type="button"
          class="flex items-center gap-2 px-4 py-2.5 border border-gray-200 rounded-lg bg-white text-sm font-medium text-gray-700 hover:bg-gray-50 focus:ring-2 focus:ring-blue-500 outline-none transition"
          :class="{ 'ring-2 ring-blue-500 border-blue-500 bg-blue-50': showAll }"
          @click="showAll = !showAll"
        >
          Hepsini göster
        </button>
      </div>
      <p class="mt-2 text-sm text-gray-500 hint-line">
        <template v-if="showAll || hasSearch">
          {{ filteredUsers.length }} kullanıcı listeleniyor
          <span v-if="filteredUsers.length < users.length">(toplam {{ users.length }})</span>
          <span v-if="selectedFilter !== 'hicbiri'" class="text-blue-600"> · Filtre: {{ filterOptions.find(o => o.value === selectedFilter)?.label }}</span>
        </template>
        <template v-else>Ad soyad, e-posta veya e-posta uzantısından en az birini doldurarak arama yapın. Veya "Hepsini göster" ile tüm listeyi açın.</template>
      </p>
    </div>

    <div class="bg-white rounded-2xl shadow-sm border border-gray-100 overflow-hidden table-fixed-box">
      <div v-if="loading" class="table-fixed-inner table-placeholder">
        <p class="text-gray-500">Yükleniyor...</p>
      </div>
      <template v-else>
        <div v-if="!showAll && !hasSearch" class="table-fixed-inner table-placeholder">
          <p class="text-gray-500">Listeyi görmek için yukarıdaki <strong>Hepsini göster</strong> butonuna tıklayın.</p>
        </div>
        <div v-else class="table-fixed-inner table-scroll-wrap">
          <div v-if="!filteredUsers.length" class="table-placeholder">
            <p class="text-gray-500">{{ hasSearch ? 'Arama kriterine uygun kullanıcı bulunamadı.' : 'Kullanıcı bulunamadı.' }}</p>
          </div>
          <div v-else class="overflow-x-auto overflow-y-auto h-full">
            <table class="w-full text-left">
              <thead>
                <tr class="text-gray-400 text-xs font-semibold uppercase tracking-widest border-b border-gray-100 bg-gray-50/80">
                  <th class="px-6 py-4">Ad Soyad / E-posta</th>
                  <th class="px-6 py-4">Rol</th>
                  <th class="px-6 py-4">Birim</th>
                  <th class="px-6 py-4">Oluşturulma</th>
                  <th class="px-6 py-4">İşlemler</th>
                </tr>
              </thead>
              <tbody>
                <tr
                  v-for="u in filteredUsers"
                  :key="u.Id ?? u.id"
                  class="border-t border-gray-100 hover:bg-gray-50/50 transition"
                >
                  <td class="px-6 py-4">
                    <p class="font-bold text-gray-900">{{ (u.FullName ?? u.fullName) || '—' }}</p>
                    <p class="text-sm text-gray-500">{{ u.Email ?? u.email }}</p>
                  </td>
                  <td class="px-6 py-4">
                    <template v-if="isAdminRole(u)">
                      <span class="inline-flex items-center px-3 py-1 rounded-full text-xs font-bold bg-purple-100 text-purple-800">Sistem Yöneticisi</span>
                    </template>
                    <select
                      v-else
                      :value="roleValue(u)"
                      class="rounded-lg border border-gray-200 bg-white px-3 py-2 text-sm font-medium text-gray-800 focus:ring-2 focus:ring-blue-500 focus:border-transparent outline-none cursor-pointer"
                      :disabled="roleLoading[u.Id ?? u.id]"
                      @change="onRoleChange(u, $event)"
                    >
                      <option value="User">Kullanıcı</option>
                      <option value="Manager">Yönetici</option>
                    </select>
                  </td>
                  <td class="px-6 py-4 text-gray-700">{{ (u.UnitName ?? u.unitName) || '—' }}</td>
                  <td class="px-6 py-4 text-gray-600 text-sm">{{ formatDate(u.CreatedAt ?? u.createdAt) }}</td>
                  <td class="px-6 py-4">
                    <button
                      v-if="!isAdminRole(u)"
                      type="button"
                      class="px-3 py-1.5 bg-red-600 text-white text-sm font-medium rounded-lg hover:bg-red-700 transition disabled:opacity-50"
                      :disabled="deleteLoading[u.Id ?? u.id]"
                      @click="confirmDelete(u)"
                    >
                      {{ deleteLoading[u.Id ?? u.id] ? 'Siliniyor...' : 'Sil' }}
                    </button>
                    <span v-else class="text-xs text-gray-400">—</span>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </template>
    </div>
  </div>
</template>

<script setup>
definePageMeta({ layout: 'admin', middleware: 'auth' });

const { token, isSystemAdmin } = useAuth();
const users = ref([]);
const searchName = ref('');
const searchEmail = ref('');
const searchDomain = ref('');
const loading = ref(true);
const roleLoading = ref({});
const deleteLoading = ref({});
const filterOpen = ref(false);
const selectedFilter = ref('hicbiri');
const showAll = ref(false);

const filterOptions = [
  { value: 'hicbiri', label: 'Hiçbiri' },
  { value: 'yoneticiler', label: 'Yöneticiler' },
  { value: 'kullanicilar', label: 'Kullanıcılar' },
  { value: 'sirketler', label: 'Şirketler' },
  { value: 'birimler', label: 'Birimler' },
];

function applyRoleFilter(list, filter) {
  if (filter === 'hicbiri' || filter === 'sirketler') return list;
  if (filter === 'yoneticiler') return list.filter((u) => { const r = (u.Role ?? u.role ?? '').toLowerCase(); return r === 'manager' || r === 'admin'; });
  if (filter === 'kullanicilar') return list.filter((u) => (u.Role ?? u.role ?? '').toLowerCase() === 'user');
  if (filter === 'birimler') return list.filter((u) => u.UnitId != null || u.unitId != null);
  return list;
}

const hasSearch = computed(() => {
  const n = (searchName.value || '').trim();
  const e = (searchEmail.value || '').trim();
  const d = (searchDomain.value || '').trim();
  return !!n || !!e || !!d;
});

function clearSearch() {
  searchName.value = '';
  searchEmail.value = '';
  searchDomain.value = '';
}

const filteredUsers = computed(() => {
  if (!showAll.value && !hasSearch.value) return [];
  const qName = (searchName.value || '').trim().toLowerCase();
  const qEmail = (searchEmail.value || '').trim().toLowerCase();
  const qDomain = (searchDomain.value || '').trim().toLowerCase();
  let list = !hasSearch.value
    ? [...users.value]
    : users.value.filter((u) => {
        const name = (u.FullName ?? u.fullName ?? '').toString().toLowerCase();
        const email = (u.Email ?? u.email ?? '').toString().toLowerCase();
        const domain = email.includes('@') ? email.split('@')[1] || '' : '';
        const matchName = !qName || name.includes(qName);
        const matchEmail = !qEmail || email.includes(qEmail);
        const matchDomain = !qDomain || domain.includes(qDomain);
        return matchName && matchEmail && matchDomain;
      });
  return applyRoleFilter(list, selectedFilter.value);
});

onMounted(() => {
  if (!token.value) {
    navigateTo('/login');
    return;
  }
  if (!isSystemAdmin.value) {
    navigateTo('/dashboard');
    return;
  }
  fetchUsers();
});

function roleValue(u) {
  const r = (u.Role ?? u.role ?? '').toString();
  if (r.toLowerCase() === 'manager') return 'Manager';
  return 'User';
}

function isAdminRole(u) {
  return (u.Role ?? u.role ?? '').toLowerCase() === 'admin';
}

function formatDate(value) {
  if (!value) return '—';
  let v = value;
  if (typeof v === 'string' && /^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}/.test(v) && !/Z|[+-]\d{2}:?\d{2}$/.test(v))
    v = v.replace(/\.\d{3}$/, '') + 'Z';
  const d = new Date(v);
  return d.toLocaleString('tr-TR', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
    timeZone: 'Europe/Istanbul'
  });
}

async function fetchUsers() {
  loading.value = true;
  try {
    const data = await $fetch('https://localhost:44365/api/Admin/users', {
      headers: { Authorization: `Bearer ${token.value}` }
    });
    users.value = Array.isArray(data) ? data : [];
  } catch (e) {
    users.value = [];
  } finally {
    loading.value = false;
  }
}

async function onRoleChange(u, event) {
  const newRole = event.target.value;
  const currentRole = roleValue(u);
  if (newRole === currentRole) return;
  const ok = await useConfirm().showConfirm({
    title: 'Rol değişikliği',
    message: `Rolü "${newRole}" olarak değiştirmek istediğinize emin misiniz?`,
  });
  if (!ok) {
    event.target.value = currentRole;
    return;
  }
  const id = u.Id ?? u.id;
  if (newRole === 'Manager') {
    promoteToManager(id, u, event);
  } else {
    demoteToUser(id, u, event);
  }
}

async function promoteToManager(id, u, event) {
  roleLoading.value = { ...roleLoading.value, [id]: true };
  try {
    await $fetch(`https://localhost:44365/api/Admin/promote-to-manager/${id}`, {
      method: 'POST',
      headers: { Authorization: `Bearer ${token.value}` }
    });
    const idx = users.value.findIndex(x => (x.Id ?? x.id) === id);
    if (idx !== -1) {
      users.value[idx] = { ...users.value[idx], Role: 'Manager', role: 'Manager' };
    }
  } catch (e) {
    const msg = (e?.data?.message) || 'Atama yapılamadı.';
    useToast().showError(msg);
    event.target.value = 'User';
  } finally {
    roleLoading.value = { ...roleLoading.value, [id]: false };
  }
}

async function demoteToUser(id, u, event) {
  roleLoading.value = { ...roleLoading.value, [id]: true };
  try {
    await $fetch(`https://localhost:44365/api/Admin/demote-to-user/${id}`, {
      method: 'POST',
      headers: { Authorization: `Bearer ${token.value}` }
    });
    const idx = users.value.findIndex(x => (x.Id ?? x.id) === id);
    if (idx !== -1) {
      users.value[idx] = { ...users.value[idx], Role: 'User', role: 'User' };
    }
  } catch (e) {
    const msg = (e?.data?.message) || 'Yetki düşürülemedi.';
    useToast().showError(msg);
    event.target.value = 'Manager';
  } finally {
    roleLoading.value = { ...roleLoading.value, [id]: false };
  }
}

async function confirmDelete(u) {
  const name = (u.FullName ?? u.fullName) || (u.Email ?? u.email);
  const ok = await useConfirm().showConfirm({
    title: 'Kullanıcıyı sil',
    message: `"${name}" kullanıcısını kalıcı olarak silmek istediğinize emin misiniz?`,
    confirmText: 'Sil',
    danger: true,
  });
  if (!ok) return;
  deleteUser(u);
}

async function deleteUser(u) {
  const id = u.Id ?? u.id;
  if (id == null) return;
  deleteLoading.value = { ...deleteLoading.value, [id]: true };
  try {
    await $fetch(`https://localhost:44365/api/Admin/users/${id}`, {
      method: 'DELETE',
      headers: { Authorization: `Bearer ${token.value}` }
    });
    users.value = users.value.filter(x => (x.Id ?? x.id) !== id);
  } catch (e) {
    const msg = (e?.data?.message) || 'Kullanıcı silinemedi.';
    useToast().showError(msg);
  } finally {
    deleteLoading.value = { ...deleteLoading.value, [id]: false };
  }
}
</script>

<style scoped>
/* Liste kutusu her zaman aynı yükseklikte kalır; sayfa hareket etmez */
.table-fixed-box {
  height: 420px;
}
.table-fixed-inner {
  height: 100%;
  display: flex;
  flex-direction: column;
  align-items: stretch;
}
.table-placeholder {
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 2rem;
  text-align: center;
}
.table-scroll-wrap {
  min-height: 0;
  flex: 1;
  display: flex;
  flex-direction: column;
}
.table-scroll-wrap .table-placeholder {
  flex: 1;
}
.hint-line {
  min-height: 1.5rem;
}
</style>
