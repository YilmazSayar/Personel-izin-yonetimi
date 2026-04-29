export interface CertificateInfo {
  serialNumber: string
  id: string
  personFullname: string
  validFrom: string
  validTo: string
  citizenshipNo: string
  data: string
  pkcsLibrary: string
  slot: string
  isFinancialSeal: boolean
  certificateIndex?: number | null
}

export interface SignStepTwoRequest {
  keyId: string
  keySecret: string
  state: string
  pkcsLibrary: string
  slot: string
  pin: string
  certificateIndex?: number | null
}

export interface SignStepTwoResult {
  signedData?: string
  error?: string
}

// localhost önce, 127.0.0.1 fallback (bazı sistemlerde IPv6 çözümleme sorunu olabilir)
const SIGNER_URLS = [
  'http://localhost:8099',
  'http://127.0.0.1:8099',
  'https://localsigner.onaylarim.com:8099',
  'http://localsigner.onaylarim.com:8099',
]

const fetchWithTimeout = (url: string, opts: RequestInit = {}, timeoutMs = 3000): Promise<Response> => {
  return new Promise((resolve, reject) => {
    const timer = setTimeout(() => reject(new Error('timeout')), timeoutMs)
    fetch(url, opts)
      .then(res => { clearTimeout(timer); resolve(res) })
      .catch(err => { clearTimeout(timer); reject(err) })
  })
}

export const useLocalSigner = () => {
  const workingUrl = ref<string | null>(null)
  const isConnected = ref(false)
  const certificates = ref<CertificateInfo[]>([])
  const status = ref<'idle' | 'connecting' | 'connected' | 'error'>('idle')
  const statusMessage = ref('')
  const debugLog = ref<string[]>([])

  const log = (msg: string) => {
    debugLog.value.push(msg)
    console.log('[LocalSigner]', msg)
  }

  /** Ping + Reset tek adımda: bir istekle hem bağlantıyı test eder hem sertifikaları alır */
  const tryResetAt = async (url: string): Promise<CertificateInfo[] | null> => {
    log(`Deneniyor: ${url}/Reset`)
    try {
      const res = await fetchWithTimeout(
        `${url}/Reset`,
        { method: 'POST', headers: { 'Content-Type': 'application/json' } },
        3000
      )
      log(`${url} → HTTP ${res.status}`)

      if (!res.ok) {
        log(`${url} başarısız: HTTP ${res.status}`)
        return null
      }

      let data: any
      try { data = await res.json() } catch { log(`${url} JSON parse hatası`); return null }

      log(`${url} yanıt: ${JSON.stringify(data).slice(0, 200)}`)

      // status=1 → kart okuyucu hazırlanıyor, polling yap
      if (data?.status === 1) {
        log(`${url} kart okuyucu başlatılıyor, bekleniyor...`)
        for (let i = 0; i < 10; i++) {
          await new Promise(r => setTimeout(r, 600))
          const r2 = await tryResetAt(url)
          if (r2 !== null) return r2
        }
        return null
      }

      // Sertifika listesi kontrolü
      const certs: CertificateInfo[] | null =
        Array.isArray(data?.certificates) && data.certificates.length > 0
          ? data.certificates
          : Array.isArray(data) && data.length > 0
            ? data
            : null

      if (certs) {
        log(`${url} → ${certs.length} sertifika bulundu ✓`)
        return certs
      }

      log(`${url} yanıtta sertifika yok: ${data?.error ?? 'bilinmiyor'}`)
      return null
    } catch (e: any) {
      log(`${url} hata: ${e?.message ?? String(e)}`)
      return null
    }
  }

  const tryConnect = async (): Promise<boolean> => {
    if (!import.meta.client) return false

    status.value = 'connecting'
    workingUrl.value = null
    isConnected.value = false
    certificates.value = []
    debugLog.value = []

    for (const url of SIGNER_URLS) {
      statusMessage.value = `Bağlanılıyor: ${url} ...`
      const certs = await tryResetAt(url)
      if (certs) {
        workingUrl.value = url
        certificates.value = certs
        isConnected.value = true
        status.value = 'connected'
        statusMessage.value = `${certs.length} sertifika bulundu.`
        return true
      }
    }

    status.value = 'error'
    // Detaylı hata + debug çıktısını göster
    statusMessage.value =
      'LocalSigner uygulamasına bağlanılamadı. ' +
      'Tarayıcı konsolunda [LocalSigner] loglarını inceleyin. ' +
      `Denenen URL'ler: ${SIGNER_URLS.join(', ')}`
    return false
  }

  const signStepTwo = async (req: SignStepTwoRequest): Promise<SignStepTwoResult> => {
    if (!workingUrl.value) return { error: 'LocalSigner bağlı değil.' }
    try {
      const res = await fetchWithTimeout(
        `${workingUrl.value}/signStepTwo`,
        {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify(req),
        },
        30000
      )
      const data = await res.json()
      if (data?.error) return { error: data.error }
      return { signedData: data.signedData }
    } catch (e: any) {
      return { error: `İmzalama hatası: ${e?.message ?? String(e)}` }
    }
  }

  return {
    workingUrl: readonly(workingUrl),
    isConnected: readonly(isConnected),
    certificates: readonly(certificates),
    status: readonly(status),
    statusMessage: readonly(statusMessage),
    debugLog: readonly(debugLog),
    tryConnect,
    signStepTwo,
  }
}
