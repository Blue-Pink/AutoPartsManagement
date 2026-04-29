import { createRouter, createWebHistory } from 'vue-router'
import Login from '@/views/Login.vue'
import Home from '@/views/Home.vue'
import Part from '@/views/part/Part.vue'
import Supplier from '@/views/inbound-order/Supplier.vue'
import InboundOrder from '@/views/inbound-order/InboundOrder.vue'
import InboundOrderEdit from '@/views/inbound-order/InboundOrderEdit.vue'
import User from '@/views/user/User.vue'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/Part',
      name: '配件管理',
      component: Part,
    },
    {
      path: '/Supplier',
      name: '供应商管理',
      component: Supplier,
    },
    {
      path: '/InboundOrder',
      name: '入库单管理',
      component: InboundOrder,
    },
    {
      path: '/InboundOrder/edit/:id?',
      name: '入库单编辑',
      component: InboundOrderEdit,
    },
    {
      path: '/user',
      name: '用户管理',
      component: User,
    },
    {
      path: '/Login',
      name: '登录',
      component: Login,
    },
    {
      path: '/',
      name: '首页',
      component: Home,
    },
  ],
})

export default router
