# Admin Panel Entegrasyonu – Cursor Prompt

**Bu metni başka projeyi Cursor’da açtıktan sonra yapıştır. Hedef: Bu projede tanımlı “Sistem Yönetimi” admin panelini birebir entegre etmek.**

---

## 1. Genel Hedef

Bu projeye **Sistem Yönetimi (Admin Panel)** ekle:

- Sadece **Admin** rolüne sahip kullanıcı erişebilsin.
- Giriş: **admin99@gmail.com** / **9876** (sistem yöneticisi; bu hesap backend’de otomatik oluşturulacak).
- Admin panelde: **Arama** (e-posta veya ad soyad ile; baştan eşleşme, arama boşken liste boş), **tablo** (Ad Soyad/E-posta, Rol, Birim, Kalan İzin, Oluşturulma, İşlemler), **Rol dropdown** (User/Manager, değişince onay), **Sil** butonu. Sistem yöneticisi silinemez/düşürülemez.
- Sistem yöneticisi girişte **Dashboard menüsü görünmesin**; sadece **Sistem Yönetimi** ve **Çıkış Yap** olsun. Refresh’te Dashboard bir an bile görünmesin (rol yüklenene kadar Dashboard linki gösterilmesin).

---

## 2. Backend (ASP.NET Core)

### 2.1 User modeli

`User` entity’sinde mutlaka olmalı: `Id`, `FullName`, `Email`, `PasswordHash`, `Role`, `UnitId` (nullable), `RemainingLeaveDays`, **`CreatedAt`** (DateTime). Yoksa ekle:

```csharp
public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
```

Veritabanında `Users` tablosuna `CreatedAt` sütunu yoksa ekle (örn. migration veya startup’ta):

```sql
ALTER TABLE "Users" ADD COLUMN IF NOT EXISTS "CreatedAt" timestamp with time zone NOT NULL DEFAULT (now() AT TIME ZONE 'UTC');
```

### 2.2 AdminController

Aşağıdaki controller’ı ekle. Namespace ve `AppDbContext` adını projedeki yapıya göre değiştir.

**Dosya:** `Controllers/AdminController.cs`

