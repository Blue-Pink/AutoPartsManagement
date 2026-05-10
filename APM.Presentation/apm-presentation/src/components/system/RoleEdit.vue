<script setup lang="ts">
import { ref, watch } from 'vue'
import { ElMessage, type FormInstance, type FormRules } from 'element-plus'
import UsualEntityService from '@/services/UsualEntityService'
import type { Role } from '@/interfaces/Entities'
import { _initialRole } from '@/utils/initialEntity'
import { ConstDictionary } from '@/utils/const-dictionary'

const props = defineProps<{ modelValue: boolean; role: Role | null }>()
const emit = defineEmits(['update:modelValue', 'saved'])

const visible = ref(props.modelValue)
const formRef = ref<FormInstance>()
const role = ref<Role>({ ..._initialRole })
const rules = ref<FormRules>({
  roleName: [{ required: true, message: '请输入角色名', trigger: 'blur' }],
})

watch(
  () => props.modelValue,
  (v) => (visible.value = v),
)

watch(visible, (v) => emit('update:modelValue', v))

watch(
  () => props.role,
  (r: Role | null) => {
    try {
      if (r && r.id && r.id !== ConstDictionary.EMPTY_GUID) {
        UsualEntityService.Get<Role>('Role', r.id ?? '').then((res) => {
          if (res.data) {
            role.value = res.data as Role
          } else {
            visible.value = false
          }
        })
      } else {
        role.value = { ..._initialRole }
      }
    } catch (error) {
      visible.value = false
      console.log('加载用户数据失败', error)
    }
  },
  {
    immediate: true,
  },
)

const close = () => {
  formRef.value?.clearValidate()
  emit('update:modelValue', false)
}

const handleSave = async () => {
  try {
    await formRef.value?.validate?.()
    await UsualEntityService.Edit<Role>('Role', role.value)
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
    :title="role.id ? '角色编辑' : '角色新增'"
    width="30vw"
    @close="close"
  >
    <el-form :model="role" :rules="rules" ref="formRef" label-width="100px">
      <el-form-item label="角色名" prop="roleName">
        <el-input v-model="role.roleName" />
      </el-form-item>
      <el-form-item label="备注" prop="description">
        <el-input v-model="role.description" />
      </el-form-item>
    </el-form>
    <template #footer>
      <el-button @click="close">取消</el-button>
      <el-button type="primary" @click="handleSave">保存</el-button>
    </template>
  </el-dialog>
</template>
