/**
 * Merkezi auth state: userId, token, userName.
 * Giriş yapıldığında setUser ile set edin; tüm sayfalar bu composable ile aynı değerleri kullanır.
 */
export function useAuth() {
  const token = useState('auth-token', () => null);
  const userId = useState('auth-userId', () => null);
  const userName = useState('auth-userName', () => null);
  const userRole = useState('auth-userRole', () => null);
  const mustChangePassword = useState('auth-mustChangePassword', () => false);

  if (import.meta.client && typeof localStorage !== 'undefined') {
    if (!token.value) token.value = localStorage.getItem('token');
    if (!userId.value) userId.value = localStorage.getItem('userId');
    if (!userName.value) userName.value = localStorage.getItem('userName');
    if (!userRole.value) userRole.value = localStorage.getItem('userRole');
    if (!mustChangePassword.value) mustChangePassword.value = localStorage.getItem('mustChangePassword') === 'true';
  }

  function setUser({ token: t, userId: id, userName: name, userRole: role, mustChangePassword: mcp }) {
    const newToken = t ?? token.value;
    const newUserId = id ?? userId.value;
    const newName = name ?? userName.value;
    const newRole = role ?? userRole.value;
    const newMcp = mcp !== undefined ? mcp : mustChangePassword.value;

    token.value = newToken;
    userId.value = newUserId != null ? String(newUserId) : null;
    userName.value = newName;
    userRole.value = newRole;
    mustChangePassword.value = !!newMcp;

    if (import.meta.client && typeof localStorage !== 'undefined') {
      if (newToken) localStorage.setItem('token', newToken);
      else localStorage.removeItem('token');
      if (newUserId != null) localStorage.setItem('userId', String(newUserId));
      else localStorage.removeItem('userId');
      if (newName) localStorage.setItem('userName', newName);
      else localStorage.removeItem('userName');
      if (newRole) localStorage.setItem('userRole', newRole);
      else localStorage.removeItem('userRole');
      if (newMcp) localStorage.setItem('mustChangePassword', 'true');
      else localStorage.removeItem('mustChangePassword');
    }
  }

  function clearMustChangePassword() {
    mustChangePassword.value = false;
    if (import.meta.client && typeof localStorage !== 'undefined') {
      localStorage.removeItem('mustChangePassword');
    }
  }

  function clearUser() {
    token.value = null;
    userId.value = null;
    userName.value = null;
    userRole.value = null;
    mustChangePassword.value = false;
    if (import.meta.client && typeof localStorage !== 'undefined') {
      localStorage.removeItem('token');
      localStorage.removeItem('userId');
      localStorage.removeItem('userName');
      localStorage.removeItem('userRole');
      localStorage.removeItem('mustChangePassword');
    }
  }

  const isLoggedIn = computed(() => !!(token.value && userId.value));

  const isAdmin = computed(() => (userRole.value || '').toLowerCase() === 'manager');
  const isSystemAdmin = computed(() => (userRole.value || '').toLowerCase() === 'admin');

  return {
    token,
    userId,
    userName,
    userRole,
    mustChangePassword,
    isAdmin,
    isSystemAdmin,
    setUser,
    clearUser,
    clearMustChangePassword,
    isLoggedIn,
  };
}
