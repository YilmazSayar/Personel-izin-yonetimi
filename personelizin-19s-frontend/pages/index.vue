<template>
  <div class="min-h-screen bg-white selection:bg-blue-100 flex flex-col">
    <nav class="max-w-7xl mx-auto w-full px-6 py-6 flex justify-between items-center">
      <div class="logo-text text-2xl font-extrabold text-blue-600 tracking-tight">İzinlerim</div>
    </nav>

    <header class="max-w-7xl mx-auto w-full px-6 pt-12 pb-24 flex flex-col lg:flex-row items-center gap-12 lg:gap-16 flex-1">
      <div class="flex-1 text-center lg:text-left">
        <div
          class="inline-block bg-blue-50 text-blue-600 px-4 py-2 rounded-full text-xs font-black uppercase tracking-widest mb-6"
        >
          ✨ Personel İzin Yönetim Sistemi
        </div>
        <h1 class="text-4xl sm:text-5xl md:text-6xl lg:text-7xl font-black text-gray-900 leading-[1.1] mb-6">
          İzin Süreçlerinizi
          <br />
          <span class="text-blue-600 underline decoration-blue-200 underline-offset-8">Dijitalleştirin.</span>
        </h1>
        <p class="text-lg sm:text-xl text-gray-500 font-medium mb-10 max-w-xl mx-auto lg:mx-0 leading-relaxed">
          İzinlerim ile taleplerinizi saniyeler içinde oluşturun, onay süreçlerini şeffaf bir şekilde takip edin.
          Karmaşık formlara veda edin.
        </p>
        <div class="flex flex-col sm:flex-row justify-center lg:justify-start gap-4">
          <NuxtLink
            to="/register"
            class="bg-gray-900 text-white px-8 py-4 rounded-3xl font-black text-base hover:shadow-2xl hover:-translate-y-0.5 transition-all duration-300 text-center"
          >
            Hemen Başlayın
          </NuxtLink>
          <NuxtLink
            to="/login"
            class="border-2 border-gray-200 text-gray-700 px-8 py-4 rounded-3xl font-black text-base hover:bg-gray-50 transition-all text-center"
          >
            Sisteme Giriş
          </NuxtLink>
        </div>
      </div>

      <div class="flex-1 w-full max-w-3xl xl:max-w-4xl lg:max-w-none">
        <div class="relative group">
          <div
            class="absolute -inset-4 bg-gradient-to-tr from-blue-400 to-indigo-400 rounded-[3rem] blur-2xl opacity-20 group-hover:opacity-30 transition-opacity"
          />
          <div
            class="relative bg-white border-[12px] border-gray-100 rounded-[3rem] shadow-2xl overflow-hidden min-h-[340px] aspect-video xl:min-h-[420px] group-hover:border-gray-200 transition-all"
          >
            <div class="w-full h-full min-h-[340px] xl:min-h-[420px] bg-gradient-to-br from-blue-50/50 to-white relative">
              <video
                ref="videoEl"
                autoplay
                loop
                muted
                playsinline
                class="absolute inset-0 w-full h-full object-cover"
                src="/videos/tanitim.mp4"
                @error="onVideoError"
              />
            </div>
            <div class="absolute inset-0 bg-gradient-to-t from-black/10 to-transparent pointer-events-none rounded-[2.25rem]" />
          </div>
        </div>
      </div>
    </header>

    <footer class="border-t border-gray-100 py-8 mt-auto">
      <div class="max-w-7xl mx-auto px-6">
        <p class="text-gray-400 font-bold text-sm">© 2026 İzinlerim. Tüm hakları saklıdır.</p>
      </div>
    </footer>
  </div>
</template>

<script setup>
definePageMeta({
  layout: false,
});

useHead({
  link: [
    {
      rel: 'stylesheet',
      href: 'https://fonts.googleapis.com/css2?family=Plus+Jakarta+Sans:wght@400;500;600;700;800&display=swap',
    },
  ],
});

const videoEl = ref(null);

function onVideoError() {
  if (videoEl.value) videoEl.value.style.display = 'none';
}

onMounted(() => {
  videoEl.value?.play?.().catch(() => {});
});
</script>

<style scoped>
.logo-text {
  font-family: 'Plus Jakarta Sans', sans-serif;
  font-weight: 800;
}
</style>
