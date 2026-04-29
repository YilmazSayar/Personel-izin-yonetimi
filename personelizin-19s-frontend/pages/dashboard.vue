<template>
  <div>
    <!-- Bir Birime Katılın: her zaman göster (kullanıcı birden fazla birime katılabilir) -->
    <div class="join-unit-card mb-8">
      <div class="join-unit-inner">
        <div class="join-unit-content">
          <div class="join-unit-icon">
            <svg xmlns="http://www.w3.org/2000/svg" width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M4.5 16.5c-1.5 1.26-2 5-2 5s3.74-.5 5-2c.71-.84.7-2.13-.09-2.91a2.18 2.18 0 0 0-2.91-.09z"/><path d="m12 15-3-3a22 22 0 0 1 2-3.95A12.88 12.88 0 0 1 22 2c0 2.72-.78 7.5-6 11a22.35 22.35 0 0 1-4 2z"/><path d="M9 12H4s.55-3.03 2-4c1.62-1.08 5 0 5 0"/><path d="M12 15v5"/></svg>
          </div>
          <div class="join-unit-text">
            <h3 class="join-unit-title">Bir Birime Katılın</h3>
            <p class="join-unit-desc">İzin talebinde bulunabilmek için yöneticinizden aldığınız 6 haneli davet kodunu aşağıya girin.</p>
          </div>
        </div>
        <div class="join-unit-form">
          <input
            v-model="joinCode"
            type="text"
            placeholder="KOD GİRİN"
            maxlength="12"
            class="join-unit-input"
          />
          <button type="button" class="join-unit-btn" @click="handleJoinUnit" :disabled="joinLoading">
            {{ joinLoading ? '...' : 'KATIL' }}
          </button>
        </div>
      </div>
    </div>

    <div class="grid grid-cols-1 md:grid-cols-3 gap-6 mb-8">
          <div class="bg-white p-6 rounded-2xl shadow-sm border border-gray-100">
            <p class="text-xs text-gray-400 font-bold uppercase">Kalan İzin Hakkı</p>
            <p class="text-3xl font-black text-blue-600">{{ remainingLeaveDaysLoaded ? remainingLeaveDays : '—' }} {{ remainingLeaveDaysLoaded ? 'Gün' : '' }}</p>
          </div>
          <div class="bg-white p-6 rounded-2xl shadow-sm border border-gray-100">
            <p class="text-xs text-gray-400 font-bold uppercase">Bekleyen Talepler</p>
            <p class="text-3xl font-black text-yellow-600">
              {{ leaves.filter(l => (l.status || l.Status || '') === 'Beklemede' || (l.status || l.Status || '') === 'Pending').length }}
            </p>
          </div>
          <div class="bg-white p-6 rounded-2xl shadow-sm border border-gray-100">
            <p class="text-xs text-gray-400 font-bold uppercase">Onaylanan İzinler</p>
            <p class="text-3xl font-black text-green-600">
              {{ leaves.filter(l => (l.status || l.Status || '') === 'Onaylandı' || (l.status || l.Status || '') === 'Approved').length }}
            </p>
          </div>
        </div>

        <div class="bg-white rounded-2xl shadow-sm overflow-hidden border border-gray-100">
          <div class="p-6 border-b flex justify-between items-center bg-gray-50/50">
            <h3 class="font-bold text-lg">Son İzin Taleplerim</h3>
            <button @click="isModalOpen = true" class="bg-blue-600 text-white px-5 py-2.5 rounded-xl font-bold hover:bg-blue-700 transition">+ Yeni İzin Talebi</button>
          </div>
          <div class="overflow-x-auto">
            <table class="w-full text-left">
              <thead>
                <tr class="text-gray-400 text-[11px] uppercase tracking-widest border-b">
                  <th class="px-6 py-4">Birim</th>
                  <th class="px-6 py-4">Kullanıcı</th>
                  <th class="px-6 py-4">Tür</th>
                  <th class="px-6 py-4">Başlangıç</th>
                  <th class="px-6 py-4">Bitiş</th>
                  <th class="px-6 py-4">Durum</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="leave in leaves" :key="leave.id" @click="openDescriptionModal(leave)" class="border-t hover:bg-blue-50 cursor-pointer">
                  <td class="px-6 py-4 font-medium text-gray-700">{{ ((leave.unitName ?? leave.UnitName) ?? '').trim() || '—' }}</td>
                  <td class="px-6 py-4 font-medium text-sm">{{ leave.userEmail ?? leave.UserEmail ?? '—' }}</td>
                  <td class="px-6 py-4 font-bold">{{ leave.type || leave.Type }}</td>
                  <td class="px-6 py-4 text-sm">{{ formatDate(leave.startDate || leave.StartDate) }}</td>
                  <td class="px-6 py-4 text-sm">{{ formatDate(leave.endDate || leave.EndDate) }}</td>
                  <td class="px-6 py-4">
                    <span :class="getStatusClass(leave.status || leave.Status)" class="px-3 py-1 rounded-full text-[10px] uppercase font-black">
                      {{ statusLabel(leave.status || leave.Status) }}
                    </span>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>

    <div v-if="descriptionModalOpen" class="fixed inset-0 bg-blue-900/40 backdrop-blur-sm flex items-center justify-center z-50 p-4" @click.self="descriptionModalOpen = false">
      <div class="bg-white rounded-3xl p-8 w-full max-w-lg shadow-2xl">
        <h3 class="text-xl font-bold mb-4">İzin Talebi Detayı</h3>
        <div class="space-y-3 text-sm">
          <p><span class="text-gray-500 font-medium">Birim:</span> {{ selectedLeave ? (((selectedLeave.unitName ?? selectedLeave.UnitName) ?? '').trim() || 'Belirtilmedi') : '' }}</p>
          <p><span class="text-gray-500 font-medium">Tür:</span> {{ selectedLeave ? (selectedLeave.type || selectedLeave.Type) : '' }}</p>
          <p><span class="text-gray-500 font-medium">Başlangıç – Bitiş:</span> {{ selectedLeave ? formatDate(selectedLeave.startDate || selectedLeave.StartDate) + ' – ' + formatDate(selectedLeave.endDate || selectedLeave.EndDate) : '' }}</p>
          <p><span class="text-gray-500 font-medium">Durum:</span> {{ selectedLeave ? statusLabel(selectedLeave.status || selectedLeave.Status) : '' }}</p>
          <p class="pt-2 border-t"><span class="text-gray-500 font-medium block mb-1 text-xs">Açıklama:</span>
            <span class="text-gray-800">{{ selectedLeave ? (selectedLeave.description || selectedLeave.Description || 'Açıklama yok.') : '' }}</span>
          </p>
        </div>
        <div class="mt-6 flex justify-end">
          <button @click="descriptionModalOpen = false" class="px-5 py-2.5 bg-gray-200 hover:bg-gray-300 rounded-xl font-bold transition">Kapat</button>
        </div>
      </div>
    </div>

    <div v-if="isModalOpen" class="fixed inset-0 bg-blue-900/40 backdrop-blur-sm flex items-center justify-center z-50 p-4">
      <div class="bg-white rounded-3xl p-8 w-full max-w-md shadow-2xl">
        <h3 class="text-2xl font-black mb-6">Yeni İzin Talebi</h3>
        <div class="space-y-5">
          <select v-model="newLeave.unitId" class="w-full border rounded-xl p-3.5 bg-gray-50 font-medium outline-none">
            <option :value="null">Birim seçin (isteğe bağlı)</option>
            <option v-for="u in myUnits" :key="u.id || u.Id" :value="u.id || u.Id">{{ u.name || u.Name }}</option>
          </select>
          <select v-model="newLeave.type" class="w-full border rounded-xl p-3.5 bg-gray-50 font-medium outline-none">
            <option>Yıllık İzin</option>
            <option>Mazeret İzni</option>
            <option>Hastalık İzni</option>
          </select>
          <div class="grid grid-cols-2 gap-4">
            <input v-model="newLeave.startDate" type="date" class="w-full border rounded-xl p-3 bg-gray-50 text-sm" :min="todayStr">
            <input v-model="newLeave.endDate" type="date" class="w-full border rounded-xl p-3 bg-gray-50 text-sm" :min="minEndDateStr">
          </div>
          <div>
            <textarea v-model="newLeave.description" rows="3" maxlength="280" placeholder="Açıklama (en fazla 280 karakter)" class="w-full border rounded-xl p-3 bg-gray-50 text-sm outline-none"></textarea>
            <p class="text-right text-xs text-gray-400 mt-1">{{ (newLeave.description || '').length }}/280</p>
          </div>
          <!-- Belge yükleme (isteğe bağlı) -->
          <div>
            <label class="block text-sm font-semibold text-gray-700 mb-1.5">
              Belge Ekle <span class="font-normal text-gray-400">(isteğe bağlı — PDF, JPG, PNG, maks. 10 MB)</span>
            </label>
            <label class="flex items-center gap-3 border-2 border-dashed border-gray-200 rounded-xl p-3 cursor-pointer hover:border-blue-400 hover:bg-blue-50 transition">
              <svg class="w-5 h-5 text-blue-500 shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15.172 7l-6.586 6.586a2 2 0 102.828 2.828l6.414-6.586a4 4 0 00-5.656-5.656l-6.415 6.585a6 6 0 108.486 8.486L20.5 13"/></svg>
              <span class="text-sm text-gray-600 flex-1 truncate">{{ leaveDocument ? leaveDocument.name : 'Dosya seçmek için tıklayın' }}</span>
              <input type="file" class="hidden" accept=".pdf,.jpg,.jpeg,.png" @change="onDocumentChange" />
            </label>
            <button v-if="leaveDocument" type="button" @click="leaveDocument = null" class="mt-1 text-xs text-red-500 hover:underline">Dosyayı kaldır</button>
          </div>
        </div>
        <div class="mt-8 flex gap-3">
          <button @click="isModalOpen = false; leaveDocument = null" class="flex-1 py-4 font-bold text-gray-500 transition">Vazgeç</button>
          <button @click="submitLeaveRequest" class="flex-1 py-4 bg-blue-600 text-white rounded-xl font-bold hover:bg-blue-700 transition">Gönder</button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
