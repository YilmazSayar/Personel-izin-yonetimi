<template>
  <div
    v-if="open"
    class="fixed inset-0 bg-black/60 flex items-center justify-center z-50 p-4"
    @click.self="$emit('close')"
  >
    <div class="bg-white rounded-2xl shadow-2xl w-full max-w-lg overflow-hidden">
      <!-- Başlık -->
      <div class="bg-gradient-to-r from-blue-700 to-blue-500 px-6 py-4 flex items-center justify-between">
        <div>
          <h2 class="text-white font-bold text-lg">E-İmza ile Onayla</h2>
          <p class="text-blue-100 text-sm mt-0.5">PAdES Elektronik İmza</p>
        </div>
        <button @click="$emit('close')" class="text-white/70 hover:text-white text-2xl leading-none">&times;</button>
      </div>

      <div class="p-6 space-y-5">
        <!-- İzin özeti -->
        <div class="bg-blue-50 border border-blue-100 rounded-xl p-4 text-sm space-y-1">
          <p><span class="text-gray-500 font-medium">Çalışan:</span> {{ permission?.userEmail || permission?.UserEmail }}</p>
          <p><span class="text-gray-500 font-medium">Tür:</span> {{ permission?.type || permission?.Type }}</p>
          <p>
            <span class="text-gray-500 font-medium">Tarih:</span>
            {{ formatDate(permission?.startDate || permission?.StartDate) }} –
            {{ formatDate(permission?.endDate || permission?.EndDate) }}
          </p>
        </div>

        <!-- Adım göstergesi -->
        <div class="flex items-center gap-2 text-xs">
          <StepBadge :n="1" label="Bağlan" :active="step >= 1" :done="step > 1" />
          <div class="flex-1 h-px bg-gray-200" />
          <StepBadge :n="2" label="Sertifika" :active="step >= 2" :done="step > 2" />
          <div class="flex-1 h-px bg-gray-200" />
          <StepBadge :n="3" label="İmzala" :active="step >= 3" :done="step > 3" />
          <div class="flex-1 h-px bg-gray-200" />
          <StepBadge :n="4" label="Tamamlandı" :active="step >= 4" :done="step >= 4" />
        </div>

        <!-- Step 1: Bağlan -->
        <template v-if="step === 1">
          <div class="text-center py-4 space-y-3">
            <div v-if="localSigner.status.value === 'connecting'" class="text-blue-600">
              <div class="animate-spin w-8 h-8 border-4 border-blue-200 border-t-blue-600 rounded-full mx-auto mb-2" />
              <p class="text-sm">{{ localSigner.statusMessage.value }}</p>
            </div>
            <div v-else-if="localSigner.status.value === 'error'" class="text-red-600 space-y-2">
              <p class="text-sm">{{ localSigner.statusMessage.value }}</p>
              <p class="text-xs text-gray-500">
                LocalSigner uygulamasını indirmek için:
                <a href="https://www.onaylarim.com" target="_blank" class="text-blue-600 underline">onaylarim.com</a>
              </p>
              <button @click="startConnect" class="mt-2 px-4 py-2 bg-blue-600 text-white rounded-lg text-sm hover:bg-blue-700">
                Tekrar Dene
              </button>
            </div>
            <div v-else class="space-y-3">
              <p class="text-sm text-gray-600">E-imza tokenınızın bilgisayara takılı olduğundan emin olun.</p>
              <button @click="startConnect" class="px-6 py-2.5 bg-blue-600 text-white rounded-xl font-semibold hover:bg-blue-700">
                LocalSigner'a Bağlan
              </button>
            </div>
          </div>
        </template>

        <!-- Step 2: Sertifika seç + PIN -->
        <template v-if="step === 2">
          <div class="space-y-4">
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Sertifika Seçin</label>
              <select
                v-model="selectedCertIndex"
                class="w-full border border-gray-200 rounded-lg px-3 py-2.5 text-sm focus:ring-2 focus:ring-blue-500 outline-none"
              >
                <option v-for="(cert, i) in localSigner.certificates.value" :key="cert.id" :value="i">
                  {{ cert.personFullname || cert.serialNumber }} — Geçerlilik: {{ formatDate(cert.validTo) }}
                </option>
              </select>
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">PIN</label>
              <input
                v-model="pin"
                type="password"
                placeholder="E-imza PIN'ini girin"
                class="w-full border border-gray-200 rounded-lg px-3 py-2.5 text-sm focus:ring-2 focus:ring-blue-500 outline-none"
                @keyup.enter="startSigning"
              />
            </div>
            <p v-if="errorMessage" class="text-red-600 text-sm bg-red-50 rounded-lg px-3 py-2">{{ errorMessage }}</p>
            <div class="flex gap-3 pt-1">
              <button @click="step = 1" class="flex-1 px-4 py-2.5 border border-gray-200 rounded-xl text-sm font-medium hover:bg-gray-50">
                Geri
              </button>
              <button
                @click="startSigning"
                :disabled="!pin || signingInProgress"
                class="flex-1 px-4 py-2.5 bg-green-600 text-white rounded-xl text-sm font-semibold hover:bg-green-700 disabled:opacity-50 disabled:cursor-not-allowed"
              >
                İmzala ve Onayla
              </button>
            </div>
          </div>
        </template>

        <!-- Step 3: İmzalanıyor -->
        <template v-if="step === 3">
          <div class="text-center py-6 space-y-3">
            <div class="animate-spin w-10 h-10 border-4 border-blue-200 border-t-blue-600 rounded-full mx-auto" />
            <p class="text-sm font-medium text-gray-700">{{ signingMessage }}</p>
            <p class="text-xs text-gray-400">Lütfen bekleyin...</p>
          </div>
        </template>

        <!-- Step 4: Tamamlandı -->
        <template v-if="step === 4">
          <div class="text-center py-6 space-y-4">
            <div class="w-14 h-14 bg-green-100 rounded-full flex items-center justify-center mx-auto">
              <svg class="w-8 h-8 text-green-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 13l4 4L19 7" />
              </svg>
            </div>
            <div>
              <p class="font-semibold text-gray-800">İzin başarıyla onaylandı!</p>
              <p class="text-sm text-gray-500 mt-1">PDF, elektronik imzanızla imzalanarak onaylandı.</p>
            </div>
            <div class="flex gap-3">
              <button
                v-if="downloadOperationId"
                @click="downloadSignedPdf"
                class="flex-1 px-4 py-2.5 bg-blue-600 text-white rounded-xl text-sm font-semibold hover:bg-blue-700"
              >
                İmzalı PDF'i İndir
              </button>
              <button
                @click="$emit('approved'); $emit('close')"
                class="flex-1 px-4 py-2.5 bg-gray-100 text-gray-700 rounded-xl text-sm font-semibold hover:bg-gray-200"
              >
                Kapat
              </button>
            </div>
          </div>
        </template>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { CertificateInfo } from '~/composables/useLocalSigner'

