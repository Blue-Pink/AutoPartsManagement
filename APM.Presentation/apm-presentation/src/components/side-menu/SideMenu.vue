<script setup lang="ts">
import router from '@/router'
</script>

<template>
  <div class="side-menu">
    <div class="menu-title">APM 汽车配件管理系统</div>
    <el-menu
      active-text-color="var(--basic-hightlight-color)"
      background-color="transparent"
      class="apm-menu"
      :default-active="router.currentRoute.value.path"
      router
    >
      <el-sub-menu index="1">
        <template #title>
          <el-icon><location /></el-icon>
          <span>配件管理</span>
        </template>
        <el-menu-item index="/">首页</el-menu-item>
        <el-menu-item index="/Part">配件管理</el-menu-item>
        <el-menu-item index="/Supplier">供应商管理</el-menu-item>
        <el-menu-item index="/InboundOrder">入库单管理</el-menu-item>
        <el-menu-item index="/Orders">订单管理</el-menu-item>
      </el-sub-menu>
      <el-sub-menu index="1">
        <template #title>
          <el-icon><location /></el-icon>
          <span>配件管理</span>
        </template>
        <el-menu-item index="/User">用户管理</el-menu-item>
        <el-menu-item index="/Settings">系统设置</el-menu-item>
      </el-sub-menu>
    </el-menu>
  </div>
</template>

<style scoped>
.side-menu {
  height: 100vh;
  background-color: var(--basic-color-white);
  box-shadow: var(--basic-box-shadow);
  --el-menu-bg-color: var(--basic-backcolor);
}
.menu-title {
  font-size: 150%;
  font-weight: 700;
  text-align: center;
  padding: 18px 0;
}
</style>