```csharp
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
// using personelizin_backend.Data;  → kendi DbContext namespace'in
// using personelizin_backend.Models; → kendi Models namespace'in

namespace personelizin_backend.Controllers  // kendi namespace'in
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;  // kendi DbContext adın

        public AdminController(AppDbContext context) => _context = context;

        private int? GetCurrentUserId()
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(claim, out var id) ? id : null;
        }

        private async Task<bool> IsAdminAsync()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return false;
            var user = await _context.Users.FindAsync(userId.Value);
            return user != null && string.Equals(user.Role, "Admin", StringComparison.OrdinalIgnoreCase);
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            if (!await IsAdminAsync()) return Forbid();
            var list = await _context.Users
                .AsNoTracking()
                .OrderBy(u => u.FullName)
                .Select(u => new
                {
                    u.Id,
                    u.Email,
                    u.FullName,
                    u.Role,
                    u.UnitId,
                    u.RemainingLeaveDays,
                    u.CreatedAt,
                    UnitName = u.UnitId != null ? _context.Units.Where(t => t.Id == u.UnitId).Select(t => t.Name).FirstOrDefault() : null
                })
                .ToListAsync();
            var result = list.Select(u => new
            {
                u.Id,
                u.Email,
                u.FullName,
                u.Role,
                u.UnitId,
                u.RemainingLeaveDays,
                CreatedAt = u.CreatedAt.Kind == DateTimeKind.Utc ? u.CreatedAt : DateTime.SpecifyKind(u.CreatedAt, DateTimeKind.Utc),
                u.UnitName
            }).ToList();
            return Ok(result);
        }

        [HttpGet("user-by-email")]
        public async Task<IActionResult> GetUserByEmail([FromQuery] string? email)
        {
            if (!await IsAdminAsync()) return Forbid();
            if (string.IsNullOrWhiteSpace(email)) return BadRequest("E-posta adresi girin.");
            var user = await _context.Users
                .AsNoTracking()
                .Where(u => u.Email != null && u.Email.Trim().ToLower() == email.Trim().ToLower())
                .Select(u => new { u.Id, u.Email, u.FullName, u.Role, u.UnitId, u.CreatedAt, UnitName = u.UnitId != null ? _context.Units.Where(t => t.Id == u.UnitId).Select(t => t.Name).FirstOrDefault() : null })
                .FirstOrDefaultAsync();
            if (user == null) return NotFound(new { message = "Bu e-posta ile kayıtlı hesap bulunamadı." });
            var createdAtUtc = user.CreatedAt.Kind == DateTimeKind.Utc ? user.CreatedAt : DateTime.SpecifyKind(user.CreatedAt, DateTimeKind.Utc);
            return Ok(new { user.Id, user.Email, user.FullName, user.Role, user.UnitId, CreatedAt = createdAtUtc, user.UnitName });
        }

        [HttpPost("promote-to-manager/{userId:int}")]
        public async Task<IActionResult> PromoteToManager(int userId)
        {
            if (!await IsAdminAsync()) return Forbid();
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound(new { message = "Kullanıcı bulunamadı." });
            if (string.Equals(user.Role, "Manager", StringComparison.OrdinalIgnoreCase)) return Ok(new { message = "Bu kullanıcı zaten yönetici." });
            user.Role = "Manager";
            await _context.SaveChangesAsync();
            return Ok(new { message = "Kullanıcı yönetici olarak atandı.", userId = user.Id });
        }

        [HttpPost("demote-to-user/{userId:int}")]
        public async Task<IActionResult> DemoteToUser(int userId)
        {
            if (!await IsAdminAsync()) return Forbid();
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound(new { message = "Kullanıcı bulunamadı." });
            if (string.Equals(user.Role, "Admin", StringComparison.OrdinalIgnoreCase)) return BadRequest("Sistem yöneticisi hesabının yetkisi düşürülemez.");
            user.Role = "User";
            await _context.SaveChangesAsync();
            return Ok(new { message = "Kullanıcı yetkisi düşürüldü.", userId = user.Id });
        }

        [HttpDelete("users/{userId:int}")]
        public async Task<IActionResult> DeleteUser(int userId)
        {
            if (!await IsAdminAsync()) return Forbid();
            var currentId = GetCurrentUserId();
            if (currentId == userId) return BadRequest("Kendi hesabınızı silemezsiniz.");
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound(new { message = "Kullanıcı bulunamadı." });
            if (string.Equals(user.Role, "Admin", StringComparison.OrdinalIgnoreCase)) return BadRequest("Sistem yöneticisi hesabı silinemez.");
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Kullanıcı silindi." });
        }
    }
}
```

### 2.3 Uygulama başlangıcında Admin kullanıcı

Uygulama ayağa kalkarken (ör. `Program.cs` veya startup’ta) aşağıyı çalıştır. DbContext’i scope’tan al:

```csharp
const string adminEmail = "admin99@gmail.com";
const string adminPassword = "9876";
if (!context.Users.Any(u => u.Email == adminEmail))
{
    context.Users.Add(new User
    {
        FullName = "Sistem Yöneticisi",
        Email = adminEmail,
        PasswordHash = adminPassword,
        Role = "Admin",
        CreatedAt = DateTime.UtcNow
    });
    context.SaveChanges();
}
```

(Bu projede `User` şifreyi düz metin tutuyorsa aynen böyle; hash kullanıyorsanız `PasswordHash` alanını kendi hash’leme mantığına göre doldur.)

---

## 3. Frontend (Nuxt 3)

### 3.1 useAuth composable

`composables/useAuth.js` (veya `.ts`) içinde:

- `userRole` state’i ve localStorage ile senkron olmalı.
- **`isSystemAdmin`** ekle: `computed(() => (userRole.value || '').toLowerCase() === 'admin')`.
- Return objesine `isSystemAdmin` ekle.

Örnek (mevcut yapına göre uyarla):

```javascript
const isSystemAdmin = computed(() => (userRole.value || '').toLowerCase() === 'admin');
return {
  token,
  userId,
  userName,
  userRole,
  isAdmin,        // manager
  isSystemAdmin,  // admin
  setUser,
  clearUser,
  isLoggedIn,
};
```

### 3.2 Login sonrası yönlendirme

Giriş başarılı olduğunda, dönen rol **"Admin"** ise `/admin-panel`, değilse `/dashboard` (veya ana sayfa) kullan:

```javascript
const role = (userRole || '').toLowerCase();
navigateTo(role === 'admin' ? '/admin-panel' : '/dashboard');
```

### 3.3 Admin layout (sidebar)

Kullandığın admin layout dosyasında (ör. `layouts/admin.vue`):

