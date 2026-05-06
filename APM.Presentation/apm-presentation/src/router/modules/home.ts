import type { RouteRecordRaw } from 'vue-router'

// 这里的 Layout 通常是你系统的主界面外壳
const Layout = () => import('@/views/layout/Layout.vue')

const homeRoutes: RouteRecordRaw[] = [
    {
        path: '/',
        component: Layout,
        redirect: '/',
        meta: { title: '供应商管理', icon: 'User' },
        children: [
            {
                path: '/',
                name: '/',
                component: () => import('@/views/Home.vue'),
                meta: { title: '首页' }
            },
        ]
    },
]

export default homeRoutes