definePageMeta({ layout: 'admin', middleware: 'auth' })
import { ref, computed, onMounted } from 'vue';

const { token, userId, isAdmin } = useAuth();
const { statusLabel } = useTurkishLabels();

const leaves = ref([]);
const myUnits = ref([]);
const leaveDocument = ref(null);
const onDocumentChange = (e) => { leaveDocument.value = e.target.files?.[0] ?? null; };
const remainingLeaveDays = ref(null);
const remainingLeaveDaysLoaded = ref(false);
const userUnitId = ref(undefined);
const joinCode = ref('');
const joinLoading = ref(false);
const isModalOpen = ref(false);
const descriptionModalOpen = ref(false);
const selectedLeave = ref(null);
const newLeave = ref({ unitId: null, type: 'Yıllık İzin', startDate: '', endDate: '', description: '' });

const todayStr = computed(() => {
  const d = new Date();
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
});
const minEndDateStr = computed(() => newLeave.value.startDate || todayStr.value);

const getStatusClass = (status) => {
  const s = (status || '').toString().toLowerCase();
  if (s === 'onaylandı' || s === 'approved') return 'bg-green-100 text-green-700 border border-green-200';
  if (s === 'beklemede' || s === 'pending') return 'bg-yellow-100 text-yellow-700 border border-yellow-200';
  return 'bg-red-100 text-red-700 border border-red-200';
};

