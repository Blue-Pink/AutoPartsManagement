import type { RouteRecordRaw } from 'vue-router'

// 这里的 Layout 通常是你系统的主界面外壳
const Layout = () => import('@/views/layout/Layout.vue')

const userRoutes: RouteRecordRaw[] = [
    {
        path: '/system',
        component: Layout,
        redirect: '/system/user-list',
        meta: { title: '用户管理', icon: 'User' },
        children: [
            {
                path: 'user-list',
                name: 'UserList',
                component: () => import('@/views/system/User.vue'),
                meta: { title: '用户档案' }
            },
            {
                path: 'settings',
                name: 'SystemSettings',
                component: () => import('@/views/system/Settings.vue'),
                meta: { title: '系统设置' }
            },
            {
                path: 'role-list',
                name: 'RoleList',
                component: () => import('@/views/system/Role.vue'),
                meta: { title: '角色管理' }
            },
            {
                path: 'role-permission',
                name: 'RolePermission',
                component: () => import('@/views/system/RolePermission.vue'),
                meta: { title: '角色权限管理' }
            }
        ]
    },
]

export default userRoutes