<template>
  <div>
    <h1 class="text-3xl font-bold mb-6 text-gray-800">İzin Talepleri</h1>
    <p class="text-gray-500 mb-6">Birimlerinizdeki tüm izin taleplerini burada görüntüleyip onaylayabilir veya reddedebilirsiniz.</p>

    <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-4 mb-4 space-y-4">
      <div class="flex flex-wrap items-center gap-3">
        <span class="text-sm font-medium text-gray-700">Durum filtresi:</span>
        <div class="flex flex-wrap gap-2">
          <button
            v-for="opt in statusFilterOptions"
            :key="opt.value"
            type="button"
            class="px-4 py-2 rounded-xl text-sm font-semibold transition"
            :class="statusFilter === opt.value
              ? opt.activeClass
              : 'bg-gray-100 text-gray-600 hover:bg-gray-200'"
            @click="statusFilter = opt.value"
          >
            {{ opt.label }} ({{ opt.count }})
          </button>
        </div>
      </div>
      <div>
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
          {{ filteredByStatus.length }} talep listeleniyor
        </p>
      </div>
    </div>

    <div v-if="isLoading" class="flex items-center justify-center py-12 text-gray-500">
      <span class="animate-pulse">Yükleniyor...</span>
    </div>

    <template v-else>
      <!-- Tek filtre seçiliyse tek tablo -->
      <div v-if="statusFilter !== 'hepsi'" class="bg-white rounded-2xl shadow-sm border border-gray-100 overflow-hidden">
        <h2 class="text-lg font-bold px-6 py-3 border-b border-gray-100" :class="currentFilterTitleClass">
          {{ statusFilterOptions.find(o => o.value === statusFilter)?.label }}
        </h2>
        <div class="overflow-x-auto">
          <table class="w-full text-left">
            <thead class="bg-gray-50 border-b border-gray-100">
              <tr class="text-gray-500 text-xs uppercase tracking-widest font-bold">
                <th class="px-6 py-4">Birim</th>
                <th class="px-6 py-4">E-posta</th>
                <th class="px-6 py-4">Tür</th>
                <th class="px-6 py-4">Başlangıç</th>
                <th class="px-6 py-4">Bitiş</th>
                <th class="px-6 py-4">Durum</th>
                <th v-if="statusFilter === 'beklemede'" class="px-6 py-4 text-right">İşlem</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-gray-50">
              <tr
                v-for="req in filteredByStatus"
                :key="req.id || req.Id"
                class="hover:bg-gray-50/50 transition cursor-pointer"
                @click="openDetailModal(req)"
              >
                <td class="px-6 py-4 text-sm font-medium text-gray-700">{{ req.unitName || req.UnitName || '—' }}</td>
                <td class="px-6 py-4 text-sm">{{ req.userEmail || req.UserEmail }}</td>
                <td class="px-6 py-4 text-sm">{{ req.type || req.Type }}</td>
                <td class="px-6 py-4 text-sm">{{ formatDate(req.startDate || req.StartDate) }}</td>
                <td class="px-6 py-4 text-sm">{{ formatDate(req.endDate || req.EndDate) }}</td>
                <td class="px-6 py-4">
                  <span :class="getStatusClass(req.status || req.Status)" class="px-2.5 py-1 rounded-lg text-xs font-bold uppercase">{{ statusLabel(req.status || req.Status) }}</span>
                </td>
                <td v-if="statusFilter === 'beklemede'" class="px-6 py-4 text-right" @click.stop>
                  <button @click="openSignModal(req)" class="text-green-600 hover:underline font-bold mr-3">Onayla ve İmzala</button>
                  <button @click="rejectRequest(req.id || req.Id)" class="text-red-600 hover:underline font-bold">Reddet</button>
                </td>
              </tr>
              <tr v-if="filteredByStatus.length === 0">
                <td :colspan="statusFilter === 'beklemede' ? 7 : 6" class="px-6 py-8 text-center text-gray-400">Bu filtreye uygun talep yok.</td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <!-- Hepsi seçiliyse: Onaylanan, Reddedilen, Bekleyen ayrı bloklar -->
      <template v-else>
        <div class="mb-8">
          <h2 class="text-lg font-bold text-green-800 mb-3 flex items-center gap-2">
            <span class="bg-green-100 text-green-700 px-2.5 py-0.5 rounded-lg text-sm">Onaylanan talepler</span>
            <span class="text-gray-500 font-normal text-sm">({{ approvedRequests.length }})</span>
          </h2>
          <div class="bg-white rounded-2xl shadow-sm border border-gray-100 overflow-hidden">
            <div class="overflow-x-auto">
              <table class="w-full text-left">
                <thead class="bg-gray-50 border-b border-gray-100">
                  <tr class="text-gray-500 text-xs uppercase tracking-widest font-bold">
                    <th class="px-6 py-4">Birim</th>
                    <th class="px-6 py-4">E-posta</th>
                    <th class="px-6 py-4">Tür</th>
                    <th class="px-6 py-4">Başlangıç</th>
                    <th class="px-6 py-4">Bitiş</th>
                    <th class="px-6 py-4">Durum</th>
                  </tr>
                </thead>
                <tbody class="divide-y divide-gray-50">
                  <tr
                    v-for="req in approvedRequests"
                    :key="req.id || req.Id"
                    class="hover:bg-gray-50/50 transition cursor-pointer"
                    @click="openDetailModal(req)"
                  >
                    <td class="px-6 py-4 text-sm font-medium text-gray-700">{{ req.unitName || req.UnitName || '—' }}</td>
                    <td class="px-6 py-4 text-sm">{{ req.userEmail || req.UserEmail }}</td>
                    <td class="px-6 py-4 text-sm">{{ req.type || req.Type }}</td>
                    <td class="px-6 py-4 text-sm">{{ formatDate(req.startDate || req.StartDate) }}</td>
                    <td class="px-6 py-4 text-sm">{{ formatDate(req.endDate || req.EndDate) }}</td>
                    <td class="px-6 py-4">
                      <span :class="getStatusClass(req.status || req.Status)" class="px-2.5 py-1 rounded-lg text-xs font-bold uppercase">{{ statusLabel(req.status || req.Status) }}</span>
                    </td>
                  </tr>
                  <tr v-if="approvedRequests.length === 0">
                    <td colspan="6" class="px-6 py-8 text-center text-gray-400">Onaylanan talep yok.</td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>
        </div>

        <div class="mb-8">
          <h2 class="text-lg font-bold text-red-800 mb-3 flex items-center gap-2">
            <span class="bg-red-100 text-red-700 px-2.5 py-0.5 rounded-lg text-sm">Reddedilen talepler</span>
            <span class="text-gray-500 font-normal text-sm">({{ rejectedRequests.length }})</span>
          </h2>
          <div class="bg-white rounded-2xl shadow-sm border border-gray-100 overflow-hidden">
            <div class="overflow-x-auto">
              <table class="w-full text-left">
                <thead class="bg-gray-50 border-b border-gray-100">
                  <tr class="text-gray-500 text-xs uppercase tracking-widest font-bold">
                    <th class="px-6 py-4">Birim</th>
                    <th class="px-6 py-4">E-posta</th>
                    <th class="px-6 py-4">Tür</th>
                    <th class="px-6 py-4">Başlangıç</th>
                    <th class="px-6 py-4">Bitiş</th>
                    <th class="px-6 py-4">Durum</th>
                  </tr>
                </thead>
                <tbody class="divide-y divide-gray-50">
                  <tr
                    v-for="req in rejectedRequests"
                    :key="req.id || req.Id"
                    class="hover:bg-gray-50/50 transition cursor-pointer"
                    @click="openDetailModal(req)"
                  >
                    <td class="px-6 py-4 text-sm font-medium text-gray-700">{{ req.unitName || req.UnitName || '—' }}</td>
                    <td class="px-6 py-4 text-sm">{{ req.userEmail || req.UserEmail }}</td>
                    <td class="px-6 py-4 text-sm">{{ req.type || req.Type }}</td>
                    <td class="px-6 py-4 text-sm">{{ formatDate(req.startDate || req.StartDate) }}</td>
                    <td class="px-6 py-4 text-sm">{{ formatDate(req.endDate || req.EndDate) }}</td>
                    <td class="px-6 py-4">
                      <span :class="getStatusClass(req.status || req.Status)" class="px-2.5 py-1 rounded-lg text-xs font-bold uppercase">{{ statusLabel(req.status || req.Status) }}</span>
                    </td>
                  </tr>
                  <tr v-if="rejectedRequests.length === 0">
                    <td colspan="6" class="px-6 py-8 text-center text-gray-400">Reddedilen talep yok.</td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>
        </div>

        <div>
          <h2 class="text-lg font-bold text-amber-800 mb-3 flex items-center gap-2">
            <span class="bg-amber-100 text-amber-700 px-2.5 py-0.5 rounded-lg text-sm">Bekleyen talepler</span>
            <span class="text-gray-500 font-normal text-sm">({{ pendingRequests.length }})</span>
          </h2>
          <div class="bg-white rounded-2xl shadow-sm border border-gray-100 overflow-hidden">
            <div class="overflow-x-auto">
              <table class="w-full text-left">
                <thead class="bg-gray-50 border-b border-gray-100">
                  <tr class="text-gray-500 text-xs uppercase tracking-widest font-bold">
                    <th class="px-6 py-4">Birim</th>
                    <th class="px-6 py-4">E-posta</th>
                    <th class="px-6 py-4">Tür</th>
                    <th class="px-6 py-4">Başlangıç</th>
                    <th class="px-6 py-4">Bitiş</th>
                    <th class="px-6 py-4">Durum</th>
                    <th class="px-6 py-4 text-right">İşlem</th>
                  </tr>
                </thead>
                <tbody class="divide-y divide-gray-50">
                  <tr
                    v-for="req in pendingRequests"
                    :key="req.id || req.Id"
                    class="hover:bg-gray-50/50 transition cursor-pointer"
                    @click="openDetailModal(req)"
                  >
                    <td class="px-6 py-4 text-sm font-medium text-gray-700">{{ req.unitName || req.UnitName || '—' }}</td>
                    <td class="px-6 py-4 text-sm">{{ req.userEmail || req.UserEmail }}</td>
                    <td class="px-6 py-4 text-sm">{{ req.type || req.Type }}</td>
                    <td class="px-6 py-4 text-sm">{{ formatDate(req.startDate || req.StartDate) }}</td>
                    <td class="px-6 py-4 text-sm">{{ formatDate(req.endDate || req.EndDate) }}</td>
                    <td class="px-6 py-4">
                      <span :class="getStatusClass(req.status || req.Status)" class="px-2.5 py-1 rounded-lg text-xs font-bold uppercase">{{ statusLabel(req.status || req.Status) }}</span>
                    </td>
                    <td class="px-6 py-4 text-right" @click.stop>
                      <button @click="openSignModal(req)" class="text-green-600 hover:underline font-bold mr-3">Onayla ve İmzala</button>
                      <button @click="rejectRequest(req.id || req.Id)" class="text-red-600 hover:underline font-bold">Reddet</button>
                    </td>
                  </tr>
                  <tr v-if="pendingRequests.length === 0">
                    <td colspan="7" class="px-6 py-8 text-center text-gray-400">Bekleyen talep yok.</td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>
        </div>
      </template>
    </template>

    <div v-if="detailModalOpen" class="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4" @click.self="detailModalOpen = false">
      <div class="bg-white rounded-2xl shadow-xl max-w-lg w-full p-6">
        <h3 class="text-xl font-bold text-gray-800 mb-4">İzin Talebi Detayı</h3>
        <div class="space-y-3 text-sm">
          <p><span class="text-gray-500 font-medium">Birim:</span> {{ selectedRequest ? (selectedRequest.unitName || selectedRequest.UnitName || '—') : '' }}</p>
          <p><span class="text-gray-500 font-medium">E-posta:</span> {{ selectedRequest ? (selectedRequest.userEmail || selectedRequest.UserEmail) : '' }}</p>
          <p><span class="text-gray-500 font-medium">Tür:</span> {{ selectedRequest ? (selectedRequest.type || selectedRequest.Type) : '' }}</p>
          <p><span class="text-gray-500 font-medium">Başlangıç – Bitiş:</span> {{ selectedRequest ? formatDate(selectedRequest.startDate || selectedRequest.StartDate) + ' – ' + formatDate(selectedRequest.endDate || selectedRequest.EndDate) : '' }}</p>
          <p><span class="text-gray-500 font-medium">Durum:</span> {{ selectedRequest ? statusLabel(selectedRequest.status || selectedRequest.Status) : '' }}</p>
          <p class="pt-2 border-t border-gray-100">
            <span class="text-gray-500 font-medium block mb-1">Çalışanın açıklaması:</span>
            <span class="text-gray-800 whitespace-pre-wrap">{{ selectedRequest ? (selectedRequest.description || selectedRequest.Description || 'Açıklama yok.') : '' }}</span>
          </p>
          <p v-if="selectedRequest" class="text-gray-500 text-xs mt-2">
            <span class="font-medium">Talebin gönderilme tarihi:</span>
            {{ displaySentAt(selectedRequest) }}
          </p>
          <p v-if="selectedRequest?.hasDocument || selectedRequest?.HasDocument" class="mt-2 text-sm">
            <span class="text-gray-500 font-medium">Eklenen belge:</span>
            <span class="ml-1 inline-flex items-center gap-1 bg-blue-50 text-blue-700 px-2 py-0.5 rounded text-xs font-medium">
              📎 {{ selectedRequest?.documentOriginalName || selectedRequest?.DocumentOriginalName || 'Belge mevcut' }}
            </span>
          </p>
        </div>
        <div class="mt-6 flex justify-end gap-2">
          <button @click="detailModalOpen = false" class="px-4 py-2 bg-gray-200 hover:bg-gray-300 rounded-lg font-medium">Kapat</button>
          <template v-if="selectedRequest && isPending(selectedRequest.status || selectedRequest.Status)">
            <button @click="detailModalOpen = false; openSignModal(selectedRequest)" class="px-4 py-2 bg-green-600 text-white rounded-lg font-medium hover:bg-green-700">Onayla ve İmzala</button>
            <button @click="rejectRequest(selectedRequest.id || selectedRequest.Id); detailModalOpen = false" class="px-4 py-2 bg-red-600 text-white rounded-lg font-medium hover:bg-red-700">Reddet</button>
          </template>
        </div>
      </div>
    </div>
  </div>

  <!-- PAdES İmza Modalı -->
  <PadesSignModal
    :open="signModalOpen"
    :permission="signModalRequest"
    @close="signModalOpen = false"
    @approved="onApproved"
  />
