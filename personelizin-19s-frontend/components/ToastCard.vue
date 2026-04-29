<template>
  <div class="toast-container">
    <TransitionGroup name="toast">
      <div
        v-for="item in toasts"
        :key="item.id"
        :class="['toast-card', `toast-card--${item.type}`]"
        role="alert"
      >
        <span class="toast-icon" aria-hidden="true">
          <template v-if="item.type === 'error'">⚠</template>
          <template v-else-if="item.type === 'success'">✓</template>
          <template v-else>ℹ</template>
        </span>
        <p class="toast-text">{{ item.text }}</p>
        <button
          type="button"
          class="toast-close"
          aria-label="Kapat"
          @click="dismiss(item.id)"
        >
          ×
        </button>
      </div>
    </TransitionGroup>
  </div>
</template>

<script setup>
const { toasts, dismiss } = useToast()
</script>

<style scoped>
.toast-container {
  position: fixed;
  top: 1rem;
  right: 1rem;
  z-index: 9999;
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
  max-width: min(420px, calc(100vw - 2rem));
  pointer-events: none;
}
.toast-container > * {
  pointer-events: auto;
}

.toast-card {
  display: flex;
  align-items: flex-start;
  gap: 0.75rem;
  padding: 0.875rem 1rem;
  border-radius: 12px;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.12), 0 0 1px rgba(0, 0, 0, 0.08);
  border: 1px solid transparent;
  background: #fff;
  min-width: 280px;
}

.toast-card--error {
  background: #fef2f2;
  border-color: #fecaca;
  color: #991b1b;
}
.toast-card--error .toast-icon { color: #dc2626; }

.toast-card--success {
  background: #f0fdf4;
  border-color: #bbf7d0;
  color: #166534;
}
.toast-card--success .toast-icon { color: #16a34a; }

.toast-card--info {
  background: #eff6ff;
  border-color: #bfdbfe;
  color: #1e40af;
}
.toast-card--info .toast-icon { color: #2563eb; }

.toast-icon {
  flex-shrink: 0;
  width: 1.5rem;
  height: 1.5rem;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.1rem;
  font-weight: bold;
}

.toast-text {
  flex: 1;
  margin: 0;
  font-size: 0.9rem;
  font-weight: 500;
  line-height: 1.4;
  word-break: break-word;
}

.toast-close {
  flex-shrink: 0;
  width: 1.75rem;
  height: 1.75rem;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 0;
  border: none;
  background: transparent;
  color: inherit;
  opacity: 0.7;
  font-size: 1.25rem;
  line-height: 1;
  cursor: pointer;
  border-radius: 6px;
  transition: opacity 0.15s, background 0.15s;
}
.toast-close:hover {
  opacity: 1;
  background: rgba(0, 0, 0, 0.06);
}

/* Transition */
.toast-enter-active,
.toast-leave-active {
  transition: transform 0.25s ease, opacity 0.25s ease;
}
.toast-enter-from,
.toast-leave-to {
  transform: translateX(100%);
  opacity: 0;
}
.toast-move {
  transition: transform 0.25s ease;
}
</style>
