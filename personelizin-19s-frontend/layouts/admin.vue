<template>
  <div class="min-h-screen bg-gray-100 flex">
    <aside class="fixed top-0 left-0 z-20 h-screen w-64 bg-blue-900 text-white hidden md:flex md:flex-col border-r border-blue-800">
      <div class="p-4 border-b border-blue-800 flex items-center justify-between gap-2">
        <span class="text-2xl font-bold tracking-tight">Seneka İzin</span>
        <div v-if="isAdmin" class="relative">
          <button
            type="button"
            class="p-1.5 rounded-lg hover:bg-blue-800 transition text-blue-200 hover:text-white"
            :class="{ 'bg-blue-800': menuOpen }"
            aria-haspopup="true"
            :aria-expanded="menuOpen"
            aria-label="Menü"
            @click="menuOpen = !menuOpen"
          >
            <span class="text-lg leading-none">⋮</span>
          </button>
          <div v-if="menuOpen" class="absolute left-0 top-full mt-1 py-1 min-w-[180px] bg-white text-gray-800 rounded-lg shadow-xl border border-gray-100 z-50">
            <button
              type="button"
              class="w-full text-left px-4 py-2.5 text-sm font-medium hover:bg-blue-50 text-gray-700 flex items-center gap-2"
              @click="openCreateUser(); menuOpen = false"
            >
              <span>➕</span> Kullanıcı oluştur
            </button>
          </div>
          <div v-if="menuOpen" class="fixed inset-0 z-40" aria-hidden="true" @click="menuOpen = false" />
        </div>
      </div>
      <nav class="mt-6 px-4 space-y-2 flex-1">
        <NuxtLink
          v-if="userRole && !isSystemAdmin"
          to="/dashboard"
          :class="['flex items-center gap-3 py-3 px-4 rounded transition', isDashboard ? 'bg-blue-700' : 'hover:bg-blue-800']"
        >
          <span>🏠</span> İzinlerim
        </NuxtLink>

        <template v-if="isSystemAdmin">
          <div class="pt-4 pb-2 px-4 text-xs font-bold text-blue-200 uppercase tracking-widest">Sistem</div>
          <NuxtLink
            to="/admin-panel"
            :class="['flex items-center gap-3 py-3 px-4 rounded transition', isAdminPanel ? 'bg-blue-700' : 'hover:bg-blue-800']"
          >
            <span>⚙️</span> Sistem Yönetimi
          </NuxtLink>
        </template>

        <template v-if="isAdmin">
          <div class="pt-4 pb-2 px-4 text-xs font-bold text-blue-200 uppercase tracking-widest">Yönetim</div>
          <NuxtLink
            to="/admin/users"
            :class="['flex items-center gap-3 py-3 px-4 rounded transition', isPersonelListesi ? 'bg-blue-700' : 'hover:bg-blue-800']"
          >
            <span>👥</span> Personel Listesi
          </NuxtLink>
          <NuxtLink
            to="/admin/requests"
            :class="['flex items-center justify-between gap-3 py-3 px-4 rounded transition', isIzinTalepleri ? 'bg-blue-700' : 'hover:bg-blue-800']"
          >
            <span class="flex items-center gap-3">
              <span>📄</span> İzin Talepleri
            </span>
            <span
              v-if="pendingRequestsCount > 0"
              class="min-w-[22px] h-[22px] rounded-full bg-red-500 text-white text-xs font-bold flex items-center justify-center flex-shrink-0"
              aria-label="Bekleyen talep sayısı"
            >
              {{ pendingRequestsCount > 99 ? '99+' : pendingRequestsCount }}
            </span>
          </NuxtLink>
          <NuxtLink
            to="/admin/units"
            :class="['flex items-center gap-3 py-3 px-4 rounded transition', isBirimYonetimi ? 'bg-blue-700' : 'hover:bg-blue-800']"
          >
            <span>🏢</span> Birim Yönetimi
          </NuxtLink>
        </template>
      </nav>

      <div class="relative p-4 border-t border-blue-800">
        <button
          type="button"
          class="w-full flex items-center gap-3 rounded-xl px-3 py-2.5 hover:bg-blue-800 transition text-left"
          :class="{ 'bg-blue-800': profileMenuOpen }"
          @click="profileMenuOpen = !profileMenuOpen"
        >
          <div class="w-9 h-9 rounded-full bg-white/20 flex items-center justify-center font-bold uppercase">
            {{ displayName.charAt(0) }}
          </div>
          <div class="min-w-0 flex-1">
            <p class="text-sm font-semibold truncate">{{ displayName }}</p>
            <p class="text-xs text-blue-100 truncate">{{ roleLabel(userRole) }}</p>
          </div>
        </button>

        <div
          v-if="profileMenuOpen"
          class="absolute left-4 right-4 bottom-[72px] rounded-xl bg-white text-gray-800 shadow-xl border border-gray-100 overflow-hidden z-50"
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
            @click="handleLogout"
          >
            <span>🚪</span> Oturumu kapat
          </button>
        </div>
        <div v-if="profileMenuOpen" class="fixed inset-0 z-40" aria-hidden="true" @click="profileMenuOpen = false" />
      </div>
    </aside>

    <main class="flex-1 min-w-0 md:ml-64 overflow-y-auto">
      <header class="bg-white shadow-sm px-8 py-4 flex items-center sticky top-0 z-10">
        <h2 class="text-xl font-semibold text-gray-800">Yönetim Paneli</h2>
      </header>

      <div class="p-8 bg-gray-100 min-h-[calc(100vh-72px)]">
        <slot />
      </div>
    </main>

    <!-- Kullanıcı oluşturma modalı -->
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

    <Teleport to="body">
      <div
        v-if="createUserModalOpen"
        class="fixed inset-0 z-[100] flex items-center justify-center p-4 bg-black/50"
        role="dialog"
        aria-modal="true"
        aria-labelledby="create-user-title"
      >
        <div class="bg-white rounded-2xl shadow-xl border border-gray-100 w-full max-w-md" @click.stop>
          <div class="p-6 border-b border-gray-100">
            <h2 id="create-user-title" class="text-xl font-bold text-gray-800">Yeni Kullanıcı Oluştur</h2>
            <p class="text-sm text-gray-500 mt-1">E-posta, ad soyad ve şifre girin.</p>
          </div>
          <form class="p-6 space-y-4" @submit.prevent="submitCreateUser">
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">E-posta *</label>
              <input
                v-model="createForm.email"
                type="email"
                required
                class="w-full px-4 py-2.5 border border-gray-200 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent outline-none"
                placeholder="ornek@firma.com"
              />
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Ad Soyad</label>
              <input
                v-model="createForm.fullName"
                type="text"
                maxlength="100"
                class="w-full px-4 py-2.5 border border-gray-200 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent outline-none"
                placeholder="Ad Soyad"
              />
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Şifre *</label>
              <input
                v-model="createForm.password"
                type="password"
                required
                class="w-full px-4 py-2.5 border border-gray-200 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent outline-none"
                placeholder="••••••••"
              />
            </div>
            <div v-if="isSystemAdmin">
              <label class="block text-sm font-medium text-gray-700 mb-1">Rol</label>
              <select
                v-model="createForm.role"
                class="w-full px-4 py-2.5 border border-gray-200 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent outline-none"
              >
                <option value="User">Kullanıcı</option>
                <option value="Manager">Yönetici</option>
                <option value="Admin">Sistem Yöneticisi</option>
              </select>
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Birim</label>
              <select
                v-model="createForm.unitId"
                class="w-full px-4 py-2.5 border border-gray-200 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent outline-none"
              >
                <option :value="null">— Yöneticinin birimlerinden seçin —</option>
                <option v-for="u in createModalUnits" :key="u.Id ?? u.id" :value="(u.Id ?? u.id)">{{ u.Name ?? u.name }}</option>
              </select>
              <p v-if="createModalUnitsLoading" class="mt-1 text-xs text-gray-500">Birimler yükleniyor...</p>
              <p v-else-if="createModalUnits.length === 0" class="mt-1 text-xs text-amber-600">Henüz birim yok. Önce Birim Yönetimi’nden birim oluşturun.</p>
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Kalan izin (gün)</label>
              <input
                v-model.number="createForm.remainingLeaveDays"
                type="number"
                min="0"
                class="w-full px-4 py-2.5 border border-gray-200 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent outline-none"
              />
            </div>
            <div class="flex gap-3 pt-2">
              <button
                type="button"
                class="flex-1 px-4 py-2.5 border border-gray-200 rounded-xl font-medium text-gray-700 hover:bg-gray-50 transition"
                @click="closeCreateUserModal()"
              >
                İptal
              </button>
              <button
                type="submit"
                class="flex-1 px-4 py-2.5 bg-blue-600 text-white rounded-xl font-medium hover:bg-blue-700 transition disabled:opacity-50"
                :disabled="createSubmitting"
              >
                {{ createSubmitting ? 'Kaydediliyor...' : 'Oluştur' }}
              </button>
            </div>
          </form>
        </div>
        <div class="absolute inset-0 -z-10" aria-hidden="true" @click="closeCreateUserModal()" />
      </div>
    </Teleport>
  </div>