</template>

<script setup>
definePageMeta({ layout: 'admin', middleware: 'auth' })

const { token, isAdmin } = useAuth();
const { statusLabel } = useTurkishLabels();
const { refresh: refreshPendingCount } = usePendingRequestsCount();
const requests = ref([]);
const searchQuery = ref('');
const statusFilter = ref('hepsi'); // 'hepsi' | 'onaylanan' | 'reddedilen' | 'beklemede'
const isLoading = ref(true);
const detailModalOpen = ref(false);
const selectedRequest = ref(null);
const signModalOpen = ref(false);
const signModalRequest = ref(null);

/** Arama: ad veya e-posta ile filtrele. */
const filteredRequests = computed(() => {
  const q = (searchQuery.value || '').trim().toLowerCase();
  if (!q) return requests.value;
  return requests.value.filter((req) => {
    const email = (req.userEmail ?? req.UserEmail ?? '').toString().toLowerCase();
    const name = (req.userFullName ?? req.UserFullName ?? '').toString().toLowerCase();
    const unitName = (req.unitName ?? req.UnitName ?? '').toString().toLowerCase();
    return email.includes(q) || name.includes(q) || unitName.includes(q);
  });
});

const statusNorm = (s) => String(s ?? '').toLowerCase();
const isApproved = (s) => statusNorm(s) === 'approved' || statusNorm(s) === 'onaylandı';
const isRejected = (s) => statusNorm(s) === 'rejected' || statusNorm(s) === 'reddedildi';
const isPendingStatus = (s) => statusNorm(s) === 'pending' || statusNorm(s) === 'beklemede';

