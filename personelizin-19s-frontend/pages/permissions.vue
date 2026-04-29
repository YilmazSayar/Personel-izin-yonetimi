<template>
  <div class="p-6">
    <div class="flex justify-between items-center mb-6">
      <h2 class="text-2xl font-bold text-gray-800">İzin Taleplerim</h2>
      <button 
        @click="showModal = true" 
        class="bg-green-600 hover:bg-green-700 text-white px-4 py-2 rounded transition">
        + Yeni İzin Talebi
      </button>
    </div>

    <div class="bg-white rounded-lg shadow overflow-hidden">
      <table class="w-full text-left">
        <thead class="bg-gray-200">
          <tr>
            <th class="p-3">Birim</th>
            <th class="p-3">Başlangıç</th>
            <th class="p-3">Bitiş</th>
            <th class="p-3">Açıklama</th>
            <th class="p-3">Durum</th>
          </tr>
        </thead>
        <tbody>
          <tr v-if="permissions.length === 0">
            <td colspan="5" class="p-4 text-center text-gray-500">Henüz izin talebiniz yok.</td>
          </tr>
          <tr
            v-for="item in permissions"
            :key="item.id"
            class="border-b hover:bg-blue-50 cursor-pointer transition-colors"
            @click="openDescriptionModal(item)"
          >
            <td class="p-3 font-medium text-gray-700">{{ ((item.unitName ?? item.UnitName) ?? '').trim() || '—' }}</td>
            <td class="p-3">{{ formatDate(item.startDate || item.StartDate) }}</td>
            <td class="p-3">{{ formatDate(item.endDate || item.EndDate) }}</td>
            <td class="p-3 max-w-[200px] truncate">{{ item.description || item.Description || '—' }}</td>
            <td class="p-3">
              <span :class="statusClass(item.status || item.Status)" class="px-2 py-1 rounded text-sm">
                {{ statusLabel(item.status || item.Status) }}
              </span>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Açıklama modalı: talebe tıklanınca açılır -->
    <div v-if="descriptionModalOpen" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4" @click.self="descriptionModalOpen = false">
      <div class="bg-white p-6 rounded-lg shadow-xl w-full max-w-md">
        <h3 class="text-xl font-bold mb-4">İzin Talebi – Açıklama</h3>
        <div class="space-y-2 text-sm mb-4">
          <p><span class="text-gray-500 font-medium">Birim:</span> {{ selectedItem ? (selectedItem.unitName || selectedItem.UnitName || 'Belirtilmedi') : '' }}</p>
          <p><span class="text-gray-500 font-medium">Başlangıç:</span> {{ selectedItem ? formatDate(selectedItem.startDate || selectedItem.StartDate) : '' }}</p>
          <p><span class="text-gray-500 font-medium">Bitiş:</span> {{ selectedItem ? formatDate(selectedItem.endDate || selectedItem.EndDate) : '' }}</p>
          <p><span class="text-gray-500 font-medium">Durum:</span> {{ selectedItem ? statusLabel(selectedItem.status || selectedItem.Status) : '' }}</p>
          <p class="pt-2 border-t mt-2"><span class="text-gray-500 font-medium block mb-1">Açıklama:</span>
            <span class="text-gray-800 whitespace-pre-wrap">{{ selectedItem ? (selectedItem.description || selectedItem.Description || 'Açıklama yok.') : '' }}</span>
          </p>
        </div>
        <div class="flex justify-end">
          <button @click="descriptionModalOpen = false" class="bg-gray-500 text-white px-4 py-2 rounded hover:bg-gray-600">Kapat</button>
        </div>
      </div>
    </div>

    <div v-if="showModal" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
      <div class="bg-white p-6 rounded-lg shadow-xl w-96">
        <h3 class="text-xl font-bold mb-4">Yeni İzin Talebi</h3>
        
        <form @submit.prevent="submitPermission">
          <div class="mb-4">
            <label class="block text-gray-700 text-sm font-bold mb-2">Birim seçimi</label>
            <select v-model="newPermission.unitId" class="w-full border rounded p-2">
              <option :value="null">Birim seçin (isteğe bağlı)</option>
              <option v-for="u in myUnits" :key="u.id" :value="u.id || u.Id">{{ u.name || u.Name }} ({{ u.code || u.Code }})</option>
            </select>
          </div>
          <div class="mb-4">
            <label class="block text-gray-700 text-sm font-bold mb-2">Başlangıç Tarihi</label>
            <input v-model="newPermission.startDate" type="datetime-local" class="w-full border rounded p-2" required :min="minStartDateTime">
          </div>

          <div class="mb-4">
            <label class="block text-gray-700 text-sm font-bold mb-2">Bitiş Tarihi</label>
            <input v-model="newPermission.endDate" type="datetime-local" class="w-full border rounded p-2" required :min="minEndDateTime">
          </div>

          <div class="mb-4">
            <label class="block text-gray-700 text-sm font-bold mb-2">Açıklama (en fazla 280 karakter)</label>
            <textarea v-model="newPermission.description" class="w-full border rounded p-2" rows="3" maxlength="280" placeholder="Neden izin istiyorsunuz?"></textarea>
            <p class="text-right text-xs text-gray-400 mt-1">{{ (newPermission.description || '').length }}/280</p>
          </div>

          <!-- Belge yükleme -->
          <div class="mb-4">
            <label class="block text-gray-700 text-sm font-bold mb-2">
              Belge Ekle <span class="font-normal text-gray-400">(isteğe bağlı — PDF, JPG, PNG)</span>
            </label>
            <label class="flex items-center gap-2 border-2 border-dashed border-gray-200 rounded p-2.5 cursor-pointer hover:border-blue-400 hover:bg-blue-50 transition">
              <svg class="w-4 h-4 text-blue-500 shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15.172 7l-6.586 6.586a2 2 0 102.828 2.828l6.414-6.586a4 4 0 00-5.656-5.656l-6.415 6.585a6 6 0 108.486 8.486L20.5 13"/></svg>
              <span class="text-sm text-gray-600 flex-1 truncate">{{ permDocument ? permDocument.name : 'Dosya seçmek için tıklayın' }}</span>
              <input type="file" class="hidden" accept=".pdf,.jpg,.jpeg,.png" @change="onPermDocChange" />
            </label>
            <button v-if="permDocument" type="button" @click="permDocument = null" class="mt-1 text-xs text-red-500 hover:underline">Dosyayı kaldır</button>
          </div>

          <div class="flex justify-end gap-2">
            <button type="button" @click="showModal = false; permDocument = null" class="bg-gray-500 text-white px-4 py-2 rounded hover:bg-gray-600">Vazgeç</button>
            <button type="submit" class="bg-blue-600 text-white px-4 py-2 rounded hover:bg-blue-700">Gönder</button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup>

