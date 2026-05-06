<script setup lang="ts">
import { ref, reactive, watch } from 'vue'
import { ElMessage, type FormInstance, type FormRules } from 'element-plus'
import type { Customer } from '@/interfaces/Entities'
import UsualEntityService from '@/services/UsualEntityService'
import type { UsualApiData } from '@/interfaces/HttpReponse'
import { _initialCustomer } from '@/utils/initialEntity'
import { ConstDictionary } from '@/utils/const-dictionary'

const props = defineProps<{
  modelValue: boolean
  customer: Customer | null
}>()
const emit = defineEmits(['update:modelValue', 'saved'])

const visible = ref(props.modelValue)
const formRef = ref<FormInstance>()
const customer = ref<Customer>({ ..._initialCustomer })

const rules = reactive<FormRules>({
  name: [{ required: true, message: '请输入客户名称', trigger: 'blur' }],
  phone: [{ required: true, message: '请输入联系电话', trigger: 'blur' }],
  address: [{ required: true, message: '请输入地址', trigger: 'blur' }],
})

const close = () => {
  formRef.value?.clearValidate()
  emit('update:modelValue', false)
}

const handleSave = async () => {
  try {
    await formRef.value?.validate()
    await UsualEntityService.Edit('Customer', customer.value)
    ElMessage.success('保存成功')
    emit('saved')
    close()
  } catch (error) {
    console.error('EditCustomer failed', error)
  }
}

watch(
  () => props.modelValue,
  (val: boolean) => {
    visible.value = val
  },
)

watch(visible, (val: boolean) => {
  emit('update:modelValue', val)
})

watch(
  () => props.customer,
  (c: Customer | null) => {
    if (c && c.id && c.id !== ConstDictionary.EMPTY_GUID) {
      UsualEntityService.Get<Customer>('Customer', c.id)
        .then((res: UsualApiData<Customer>) => {
          if (res.data) {
            customer.value = res.data as Customer
          }
        })
        .catch((error) => {
          console.error('加载客户数据失败', error)
        })
    } else {
      customer.value = { ..._initialCustomer }
    }
  },
  { immediate: true },
)
</script>

<template>
  <el-dialog
    v-model="visible"
    :title="customer.id ? '客户编辑' : '客户新增'"
    width="35vw"
    @close="close"
  >
    <el-form :model="customer" :rules="rules" label-width="100px" ref="formRef">
      <el-form-item label="名称" prop="name">
        <el-input v-model="customer.name" />
      </el-form-item>
      <el-form-item label="联系人" prop="contactPerson">
        <el-input v-model="customer.contactPerson" />
      </el-form-item>
      <el-form-item label="电话" prop="phone">
        <el-input v-model="customer.phone" />
      </el-form-item>
      <el-form-item label="地址" prop="address">
        <el-input v-model="customer.address" />
      </el-form-item>
      <el-form-item label="备注" prop="Remark">
        <el-input type="textarea" v-model="customer.remark" />
      </el-form-item>
    </el-form>
    <template #footer>
      <el-button @click="close">取消</el-button>
      <el-button type="primary" @click="handleSave">保存</el-button>
    </template>
  </el-dialog>
</template>

<style scoped></style>
