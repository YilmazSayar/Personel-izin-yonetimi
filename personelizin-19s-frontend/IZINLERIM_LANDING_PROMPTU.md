# İzinlerim Landing Sayfası – Cursor Prompt

**Bu metni başka bir projeyi Cursor'da açtıktan sonra yapıştır. Hedef: Bu projede "İzinlerim" personel izin yönetim sistemi landing (giriş) sayfasını birebir oluşturmak.**

---

## 1. Genel Hedef

Projede **ana sayfa (landing)** olarak şunlar olsun:

- **Header:** Solda "İzinlerim" logosu (mavi, kalın, uppercase), sağda "Giriş Yap" (link) ve "Hemen Kaydol" (mavi buton).
- **Hero:** Solda rozet "Personel İzin Yönetim Sistemi", başlık "İzin Süreçlerinizi Dijitalleştirin." (Dijitalleştirin mavi ve altı çizgili), açıklama paragrafı, iki buton: "Hemen Başlayın" (koyu) ve "Sisteme Giriş" (çerçeveli).
- **Sağ bölüm:** Tanıtım videosu (otomatik oynar, sessiz, döngüde); video yoksa statik kart: "izinlerim" + "Personel İzin Takibi Artık Çok Kolay".
- **Footer:** © 2026 İzinlerim, Kullanım Koşulları, Gizlilik Politikası.

**Teknoloji:** Nuxt 3 + Tailwind CSS. Sayfa `layout: false` ile tam ekran (sidebar yok).

---

## 2. Proje Kurulumu (Henüz yoksa)

```bash
npx nuxi@latest init my-app
cd my-app
npm install
npm install -D @nuxtjs/tailwindcss
```

`nuxt.config.ts` içinde Tailwind modülü:

```ts
export default defineNuxtConfig({
  modules: ['@nuxtjs/tailwindcss']
})
```

Video için klasör:

