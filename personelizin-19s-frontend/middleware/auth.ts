export default defineNuxtRouteMiddleware((to) => {
  if (import.meta.client) {
    const token = localStorage.getItem('token')
    const publicPaths = ['/', '/login', '/register']
    if (!token && !publicPaths.includes(to.path)) {
      return navigateTo('/login')
    }
    if (token && localStorage.getItem('mustChangePassword') === 'true' && to.path !== '/change-password') {
      return navigateTo('/change-password')
    }
  }
})