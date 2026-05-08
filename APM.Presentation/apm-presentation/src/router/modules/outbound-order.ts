import type { RouteRecordRaw } from 'vue-router'

// 这里的 Layout 通常是你系统的主界面外壳
const Layout = () => import('@/views/layout/Layout.vue')

const outboundRoutes: RouteRecordRaw[] = [
    {
        path: '/outbound-order',
        component: Layout,
        redirect: '/outbound-order/list',
        meta: { title: '出库管理', icon: 'Goods' },
        children: [
            {
                path: 'list',
                name: 'OutboundList',
                component: () => import('@/views/outbound-order/OutboundOrder.vue'),
                meta: { title: '配件档案' }
            },
            {
                path: 'edit/:id?',
                name: 'OutboundEdit',
                component: () => import('@/views/outbound-order/OutboundOrderEdit.vue'),
                meta: { title: '编辑出库单', hidden: true }
            }
        ]
    },
]

export default outboundRoutes