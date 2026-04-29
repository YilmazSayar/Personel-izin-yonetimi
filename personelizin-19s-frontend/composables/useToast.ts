export type ToastType = 'error' | 'success' | 'info'

export interface ToastItem {
  id: number
  type: ToastType
  text: string
}

const toasts = ref<ToastItem[]>([])
let nextId = 1
let autoDismissTimer: ReturnType<typeof setTimeout>[] = []

const AUTO_DISMISS_MS = 5000

function dismiss(id: number) {
  toasts.value = toasts.value.filter((t) => t.id !== id)
}

function addToast(type: ToastType, text: string) {
  const id = nextId++
  const item: ToastItem = { id, type, text }
  toasts.value = [...toasts.value, item]

  const timer = setTimeout(() => {
    dismiss(id)
  }, AUTO_DISMISS_MS)
  autoDismissTimer.push(timer)
}

export function useToast() {
  return {
    toasts: readonly(toasts),
    showError(text: string) {
      addToast('error', text)
    },
    showSuccess(text: string) {
      addToast('success', text)
    },
    showInfo(text: string) {
      addToast('info', text)
    },
    dismiss,
  }
}
