<script setup lang="ts">
import { ref, watch } from 'vue'
import { ElMessage, type FormInstance, type FormRules } from 'element-plus'
import UserService from '@/services/UserService'
import UsualEntityService from '@/services/UsualEntityService'
import type { User, Role } from '@/interfaces/DTOEntities'
import { _initialUser } from '@/utils/initialEntity'

const props = defineProps<{ modelValue: boolean; user: User | null }>()
const emit = defineEmits(['update:modelValue', 'saved'])

const visible = ref(props.modelValue)
const formRef = ref<FormInstance>()
const user = ref<User>({ ..._initialUser })
const editPassword = ref(false)
const rolesList = ref<Role[]>([])
const rules = ref<FormRules>({
  username: [{ required: true, message: '请输入用户名', trigger: 'blur' }],
  realname: [{ required: true, message: '请输入真实姓名', trigger: 'blur' }],
  password: [
    {
      required: true,
      validator: (rule, value, callback) => {
        if (!user.value.id && !value) {
          return callback(new Error('请输入密码'))
        }
        if (value) {
          if (value.length < 8 || value.length > 30)
            return callback(new Error('密码长度必须在 8-30 之间'))
          if (!/^[a-zA-Z0-9!@#$%^&*()_+\-=[\]{};':"\\|,.<>/?]+$/.test(value))
            return callback(new Error('密码只能包含字母、数字和英文标点符号'))
        }
        callback()
      },
      trigger: 'blur',
    },
  ],
  confirmPassword: [
    {
      required: true,
      validator: (rule, value, callback) => {
        if (!user.value.id && !value) {
          return callback(new Error('请输入确认密码'))
        }
        if (value && (user.value.password ?? '') !== value) {
          return callback(new Error('两次密码不一致'))
        }
        callback()
      },
      trigger: 'blur',
    },
  ],
  roleIds: [
    {
      required: true,
      validator: (rule, value, callback) => {
        const val = value ?? []
        if (!Array.isArray(val) || val.length === 0)
          return callback(new Error('请至少指派一个角色'))
        callback()
      },
      trigger: 'change',
    },
  ],
})

watch(
  () => props.modelValue,
  (v) => (visible.value = v),
)

watch(visible, (v) => emit('update:modelValue', v))

watch(
  () => props.user,
  (u: User | null) => {
    if (u && u.id) {
      UsualEntityService.Get<User>('User', u.id ?? '').then((res) => {
        if (res.data) {
          user.value = res.data as User
          user.value.roles = u.roles || []
          user.value.roleIds = u.roles?.map((r) => r.id).filter((id): id is string => !!id) || []
        } else {
          visible.value = false
        }
      })
    } else {
      user.value = { ..._initialUser }
    }
  },
  {
    immediate: true,
  },
)

watch(
  () => user.value.roleIds,
  (roleIds) => {
    if (roleIds && roleIds.length && rolesList.value) {
      user.value.roles = rolesList.value.filter((r) => roleIds?.includes(r.id ?? '')) || []
    }
  },
)

// 当密码变化时，触发对确认密码字段的验证（如果用户已输入确认密码）
watch(
  () => user.value.password,
  () => {
    if (user.value && user.value.confirmPassword && formRef.value?.validateField) {
      formRef.value.validateField('confirmPassword')
    }
  },
)

// 加载角色列表
async function loadRoles() {
  try {
    const res = await UserService.GetRoles(1, 1000)
    if ((res as any).dataList) rolesList.value = (res as any).dataList as Role[]
  } catch (e) {
    console.error('加载角色失败', e)
  }
}

watch(
  () => visible.value,
  (v) => {
    if (v) loadRoles()
  },
)

const close = () => {
  editPassword.value = false
  emit('update:modelValue', false)
}

const handleSave = async () => {
  try {
    await formRef.value?.validate?.()
    if ((user.value.password ?? '') !== (user.value.confirmPassword ?? '')) {
      ElMessage.error('两次密码不一致')
      return
    }
    await UserService.EditUser(user.value)
    ElMessage.success('保存成功')
    emit('saved')
    close()
  } catch (e) {
    console.error(e)
  }
}
</script>

<template>
  <el-dialog
    v-model="visible"
    :title="user.id ? '用户编辑' : '用户新增'"
    width="30vw"
    @close="close"
  >
    <el-form :model="user" :rules="rules" ref="formRef" label-width="100px">
      <el-form-item label="用户名" prop="username">
        <el-input v-model="user.username" />
      </el-form-item>
      <el-form-item label="真实姓名" prop="realname">
        <el-input v-model="user.realname" />
      </el-form-item>
      <el-form-item label="启用" prop="isActive">
        <el-switch v-model="user.isActive" active-color="#13ce66" inactive-color="#ff4949" />
      </el-form-item>
      <el-form-item label="角色" prop="roleIds">
        <el-select v-model="user.roleIds" multiple placeholder="请选择角色">
          <el-option
            v-for="r in rolesList || []"
            :key="r.id"
            :label="r.description || r.roleName || r.id"
            :value="r.id"
          />
        </el-select>
      </el-form-item>
      <el-form-item v-if="user && user.id">
        <el-button link type="primary" size="small" @click="editPassword = !editPassword"
          >修改密码</el-button
        >
      </el-form-item>
      <el-form-item label="密码" prop="password" v-if="editPassword || !user.id">
        <el-input type="password" v-model="user.password" />
      </el-form-item>
      <el-form-item label="确认密码" prop="confirmPassword" v-if="editPassword || !user.id">
        <el-input type="password" v-model="user.confirmPassword" />
      </el-form-item>
    </el-form>
    <template #footer>
      <el-button @click="close">取消</el-button>
      <el-button type="primary" @click="handleSave">保存</el-button>
    </template>
  </el-dialog>
</template>
