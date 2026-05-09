import { createRouter, createWebHistory, type RouteRecordRaw } from 'vue-router'
import UserService from '@/services/UserService'

// 1. 静态路由（无需权限，直接访问）
const staticRoutes: RouteRecordRaw[] = [
  {
    path: '/login',
    name: 'Login',
    component: () => import('@/views/login/Login.vue'),
    meta: { isPublic: true }
  }
]

// 2. 动态扫描 modules 文件夹下的所有路由模块
const modules = import.meta.glob('./modules/*.ts', { eager: true })
const asyncRoutes: RouteRecordRaw[] = []

Object.keys(modules).forEach((key) => {
  const mod = (modules[key] as any).default
  if (mod) {
    asyncRoutes.push(...mod)
  }
})

// 3. 组合所有路由
const router = createRouter({
  history: createWebHistory(),
  routes: [...staticRoutes, ...asyncRoutes]
})

// 4. 路由守卫逻辑（保持你之前的 Token 检查）
router.beforeEach(async (to, from) => {
  try {
    // if (to.meta.isPublic) {
    //   return true
    // }

    const res = await UserService.CheckUserToken()
    if (!res.data)
      return { path: '/login' }

    return to.path === '/login' ? { path: '/' } : true
  } catch (error) {
    console.log(router, error)
    if (to.path !== '/login') {
      return { path: '/login' };
    }
    return true;
  }
})

export default router
