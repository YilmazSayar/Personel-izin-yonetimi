<template>
  <div>
    <div class="bg-white rounded-2xl shadow-sm overflow-hidden mb-8 border border-gray-100">
      <div class="p-6 border-b flex flex-wrap items-center justify-between gap-4 bg-gray-50/50">
        <h3 class="font-bold text-lg text-gray-800">Birim Yönetimi</h3>
        <div class="flex items-center gap-2 flex-1 max-w-md">
          <input v-model="newUnitName" type="text" placeholder="Birim adı" class="flex-1 border rounded-xl px-4 py-2.5 bg-white text-sm outline-none focus:ring-2 focus:ring-blue-500" @keydown.enter.prevent="createUnit" />
          <button @click="createUnit" class="bg-blue-600 text-white px-5 py-2.5 rounded-xl font-bold hover:bg-blue-700 transition">Birim Oluştur</button>
        </div>
      </div>
      <div class="overflow-x-auto">
        <table class="w-full text-left">
          <thead>
            <tr class="text-gray-400 text-[11px] uppercase tracking-widest border-b">
              <th class="px-6 py-4">Birim adı</th>
              <th class="px-6 py-4">Davet kodu</th>
              <th class="px-6 py-4">Oluşturulma</th>
              <th class="px-6 py-4 text-right">İşlem</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="unit in units" :key="unit.id" class="border-t hover:bg-gray-50">
              <td class="px-6 py-4 font-medium">{{ unit.name || unit.Name }}</td>
              <td class="px-6 py-4 font-mono font-bold text-blue-600">{{ unit.code || unit.Code }}</td>
              <td class="px-6 py-4 text-sm text-gray-500">{{ formatDate(unit.createdAt || unit.CreatedAt) }}</td>
              <td class="px-6 py-4 text-right">
                <div class="flex flex-wrap gap-2 justify-end">
                  <NuxtLink
                    :to="`/admin/units/${unit.id ?? unit.Id}`"
                    class="inline-flex items-center px-3 py-1.5 rounded-lg text-xs font-semibold text-white bg-blue-600 hover:bg-blue-700 shadow-sm hover:shadow transition no-underline"
                  >
                    Birimdeki çalışanlar
                  </NuxtLink>
                  <button
                    type="button"
                    class="inline-flex items-center px-3 py-1.5 rounded-lg text-xs font-semibold text-white bg-red-600 hover:bg-red-700 shadow-sm hover:shadow transition disabled:opacity-50 disabled:cursor-not-allowed"
                    :disabled="unitDeleteLoading[unit.id ?? unit.Id]"
                    @click="deleteUnit(unit)"
                  >
                    {{ unitDeleteLoading[unit.id ?? unit.Id] ? 'Kapatılıyor...' : 'Birimi kapat' }}
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <div class="bg-white rounded-2xl shadow-sm overflow-hidden border border-gray-100">
      <div class="p-6 border-b bg-gray-50/50">
        <h3 class="font-bold text-lg">Birimlerim</h3>
        <p class="text-sm text-gray-500 mt-1">Çalışanlar kayıt olurken yöneticinin verdiği davet kodunu girer.</p>
      </div>
      <div class="overflow-x-auto">
        <table class="w-full text-left">
          <thead>
            <tr class="text-gray-400 text-[11px] uppercase tracking-widest border-b">
              <th class="px-6 py-4">Birim adı</th>
              <th class="px-6 py-4">Davet kodu</th>
              <th class="px-6 py-4 text-right">İşlem</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="unit in myUnits" :key="unit.id" class="border-t hover:bg-gray-50">
              <td class="px-6 py-4 font-medium">{{ unit.name || unit.Name }}</td>
              <td class="px-6 py-4 font-mono font-bold text-blue-600">{{ unit.code || unit.Code }}</td>
              <td class="px-6 py-4 text-right">
                <div class="flex flex-wrap gap-2 justify-end">
                  <NuxtLink
                    :to="`/admin/units/${unit.id ?? unit.Id}`"
                    class="inline-flex items-center px-3 py-1.5 rounded-lg text-xs font-semibold text-white bg-blue-600 hover:bg-blue-700 shadow-sm hover:shadow transition no-underline"
                  >
                    Birimdeki çalışanlar
                  </NuxtLink>
                  <button
                    type="button"
                    class="inline-flex items-center px-3 py-1.5 rounded-lg text-xs font-semibold text-white bg-red-600 hover:bg-red-700 shadow-sm hover:shadow transition disabled:opacity-50 disabled:cursor-not-allowed"
                    :disabled="unitDeleteLoading[unit.id ?? unit.Id]"
                    @click="deleteUnit(unit)"
                  >
                    {{ unitDeleteLoading[unit.id ?? unit.Id] ? 'Kapatılıyor...' : 'Birimi kapat' }}
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</template>

<script setup>
definePageMeta({ layout: 'admin', middleware: 'auth' })

const { token, isAdmin } = useAuth();
const units = ref([]);
const myUnits = ref([]);
const newUnitName = ref('');
const unitDeleteLoading = ref({});

const formatDate = (val) => val ? new Date(val).toLocaleDateString('tr-TR') : '';

const fetchUnits = async () => {
  if (!token.value || !isAdmin.value) return;
  const data = await $fetch('https://localhost:44365/api/Units', { headers: { Authorization: `Bearer ${token.value}` } });
  units.value = Array.isArray(data) ? data : [];
};

const fetchMyUnits = async () => {
  try {
    const data = await $fetch('https://localhost:44365/api/Units', { headers: { Authorization: `Bearer ${token.value}` } });
    myUnits.value = Array.isArray(data) ? data : [];
  } catch (e) { myUnits.value = []; }
};

const createUnit = async () => {
  try {
    await $fetch('https://localhost:44365/api/Units', { method: 'POST', headers: { Authorization: `Bearer ${token.value}` }, body: { name: newUnitName.value } });
    newUnitName.value = ''; fetchUnits(); fetchMyUnits(); useToast().showSuccess("Birim oluşturuldu. Davet kodunu çalışanlarınızla paylaşın.");
  } catch (e) { useToast().showError("Birim oluşturulamadı."); }
};

async function deleteUnit(unit) {
  const id = unit.id ?? unit.Id;
  const name = unit.name ?? unit.Name;
  const ok = await useConfirm().showConfirm({
    title: 'Birimi kapat',
    message: `"${name}" birimini kapatmak (silmek) istediğinize emin misiniz? Birimdeki çalışanların birim ataması kaldırılır, hesapları silinmez.`,
    confirmText: 'Birimi kapat',
    danger: true
  });
  if (!ok) return;
  unitDeleteLoading.value = { ...unitDeleteLoading.value, [id]: true };
  try {
    await $fetch(`https://localhost:44365/api/Units/${id}`, {
      method: 'DELETE',
      headers: { Authorization: `Bearer ${token.value}` }
    });
    units.value = units.value.filter((u) => (u.id ?? u.Id) !== id);
    myUnits.value = myUnits.value.filter((u) => (u.id ?? u.Id) !== id);
    useToast().showSuccess('Birim kapatıldı.');
  } catch (e) {
    const msg = e?.data?.message ?? e?.data?.Message ?? (typeof e?.data === 'string' ? e.data : null) ?? 'Birim kapatılamadı.';
    useToast().showError(msg);
  } finally {
    unitDeleteLoading.value = { ...unitDeleteLoading.value, [id]: false };
  }
}

onMounted(() => { fetchUnits(); fetchMyUnits(); });
</script>
