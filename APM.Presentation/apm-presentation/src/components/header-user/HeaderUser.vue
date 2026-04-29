<script setup lang="ts">
import { ref, onMounted } from 'vue'
import UserService from '@/services/UserService'
import type { User } from '@/interfaces/DTOEntities'
import { _initialUser } from '@/utils/initialEntity'
import { MoonNight, Sunrise } from '@element-plus/icons-vue'

const currentUser = ref<User>({ ..._initialUser })
const uiMod = ref<'light' | 'dark'>(
  localStorage.getItem('apm-ui-mode') === 'dark' ? 'dark' : 'light',
)

const loadCurrentUser = async () => {
  try {
    const res = await UserService.GetCurrentUser()
    currentUser.value = res.data || { ..._initialUser }
  } catch (error) {
    console.warn('获取当前用户信息失败', error)
  }
}

const changeUIMod = () => {
  const html = document.documentElement
  if (html.classList.contains('dark')) {
    html.classList.remove('dark')
    localStorage.setItem('apm-ui-mode', 'light')
    uiMod.value = 'light'
  } else {
    html.classList.add('dark')
    localStorage.setItem('apm-ui-mode', 'dark')
    uiMod.value = 'dark'
  }
}

onMounted(loadCurrentUser)
</script>

<template>
  <div class="header-user">
    <div class="ui-avatar" @click="changeUIMod">
      <el-button :icon="uiMod === 'dark' ? MoonNight : Sunrise" circle plain />
    </div>
    <div class="username">{{ currentUser?.realname }}</div>
  </div>
</template>

<style scoped>
.header-user {
  display: flex;
  margin-left: auto;
  align-items: center;
  padding: 0 20px;
}

.username {
  font-weight: 700;
}

.ui-avatar {
  width: 38px;
  height: 38px;
  border-radius: 50%;
  background-color: var(--basic-backcolor-light);
  display: flex;
  align-items: center;
  justify-content: center;
  margin-right: 10px;
}
</style>