/** 3 ayrı liste: Onaylanan, Reddedilen, Bekleyen */
const approvedRequests = computed(() =>
  filteredRequests.value.filter((req) => isApproved(req.status ?? req.Status))
);
const rejectedRequests = computed(() =>
  filteredRequests.value.filter((req) => isRejected(req.status ?? req.Status))
);
const pendingRequests = computed(() =>
  filteredRequests.value.filter((req) => isPendingStatus(req.status ?? req.Status))
);

/** Durum filtresi seçenekleri (sayılarla) */
const statusFilterOptions = computed(() => [
  { value: 'hepsi', label: 'Hepsi', count: filteredRequests.value.length, activeClass: 'bg-gray-700 text-white' },
  { value: 'onaylanan', label: 'Onaylanan', count: approvedRequests.value.length, activeClass: 'bg-green-600 text-white' },
  { value: 'reddedilen', label: 'Reddedilen', count: rejectedRequests.value.length, activeClass: 'bg-red-600 text-white' },
  { value: 'beklemede', label: 'Beklemede', count: pendingRequests.value.length, activeClass: 'bg-amber-500 text-white' },
]);

/** Seçili durum filtresine göre liste */
const filteredByStatus = computed(() => {
  if (statusFilter.value === 'onaylanan') return approvedRequests.value;
  if (statusFilter.value === 'reddedilen') return rejectedRequests.value;
  if (statusFilter.value === 'beklemede') return pendingRequests.value;
  return filteredRequests.value;
});

