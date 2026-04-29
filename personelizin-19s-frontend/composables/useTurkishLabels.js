/**
 * Rol ve durum değerlerini Türkçe göstermek için kullanılır.
 * API İngilizce (Manager, Approved vb.) döndürebilir; arayüzde Türkçe gösteririz.
 */
export function useTurkishLabels() {
  function roleLabel(role) {
    const r = String(role ?? '').toLowerCase()
    if (r === 'admin') return 'Sistem Yöneticisi'
    if (r === 'manager') return 'Yönetici'
    if (r === 'user') return 'Kullanıcı'
    return role ? String(role) : '—'
  }

  function statusLabel(status) {
    const s = String(status ?? '').toLowerCase()
    if (s === 'approved' || s === 'onaylandı') return 'Kabul edildi'
    if (s === 'rejected' || s === 'reddedildi') return 'Reddedildi'
    if (s === 'pending' || s === 'beklemede') return 'Beklemede'
    return status ? String(status) : 'Beklemede'
  }

  return { roleLabel, statusLabel }
}
