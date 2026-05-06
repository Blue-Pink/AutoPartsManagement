import type { RouteRecordRaw } from 'vue-router'

// 这里的 Layout 通常是你系统的主界面外壳
const Layout = () => import('@/views/layout/Layout.vue')

const supplierRoutes: RouteRecordRaw[] = [
    {
        path: '/supplier',
        component: Layout,
        redirect: '/supplier/list',
        meta: { title: '供应商管理', icon: 'User' },
        children: [
            {
                path: 'list',
                name: 'SupplierList',
                component: () => import('@/views/supplier/Supplier.vue'),
                meta: { title: '供应商档案' }
            },
            // 以后增加供应商预警、分类等，直接在这里加
        ]
    },
]

export default supplierRoutes