- **Dashboard linki** sadece rol belli ve sistem yöneticisi **değilse** görünsün (refresh’te bir an görünmesin diye):
  - `v-if="userRole && !isSystemAdmin"` (Dashboard için).
- **Sistem yöneticisi** için ayrı blok:
  - `v-if="isSystemAdmin"` içinde sadece **Sistem Yönetimi** (`/admin-panel`) ve **Çıkış Yap**.
- `userRole`, `isSystemAdmin`, `isAdmin` layout’ta useAuth’tan alınsın.
- `accountTypeLabel`: sistem yöneticisi için "Sistem Yöneticisi" yaz.

Örnek (sadece ilgili kısım):

```vue
<NuxtLink v-if="userRole && !isSystemAdmin" to="/dashboard" ...>Dashboard</NuxtLink>
<template v-if="isSystemAdmin">
  <div class="...">Sistem</div>
  <NuxtLink to="/admin-panel" ...>Sistem Yönetimi</NuxtLink>
</template>
```

### 3.4 Admin panel sayfası

**Dosya:** `pages/admin-panel.vue`

- `definePageMeta({ layout: 'admin', middleware: 'auth' })`.
- Sadece `isSystemAdmin` ise içeriği göster; değilse `navigateTo('/dashboard')`.
- API base URL’i projedeki backend adresine göre tek yerde tanımla (örn. `const API_BASE = 'https://localhost:44365'` veya env’den oku). Aşağıdaki snippet’te `API_BASE` kullan.

**Özellikler:**

- Üstte **arama kutusu**: placeholder "E-posta veya ad soyad yazın (harf taraması yapılır)...". Arama **boşken** tabloda **hiç kayıt** gösterilmesin.
- Filtreleme: **Baştan eşleşme** (prefix). Yani `searchQuery` sadece ad soyad veya e-posta **başlangıcıyla** eşleşen kullanıcıları getirsin (içinde geçen ama başta olmayan eşleşmeler gelmesin).
- Tablo: Ad Soyad / E-posta, Rol (dropdown: User | Manager), Birim, Kalan İzin, Oluşturulma (Türkiye saati), İşlemler (Sil). Sistem yöneticisi satırında rol dropdown ve silme olmasın.
- Rol değiştirince: "Rolü … olarak değiştirmek istediğinize emin misiniz?" onayı; onaydan sonra ilgili API (promote/demote) çağrılsın.
- Sil: Onay sonrası DELETE `/api/Admin/users/{id}`.

Aşağıda **tam** `admin-panel.vue` kodu var. Projede API adresi farklıysa `API_BASE` değerini (ve gerekirse `$fetch` URL’lerini) değiştir.

