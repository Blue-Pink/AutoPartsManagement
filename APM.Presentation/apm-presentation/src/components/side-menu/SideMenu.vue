<script setup lang="ts"></script>

<template>
  <div class="side-menu">
    <div class="menu-title">APM 汽车配件管理系统</div>
    <el-menu default-active="1" class="side-menu-list" router="false" unique-opened>
      <el-menu-item index="/">首页</el-menu-item>
      <el-menu-item index="2">库存管理</el-menu-item>
      <el-menu-item index="3">订单管理</el-menu-item>
      <el-menu-item index="4">系统设置</el-menu-item>
    </el-menu>
  </div>
</template>

<style scoped>
.side-menu {
  height: 100vh;
  background-color: var(--basic-color-white);
}
.menu-title {
  font-size: 170%;
  font-weight: 700;
  text-align: center;
  padding: 10px 0;
}

.side-menu-list {
  border-right: none;
}

:deep(.side-menu-list .el-menu-item) {
  margin: 5px;
}
</style>
