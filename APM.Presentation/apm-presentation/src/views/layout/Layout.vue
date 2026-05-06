<script setup lang="ts">
import SideMenu from '@/components/side-menu/SideMenu.vue'
import HeaderUser from '@/components/header-user/HeaderUser.vue'
</script>

<template>
  <el-container>
    <el-aside width="320px">
      <SideMenu />
    </el-aside>
    <el-container>
      <el-header class="app-header">
        <HeaderUser />
      </el-header>
      <el-main class="app-main">
        <router-view />
      </el-main>
    </el-container>
  </el-container>
</template>

<style scoped>
.app-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 0 20px;
}
.el-main.app-main {
  padding-top: 0;
  padding-bottom: 0;
}
</style>
