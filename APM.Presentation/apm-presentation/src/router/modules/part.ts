import type { RouteRecordRaw } from 'vue-router'

// 这里的 Layout 通常是你系统的主界面外壳
const Layout = () => import('@/views/layout/Layout.vue')

const partRoutes: RouteRecordRaw[] = [
    {
        path: '/part',
        component: Layout,
        redirect: '/part/list',
        meta: { title: '配件管理', icon: 'Goods' },
        children: [
            {
                path: 'list',
                name: 'PartList',
                component: () => import('@/views/part/Part.vue'),
                meta: { title: '配件档案' }
            },
            // 以后增加配件预警、分类等，直接在这里加
        ]
    },
]

export default partRoutes