</template>

<script setup>
const route = useRoute();
const { clearUser, setUser, userName, userRole, isAdmin, isSystemAdmin } = useAuth();
const { token } = useAuth();
const { isOpen: createUserModalOpen, open: openCreateUserModal, close: closeCreateUserModal, notifyCreated } = useCreateUserModal();
const { roleLabel } = useTurkishLabels();
const { count: pendingRequestsCount, refresh: refreshPendingCount } = usePendingRequestsCount();
const toast = useToast();

const menuOpen = ref(false);
const profileMenuOpen = ref(false);
const editNameModalOpen = ref(false);
const editNameSubmitting = ref(false);
const editFullName = ref('');
const createForm = ref({
  email: '',
  fullName: '',
  password: '',
  role: 'User',
  unitId: null,
  remainingLeaveDays: 14,
});
const createSubmitting = ref(false);
const createModalUnits = ref([]);
const createModalUnitsLoading = ref(false);

async function fetchUnitsForModal() {
  createModalUnitsLoading.value = true;
  const t = token.value || (import.meta.client && typeof localStorage !== 'undefined' ? localStorage.getItem('token') : null);
  try {
    if (t) {
      const data = await $fetch('https://localhost:44365/api/Units', { headers: { Authorization: `Bearer ${t}` } });
      createModalUnits.value = Array.isArray(data) ? data : (data && Array.isArray(data.data) ? data.data : (data && data.data ? [data.data] : []));
    } else {
      createModalUnits.value = [];
    }
  } catch (_) {
    createModalUnits.value = [];
  } finally {
    createModalUnitsLoading.value = false;
  }
}

