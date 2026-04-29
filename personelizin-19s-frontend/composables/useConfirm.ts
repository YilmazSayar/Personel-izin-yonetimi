export interface ConfirmOptions {
  title?: string
  message: string
  confirmText?: string
  cancelText?: string
  danger?: boolean
}

interface ConfirmState {
  visible: boolean
  title: string
  message: string
  confirmText: string
  cancelText: string
  danger: boolean
  resolve: ((value: boolean) => void) | null
}

const defaultState: ConfirmState = {
  visible: false,
  title: 'Onay',
  message: '',
  confirmText: 'Tamam',
  cancelText: 'İptal',
  danger: false,
  resolve: null,
}

const state = ref<ConfirmState>({ ...defaultState })

export function useConfirm() {
  function showConfirm(options: ConfirmOptions | string): Promise<boolean> {
    const opts = typeof options === 'string' ? { message: options } : options
    return new Promise((resolve) => {
      state.value = {
        visible: true,
        title: opts.title ?? 'Onay',
        message: opts.message,
        confirmText: opts.confirmText ?? 'Tamam',
        cancelText: opts.cancelText ?? 'İptal',
        danger: opts.danger ?? false,
        resolve,
      }
    })
  }

  function choose(result: boolean) {
    state.value.resolve?.(result)
    state.value = { ...defaultState }
  }

  return {
    state: readonly(state),
    showConfirm,
    choose,
  }
}
