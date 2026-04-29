<script setup lang="ts">
import { ref, watch, onMounted } from 'vue'
import { ElMessage, type FormInstance } from 'element-plus'
import InboundOrderService from '@/services/InboundOrderService'
import PartService from '@/services/PartService'
import type { InboundItem } from '@/interfaces/DTOEntities'
import { _initialInboundItem } from '@/utils/initialEntity'
import UsualEntityService from '@/services/UsualEntityService'

const props = defineProps<{
  modelValue: boolean
  item: InboundItem | null
  orderId: string | null
}>()
const emit = defineEmits(['update:modelValue', 'saved'])

const visible = ref(props.modelValue)
const item = ref<InboundItem>({ ..._initialInboundItem })
const parts = ref<any[]>([])
const formRef = ref<FormInstance>()

const loadParts = async () => {
  try {
    const res = await PartService.GetParts(1, 1000)
    parts.value = res.dataList || []
  } catch (e) {
    console.error(e)
  }
}

const close = () => {
  emit('update:modelValue', false)
}

const handleSave = async () => {
  try {
    await formRef.value?.validate?.()
    item.value.inboundOrderId = props.orderId || item.value.inboundOrderId
    item.value.totalAmount = (item.value.quantity || 0) * (item.value.price || 0)
    await UsualEntityService.Create('InboundItem', item.value)
    ElMessage.success('保存成功')
    emit('saved')
    close()
  } catch (e) {
    console.error(e)
  }
}

watch(
  () => props.modelValue,
  (v) => (visible.value = v),
)
watch(visible, (v) => emit('update:modelValue', v))

watch(
  () => props.item,
  (it) => {
    if (it && it.id) item.value = { ...it }
    else
      item.value = {
        ..._initialInboundItem,
        inboundOrderId: props.orderId || item.value.inboundOrderId,
      }
  },
)

onMounted(() => {
  loadParts()
})
</script>

<template>
  <el-dialog v-model="visible" title="明细编辑" width="36vw" @close="close">
    <el-form :model="item" ref="formRef" label-width="100px">
      <el-form-item label="配件" prop="partId">
        <el-select v-model="item.partId" placeholder="选择配件">
          <el-option v-for="p in parts" :key="p.id" :label="p.partName" :value="p.id" />
        </el-select>
      </el-form-item>
      <el-form-item label="数量" prop="quantity">
        <el-input-number v-model="item.quantity" :min="0" />
      </el-form-item>
      <el-form-item label="单价" prop="price">
        <el-input-number v-model="item.price" :min="0" :step="0.01" />
      </el-form-item>
    </el-form>
    <template #footer>
      <el-button @click="close">取消</el-button>
      <el-button type="primary" @click="handleSave">保存</el-button>
    </template>
  </el-dialog>
</template>
