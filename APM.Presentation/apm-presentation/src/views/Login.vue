<script setup lang="ts">
import { reactive, ref } from 'vue'
import UserService from '@/services/UserService'
import type { User } from '@/interfaces/DTOEntities'
import { userAuthStore } from '@/stores/auth'
import { ElMessage, type FormInstance, type FormRules } from 'element-plus'
import router from '@/router'

const userAuth = userAuthStore()
const userFormRef = ref<FormInstance>()

const user = reactive<User>({
  username: '',
  password: '',
} as User)

//定义表单验证规则(用户名必填8-20为仅字母数字-的组合,密码必填8-20为字母数字英文标点符的组合)
const rules = reactive<FormRules>({
  username: [
    { required: true, message: '请输入用户名', trigger: 'blur' },
    { min: 8, max: 20, message: '用户名长度必须在 8-20 之间', trigger: 'blur' },
    { pattern: /^[a-zA-Z0-9-]+$/, message: '用户名只能包含字母、数字和 -', trigger: 'blur' },
  ],
  password: [
    { required: true, message: '请输入密码', trigger: 'blur' },
    { min: 8, max: 30, message: '密码长度必须在 8-30 之间', trigger: 'blur' },
    {
      pattern: /^[a-zA-Z0-9!@#$%^&*()_+\-=[\]{};':"\\|,.<>/?]+$/,
      message: '密码只能包含字母、数字和英文标点符号',
      trigger: 'blur',
    },
  ],
})

const login = async (userForm: FormInstance | undefined) => {
  if (!userForm) return
  await userForm.validate((valid, fields) => {
    if (valid) {
      UserService.UserLogin(user)
        .then((res) => {
          if (res.data) {
            localStorage.setItem('token', res.data)
            userAuth.setLoginState(true)
            ElMessage({
              message: '登录成功',
              type: 'success',
              plain: true,
            })
            router.push('/')
          } else {
            throw new Error('登录失败，未返回token')
          }
        })
        .catch((err) => {
          console.warn('登录失败:', err)
        })
    } else {
      console.warn('表单验证失败:', fields)
    }
  })
}
</script>

<template>
  <div class="login-container">
    <h1 class="login-container-title">正在登录APM</h1>
    <el-form ref="userFormRef" :model="user" :rules="rules" label-position="top">
      <el-form-item label="用户名" prop="username">
        <el-input v-model="user.username" placeholder="请输入用户名" />
      </el-form-item>
      <el-form-item label="密码" prop="password">
        <el-input type="password" v-model="user.password" placeholder="请输入密码" />
      </el-form-item>
      <el-form-item class="login-button-container">
        <el-button type="primary" @click="login(userFormRef)">登录</el-button>
      </el-form-item>
    </el-form>
  </div>
</template>

<style scoped>
.login-container {
  max-width: 420px;
  margin: 40px auto;
  background-color: var(--basic-color-white);
  padding: 50px;
  border-radius: var(--basic-border-radius);
  margin-top: 10%;
  box-shadow: var(--basic-box-shadow);
}

.login-container .login-container-title {
  margin: 0 0 45px 0;
}

:deep(.login-button-container .el-form-item__content) {
  flex-direction: column;
  margin-top: 60px;
}
</style>