const formatDate = (val) => val ? new Date(val).toLocaleDateString('tr-TR') : '';

const fetchLeaves = async () => {
  try {
    const data = await $fetch('https://localhost:44365/api/Permissions', { headers: { Authorization: `Bearer ${token.value}` } });
    leaves.value = Array.isArray(data) ? data : [];
  } catch (e) { console.error(e); }
};

const fetchMyUnits = async () => {
  try {
    const data = await $fetch('https://localhost:44365/api/Units', { headers: { Authorization: `Bearer ${token.value}` } });
    myUnits.value = Array.isArray(data) ? data : [];
  } catch (e) { myUnits.value = []; }
};

const fetchMe = async () => {
  try {
    const data = await $fetch('https://localhost:44365/api/Auth/me', { headers: { Authorization: `Bearer ${token.value}` } });
    const days = data?.remainingLeaveDays ?? data?.RemainingLeaveDays;
    remainingLeaveDays.value = days != null ? Number(days) : 14;
    userUnitId.value = data?.unitId ?? data?.UnitId ?? null;
  } catch (e) {
    remainingLeaveDays.value = 14;
    userUnitId.value = null;
  } finally {
    remainingLeaveDaysLoaded.value = true;
  }
};

const handleJoinUnit = async () => {
  const code = String(joinCode.value || '').replace(/[^A-Za-z0-9]/g, '').toUpperCase().slice(0, 6);
  if (!code) {
    useToast().showError('Lütfen 6 haneli davet kodunu girin.');
    return;
  }
  joinLoading.value = true;
  try {
    await $fetch('https://localhost:44365/api/Auth/join-unit', {
      method: 'POST',
      headers: { Authorization: `Bearer ${token.value}` },
      body: { inviteCode: code }
    });
    joinCode.value = '';
    await fetchMe();
    await fetchMyUnits();
    useToast().showSuccess('Birime başarıyla katıldınız.');
  } catch (e) {
    const msg = (typeof e?.data === 'string' ? e.data : (e?.data?.message || e?.data?.Message)) || e?.message || 'Davet kodu geçersiz.';
    useToast().showError(msg);
  } finally {
    joinLoading.value = false;
  }
};