/** Tek tablo modunda başlık rengi */
const currentFilterTitleClass = computed(() => {
  if (statusFilter.value === 'onaylanan') return 'text-green-800';
  if (statusFilter.value === 'reddedilen') return 'text-red-800';
  if (statusFilter.value === 'beklemede') return 'text-amber-800';
  return 'text-gray-800';
});

const openDetailModal = (req) => {
  selectedRequest.value = req;
  detailModalOpen.value = true;
};

const openSignModal = (req) => {
  signModalRequest.value = req;
  signModalOpen.value = true;
};

const onApproved = async () => {
  signModalOpen.value = false;
  await fetchRequests();
  refreshPendingCount();
  useToast().showSuccess('İzin talep onaylandı ve e-imza ile imzalandı.');
};

const formatDate = (val) => val ? new Date(val).toLocaleDateString('tr-TR') : '';
const formatDateTime = (val) => val ? new Date(val).toLocaleString('tr-TR', { dateStyle: 'short', timeStyle: 'short' }) : '';
/** API'den gelen createdAt (camelCase veya PascalCase) – sadece talebin gerçek gönderilme tarihi. */
const getCreatedAt = (req) => req?.createdAt ?? req?.CreatedAt ?? null;
/** Talebin gönderilme tarihi: sadece CreatedAt kullanılır (başlangıç tarihi ile karıştırılmaz). */
const displaySentAt = (req) => {
  const sent = getCreatedAt(req);
  if (sent) return new Date(sent).toLocaleString('tr-TR', { dateStyle: 'short', timeStyle: 'short' });
  return '—';
};
const isPending = (status) => ['Pending', 'Beklemede'].includes(String(status || ''));

