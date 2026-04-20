<script setup lang="ts">
import { onMounted } from 'vue'
import UserRoleService from './services/UserRoleService'
import router from './router'
import { userAuthStore } from '@/stores/auth'
import SideMenu from '@/components/side-menu/SideMenu.vue'
import HeaderUser from '@/components/header-user/HeaderUser.vue'

const userAuth = userAuthStore()

onMounted(() => {
  const checkToken = () => {
    UserRoleService.CheckUserToken()
      .then((res) => {
        if (res.data) {
          //判断路由是否在login页面,跳转到首页
          userAuth.setLoginState(true)
          if (router.currentRoute.value.fullPath.includes('/login')) router.push('/')
        } else {
          userAuth.setLoginState(false)
          router.push('/login')
        }
      })
      .catch(() => {
        userAuth.setLoginState(false)
        router.push('/login')
      })
  }

  //现在与每三分钟检查一次用户token信息
  checkToken()
  setInterval(() => {
    checkToken()
  }, 30 * 1000) // 每30s检查一次
})
</script>

<template>
  <div class="common-layout">
    <el-container>
      <el-aside width="320px" v-if="userAuth.loginState">
        <SideMenu />
      </el-aside>
      <el-container>
        <el-header class="app-header">
          <div class="header-title"></div>
          <HeaderUser v-if="userAuth.loginState" />
        </el-header>
        <el-main>
          <router-view />
        </el-main>
      </el-container>
    </el-container>
  </div>
</template>

<style scoped>
.app-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 0 20px;
}
</style>