const openDescriptionModal = (leave) => { selectedLeave.value = leave; descriptionModalOpen.value = true; };

const submitLeaveRequest = async () => {
  try {
    const form = new FormData();
    if (newLeave.value.unitId != null) form.append('unitId', String(newLeave.value.unitId));
    form.append('type', newLeave.value.type || 'Yıllık İzin');
    form.append('startDate', newLeave.value.startDate);
    form.append('endDate', newLeave.value.endDate);
    if (newLeave.value.description) form.append('description', newLeave.value.description);
    if (leaveDocument.value) form.append('document', leaveDocument.value);

    await $fetch('https://localhost:44365/api/Permissions', {
      method: 'POST',
      headers: { Authorization: `Bearer ${token.value}` },
      body: form,
    });
    isModalOpen.value = false;
    leaveDocument.value = null;
    fetchLeaves();
    fetchMe();
    useToast().showSuccess("Talep gönderildi.");
  } catch (e) {
    const msg = (typeof e?.data === 'string' ? e.data : (e?.data?.message || e?.data?.Message)) || e?.message || "Talep gönderilemedi.";
    useToast().showError(msg);
  }
};

onMounted(() => { fetchLeaves(); fetchMyUnits(); fetchMe(); });
</script>

<style scoped>
.join-unit-card {
  background: white;
  border-radius: 1rem;
  box-shadow: 0 1px 3px rgba(0,0,0,0.08);
  border: 1px solid #e5e7eb;
  overflow: hidden;
}
.join-unit-inner {
  padding: 1.5rem 1.75rem;
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 1rem;
  justify-content: space-between;
}
.join-unit-content {
  display: flex;
  align-items: flex-start;
  gap: 1rem;
}
.join-unit-icon {
  color: #2563eb;
  flex-shrink: 0;
}
.join-unit-title {
  font-size: 1.125rem;
  font-weight: 700;
  color: #1f2937;
  margin: 0 0 0.25rem 0;
}
.join-unit-desc {
  font-size: 0.875rem;
  color: #6b7280;
  margin: 0;
  max-width: 420px;
}
.join-unit-form {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  flex-shrink: 0;
}
.join-unit-input {
  width: 140px;
  padding: 0.6rem 0.75rem;
  border: 1px solid #e5e7eb;
  border-radius: 0.5rem;
  font-size: 0.875rem;
  letter-spacing: 0.05em;
  text-transform: uppercase;
}
.join-unit-input::placeholder { color: #9ca3af; }
.join-unit-btn {
  padding: 0.6rem 1.25rem;
  background: #2563eb;
  color: white;
  border: none;
  border-radius: 0.5rem;
  font-weight: 700;
  font-size: 0.875rem;
  cursor: pointer;
}
.join-unit-btn:hover:not(:disabled) { background: #1d4ed8; }
.join-unit-btn:disabled { opacity: 0.7; cursor: not-allowed; }
</style>