definePageMeta({
  middleware: 'auth' // middleware klasöründeki dosya ismiyle aynı olmalı
})

import { ref, computed, onMounted } from 'vue';

const { token, userId } = useAuth();
const permissions = ref([]);
const myUnits = ref([]);
const showModal = ref(false);
const descriptionModalOpen = ref(false);
const selectedItem = ref(null);
const toast = useToast();
const permDocument = ref(null);
const onPermDocChange = (e) => { permDocument.value = e.target.files?.[0] ?? null; };

const fetchMyUnits = async () => {
  if (!token.value) return;
  try {
    const data = await $fetch('https://localhost:44365/api/Units', {
      headers: { Authorization: `Bearer ${token.value}` }
    });
    myUnits.value = Array.isArray(data) ? data : [];
  } catch (e) {
    myUnits.value = [];
  }
};

const openDescriptionModal = (item) => {
  selectedItem.value = item;
  descriptionModalOpen.value = true;
};

// Form Verisi
const newPermission = ref({
  unitId: null,
  startDate: '',
  endDate: '',
  description: ''
});

const toYYYYMMDD = (d) => {
  const x = new Date(d);
  const y = x.getFullYear(), m = String(x.getMonth() + 1).padStart(2, '0'), day = String(x.getDate()).padStart(2, '0');
  return `${y}-${m}-${day}`;
};
const minStartDateTime = computed(() => toYYYYMMDD(new Date()) + 'T00:00');
const minEndDateTime = computed(() => {
  const start = newPermission.value.startDate;
  if (start) return start.slice(0, 16);
  return minStartDateTime.value;
});

onMounted(async () => {
  const id = userId.value;
  if (id) {
    await fetchMyUnits();
    await fetchMyPermissions();
  } else {
    toast.showError("Oturum bilgisi bulunamadı! Lütfen giriş yapın.");
  }
});

// İzinleri Listeleme Fonksiyonu
const fetchMyPermissions = async () => {
  if (!token.value) return;
  try {
    const data = await $fetch('https://localhost:44365/api/Permissions/my-permissions', {
      headers: { Authorization: `Bearer ${token.value}` }
    });
    permissions.value = data;
  } catch (error) {
    console.error("Veriler çekilemedi:", error);
    toast.showError("İzinler yüklenirken hata oluştu.");
  }
};

// Yeni İzin Gönderme Fonksiyonu (Create)
const submitPermission = async () => {
  if (!token.value) {
    toast.showError("Oturum bulunamadı, lütfen tekrar giriş yapın.");
    return;
  }
  try {
    const form = new FormData();
    if (newPermission.value.unitId != null) form.append('unitId', String(newPermission.value.unitId));
    form.append('startDate', newPermission.value.startDate);
    form.append('endDate', newPermission.value.endDate);
    if (newPermission.value.description) form.append('description', newPermission.value.description);
    if (permDocument.value) form.append('document', permDocument.value);

    await $fetch('https://localhost:44365/api/Permissions', {
      method: 'POST',
      headers: { Authorization: `Bearer ${token.value}` },
      body: form,
    });
    permDocument.value = null;

    // Başarılı olursa:
    toast.showSuccess("İzin talebi başarıyla oluşturuldu!");
    showModal.value = false; // Modalı kapat
    newPermission.value = { unitId: null, startDate: '', endDate: '', description: '' }; // Formu temizle
    await fetchMyPermissions(); // Listeyi yenile

  } catch (error) {
    console.error("Kayıt hatası:", error);
    const msg = (typeof error?.data === 'string' ? error.data : (error?.data?.message ?? error?.data?.Message)) ?? error?.message ?? "İzin oluşturulurken hata oluştu.";
    toast.showError(msg);
  }
};

// Yardımcı Fonksiyonlar
const formatDate = (dateString) => {
  if (!dateString) return '';
  return new Date(dateString).toLocaleDateString('tr-TR');
};

const { statusLabel } = useTurkishLabels();
const statusClass = (status) => {
  const s = String(status ?? '').toLowerCase();
  if (s === 'approved' || s === 'onaylandı') return 'bg-green-100 text-green-800 font-bold';
  if (s === 'rejected' || s === 'reddedildi') return 'bg-red-100 text-red-800 font-bold';
  return 'bg-yellow-100 text-yellow-800 font-bold';
};
</script>