- `public/videos/` oluştur.
- Tanıtım videosunu (Remotion'dan export ettiğin MP4 veya herhangi bir MP4) `public/videos/tanitim.mp4` olarak koy. Video yoksa aşağıdaki kodda "video yok" durumunda statik kart gösterilir.

---

## 3. Ana Sayfa Kodu

**Dosya:** `pages/index.vue`

Aşağıdaki kodu olduğu gibi kullan. Projede `useAuth` yoksa "Oturum yönlendirmesi" bölümünü kaldırıp sadece `layout: false` ve statik içeriği bırak.

```vue
<template>
  <div class="min-h-screen bg-white selection:bg-blue-100">
    <nav class="max-w-7xl mx-auto px-6 py-6 flex justify-between items-center">
      <div class="text-2xl font-black text-blue-600 tracking-tighter uppercase italic">İzinlerim</div>
      <div class="flex items-center gap-6">
        <NuxtLink to="/login" class="font-bold text-gray-600 hover:text-blue-600 transition-colors">Giriş Yap</NuxtLink>
        <NuxtLink to="/register" class="bg-blue-600 text-white px-6 py-3 rounded-2xl font-black hover:bg-blue-700 transition-all shadow-lg shadow-blue-200 active:scale-95">
          Hemen Kaydol
        </NuxtLink>
      </div>
    </nav>

    <header class="max-w-7xl mx-auto px-6 pt-20 pb-32 flex flex-col lg:flex-row items-center gap-16">
      <div class="flex-1 text-center lg:text-left">
        <div class="inline-block bg-blue-50 text-blue-600 px-4 py-2 rounded-full text-xs font-black uppercase tracking-widest mb-6">
          ✨ Personel İzin Yönetim Sistemi
        </div>
        <h1 class="text-5xl md:text-7xl font-black text-gray-900 leading-[1.1] mb-8">
          İzin Süreçlerinizi <br/> <span class="text-blue-600 underline decoration-blue-100 underline-offset-8">Dijitalleştirin.</span>
        </h1>
        <p class="text-xl text-gray-500 font-medium mb-12 max-w-xl mx-auto lg:mx-0 leading-relaxed">
          İzinlerim ile taleplerinizi saniyeler içinde oluşturun, onay süreçlerini şeffaf bir şekilde takip edin. Karmaşık formlara veda edin.
        </p>
        <div class="flex flex-col sm:flex-row justify-center lg:justify-start gap-4">
          <NuxtLink to="/register" class="bg-gray-900 text-white px-10 py-5 rounded-3xl font-black text-lg hover:shadow-2xl hover:-translate-y-1 transition-all duration-300 text-center">
            Hemen Başlayın
          </NuxtLink>
          <NuxtLink to="/login" class="border-2 border-gray-100 text-gray-700 px-10 py-5 rounded-3xl font-black text-lg hover:bg-gray-50 transition-all text-center">
            Sisteme Giriş
          </NuxtLink>
        </div>
      </div>

      <div class="flex-1 w-full max-w-2xl lg:max-w-none">
        <div class="relative group">
          <div class="absolute -inset-4 bg-gradient-to-tr from-blue-400 to-indigo-400 rounded-[3rem] blur-2xl opacity-20 group-hover:opacity-30 transition-opacity"></div>

          <!-- Tanıtım videosu (Remotion/MP4). Yüklenemezse statik kart gösterilir. -->
          <div class="relative bg-white border-[12px] border-gray-50 rounded-[3rem] shadow-2xl overflow-hidden aspect-video group-hover:border-white transition-all">
            <video
              v-show="hasVideo"
              autoplay
              loop
              muted
              playsinline
              class="w-full h-full object-cover"
              src="/videos/tanitim.mp4"
              @error="hasVideo = false"
            >
              Tarayıcınız video etiketini desteklemiyor.
            </video>
            <div v-show="!hasVideo" class="w-full h-full min-h-[280px] flex flex-col items-center justify-center p-8 bg-gradient-to-br from-blue-50 to-white">
              <p class="text-4xl md:text-5xl font-black text-blue-600 tracking-tight">izinlerim</p>
              <p class="text-gray-500 font-medium mt-3 text-sm md:text-base">Personel İzin Takibi Artık Çok Kolay</p>
            </div>
            <div class="absolute inset-0 bg-gradient-to-t from-black/10 to-transparent pointer-events-none"></div>
          </div>
        </div>
      </div>
    </header>

    <footer class="border-t border-gray-100 py-12 mt-auto">
      <div class="max-w-7xl mx-auto px-6 flex flex-col md:flex-row justify-between items-center gap-6">
        <p class="text-gray-400 font-bold text-sm">© 2026 İzinlerim. Tüm hakları saklıdır.</p>
        <div class="flex gap-8 text-gray-400 font-bold text-sm">
          <a href="#" class="hover:text-blue-600 transition">Kullanım Koşulları</a>
          <a href="#" class="hover:text-blue-600 transition">Gizlilik Politikası</a>
        </div>
      </div>
    </footer>
  </div>
</template>

<script setup>
definePageMeta({
  layout: false
})

// Video yüklenemezse (dosya yok vb.) statik kart gösterilir
const hasVideo = ref(true)

// Opsiyonel: Projede useAuth varsa ve giriş yapılmışsa yönlendir
// const { token, isSystemAdmin } = useAuth()
// onMounted(() => {
//   if (token.value) navigateTo(isSystemAdmin?.value ? '/admin-panel' : '/dashboard')
// })
</script>
```

**Not:** Video dosyası yoksa `<video>` `@error` ile hasVideo false olur ve statik kart gösterilir. Video her zaman kullanılacaksa "Sadece Video" bölümündeki sade sürüm kullanılabilir.

---

## 4. Sadece Video Kullanımı (Statik Kart İstemiyorsan)

Video her zaman gösterilecekse, sağ bölümü şöyle sadeleştir:

```vue
<div class="relative bg-white border-[12px] border-gray-50 rounded-[3rem] shadow-2xl overflow-hidden aspect-video group-hover:border-white transition-all">
  <video autoplay loop muted playsinline class="w-full h-full object-cover">
    <source src="/videos/tanitim.mp4" type="video/mp4">
    Tarayıcınız video etiketini desteklemiyor.
  </video>
  <div class="absolute inset-0 bg-gradient-to-t from-black/10 to-transparent pointer-events-none"></div>
</div>
```

Remotion ile ürettiğin videoyu export edip `public/videos/tanitim.mp4` olarak koyman yeterli.

---

## 5. Kontrol Listesi

- [ ] Nuxt 3 projesi var; `@nuxtjs/tailwindcss` eklendi.
- [ ] `pages/index.vue` yukarıdaki kodla oluşturuldu.
- [ ] `definePageMeta({ layout: false })` ile sayfa tam ekran.
- [ ] `public/videos/` klasörü var; tanıtım videosu `public/videos/tanitim.mp4` (Remotion veya başka kaynak).
- [ ] Giriş/Kayıt için `pages/login.vue` ve `pages/register.vue` projede tanımlı (en azından placeholder).

Bu adımlarla Cursor'da bu MD'yi yapıştırdığında aynı landing sayfası (video veya statik kart ile) oluşturulabilir.