const props = defineProps<{
  open: boolean
  permission: any
}>()

const emit = defineEmits<{
  close: []
  approved: []
}>()

const { token } = useAuth()
const BASE = 'https://localhost:44365/api'

const localSigner = useLocalSigner()

const step = ref(1)
const selectedCertIndex = ref(0)
const pin = ref('')
const errorMessage = ref('')
const signingInProgress = ref(false)
const signingMessage = ref('')
const downloadOperationId = ref<string | null>(null)

const formatDate = (val: any) => val ? new Date(val).toLocaleDateString('tr-TR') : ''

const startConnect = async () => {
  const ok = await localSigner.tryConnect()
  if (ok) step.value = 2
}

const startSigning = async () => {
  if (!pin.value || signingInProgress.value) return
  errorMessage.value = ''
  signingInProgress.value = true
  step.value = 3

  const permId = props.permission?.id || props.permission?.Id
  const cert: CertificateInfo = localSigner.certificates.value[selectedCertIndex.value]

  try {
    // Adım 1: PDF üret ve yükle
    signingMessage.value = 'İzin raporu PDF olarak oluşturuluyor...'
    const uploadRes = await $fetch<{ UploadOperationId?: string; Error?: string }>(
      `${BASE}/Signature/upload-pdf/${permId}`,
      { headers: { Authorization: `Bearer ${token.value}` } }
    )
    if (uploadRes.Error || !uploadRes.UploadOperationId)
      throw new Error(uploadRes.Error ?? 'PDF yüklenemedi')

    // Adım 2: CreateState
    signingMessage.value = 'İmza hazırlanıyor (adım 1/3)...'
    const stateRes = await $fetch<{
      State?: string; KeyID?: string; KeySecret?: string; SignOperationId?: string; Error?: string
    }>(`${BASE}/Signature/create-state`, {
      method: 'POST',
      headers: { Authorization: `Bearer ${token.value}` },
      body: {
        Certificate: cert.data,
        UploadOperationId: uploadRes.UploadOperationId,
        SignatureLevel: 'B-B',
        Profile: 'None',
        HashAlgorithm: 'SHA256',
      },
    })
    if (stateRes.Error || !stateRes.State)
      throw new Error(stateRes.Error ?? 'CreateState başarısız')

    // Adım 3: LocalSigner imzala
    signingMessage.value = 'E-imza tokenından imza alınıyor (adım 2/3)...'
    const signResult = await localSigner.signStepTwo({
      keyId: stateRes.KeyID!,
      keySecret: stateRes.KeySecret!,
      state: stateRes.State!,
      pkcsLibrary: cert.pkcsLibrary,
      slot: cert.slot,
      pin: pin.value,
      certificateIndex: cert.certificateIndex,
    })

    if (signResult.error) {
      if (signResult.error.includes('INCORRECT_PIN'))
        throw new Error('Yanlış PIN girdiniz. Lütfen tekrar deneyin.')
      if (signResult.error.includes('PIN_BLOCKED'))
        throw new Error('PIN kilitlendi. Kart yöneticisiyle iletişime geçin.')
      throw new Error(signResult.error)
    }

    // Adım 4: FinishSign + Onayla
    signingMessage.value = 'İmza tamamlanıyor ve izin onaylanıyor (adım 3/3)...'
    const finishRes = await $fetch<{ IsSuccess?: boolean; DownloadOperationId?: string; Error?: string }>(
      `${BASE}/Signature/finish-sign/${permId}`,
      {
        method: 'POST',
        headers: { Authorization: `Bearer ${token.value}` },
        body: {
          SignedData: signResult.signedData,
          KeyId: stateRes.KeyID,
          KeySecret: stateRes.KeySecret,
          SignOperationId: stateRes.SignOperationId,
        },
      }
    )

    if (finishRes.Error || !finishRes.IsSuccess)
      throw new Error(finishRes.Error ?? 'İmzalama tamamlanamadı')

    downloadOperationId.value = finishRes.DownloadOperationId ?? null
    step.value = 4
  } catch (e: any) {
    errorMessage.value = e?.message ?? String(e)
    step.value = 2
  } finally {
    signingInProgress.value = false
    pin.value = ''
  }
}

const downloadSignedPdf = async () => {
  if (!downloadOperationId.value) return
  try {
    const res = await fetch(
      `${BASE}/Signature/download/${downloadOperationId.value}`,
      { headers: { Authorization: `Bearer ${token.value}` } }
    )
    const blob = await res.blob()
    const url = URL.createObjectURL(blob)
    const a = document.createElement('a')
    a.href = url
    a.download = 'imzali_izin_raporu.pdf'
    a.click()
    URL.revokeObjectURL(url)
  } catch {
    useToast().showError('PDF indirilemedi.')
  }
}

// Modal açıldığında sıfırla
watch(() => props.open, (val) => {
  if (val) {
    step.value = 1
    pin.value = ''
    errorMessage.value = ''
    downloadOperationId.value = null
    signingInProgress.value = false
  }
})
</script>
