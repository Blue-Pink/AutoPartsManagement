<script setup lang="ts">
import { ref, watch, onMounted } from 'vue'
import { ElMessage, type FormInstance } from 'element-plus'
import PartService from '@/services/PartService'
import type { InboundItem, Part } from '@/interfaces/DTOEntities'
import { _initialInboundItem } from '@/utils/initialEntity'
import UsualEntityService from '@/services/UsualEntityService'
import { ConstDictionary } from '@/utils/const-dictionary'

const props = defineProps<{
  modelValue: boolean
  item: InboundItem | null
  orderId: string | null
}>()
const emit = defineEmits(['update:modelValue', 'saved'])
const visible = ref(props.modelValue)
const inboundItem = ref<InboundItem>({ ..._initialInboundItem })
const parts = ref<Part[]>([])
const formRef = ref<FormInstance>()
const rules = {
  partId: [{ required: true, message: '请选择配件', trigger: 'change' }],
  quantity: [{ required: true, message: '请输入数量', trigger: 'blur' }],
  price: [{ required: true, message: '请输入单价', trigger: 'blur' }],
}
const loadParts = async () => {
  try {
    const res = await PartService.GetParts(1, 1000)
    parts.value = res.dataList || []
  } catch (e) {
    console.error(e)
  }
}

const close = () => {
  formRef.value?.clearValidate()
  emit('update:modelValue', false)
}

const handlePartChange = (partId: string) => {
  const selectedPart = parts.value.find((p) => p.id === partId)
  if (selectedPart) {
    inboundItem.value.price = selectedPart.costPrice || 0
    handlePriceChange(inboundItem.value.price || 0)
  }
}

const handleQuantityChange = (quantity: number) => {
  if (inboundItem.value.price) {
    inboundItem.value.totalAmount = quantity * (inboundItem.value.price || 0) || 0.01
  }
}

const handlePriceChange = (price: number) => {
  if (inboundItem.value.quantity) {
    inboundItem.value.totalAmount = (inboundItem.value.quantity || 0) * (price || 0) || 0.01
  }
}

const handleSave = async () => {
  try {
    await formRef.value?.validate?.()
    inboundItem.value.inboundOrderId = props.orderId || inboundItem.value.inboundOrderId
    inboundItem.value.totalAmount =
      (inboundItem.value.quantity || 0) * (inboundItem.value.price || 0)
    await UsualEntityService.Edit('InboundItem', inboundItem.value)
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
  (i) => {
    if (i && i.id && i.id !== ConstDictionary.EMPTY_GUID) {
      UsualEntityService.Get<InboundItem>('InboundItem', i.id).then((res) => {
        if (res && res.data) inboundItem.value = res.data
      })
    } else
      inboundItem.value = {
        ..._initialInboundItem,
        inboundOrderId: props.orderId || inboundItem.value.inboundOrderId,
      }
  },
)

onMounted(() => {
  loadParts()
})
</script>

<template>
  <el-dialog v-model="visible" title="明细编辑" width="36vw" @close="close">
    <el-form :model="inboundItem" :rules="rules" ref="formRef" label-width="100px">
      <el-form-item label="配件" prop="partId">
        <el-select v-model="inboundItem.partId" placeholder="选择配件" @change="handlePartChange">
          <el-option
            v-for="p in parts"
            :key="p.id"
            :label="`[${p.model}] - [${p.partName}] - [¥${p.costPrice}/${p.unitName}]`"
            :value="p.id"
          />
        </el-select>
      </el-form-item>
      <el-form-item label="数量" prop="quantity">
        <el-input-number v-model="inboundItem.quantity" :min="1" @change="handleQuantityChange" />
      </el-form-item>
      <el-form-item label="单价" prop="price">
        <el-input-number
          v-model="inboundItem.price"
          :min="0.01"
          :step="0.01"
          @change="handlePriceChange"
        />
      </el-form-item>
      <el-form-item label="合计" prop="totalAmount">
        <el-input-number v-model="inboundItem.totalAmount" :min="0.01" :step="0.01" disabled />
      </el-form-item>
    </el-form>
    <template #footer>
      <el-button @click="close">取消</el-button>
      <el-button type="primary" @click="handleSave">保存</el-button>
    </template>
  </el-dialog>
</template>
