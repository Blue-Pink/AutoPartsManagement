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
      <el-menu-item index="/"
        ><template #title>
          <el-icon><house /></el-icon>
          <span>首页</span>
        </template>
      </el-menu-item>
      <el-sub-menu index="1">
        <template #title>
          <el-icon><DocumentChecked /></el-icon>
          <span>单据管理</span>
        </template>
        <el-menu-item index="/part/list">配件管理</el-menu-item>
        <el-menu-item index="/inbound-order/list">入库单管理</el-menu-item>
        <el-menu-item index="/outbound-order/list">出库单管理</el-menu-item>
      </el-sub-menu>
      <el-sub-menu index="2">
        <template #title>
          <el-icon><user /></el-icon>
          <span>关系管理</span>
        </template>
        <el-menu-item index="/supplier/list">供应商管理</el-menu-item>
        <el-menu-item index="/customer/list">客户管理</el-menu-item>
      </el-sub-menu>
      <el-sub-menu index="3">
        <template #title>
          <el-icon><setting /></el-icon>
          <span>系统设置</span>
        </template>
        <el-menu-item index="/system/user-list">用户管理</el-menu-item>
        <el-menu-item index="/system/role-list">角色管理</el-menu-item>
        <el-menu-item index="/system/role-permission">角色权限管理</el-menu-item>
        <el-menu-item index="/system/entity-modify-record">数据变更记录</el-menu-item>
        <el-menu-item index="/system/settings">系统设置</el-menu-item>
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
