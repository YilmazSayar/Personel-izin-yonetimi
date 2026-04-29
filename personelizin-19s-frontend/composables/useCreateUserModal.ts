export function useCreateUserModal() {
  const isOpen = useState('create-user-modal-open', () => false)
  const refreshTrigger = useState<number>('admin-users-refresh', () => 0)

  function open() {
    isOpen.value = true
  }

  function close() {
    isOpen.value = false
  }

  function notifyCreated() {
    refreshTrigger.value = Date.now()
  }

  return {
    isOpen: readonly(isOpen),
    open,
    close,
    notifyCreated,
    refreshTrigger: readonly(refreshTrigger),
  }
}