const getStatusClass = (status) => {
  const s = String(status || '').toLowerCase();
  if (s === 'onaylandı' || s === 'approved') return 'bg-green-100 text-green-700';
  if (s === 'beklemede' || s === 'pending') return 'bg-yellow-100 text-yellow-700';
  return 'bg-red-100 text-red-700';
};

const fetchRequests = async () => {
  if (!token.value || !isAdmin.value) return;
  isLoading.value = true;
  try {
    const units = await $fetch('https://localhost:44365/api/Units', { headers: { Authorization: `Bearer ${token.value}` } });
    const list = Array.isArray(units) ? units : [];
    const all = [];
    for (const unit of list) {
      const id = unit.id ?? unit.Id;
      if (!id) continue;
      try {
        const data = await $fetch(`https://localhost:44365/api/Permissions/by-unit/${id}`, { headers: { Authorization: `Bearer ${token.value}` } });
        const arr = Array.isArray(data) ? data : [];
        for (const r of arr) {
          all.push({ ...r, unitName: unit.name ?? unit.Name });
        }
      } catch (_) {}
    }
    // En yeniden en eskiye: önce CreatedAt, yoksa StartDate
    all.sort((a, b) => {
      const dateA = a.createdAt ?? a.CreatedAt ?? a.startDate ?? a.StartDate;
      const dateB = b.createdAt ?? b.CreatedAt ?? b.startDate ?? b.StartDate;
      return new Date(dateB || 0) - new Date(dateA || 0);
    });
    requests.value = all;
  } catch (e) {
    requests.value = [];
  } finally {
    isLoading.value = false;
  }
};

const approveRequest = async (id) => {
  try {
    await $fetch(`https://localhost:44365/api/Permissions/approve/${id}`, { method: 'POST', headers: { Authorization: `Bearer ${token.value}` } });
    await fetchRequests();
    refreshPendingCount();
  } catch (e) {
    useToast().showError('Onaylama başarısız.');
  }
};

const rejectRequest = async (id) => {
  try {
    await $fetch(`https://localhost:44365/api/Permissions/reject/${id}`, { method: 'POST', headers: { Authorization: `Bearer ${token.value}` } });
    await fetchRequests();
    refreshPendingCount();
  } catch (e) {
    useToast().showError('Reddetme başarısız.');
  }
};

onMounted(() => { fetchRequests(); });
</script>
