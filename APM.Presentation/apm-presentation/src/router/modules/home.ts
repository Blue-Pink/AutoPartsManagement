import type { RouteRecordRaw } from 'vue-router'

// 这里的 Layout 通常是你系统的主界面外壳
const Layout = () => import('@/views/layout/Layout.vue')

const homeRoutes: RouteRecordRaw[] = [
    {
        path: '/',
        component: Layout,
        redirect: '/',
        meta: { title: '首页', icon: 'House' },
        children: [
            {
                path: '/home',
                name: 'Home',
                component: () => import('@/views/Home.vue'),
                meta: { title: '首页' }
            },
            {
                path: '/',
                name: 'Dashboard',
                component: () => import('@/views/dashboard/Dashboard.vue'),
                meta: { title: '首页' }
            },
        ]
    },
]

export default homeRoutes