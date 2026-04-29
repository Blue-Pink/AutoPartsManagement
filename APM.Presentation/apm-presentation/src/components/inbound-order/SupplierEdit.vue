<script setup lang="ts">
import { ref, watch, onMounted, reactive } from 'vue'
import { ElMessage, type FormInstance, type FormRules } from 'element-plus'
import SupplierService from '@/services/SupplierService'
import type { Supplier } from '@/interfaces/DTOEntities'
import UsualEntityService from '@/services/UsualEntityService'
import type { UsualApiData } from '@/interfaces/HttpReponse'
import { _initialSupplier } from '@/utils/initialEntity'

const props = defineProps<{
  modelValue: boolean
  supplier: Supplier | null
}>()

const emit = defineEmits(['update:modelValue', 'saved'])

const visible = ref(props.modelValue)
const formRef = ref<FormInstance>()
const supplier = ref<Supplier>({ ..._initialSupplier })

const rules = reactive<FormRules>({
  name: [{ required: true, message: '请输入供应商名称', trigger: 'blur' }],
  phone: [{ required: true, message: '请输入联系电话', trigger: 'blur' }],
})

const close = () => {
  emit('update:modelValue', false)
}

const handleSave = async () => {
  try {
    await formRef.value?.validate()
    await SupplierService.EditSupplier(supplier.value)
    ElMessage.success('保存成功')
    emit('saved')
    close()
  } catch (error) {
    console.error('EditSupplier failed', error)
  }
}

onMounted(() => {})

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
  () => props.supplier,
  (s: Supplier | null) => {
    if (s && s.id) {
      UsualEntityService.Get<Supplier>('Supplier', s.id)
        .then((res: UsualApiData<Supplier>) => {
          if (res.data) {
            supplier.value = res.data as Supplier
          }
        })
        .catch((error) => {
          console.error('加载供应商数据失败', error)
        })
    } else {
      supplier.value = { ..._initialSupplier }
    }
  },
  { immediate: true },
)
</script>

<template>
  <el-dialog
    v-model="visible"
    :title="supplier.id ? '供应商编辑' : '供应商新增'"
    width="30vw"
    @close="close"
  >
    <el-form :model="supplier" :rules="rules" label-width="100px" ref="formRef">
      <el-form-item label="名称" prop="name">
        <el-input v-model="supplier.name" />
      </el-form-item>
      <el-form-item label="联系人" prop="contact">
        <el-input v-model="supplier.contact" />
      </el-form-item>
      <el-form-item label="联系电话" prop="phone">
        <el-input v-model="supplier.phone" />
      </el-form-item>
      <el-form-item label="地址" prop="address">
        <el-input v-model="supplier.address" />
      </el-form-item>
    </el-form>
    <template #footer>
      <el-button @click="close">取消</el-button>
      <el-button type="primary" @click="handleSave">保存</el-button>
    </template>
  </el-dialog>
</template>
