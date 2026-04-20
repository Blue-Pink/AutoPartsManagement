<script setup lang="ts">
import { ref, onMounted } from 'vue'
import UserRoleService from '@/services/UserRoleService'
import type { User } from '@/interfaces/DTOEntities'

const currentUser = ref<User | null>(null)

const loadCurrentUser = async () => {
  try {
    const res = await UserRoleService.GetCurrentUser()
    currentUser.value = res.data
  } catch (error) {
    console.warn('获取当前用户信息失败', error)
  }
}

onMounted(loadCurrentUser)
</script>

<template>
  <div class="header-user">
    <span class="username">{{ currentUser?.realname }}</span>
  </div>
</template>

<style scoped>
.header-user {
  display: flex;
  align-items: center;
}

.username {
  font-weight: 700;
}
</style>
