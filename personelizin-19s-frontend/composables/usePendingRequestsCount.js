export function usePendingRequestsCount() {
  const count = useState('admin-pending-requests-count', () => 0)

  async function refresh() {
    if (!import.meta.client) return
    const token = typeof localStorage !== 'undefined' ? localStorage.getItem('token') : null
    if (!token) {
      count.value = 0
      return
    }
    try {
      const units = await $fetch('https://localhost:44365/api/Units', {
        headers: { Authorization: `Bearer ${token}` },
      })
      const list = Array.isArray(units) ? units : []
      let total = 0
      for (const unit of list) {
        const id = unit.id ?? unit.Id
        if (!id) continue
        try {
          const data = await $fetch(`https://localhost:44365/api/Permissions/by-unit/${id}`, {
            headers: { Authorization: `Bearer ${token}` },
          })
          const arr = Array.isArray(data) ? data : []
          const pending = arr.filter((r) => {
            const s = String(r.status ?? r.Status ?? '').toLowerCase()
            return s === 'pending' || s === 'beklemede'
          })
          total += pending.length
        } catch (_) {}
      }
      count.value = total
    } catch (_) {
      count.value = 0
    }
  }

  return { count: readonly(count), refresh }
}