async function openCreateUser() {
  createForm.value = { email: '', fullName: '', password: '', role: 'User', unitId: null, remainingLeaveDays: 14 };
  openCreateUserModal();
  createModalUnits.value = [];
  await fetchUnitsForModal();
}

watch(createUserModalOpen, (isOpen) => {
  if (isOpen) fetchUnitsForModal();
});

async function submitCreateUser() {
  if (createSubmitting.value) return;
  createSubmitting.value = true;
  try {
    await $fetch('https://localhost:44365/api/Admin/create-user', {
      method: 'POST',
      headers: {
        Authorization: `Bearer ${token.value}`,
        'Content-Type': 'application/json',
      },
      body: {
        Email: createForm.value.email.trim(),
        FullName: createForm.value.fullName?.trim() || createForm.value.email.trim(),
        Password: createForm.value.password,
        Role: createForm.value.role,
        UnitId: createForm.value.unitId != null && createForm.value.unitId !== '' ? Number(createForm.value.unitId) : null,
        RemainingLeaveDays: createForm.value.remainingLeaveDays ?? 14,
      },
    });
    toast.showSuccess('Kullanıcı başarıyla oluşturuldu.');
    closeCreateUserModal();
    notifyCreated();
  } catch (e) {
    const msg = e?.data?.message ?? e?.data?.Message ?? (typeof e?.data === 'string' ? e.data : null) ?? 'Kullanıcı oluşturulamadı.';
    toast.showError(msg);
  } finally {
    createSubmitting.value = false;
  }
}

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

const accountTypeLabel = computed(() => {
  if (isSystemAdmin.value) return 'Sistem Yöneticisi';
  return isAdmin.value ? 'Yönetici Hesabı' : 'Kullanıcı Hesabı';
});
const displayName = computed(() => userName.value || 'Kullanıcı');

const isDashboard = computed(() => route.path === '/dashboard');
const isAdminPanel = computed(() => route.path === '/admin-panel');
const isPersonelListesi = computed(() => route.path === '/admin/users');
const isIzinTalepleri = computed(() => route.path === '/admin/requests');
const isBirimYonetimi = computed(() => route.path === '/admin/units');

onMounted(() => {
  if (isAdmin.value && token.value) refreshPendingCount();
});
watch([token, isAdmin], () => {
  if (isAdmin.value && token.value) refreshPendingCount();
}, { immediate: true });

const handleLogout = () => {
  profileMenuOpen.value = false;
  clearUser();
  navigateTo('/login');
};
</script>
