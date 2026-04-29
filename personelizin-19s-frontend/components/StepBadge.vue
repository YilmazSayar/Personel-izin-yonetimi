<template>
  <div class="flex flex-col items-center gap-1 min-w-[48px]">
    <div
      class="w-7 h-7 rounded-full flex items-center justify-center text-xs font-bold transition-colors"
      :class="done
        ? 'bg-green-500 text-white'
        : active
          ? 'bg-blue-600 text-white'
          : 'bg-gray-200 text-gray-500'"
    >
      <svg v-if="done" class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M5 13l4 4L19 7" />
      </svg>
      <span v-else>{{ n }}</span>
    </div>
    <span class="text-[10px] text-center" :class="active ? 'text-blue-600 font-semibold' : 'text-gray-400'">
      {{ label }}
    </span>
  </div>
</template>

<script setup lang="ts">
defineProps<{ n: number; label: string; active: boolean; done: boolean }>()
</script>
