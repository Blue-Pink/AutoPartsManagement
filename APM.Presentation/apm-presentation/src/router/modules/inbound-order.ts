import type { RouteRecordRaw } from 'vue-router'

// 这里的 Layout 通常是你系统的主界面外壳
const Layout = () => import('@/views/layout/Layout.vue')

const inboundRoutes: RouteRecordRaw[] = [
    {
        path: '/inbound-order',
        component: Layout,
        redirect: '/inbound-order/list',
        meta: { title: '入库管理', icon: 'Goods' },
        children: [
            {
                path: 'list',
                name: 'InboundList',
                component: () => import('@/views/inbound-order/InboundOrder.vue'),
                meta: { title: '配件档案' }
            },
            {
                path: 'edit/:id?',
                name: 'InboundEdit',
                component: () => import('@/views/inbound-order/InboundOrderEdit.vue'),
                meta: { title: '编辑入库单', hidden: true }
            }
        ]
    },
]

export default inboundRoutes