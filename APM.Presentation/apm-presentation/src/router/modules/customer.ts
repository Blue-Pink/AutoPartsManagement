import type { RouteRecordRaw } from 'vue-router'

const Layout = () => import('@/views/layout/Layout.vue')

const customerRoutes: RouteRecordRaw[] = [
    {
        path: '/customer',
        component: Layout,
        redirect: '/customer/list',
        meta: { title: '客户管理', icon: 'User' },
        children: [
            {
                path: 'list',
                name: 'CustomerList',
                component: () => import('@/views/customer/Customer.vue'),
                meta: { title: '客户档案' },
            },
        ],
    },
]

export default customerRoutes
