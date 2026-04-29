<template>
  <Teleport to="body">
    <Transition name="confirm">
      <div
        v-if="state.visible"
        class="confirm-overlay"
        role="dialog"
        aria-modal="true"
        aria-labelledby="confirm-title"
        @click.self="choose(false)"
      >
        <div :class="['confirm-card', state.danger ? 'confirm-card--danger' : '']">
          <h2 id="confirm-title" class="confirm-title">{{ state.title }}</h2>
          <p class="confirm-message">{{ state.message }}</p>
          <div class="confirm-actions">
            <button
              type="button"
              class="confirm-btn confirm-btn--cancel"
              @click="choose(false)"
            >
              {{ state.cancelText }}
            </button>
            <button
              type="button"
              :class="['confirm-btn', state.danger ? 'confirm-btn--danger' : 'confirm-btn--primary']"
              @click="choose(true)"
            >
              {{ state.confirmText }}
            </button>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup>
const { state, choose } = useConfirm()
</script>

<style scoped>
.confirm-overlay {
  position: fixed;
  inset: 0;
  z-index: 10000;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 1rem;
  background: rgba(15, 23, 42, 0.5);
  backdrop-filter: blur(4px);
}

.confirm-card {
  background: #fff;
  border-radius: 16px;
  box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.25), 0 0 0 1px rgba(0, 0, 0, 0.05);
  padding: 1.5rem 1.75rem;
  max-width: 400px;
  width: 100%;
}

.confirm-card--danger .confirm-btn--primary {
  background: #dc2626;
  color: #fff;
  border-color: #dc2626;
}
.confirm-card--danger .confirm-btn--primary:hover {
  background: #b91c1c;
  border-color: #b91c1c;
}

.confirm-title {
  margin: 0 0 0.75rem;
  font-size: 1.125rem;
  font-weight: 700;
  color: #1e293b;
}

.confirm-message {
  margin: 0 0 1.5rem;
  font-size: 0.9375rem;
  line-height: 1.5;
  color: #475569;
}

.confirm-actions {
  display: flex;
  justify-content: flex-end;
  gap: 0.75rem;
}

.confirm-btn {
  padding: 0.5rem 1.25rem;
  font-size: 0.875rem;
  font-weight: 600;
  border-radius: 10px;
  cursor: pointer;
  transition: background 0.15s, border-color 0.15s, color 0.15s;
  border: 1px solid transparent;
}

.confirm-btn--cancel {
  background: #f1f5f9;
  color: #475569;
  border-color: #e2e8f0;
}
.confirm-btn--cancel:hover {
  background: #e2e8f0;
  color: #334155;
}

.confirm-btn--primary {
  background: #2563eb;
  color: #fff;
  border-color: #2563eb;
}
.confirm-btn--primary:hover {
  background: #1d4ed8;
  border-color: #1d4ed8;
}

.confirm-btn--danger {
  background: #dc2626;
  color: #fff;
  border-color: #dc2626;
}
.confirm-btn--danger:hover {
  background: #b91c1c;
  border-color: #b91c1c;
}

/* Transition */
.confirm-enter-active,
.confirm-leave-active {
  transition: opacity 0.2s ease;
}
.confirm-enter-active .confirm-card,
.confirm-leave-active .confirm-card {
  transition: transform 0.2s ease;
}
.confirm-enter-from,
.confirm-leave-to {
  opacity: 0;
}
.confirm-enter-from .confirm-card,
.confirm-leave-to .confirm-card {
  transform: scale(0.96);
}
</style>