```vue
<template>
  <div class="max-w-6xl mx-auto">
    <h1 class="text-2xl font-bold text-gray-800 mb-2">Sistem Yönetimi</h1>
    <p class="text-gray-600 mb-6">Kullanıcıları listeleyebilir, rol atayabilir ve silebilirsiniz.</p>

    <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-4 mb-4">
      <label class="block text-sm font-medium text-gray-700 mb-2">Arama</label>
      <div class="relative">
        <span class="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400">🔍</span>
        <input
          v-model="searchQuery"
          type="text"
          placeholder="E-posta veya ad soyad yazın (harf taraması yapılır)..."
          class="w-full pl-10 pr-4 py-2.5 border border-gray-200 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent outline-none"
        />
        <button v-if="searchQuery" type="button" class="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gray-600" aria-label="Temizle" @click="searchQuery = ''">✕</button>
      </div>
      <p v-if="searchQuery.trim()" class="mt-2 text-sm text-gray-500">
        {{ filteredUsers.length }} kullanıcı listeleniyor
        <span v-if="filteredUsers.length < users.length">(toplam {{ users.length }})</span>
      </p>
      <p v-else class="mt-2 text-sm text-gray-500">Arama kutusuna e-posta veya ad soyad yazın; eşleşen kullanıcılar tabloda listelenir. Arama boşken liste gösterilmez.</p>
    </div>

    <div class="bg-white rounded-2xl shadow-sm border border-gray-100 overflow-hidden">
      <div class="overflow-x-auto">
        <table class="w-full text-left">
          <thead>
            <tr class="text-gray-400 text-xs font-semibold uppercase tracking-widest border-b border-gray-100 bg-gray-50/80">
              <th class="px-6 py-4">Ad Soyad / E-posta</th>
              <th class="px-6 py-4">Rol</th>
              <th class="px-6 py-4">Birim</th>
              <th class="px-6 py-4">Kalan İzin</th>
              <th class="px-6 py-4">Oluşturulma</th>
              <th class="px-6 py-4">İşlemler</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="u in filteredUsers" :key="u.Id ?? u.id" class="border-t border-gray-100 hover:bg-gray-50/50 transition">
              <td class="px-6 py-4">
                <p class="font-bold text-gray-900">{{ (u.FullName ?? u.fullName) || '—' }}</p>
                <p class="text-sm text-gray-500">{{ u.Email ?? u.email }}</p>
              </td>
              <td class="px-6 py-4">
                <template v-if="isAdminRole(u)">
                  <span class="inline-flex items-center px-3 py-1 rounded-full text-xs font-bold bg-purple-100 text-purple-800">Sistem Yöneticisi</span>
                </template>
                <select
                  v-else
                  :value="roleValue(u)"
                  class="rounded-lg border border-gray-200 bg-white px-3 py-2 text-sm font-medium text-gray-800 focus:ring-2 focus:ring-blue-500 focus:border-transparent outline-none cursor-pointer"
                  :disabled="roleLoading[u.Id ?? u.id]"
                  @change="onRoleChange(u, $event)"
                >
                  <option value="User">User</option>
                  <option value="Manager">Manager</option>
                </select>
              </td>
              <td class="px-6 py-4 text-gray-700">{{ (u.UnitName ?? u.unitName) || '—' }}</td>
              <td class="px-6 py-4">
                <span class="text-blue-600 font-semibold">{{ (u.RemainingLeaveDays ?? u.remainingLeaveDays) ?? 0 }} Gün</span>
              </td>
              <td class="px-6 py-4 text-gray-600 text-sm">{{ formatDate(u.CreatedAt ?? u.createdAt) }}</td>
              <td class="px-6 py-4">
                <button
                  v-if="!isAdminRole(u)"
                  type="button"
                  class="px-3 py-1.5 bg-red-600 text-white text-sm font-medium rounded-lg hover:bg-red-700 transition disabled:opacity-50"
                  :disabled="deleteLoading[u.Id ?? u.id]"
                  @click="confirmDelete(u)"
                >
                  {{ deleteLoading[u.Id ?? u.id] ? 'Siliniyor...' : 'Sil' }}
                </button>
                <span v-else class="text-xs text-gray-400">—</span>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
      <p v-if="!loading && !filteredUsers.length" class="px-6 py-8 text-center text-gray-500">
        {{ searchQuery.trim() ? 'Arama kriterine uygun kullanıcı bulunamadı.' : 'Arama yaparak kullanıcıları listeleyebilirsiniz.' }}
      </p>
      <p v-if="loading" class="px-6 py-8 text-center text-gray-500">Yükleniyor...</p>
    </div>
  </div>
</template>

<script setup>
definePageMeta({ layout: 'admin', middleware: 'auth' });

const API_BASE = 'https://localhost:44365'; // Projedeki backend base URL ile değiştir

const { token, isSystemAdmin } = useAuth();
const users = ref([]);
const searchQuery = ref('');
const loading = ref(true);
const roleLoading = ref({});
const deleteLoading = ref({});

const filteredUsers = computed(() => {
  const q = (searchQuery.value || '').trim().toLowerCase();
  if (!q) return [];
  return users.value.filter((u) => {
    const name = (u.FullName ?? u.fullName ?? '').toString().toLowerCase();
    const email = (u.Email ?? u.email ?? '').toString().toLowerCase();
    return name.startsWith(q) || email.startsWith(q);
  });
});

onMounted(() => {
  if (!token.value) { navigateTo('/login'); return; }
  if (!isSystemAdmin.value) { navigateTo('/dashboard'); return; }
  fetchUsers();
});

function roleValue(u) {
  const r = (u.Role ?? u.role ?? '').toString();
  return r.toLowerCase() === 'manager' ? 'Manager' : 'User';
}
function isAdminRole(u) {
  return (u.Role ?? u.role ?? '').toLowerCase() === 'admin';
}
function formatDate(value) {
  if (!value) return '—';
  let v = value;
  if (typeof v === 'string' && /^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}/.test(v) && !/Z|[+-]\d{2}:?\d{2}$/.test(v))
    v = v.replace(/\.\d{3}$/, '') + 'Z';
  const d = new Date(v);
  return d.toLocaleString('tr-TR', { day: '2-digit', month: '2-digit', year: 'numeric', hour: '2-digit', minute: '2-digit', timeZone: 'Europe/Istanbul' });
}

async function fetchUsers() {
  loading.value = true;
  try {
    const data = await $fetch(`${API_BASE}/api/Admin/users`, { headers: { Authorization: `Bearer ${token.value}` } });
    users.value = Array.isArray(data) ? data : [];
  } catch (e) {
    users.value = [];
  } finally {
    loading.value = false;
  }
}

function onRoleChange(u, event) {
  const newRole = event.target.value;
  const currentRole = roleValue(u);
  if (newRole === currentRole) return;
  if (!confirm(`Rolü "${newRole}" olarak değiştirmek istediğinize emin misiniz?`)) {
    event.target.value = currentRole;
    return;
  }
  const id = u.Id ?? u.id;
  if (newRole === 'Manager') promoteToManager(id, u, event);
  else demoteToUser(id, u, event);
}

async function promoteToManager(id, u, event) {
  roleLoading.value = { ...roleLoading.value, [id]: true };
  try {
    await $fetch(`${API_BASE}/api/Admin/promote-to-manager/${id}`, { method: 'POST', headers: { Authorization: `Bearer ${token.value}` } });
    const idx = users.value.findIndex(x => (x.Id ?? x.id) === id);
    if (idx !== -1) users.value[idx] = { ...users.value[idx], Role: 'Manager', role: 'Manager' };
  } catch (e) {
    alert((e?.data?.message) || 'Atama yapılamadı.');
    event.target.value = 'User';
  } finally {
    roleLoading.value = { ...roleLoading.value, [id]: false };
  }
}

async function demoteToUser(id, u, event) {
  roleLoading.value = { ...roleLoading.value, [id]: true };
  try {
    await $fetch(`${API_BASE}/api/Admin/demote-to-user/${id}`, { method: 'POST', headers: { Authorization: `Bearer ${token.value}` } });
    const idx = users.value.findIndex(x => (x.Id ?? x.id) === id);
    if (idx !== -1) users.value[idx] = { ...users.value[idx], Role: 'User', role: 'User' };
  } catch (e) {
    alert((e?.data?.message) || 'Yetki düşürülemedi.');
    event.target.value = 'Manager';
  } finally {
    roleLoading.value = { ...roleLoading.value, [id]: false };
  }
}

function confirmDelete(u) {
  const name = (u.FullName ?? u.fullName) || (u.Email ?? u.email);
  if (!confirm(`"${name}" kullanıcısını kalıcı olarak silmek istediğinize emin misiniz?`)) return;
  deleteUser(u);
}

async function deleteUser(u) {
  const id = u.Id ?? u.id;
  if (id == null) return;
  deleteLoading.value = { ...deleteLoading.value, [id]: true };
  try {
    await $fetch(`${API_BASE}/api/Admin/users/${id}`, { method: 'DELETE', headers: { Authorization: `Bearer ${token.value}` } });
    users.value = users.value.filter(x => (x.Id ?? x.id) !== id);
  } catch (e) {
    alert((e?.data?.message) || 'Kullanıcı silinemedi.');
  } finally {
    deleteLoading.value = { ...deleteLoading.value, [id]: false };
  }
}
</script>
```

---

## 4. Kontrol listesi

- [ ] Backend: `User` modelinde `CreatedAt` var; `Users` tablosunda sütun var.
- [ ] Backend: `AdminController` eklendi; namespace/DbContext projeye uyarlandı.
- [ ] Backend: Uygulama başlangıcında admin99@gmail.com / 9876 / Role=Admin seed’i çalışıyor.
- [ ] Frontend: `useAuth` içinde `isSystemAdmin` var ve döndürülüyor.
- [ ] Frontend: Login sonrası Admin ise `/admin-panel`, değilse dashboard (veya ana sayfa).
- [ ] Frontend: Admin layout’ta Dashboard `v-if="userRole && !isSystemAdmin"`; Sistem Yönetimi sadece `isSystemAdmin` için.
- [ ] Frontend: `pages/admin-panel.vue` eklendi; içinde `API_BASE` projedeki backend URL’e ayarlandı.
- [ ] Auth middleware veya route guard: `/admin-panel` sadece giriş yapmış ve Admin rolüne sahip kullanıcıya açık (isteğe göre middleware’de `isSystemAdmin` kontrolü).

Bu adımları uyguladığında, admin paneli bu projedeki davranışla uyumlu şekilde diğer projeye entegre olur.
