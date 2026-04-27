<script setup lang="ts"></script>

<template>
  <div class="side-menu">
    <div class="menu-title">APM 汽车配件管理系统</div>
    <el-menu default-active="1" class="side-menu-list" router="false" unique-opened>
      <el-menu-item index="/">首页</el-menu-item>
      <el-menu-item index="/Part">配件管理</el-menu-item>
      <el-menu-item index="/Orders">订单管理</el-menu-item>
      <el-menu-item index="/Settings">系统设置</el-menu-item>
    </el-menu>
  </div>
</template>

<style scoped>
.side-menu {
  height: 100vh;
  background-color: var(--basic-color-white);
  box-shadow: var(--basic-box-shadow);
}
.menu-title {
  font-size: 150%;
  font-weight: 700;
  text-align: center;
  padding: 18px 0;
}

.side-menu-list {
  border-right: none;
}
</style>
